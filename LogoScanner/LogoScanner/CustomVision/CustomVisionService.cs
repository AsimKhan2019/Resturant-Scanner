using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace LogoScanner
{
    public class CustomVisionService
    {
        public static async Task<PredictionResult> PredictImageContentsAsync(byte[] imageStream, CancellationToken cancellationToken)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var credentialsFile = "LogoScanner.credentials.json";
            JObject line;

            using (Stream stream = assembly.GetManifestResourceStream(credentialsFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                line = JObject.Parse(reader.ReadToEnd()); // opens credentials file, reads it and parse JSON
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", line["CustomVisionAPI"]["key"].ToString());

            byte[] imageData = imageStream;

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(imageData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(line["CustomVisionAPI"]["url"].ToString(), content);
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PredictionResult>(resultJson);
        }
    }
}
