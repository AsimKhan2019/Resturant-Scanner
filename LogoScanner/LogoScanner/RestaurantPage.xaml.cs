using LogoScanner.Helpers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace LogoScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestaurantPage : TabbedPage
    {
        public static ObservableCollection<Promotion> promotions = new ObservableCollection<Promotion>();
        public static ObservableCollection<Review> reviews = new ObservableCollection<Review>();
        public static ObservableCollection<AvailableTime> availableTimes = new ObservableCollection<AvailableTime>();

        private string micrositename;
        private string overallReviews;
        private JObject menu;

        public RestaurantPage(string micrositename)
        {
            InitializeComponent();
            this.micrositename = micrositename;

            // event called when the tab is changed by the user
            this.CurrentPageChanged += async (object sender, EventArgs e) =>
            {
                var tab = this.Children.IndexOf(this.CurrentPage);

                HomeTab.IconImageSource = "HomeIcon.png";
                MenuTab.IconImageSource = "MenuIcon.png";
                ReviewsTab.IconImageSource = "ReviewIcon.png";
                BookingTab.IconImageSource = "BookingIcon.png";

                switch (tab)
                {
                    case 0:
                        HomeTab.IconImageSource = "HomeIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, false);
                        break;

                    case 1:
                        BookingTab.IconImageSource = "BookingIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, true);
                        Title = "Book";
                        break;

                    case 2:
                        MenuTab.IconImageSource = "MenuIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, true);
                        Title = "Menu";
                        setMenu(menu);
                        break;

                    case 3:
                        ReviewsTab.IconImageSource = "ReviewIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, true);
                        Title = overallReviews;
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
                try
                {
                    JArray hasSummary = await Requests.APICallGet("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/HasMicrositeSummary", request.message);
                    JObject result = (JObject)hasSummary.First;
                    if (result["Result"] != null)
                    {
                        GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/MicrositeSummaryDetails?micrositeNames=" + this.micrositename + "&startDate=2019-11-19T10:53:39&endDate=2019-11-18T10:53:39&channelCodes=ONLINE&numberOfReviews=5", request.message);
                    }
                }
                catch (NullReferenceException e)
                {
                    await DisplayAlert("Error", "Provider" + this.micrositename + " was not found.", "OK"); // Displays an error message to the user
                }
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK"); // Displays an error message to the user
            }
        }

        private async void GetRestaurantData(string url, string token)
        {
            JArray r = await Requests.APICallGet(url, token);
            JObject result = (JObject)r.First;

            //Parse the API Call and split the JSon object into the various variables.
            Logo.Source = Utils.GetRestaurantField(result, "LogoUrl");
            NameLabel.Text = Utils.GetRestaurantField(result, "Name");
            CuisinesLabel.Text = Utils.GetRestaurantField(result, "CuisineTypes");

            // gets restaurant json object and sets the menu
            var menuUrl = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename;
            JArray restaurant = await Requests.APICallGet(menuUrl, token);
            menu = (JObject)restaurant.First;

            int price = 0;
            if (result["PricePoint"].Type != JTokenType.Null) price = Int32.Parse(result["PricePoint"].ToString());
            PriceLabel.Text = Utils.GetRestaurantField(result, "PricePoint", "£", price);

            int stars = (int)Math.Round(Double.Parse(result["AverageReviewScore"].ToString()), 0, MidpointRounding.AwayFromZero);
            StarLabel.Text = Utils.GetRestaurantField(result, "AverageReviewScore", "★", stars);

            string[] promotion_ids = Promotions.GetPromotionIDs(result);

            if (promotion_ids.Length > 0)
            {
                string promotions_url = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/Promotion?";
                StringBuilder builder = new StringBuilder();

                builder.Append(promotions_url);
                foreach (string id in promotion_ids)
                {
                    builder.Append("&promotionIds=" + id);
                }

                JArray array_promotions = await Requests.APICallGet(builder.ToString(), token);

                foreach (var pr in array_promotions)
                {
                    var valid = pr["ValidityPeriods"].First;

                    promotions.Add(new Promotion
                    {
                        Name = pr["Name"].ToString(),
                        Description = pr["Description"].ToString(),
                        StartTime = valid["StartTime"].ToString(),
                        EndTime = valid["EndTime"].ToString(),
                        StartDate = valid["StartDate"].ToString().Substring(0, 10),
                        EndDate = valid["EndDate"].ToString().Substring(0, 10),
                    });
                }
            }
            else
            {
                promotions.Add(new Promotion { Name = "No Promotions Currently Available!" });
            }

            // reviews section
            reviews.Clear();
            foreach (JToken review in result["Reviews"].ToArray())
            {
                reviews.Add(new Review
                {
                    Name = review["ReviewedBy"].ToString(),
                    Content = review["Review"].ToString(),
                    Score = Utils.GetRestaurantField((JObject)review, "AverageScore", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    ReviewDate = review["ReviewDateTime"].ToString(),
                    VisitDate = review["VisitDateTime"].ToString()
                });
            }
            ReviewsView.ItemsSource = reviews;

            overallReviews = Utils.GetRestaurantField(result, "AverageReviewScore") + "★  |  " + Utils.GetRestaurantField(result, "NumberOfReviews") + " reviews";
            Promotions.GetAvailablePromotions("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/AvailabilityForDateRangeV2?", token);

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

            availableTimes.Clear();
            AvailabilityView.ItemsSource = availableTimes;
        }

        //method to get menu for restaurant
        private void setMenu(JObject json)
        {
            if (json["Menus"].Type == JTokenType.Null || string.IsNullOrEmpty(json["Menus"].ToString()) || !json["Menus"].Any())
            {
                Menu.IsVisible = false;
                MenuLabel.Text = "No Menus Currently Available.";
            }
            else
            {
                var pdfUrl = json["Menus"][0]["StorageUrl"].ToString();
                var googleUrl = "http://drive.google.com/viewerng/viewer?embedded=true&url=";
                if (Device.RuntimePlatform == Device.iOS)
                {
                    Menu.Source = pdfUrl;
                }
                else if (Device.RuntimePlatform == Device.Android)
                {
                    Menu.Source = new UrlWebViewSource() { Url = googleUrl + pdfUrl };
                }
            }
        }

        // event triggered when the floating action button is clicked
        void FloatingButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MainPage());
        }
    }
}