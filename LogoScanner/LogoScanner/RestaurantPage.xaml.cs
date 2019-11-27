using System;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;


//Additional API Calls Required
//Promotions - GetPromotions API Call
//Get Availability for Date Range

namespace LogoScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestaurantPage : TabbedPage
    {
        private string micrositename;

        public RestaurantPage(string micrositename)
        {
            InitializeComponent();
            this.micrositename = micrositename;
            this.CurrentPageChanged += (object sender, EventArgs e) =>
            {
                var tab = this.Children.IndexOf(this.CurrentPage);

                HomeTab.IconImageSource = "HomeIcon.png";
                MenuTab.IconImageSource = "MenuIcon.png";
                ReviewsTab.IconImageSource = "ReviewIcon.png";
                ScanTab.IconImageSource = "ScanIcon.png";

                switch (tab)
                {
                    case 0:
                        HomeTab.IconImageSource = "HomeIconFilled.png";
                        break;
                    case 1:
                        MenuTab.IconImageSource = "MenuIconFilled.png";
                        break;
                    case 2:
                        ReviewsTab.IconImageSource = "ReviewIconFilled.png";
                        break;
                    case 3:
                        ScanTab.IconImageSource = "ScanIconFilled.png";
                        Navigation.PushModalAsync(new MainPage());
                        break;
                }
            };
        }

        protected override async void OnAppearing() // when page loads
        {
            base.OnAppearing();

            var request = await Requests.ConnectToResDiary(); // connect to resdiary api

            while (request.message.Equals("Unable to Connect to Internet"))
            {
                await DisplayAlert("Error", request.message, "OK"); // displays an error message to the user

                if (request.message == "Unable to Connect to Internet")
                {
                    request = await Requests.ConnectToResDiary();
                }
            }

            if (request.status.Equals("Success")) // if connection to api is successful
            {
                GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/MicrositeSummaryDetails?micrositeNames=" + this.micrositename + "&startDate=2019-11-19T10:53:39&endDate=2019-11-18T10:53:39&channelCodes=ONLINE&numberOfReviews=5", request.message);
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK"); // Displays an error message to the user
            }
        }

        private async void GetRestaurantData(string url, string token)
        {
            JObject result = await Requests.APICallGet(url, token);

            int stars = (int)Math.Round(Double.Parse(result["AverageReviewScore"].ToString()), 0, MidpointRounding.AwayFromZero);

            //Parse the API Call and split the JSon object into the various variables.
            NameLabel.Text = GetRestaurantField(result, "Name");
            Logo.Source = GetRestaurantField(result, "LogoUrl");
            StarLabel.Text = GetRestaurantField(result, "AverageReviewScore", "★", stars);
            CuisinesLabel.Text = GetRestaurantField(result, "CuisineTypes");
            PriceLabel.Text = GetRestaurantField(result, "PricePoint", "£", Int32.Parse(result["PricePoint"].ToString()));

            double latitude = Convert.ToDouble(result["Latitude"].ToString());
            double longitude = Convert.ToDouble(result["Longitude"].ToString());
            string name = result["Name"].ToString();

            var pin = new Pin()
            {
                Position = new Position(latitude, longitude),
                Label = name,
            };

            MapArea.Pins.Add(pin);
            MapArea.MoveToRegion(new MapSpan(new Position(latitude, longitude), 0.01, 0.01));

            // open up directions to restaurant in map app when map area is clicked
            MapArea.MapClicked += async (object sender, MapClickedEventArgs e) =>
            {
                await Xamarin.Essentials.Map.OpenAsync(
                    new Location(latitude, longitude),
                    new MapLaunchOptions { Name = name, NavigationMode = NavigationMode.Default }
                );
            };
        }

        // method to get field from json object
        private string GetRestaurantField(JObject json, string field)
        {
            if (json[field] == null || string.IsNullOrEmpty(json[field].ToString()))
                return "No Set " + field;
            else
                return json[field].ToString();
        }

        // method to get field from json object and produce a string with symbols which are repeated i number of times
        private string GetRestaurantField(JObject json, string field, string symbol, int i)
        {
            if (json[field] == null || string.IsNullOrEmpty(json[field].ToString()))
                return "No Set " + field;
            else
                return String.Concat(Enumerable.Repeat(symbol, i));
        }
    }
}