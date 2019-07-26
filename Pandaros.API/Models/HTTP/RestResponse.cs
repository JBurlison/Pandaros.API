using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pandaros.API.Models.HTTP
{
    public class RestResponse
    {
        public static RestResponse Success = new RestResponse() { Content = new SuccessResponse() { Success = true }.ToUTF8SerializedJson() };
        public static RestResponse BlankJsonObject = new RestResponse() { Content = "{}".ToUTF8SerializedJson() };
        public static RestResponse BlankJsonArray = new RestResponse() { Content = "[]".ToUTF8SerializedJson() };

        public byte[] Content { get; set; }
        public string ContentType { get; set; } = "application/json";
        public int HttpCode { get; set; } = 200;
    }
}
