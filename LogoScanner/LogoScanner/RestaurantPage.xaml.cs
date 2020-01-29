using Logoscanner;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace LogoScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestaurantPage : TabbedPage
    {
        private ObservableCollection<Promotions> promotions = new ObservableCollection<Promotions>();
        private ObservableCollection<Reviews> reviews = new ObservableCollection<Reviews>();
        private ObservableCollection<AvailableTimes> availabletimes = new ObservableCollection<AvailableTimes>();

        private readonly string micrositename;

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
                        NavigationPage.SetHasNavigationBar(this, false);
                        break;

                    case 1:
                        MenuTab.IconImageSource = "MenuIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, true);
                        break;

                    case 2:
                        ReviewsTab.IconImageSource = "ReviewIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, true);
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

        private async void GetAvailProm(string url, string token)
        {
            var datestart = DateTime.Now;
            var datestartstr = datestart.ToString("yyyy-MM-ddTHH:mm:ss");

            var dateend = DateTime.Now.AddDays(7.00);
            var dateendstr = dateend.ToString("yyyy-MM-ddTHH:mm:ss");

            JObject r = await Requests.APICallPost(url, token, datestartstr, dateendstr, 3);
            availabletimes.Clear();
            var capacity = 0;
            AvailabilityView.ItemsSource = availabletimes;

            if (r != null)
            {
                foreach (var day in r["AvailableDates"])
                {
                    //After client meeting if we decide to show only timeslots that day then uncomment line below
                    if (capacity <= 3)
                    {
                        Dictionary<string, string> areas = new Dictionary<string, string>();

                        foreach (var area in day["Areas"])
                        {
                            areas.Add(area["Id"].ToString(), area["Name"].ToString());
                        }
                        foreach (var timeslot in day["AvailableTimes"])
                        {
                            if (capacity == 3)
                            {
                                break;
                            }
                            else
                            {
                                StringBuilder AvailableAreas = new StringBuilder();
                                StringBuilder TimeSlot = new StringBuilder();

                                TimeSlot.Append(timeslot["TimeSlot"].ToString());

                                foreach (var availarea in timeslot["AvailableAreaIds"])
                                {
                                    AvailableAreas.Append(areas[availarea.ToString()]);
                                    AvailableAreas.Append(", ");
                                }
                                AvailableAreas.Remove(AvailableAreas.Length - 2, 2);

                                AvailableTimes at = new AvailableTimes
                                {
                                    Name = day["Date"].ToString().Substring(0, 10),
                                    Description = TimeSlot.ToString(),
                                    RestaurantAreas = AvailableAreas.ToString()
                                };

                                getValidPromotions(at);
                                availabletimes.Add(at);

                                capacity += 1;
                            }
                        }
                    }
                }
            }
            else
            {
                availabletimes.Add(new AvailableTimes { Name = "No Timeslots Currently Available!" });
            }
        }

        private async void GetRestaurantData(string url, string token)
        {
            JArray r = await Requests.APICallGet(url, token);
            JObject result = (JObject)r.First;

            //Parse the API Call and split the JSon object into the various variables.
            Logo.Source = GetRestaurantField(result, "LogoUrl");
            NameLabel.Text = GetRestaurantField(result, "Name");
            CuisinesLabel.Text = GetRestaurantField(result, "CuisineTypes");

            // gets restaurant json object and sets the menu
            var menuUrl = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename;
            JArray restaurant = await Requests.APICallGet(menuUrl, token);
            JObject menu = (JObject)restaurant.First;
            setMenu(menu);

            int price = 0;
            if (result["PricePoint"].Type != JTokenType.Null) price = Int32.Parse(result["PricePoint"].ToString());
            PriceLabel.Text = GetRestaurantField(result, "PricePoint", "£", price);

            int stars = (int)Math.Round(Double.Parse(result["AverageReviewScore"].ToString()), 0, MidpointRounding.AwayFromZero);
            StarLabel.Text = GetRestaurantField(result, "AverageReviewScore", "★", stars);

            string[] promotion_ids = GetPromotionIDs(result);

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

                    promotions.Add(new Promotions
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
                promotions.Add(new Promotions { Name = "No Promotions Currently Available!" });
            }

            // reviews section
            reviews.Clear();
            foreach (JToken review in result["Reviews"].ToArray())
            {
                reviews.Add(new Reviews
                {
                    Name = review["ReviewedBy"].ToString(),
                    Review = review["Review"].ToString(),
                    Score = GetRestaurantField((JObject)review, "AverageScore", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    ReviewDate = review["ReviewDateTime"].ToString(),
                    VisitDate = review["VisitDateTime"].ToString()
                });
            }
            ReviewsView.ItemsSource = reviews;

            OverallReviewsLabel.Text = GetRestaurantField(result, "AverageReviewScore") + "★  |  " + GetRestaurantField(result, "NumberOfReviews") + " reviews";
            GetAvailProm("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/AvailabilityForDateRangeV2?", token);

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

        // method to get field from json object
        private string GetRestaurantField(JObject json, string field)
        {
            if (json[field].Type == JTokenType.Null || string.IsNullOrEmpty(json[field].ToString()))
                return "No Set " + field;
            else if (json[field] is JArray)
            {
                StringBuilder builder = new StringBuilder();
                JToken[] cuisines = json[field].ToArray();

                foreach (string cuisine in cuisines)
                {
                    builder.Append(cuisine);
                    if (cuisines.IndexOf(cuisine) != cuisines.Count() - 1) builder.Append(", ");
                }

                return builder.ToString();
            }
            else
            {
                return json[field].ToString();
            }
        }

        // method to get field from json object and produce a string with symbols which are repeated i number of times
        private string GetRestaurantField(JObject json, string field, string symbol, int i)
        {
            if (json[field].Type == JTokenType.Null || string.IsNullOrEmpty(json[field].ToString()))
                return "No Set " + field;
            else
                return String.Concat(Enumerable.Repeat(symbol, i));
        }

        private string[] GetPromotionIDs(JObject json)
        {
            string[] returnvalue;
            promotions.Clear();

            if (json["AvailablePromotions"].Type == JTokenType.Null || string.IsNullOrEmpty(json["AvailablePromotions"].ToString()))
            {
                returnvalue = new string[0];
                returnvalue[0] = "No Promotions Currently Available";
                return returnvalue;
            }
            else
            {
                JToken[] promotion_ids = json["AvailablePromotions"].ToArray();

                int i = 0;
                returnvalue = new string[promotion_ids.Length];

                foreach (string id in promotion_ids)
                {
                    returnvalue[i] = id;
                    i++;
                }

                return returnvalue;
            }
        }

        private AvailableTimes getValidPromotions(AvailableTimes current)
        {
            StringBuilder currenttime = new StringBuilder();
            StringBuilder allpromotions = new StringBuilder();

            currenttime.Append(current.Name);
            currenttime.Append(" ");
            currenttime.Append(current.Description);

            DateTime DateofBooking = DateTime.ParseExact(currenttime.ToString(), "dd/MM/yyyy HH:mm:ss", null);
            foreach (Promotions p in promotions)
            {
                StringBuilder start = new StringBuilder();
                StringBuilder end = new StringBuilder();

                start.Append(p.StartDate);
                start.Append(" ");
                start.Append(p.StartTime);

                end.Append(p.EndDate);
                end.Append(" ");
                end.Append(p.EndTime);

                DateTime startPromo = DateTime.ParseExact(start.ToString(), "dd/MM/yyyy HH:mm:ss", null);
                DateTime endPromo = DateTime.ParseExact(end.ToString(), "dd/MM/yyyy HH:mm:ss", null);

                int res1 = DateTime.Compare(DateofBooking, startPromo);  //Should return 1 or 0 - as DateofBooking should be >= startPromo
                int res2 = DateTime.Compare(DateofBooking, endPromo);    //Should return -1 or 0 - as DateofBooking should be =< end Promo

                if (res1 >= 0 && res2 <= 0)
                {
                    allpromotions.Append("\nName: ");
                    allpromotions.Append(p.Name);
                    allpromotions.Append("\nDescription: ");
                    allpromotions.Append(p.Description);
                    allpromotions.Append("\n\n");
                }
            }

            if (allpromotions.Length > 0)
            {
                current.Promotions = allpromotions.ToString();
            }
            else
            {
                current.Promotions = "No Promotions Available";
            }

            return current;
        }

        public void booktimeslot(Object Sender, EventArgs e)
        {
            Button b = (Button)Sender;
            StackLayout selected_timeslot = (StackLayout)b.Parent;
            var date_label = (Label)selected_timeslot.Children[0];
            var date = date_label.Text;

            var time_label = (Label)selected_timeslot.Children[1];
            var time = time_label.Text;

            Booking.Makebooking(micrositename, date, time);
        }
    }
}