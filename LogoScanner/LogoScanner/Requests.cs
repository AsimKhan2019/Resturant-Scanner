using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

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
            var connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
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
                        return new Request("Fail", "Unable to connect to ReSDiary API");
                    }
                }
                catch (HttpRequestException ex) // handles request exception
                {
                    return new Request("Fail", ex.Message);
                }
            }
            else
            {
                return new Request("Fail", "Unable to Connect to Internet");
            }
        }

        public static async Task<JArray> APICallGet(string url, string token)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(requestMessage);
            JArray result;
            if (response.IsSuccessStatusCode)
            {
                //Get the Results from the API Call
                var contents = await response.Content.ReadAsStringAsync();

                if (contents[0] != '[')
                {
                    contents = contents.Insert(0, "[");
                    contents += "]";
                }

                result = JArray.Parse(contents);

                return result;
            }

            return null;
        }
    }
}