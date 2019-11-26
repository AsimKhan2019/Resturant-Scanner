using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;


//Additional API Calls Required
//Promotions - GetPromotions API Call
//Get Availability for Date Range

namespace LogoScanner
{
    public partial class RestaurantPage : ContentPage
    {

        private string reviewNo;
        private string avgReview;

        public RestaurantPage()
        {
            InitializeComponent();
        }

        public class RestService
        {
            readonly HttpClient _client;

            public RestService()
            {
                _client = new HttpClient();
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var request = await Requests.ConnectToResDiary();

            while (request.message.Equals("Unable to Connect to Internet"))
            {
                await DisplayAlert("Error", request.message, "OK"); // displays an error message to the user

                if (request.message == "Unable to Connect to Internet")
                {
                    request = await Requests.ConnectToResDiary();
                }
            }

            if (request.status.Equals("Success"))
            {
                String micrositename = "CairncrossCafe";
                String startDate = "2019-11-19T10:53:39";
                String endDate = "2019-11-18T10:53:39";
                String channelCodes = "ONLINE";
                String noofrev = "5";
                GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/MicrositeSummaryDetails?micrositeNames="+ micrositename +
                                    "&startDate="+ startDate +
                                    "&endDate=" + endDate + 
                                    "&channelCodes=" + channelCodes + 
                                    "&numberOfReviews=" + noofrev, request.message);
               
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK"); // displays an error message to the user
            }
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


                contents = contents.TrimStart('[');
                contents = contents.TrimEnd(']');



                JObject result = JObject.Parse(contents);

                //Parse the API Call and split the JSon object into the various variables.
                NameLabel.Text = (result["Name"] == null || string.IsNullOrEmpty(result["Name"].ToString()))
                                ? "Restaurant Name" : result["Name"].ToString();

                AddressLabel.Text = (result["FullAddress"] == null || string.IsNullOrEmpty(result["FullAddress"].ToString()))
                                ? "Address" : result["FullAddress"].ToString();

                Logo.Source = (result["LogoUrl"] == null || string.IsNullOrEmpty(result["LogoUrl"].ToString()))
                                ? "Logo" : result["LogoUrl"].ToString();

                reviewNo = (result["NumberOfReviews"] == null || string.IsNullOrEmpty(result["NumberOfReviews"].ToString()))
                                ? "Number of Reviews" : result["NumberOfReviews"].ToString();

                avgReview = (result["AverageReviewScore"] == null || string.IsNullOrEmpty(result["AverageReviewScore"].ToString()))
                                ? "No Average Review Score" : result["AverageReviewScore"].ToString();

                ReviewsLabel.Text += (avgReview + " out of " + reviewNo + " reviews");
                string Times = (result["AvailableTimeSlots"] == null || string.IsNullOrEmpty(result["AvailableTimeSlots"].ToString()))
                                ? "No Available TimeSlots" : result["AvailableTimeSlots"].ToString();

                string Cuisine = (result["CusineTypes"] == null || string.IsNullOrEmpty(result["CusineTypes"].ToString()))
                                ? "No Set Cusine Types" : result["CusineTypes"].ToString();

                Cusines.ItemsSource += Cuisine;

                PriceLabel.Text += (result["PricePoint"] == null || string.IsNullOrEmpty(result["PricePoint"].ToString()))
                                ? "No Price Point" : result["PricePoint"].ToString();
            }
        }
    }
}
