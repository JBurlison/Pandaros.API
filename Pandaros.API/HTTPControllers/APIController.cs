using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Pandaros.API.Extender;
using Pandaros.API.Models.HTTP;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.HTTPControllers
{
    public class APIController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/", "The Pandaros.API Rest Interface")]
        public RestResponse OpenApiJson()
        {
            var doc = GetAPIDoc();
            var outputString = new StringWriter(CultureInfo.InvariantCulture);
            var writer = new OpenApiJsonWriter(outputString);
            doc.SerializeAsV3(writer);

            return new RestResponse() { Content = Encoding.UTF8.GetBytes(outputString.ToString()) };
        }

        [PandaHttp(OperationType.Get, "/Yaml", "The Pandaros.API Rest Interface")]
        public RestResponse OpenApiYaml()
        {
            var doc = GetAPIDoc();
            var outputString = new StringWriter(CultureInfo.InvariantCulture);
            var writer = new OpenApiYamlWriter(outputString);
            doc.SerializeAsV3(writer);

            return new RestResponse() { Content = Encoding.UTF8.GetBytes(outputString.ToString()), ContentType = "text/yaml" };
        }

        public OpenApiDocument GetAPIDoc()
        {
            var openApi = new OpenApiDocument()
            {
                Info = new OpenApiInfo()
                {
                    License = new OpenApiLicense()
                    {
                        Name = "MIT",
                        Url = new Uri("https://opensource.org/licenses/MIT", UriKind.Absolute)
                    },
                    Version = "1.0.0",
                    Title = "Pandaros.API Colony Survival Rest Serivce",
                    Contact = new OpenApiContact()
                    {
                        Email = "JBurlison@Gmail.com",
                        Name = "Jim Burlison",
                        Url = new Uri("http://www.settlersmod.com", UriKind.Absolute)
                    }
                },
                Servers = new List<OpenApiServer>()
                {
                    new OpenApiServer()
                    {
                        Url = "http://127.0.0.1:10984",
                        Description = "Default server configuration. Can be modified in Pandaros.Api.json in world save."
                    }
                },
                Paths = new OpenApiPaths()
            };

            foreach (var callback in Extender.Providers.SimpleRestProvider.Endpoints.OrderBy(kvp => kvp.Key))
            {
                openApi.Paths[callback.Key] = new OpenApiPathItem()
                {
                    Operations = new Dictionary<OperationType, OpenApiOperation>()
                };

                foreach (var verbRoute in callback.Value)
                {
                    openApi.Paths[callback.Key].Operations[verbRoute.Key] = new OpenApiOperation()
                    {
                        Description = verbRoute.Value.Item1,
                        Parameters = verbRoute.Value.Item2.GetParameters().Select(p =>
                        {
                            return new OpenApiParameter()
                            {
                                AllowEmptyValue = false,
                                Name = p.Name,
                                In = ParameterLocation.Query,
                                Required = true,
                                AllowReserved = true,
                                Schema = new OpenApiSchema()
                                {
                                    Type = p.ParameterType.Name
                                }
                            };
                        }).ToList()
                    };
                }
            }

            return openApi;
        }
    }
}
