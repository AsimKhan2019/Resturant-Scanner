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
                String micrositename = "muranostreetsocial"; //Get Microsite Name from Image Recognition return value
                GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/MicrositeSummaryDetails?micrositeNames=" + micrositename + "&startDate=2019-11-19T10:53:39&endDate=2019-11-18T10:53:39&channelCodes=ONLINE&numberOfReviews=5", request.message);
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK"); // Displays an error message to the user
            }
        }

        private async void GetRestaurantData(string url, string token)
        {
            JObject result = await Requests.APICallGet(url, token);

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

            string Cuisine = (result["CuisineTypes"] == null || string.IsNullOrEmpty(result["CuisineTypes"].ToString()))
                            ? "No Set Cusine Types" : result["CuisineTypes"].ToString();

            PriceLabel.Text += (result["PricePoint"] == null || string.IsNullOrEmpty(result["PricePoint"].ToString()))
                            ? "No Price Point" : result["PricePoint"].ToString();
            }
        }
    }
