using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace LogoScanner
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public class RestService
        {
            HttpClient _client;

            public RestService()
            {
                _client = new HttpClient();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string result;
            string credentials = @"{""Username"" : ""USERNAME"", ""Password"" : ""PASSWORD""}";

            try
            {
                var content = new StringContent(credentials, Encoding.UTF8, "application/json");
                var client = new HttpClient();
                var response = await client.PostAsync("https://api.rdbranch.com/api/Jwt/v2/Authenticate", content);

                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsStringAsync();

                    string status = JObject.Parse(result)["Status"].ToString();
                    string token = JObject.Parse(result)["Token"].ToString();

                    if (status.Equals("Fail") || token == null)
                    {
                        result = "Invalid credentials.";
                    }
                    else
                    {
                        result = status;
                        GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/CairncrossCafe/Summary?numberOfReviews=5", token);


                    }
                }
                else
                {
                    result = "Unable to connect to RESDiary API.";
                }
            }
            catch (HttpRequestException ex)
            {
                result = ex.Message;
            }

            TestLabel.Text = result;

        }

        private async void GetRestaurantData(string url, string token)
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = await client.SendAsync(requestMessage);

            NameLabel.Text = requestMessage.ToString();

            if (response.IsSuccessStatusCode)
            {
                var contents = await response.Content.ReadAsStringAsync();
                RestaurantDataView.ItemsSource = contents;
                NameLabel.Text = contents;
            }
        }
    }
}
