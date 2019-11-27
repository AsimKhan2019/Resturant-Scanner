using Newtonsoft.Json;
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
        // <snippet_prediction>
        public static async Task<PredictionResult> PredictImageContentsAsync(byte[] imageStream, CancellationToken cancellationToken)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var credentialsFile = "LogoScanner.credentials.txt";
            string[] line;

            using (Stream stream = assembly.GetManifestResourceStream(credentialsFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                line = reader.ReadLine().Split('\t'); // opens credentials file, reads it and splits it via a tab
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Prediction-key", line[2]);

            byte[] imageData = imageStream;

            HttpResponseMessage response;
            using (var content = new ByteArrayContent(imageData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                response = await client.PostAsync(line[3], content);
            }

            var resultJson = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<PredictionResult>(resultJson);
        }
        // </snippet_prediction>

        private byte[] StreamToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }
    }
}
