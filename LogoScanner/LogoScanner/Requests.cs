using System.Net.Http;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace LogoScanner
{
    public class Requests
    {

        public struct Request
        {
            public string status;
            public string message;

            public Request(string status, string message)
            {
                this.status = status;
                this.message = message;
            }
        }

        public static async Task<Request> ConnectToResDiary()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var credentialsFile = "LogoScanner.credentials.txt";
            string[] line;

            using (Stream stream = assembly.GetManifestResourceStream(credentialsFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                line = reader.ReadLine().Split('\t');
            }

            string credentials = @"{""Username"" : """ + line[0] + @""", ""Password"" : """ + line[1] + @"""}";

            try
            {
                var content = new StringContent(credentials, Encoding.UTF8, "application/json");
                var client = new HttpClient();
                var response = await client.PostAsync("https://api.rdbranch.com/api/Jwt/v2/Authenticate", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    string status = JObject.Parse(result)["Status"].ToString();
                    string token = JObject.Parse(result)["Token"].ToString();

                    if (status.Equals("Fail") || token == null)
                    {
                        return new Request(status, "Invalid credentials");
                    }
                    else
                    {
                        return new Request(status, token);
                    }
                }
                else
                {
                    return new Request("Fail", "Unable to connect to RESDiary API");
                }
            }
            catch (HttpRequestException ex)
            {
                return new Request("Fail", ex.Message);
            }
        }
    }
}
