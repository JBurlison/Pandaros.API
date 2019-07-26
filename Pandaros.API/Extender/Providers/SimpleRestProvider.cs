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
                ApiCallbacks.Add(verb, new Dictionary<string, Tuple<object, MethodInfo>>(StringComparer.InvariantCultureIgnoreCase));

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
                        string methodName = context.Request.Url.AbsolutePath;

                        try
                        {
                            if (callbacks.TryGetValue(methodName, out var method))
                            {
                                var response = default(RestResponse);
                                var mehodParams = method.Item2.GetParameters();
                                
                                if (mehodParams.Length > 0)
                                {
                                    if (mehodParams.Length != context.Request.QueryString.Count)
                                    {
                                        context.Response.StatusCode = 422;
                                        context.Response.StatusDescription = "Number of parameter mis-match. Expected: " + mehodParams.Length;
                                        context.Response.OutputStream.Close();
                                        return;
                                    }

                                    foreach (var param in mehodParams)
                                    {
                                        if (!context.Request.QueryString.AllKeys.Any(k => k == param.Name))
                                        {
                                            context.Response.StatusCode = 422;
                                            context.Response.StatusDescription = "Missing Parameter. Expected: " + param.Name;
                                            context.Response.OutputStream.Close();
                                            return;
                                        }
                                    }

                                    foreach (var param in context.Request.QueryString.AllKeys)
                                    {
                                        if (!mehodParams.Any(k => k.Name == param))
                                        {
                                            context.Response.StatusCode = 422;
                                            context.Response.StatusDescription = "Unknown Parameter: " + param;
                                            context.Response.OutputStream.Close();
                                            return;
                                        }
                                    }

                                    object[] requestParams = mehodParams
                                                            .Select((p, i) => Convert.ChangeType(context.Request.QueryString[p.Name], p.ParameterType))
                                                            .ToArray();

                                    response = method.Item2.Invoke(method.Item1, requestParams) as RestResponse;
                                }
                                else
                                {
                                    response = method.Item2.Invoke(method.Item1, null) as RestResponse;
                                }

                                if (response != null)
                                {
                                    context.Response.ContentType = response.ContentType;
                                    context.Response.ContentLength64 = response.Content.Length;
                                    context.Response.OutputStream.Write(response.Content, 0, response.Content.Length);
                                }
                                else
                                {
                                    throw new InvalidCastException("All methods decorated with PandaHttp must return type RestResponse.");
                                }
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
