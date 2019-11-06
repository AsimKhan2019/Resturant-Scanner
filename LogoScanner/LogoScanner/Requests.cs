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
        // structure to store request status and corresponding message
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
                line = reader.ReadLine().Split('\t'); // opens credentials file, reads it and splits it via a tab
            }

            string credentials = @"{""Username"" : """ + line[0] + @""", ""Password"" : """ + line[1] + @"""}"; // parse in username/password to json

            try
            {
                var content = new StringContent(credentials, Encoding.UTF8, "application/json");
                var client = new HttpClient();
                var response = await client.PostAsync("https://api.rdbranch.com/api/Jwt/v2/Authenticate", content); // get response from the api

                if (response.IsSuccessStatusCode) // if call to api is successful 
                {
                    var result = await response.Content.ReadAsStringAsync();

                    string status = JObject.Parse(result)["Status"].ToString(); // parse the json to string format
                    string token = JObject.Parse(result)["Token"].ToString();

                    if (status.Equals("Fail") || token == null)
                    {
                        return new Request(status, "Invalid credentials");
                    }
                    else
                    {
                        return new Request(status, token); // return a successful request with api token
                    }
                }
                else
                {
                    return new Request("Fail", "Unable to connect to RESDiary API");
                }
            }
            catch (HttpRequestException ex) // handles request exception
            {
                return new Request("Fail", ex.Message);
            }
        }
    }
}
