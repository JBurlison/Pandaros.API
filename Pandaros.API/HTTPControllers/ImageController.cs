using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class ImageController : IPandaController
    {
        [PandaHttp(RestVerb.Get, "/Image/Get")]
        public RestResponse GetImage(string path)
        {
            return new RestResponse() { Content = File.ReadAllBytes(path), ContentType = "image/png" };
        }


    }
}
