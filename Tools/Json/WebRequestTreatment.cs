using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Bastocos.Tools.Json
{
    public class WebRequestTreatment
    {
        public Task<T> ParseRequestBody<T>(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                var body = reader.ReadToEnd();
                Console.WriteLine(body);
                var result = JsonSerializer.Deserialize<T>(body);
                return Task.FromResult(result);
            }
        }
    }
}