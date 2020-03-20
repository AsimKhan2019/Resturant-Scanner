using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace LogoScanner
{
    public static class Requests
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

        //initial connection to the resDiary API
        public static async Task<Request> ConnectToResDiary()
        {
            var connectivity = Connectivity.NetworkAccess;

            if (connectivity == NetworkAccess.Internet)
            {
                string token;
                string tokenExpiracy;

                // check token expiracy
                if (Application.Current.Properties.ContainsKey("Token") & Application.Current.Properties.ContainsKey("TokenExpiryUtc"))
                {
                    token = Application.Current.Properties["Token"].ToString(); // get token which was save locally
                    tokenExpiracy = Application.Current.Properties["TokenExpiryUtc"].ToString(); //get time of token saved locally

                    if (DateTimeOffset.Parse(tokenExpiracy, CultureInfo.CurrentCulture).UtcDateTime > DateTime.UtcNow)
                    {
                        return new Request("Success", token); // return a successful request with api token
                    }
                }
                // else get a new token

                var assembly = Assembly.GetExecutingAssembly();
                var credentialsFile = "LogoScanner.credentials.json";
                JObject line;

                using (Stream stream = assembly.GetManifestResourceStream(credentialsFile))
                using (StreamReader reader = new StreamReader(stream))
                {
                    line = JObject.Parse(reader.ReadToEnd()); // opens credentials file, reads it and parse JSON
                }

                string credentials = @"{""Username"" : """ + line["ResDiaryAPI"]["username"].ToString() + @""", ""Password"" : """ + line["ResDiaryAPI"]["password"].ToString() + @"""}"; // parse in username/password to json

                try
                {
                    var content = new StringContent(credentials, Encoding.UTF8, "application/json");
                    var client = new HttpClient();

                    var response = await client.PostAsync("https://api.rdbranch.com/api/Jwt/v2/Authenticate", content); // get response from the api

                    if (response.IsSuccessStatusCode) // if call to api is successful
                    {
                        var result = await response.Content.ReadAsStringAsync();

                        string status = JObject.Parse(result)["Status"].ToString(); // parse the json to string format
                        token = JObject.Parse(result)["Token"].ToString();

                        if (status.Equals("Fail", StringComparison.InvariantCulture) || token == null)
                        {
                            content.Dispose();
                            client.Dispose();
                            return new Request(status, "Invalid credentials");
                        }
                        else
                        {
                            Application.Current.Properties["Token"] = token; // save the token locally
                            Application.Current.Properties["TokenExpiryUtc"] = JObject.Parse(result)["TokenExpiryUtc"].ToString(); // save the time
                            content.Dispose();
                            client.Dispose();
                            return new Request(status, token); // return a successful request with api token
                        }
                    }
                    else
                    {
                        content.Dispose();
                        client.Dispose();
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

        //A method to perform an API GET request
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

                client.Dispose();
                requestMessage.Dispose();

                return result;
            }
            client.Dispose();
            requestMessage.Dispose();

            return null;
        }

        //A Method to perform the API POST request for the restaurant.
        public static async Task<JObject> APICallPost(string url, string token, string datestart, string dateend, int partysize)
        {
            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            string information = @"{""DateFrom"" : """ + datestart + @""", ""DateTo"" : """ + dateend + @""", ""PartySize"" :" + partysize + @", ""ChannelCode"" : ""ONLINE""}";
            var content = new StringContent(information, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(url, content); // get response from the api
            JObject result;

            if (response.IsSuccessStatusCode) // if call to api is successful
            {
                var retstring = await response.Content.ReadAsStringAsync();

                result = JObject.Parse(retstring);
                content.Dispose();
                client.Dispose();

                return result;
            }

            content.Dispose();
            client.Dispose();
            return null;
        }
    }
}