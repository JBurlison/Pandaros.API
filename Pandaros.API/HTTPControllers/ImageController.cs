using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Pandaros.API.Extender;
using Pandaros.API.Models;
using Pandaros.API.Models.HTTP;

namespace Pandaros.API.HTTPControllers
{
    public class ImageController : IPandaController
    {
        [PandaHttp(OperationType.Get, "/Image/Get", "Gets an image based off the path provided (relitive or absolute) for icons/textures ect for colony survival.")]
        public RestResponse GetImage(string path)
        {
            return new RestResponse() { Content = File.ReadAllBytes(path), ContentType = "image/png" };
        }


    }
}
