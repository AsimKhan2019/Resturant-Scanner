using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using LogoScanner.Models;

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
            string credentials = @"{""Username"" : ""2310049g@student.gla.ac.uk"", ""Password"" : ""}txm/ln8)Vha@Vc7x)-Lx{t+W#>nd1""}";

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
                //Get the Results from the API Call
                var contents = await response.Content.ReadAsStringAsync();
                JObject result = JObject.Parse(contents);

                //Parse the API Call and split the JSon object into the various variables.
                string Name = (result["Name"] == null || string.IsNullOrEmpty(result["Name"].ToString()))
                                ? "Restaurant Name" : result["Name"].ToString();
                NameLabel.Text = Name;

                string Address = (result["FullAddress"] == null || string.IsNullOrEmpty(result["FullAddress"].ToString()))
                                ? "Address" : result["FullAddress"].ToString();
                AddressLabel.Text = Address;

                string LogoUrl = (result["LogoUrl"] == null || string.IsNullOrEmpty(result["LogoUrl"].ToString()))
                                ? "Logo" : result["LogoUrl"].ToString();
                Logo.Source = LogoUrl;

                string NumberOfReviews = (result["NumberOfReviews"] == null || string.IsNullOrEmpty(result["NumberOfReviews"].ToString()))
                                ? "Number of Reviews" : result["NumberOfReviews"].ToString();
                NoOfReviewsLabel.Text = NumberOfReviews;

                string AvgReview = (result["AverageReviewScore"] == null || string.IsNullOrEmpty(result["AverageReviewScore"].ToString()))
                                ? "No Average Review Score" : result["AverageReviewScore"].ToString();
                
                AvgRevLabel.Text = AvgReview;


                string TimeSlots = (result["AvailableTimeSlots"] == null || string.IsNullOrEmpty(result["AvailableTimeSlots"].ToString()))
                                ? "No Available TimeSlots" : result["AvailableTimeSlots"].ToString();
                TimeSlotsLabel.Text = TimeSlots;


                string Cusine = (result["CusineTypes"] == null || string.IsNullOrEmpty(result["CusineTypes"].ToString()))
                                ? "No Set Cusine Types" : result["CusineTypes"].ToString();
                CusineLabel.Text = Cusine;

                string PricePoint = (result["PricePoint"] == null || string.IsNullOrEmpty(result["PricePoint"].ToString()))
                                ? "No Price Point" : result["PricePoint"].ToString();
                PriceLabel.Text = PricePoint;
    }
        }
    }
}
