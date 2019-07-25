using Newtonsoft.Json;
using Pandaros.API.Models.HTTP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Pandaros.API.Extender.Providers
{
    public class SimpleRestProvider : IAfterWorldLoad
    {
        public List<Type> LoadedAssembalies { get; set; } = new List<Type>();

        public string InterfaceName => nameof(IPandaController);

        public Type ClassType => null;

        private Thread _listenThread;
        public Dictionary<RestVerb, Dictionary<string, Tuple<object, MethodInfo>>> ApiCallbacks { get; set; } = new Dictionary<RestVerb, Dictionary<string, Tuple<object, MethodInfo>>>();

        public void AfterWorldLoad()
        {
            foreach (RestVerb verb in Enum.GetValues(typeof(RestVerb)))
                ApiCallbacks.Add(verb, new Dictionary<string, Tuple<object, MethodInfo>>());

            foreach (var ass in LoadedAssembalies)
            {
                var instance = Activator.CreateInstance(ass);

                foreach (var method in ass.GetMethods())
                    foreach (var someAtt in method.GetCustomAttributes(true))
                        if (someAtt is PandaHttp pandaGet)
                        {
                            if (ApiCallbacks[pandaGet.RestVerb].ContainsKey(pandaGet.Route))
                                APILogger.Log(ChatColor.red, $"Route {pandaGet.Route} already exists for virb {pandaGet.RestVerb}. Overriding existing registered {ApiCallbacks[pandaGet.RestVerb][pandaGet.Route].Item2.Name} with {method.Name}");

                            ApiCallbacks[pandaGet.RestVerb][pandaGet.Route] = Tuple.Create(instance, method);
                        }
            }

            _listenThread = new Thread(new ThreadStart(Listen));
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        public void Listen()
        {
            HttpListener listener = new HttpListener();
            var url = "http://" + APIConfiguration.CSModConfiguration.GetorDefault("PandaAPIAddress", "*") + ":" + APIConfiguration.CSModConfiguration.GetorDefault("PandaAPIPort", 10984) + "/";
            listener.Prefixes.Add(url);
            listener.Start();
            APILogger.Log(ChatColor.green, "Pandaros Rest API now listening on: {0}", url);

            while (true)
            {
                HttpListenerContext context = listener.GetContext();

                if (Enum.TryParse(context.Request.HttpMethod, true, out RestVerb verb) && ApiCallbacks.TryGetValue(verb, out var callbacks))
                    ThreadPool.QueueUserWorkItem((_) =>
                    {
                        string methodName = context.Request.Url.Segments[1].Replace("/", "");
                        string[] strParams = context.Request.Url
                                                .Segments
                                                .Skip(2)
                                                .Select(s => s.Replace("/", ""))
                                                .ToArray();

                        try
                        {
                            if (callbacks.TryGetValue(methodName, out var method))
                            {
                                var buffer = default(byte[]);
                                var mehodParams = method.Item2.GetParameters();

                                if (mehodParams.Length > 0)
                                {
                                    object[] requestParams = mehodParams
                                                            .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                                                            .ToArray();

                                    object ret = method.Item2.Invoke(method.Item1, requestParams);
                                    string retstr = JsonConvert.SerializeObject(ret);
                                    buffer = Encoding.UTF8.GetBytes(retstr);
                                }
                                else
                                {
                                    object ret = method.Item2.Invoke(method.Item1, null);
                                    string retstr = JsonConvert.SerializeObject(ret);
                                    buffer = Encoding.UTF8.GetBytes(retstr);
                                }

                                context.Response.ContentType = "application/json";
                                context.Response.ContentLength64 = buffer.Length;
                                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                            }
                            else
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                context.Response.StatusDescription = "No method registered with method name: " + methodName;
                            }
                        }
                        catch (Exception ex)
                        {
                            APILogger.LogError(ex);
                            context.Response.StatusCode = 500;
                        }

                        context.Response.OutputStream.Close();
                    });
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    context.Response.StatusDescription = "No rest verb registered with Http Mehtod: " + context.Request.HttpMethod;
                    context.Response.OutputStream.Close();
                }
            }
        }
    }
}
