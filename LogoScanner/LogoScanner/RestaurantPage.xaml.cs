using Logoscanner;
using LogoScanner.Helpers;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;
using System.Net;
using System.Reflection;

namespace LogoScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RestaurantPage : TabbedPage
    {
        public static ObservableCollection<Promotion> promotions = new ObservableCollection<Promotion>();
        public static List<Review> reviews = new List<Review>();
        public static ObservableCollection<AvailableTime> availableTimes = new ObservableCollection<AvailableTime>();

        private string micrositename;
        private string overallReviews;
        private string token;
        private JObject consumer;
        private JObject result;
        private int partysize = 3;

        public RestaurantPage(string micrositename)
        {
            //register syncfusion api
            var credentialsFile = "LogoScanner.credentials.json";
            JObject line;
            var assembly = Assembly.GetExecutingAssembly();

            using (Stream stream = assembly.GetManifestResourceStream(credentialsFile))
            using (StreamReader reader = new StreamReader(stream))
            {
                line = JObject.Parse(reader.ReadToEnd()); // opens credentials file, reads it and parse JSON
            }

            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(line["SyncfusionAPI"]["key"].ToString());

            InitializeComponent();
            this.micrositename = micrositename;

            // event called when the tab is changed by the user
            this.CurrentPageChanged += (object sender, EventArgs e) =>
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
                        Title = "Available Bookings";
                        break;

                    case 2:
                        MenuTab.IconImageSource = "MenuIconFilled.png";
                        NavigationPage.SetHasNavigationBar(this, true);
                        Title = "Menu";
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
            token = request.message;

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
                JArray hasSummary = await Requests.APICallGet("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/HasMicrositeSummary", request.message);
                JObject result = (JObject)hasSummary.First;
                if (result["Result"] != null)
                {
                    var datestart = DateTime.Now;
                    var datestartstr = datestart.ToString("yyyy-MM-ddTHH:mm:ss");

                    var dateend = DateTime.Now.AddDays(7.00);
                    var dateendstr = dateend.ToString("yyyy-MM-ddTHH:mm:ss");

                    GetRestaurantData("https://api.rdbranch.com/api/ConsumerApi/v1/MicrositeSummaryDetails?micrositeNames=" + this.micrositename + "&startDate=" + datestartstr + "&endDate=" + dateendstr + "&channelCodes=ONLINE&numberOfReviews=5", request.message);
                }
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK"); // Displays an error message to the user
            }
        }

        // populates the home tab
        private void PopulateHomeTab(JObject result)
        {
            Logo.Source = Utils.GetRestaurantField(result, "LogoUrl");
            Logo.WidthRequest = Application.Current.MainPage.Height / 8;
            Logo.HeightRequest = Application.Current.MainPage.Height / 8;

            NameLabel.Text = Utils.GetRestaurantField(result, "Name");
            CuisinesLabel.Text = Utils.GetRestaurantField(result, "CuisineTypes");

            int price = 0;
            if (result["PricePoint"].Type != JTokenType.Null) price = Int32.Parse(result["PricePoint"].ToString());
            PriceLabel.Text = Utils.GetRestaurantField(result, "PricePoint", "£", price);

            int stars = (int)Math.Round(Double.Parse(result["AverageReviewScore"].ToString()), 0, MidpointRounding.AwayFromZero);
            StarLabel.Text = Utils.GetRestaurantField(result, "AverageReviewScore", "★", stars);

            DescriptionLabel.Text = Utils.GetRestaurantField(consumer, "ShortDescription");

            var viewMoreTap = new TapGestureRecognizer();
            viewMoreTap.Tapped += async (s, e) =>
            {
                await Navigation.PushPopupAsync(new AboutPopup(Utils.GetRestaurantField(consumer, "Description")));
            };
            ViewMoreLabel.GestureRecognizers.Add(viewMoreTap);

            OpeningInformationLabel.Text = Utils.GetRestaurantField(consumer, "OpeningInformation").Replace("<br/>", Environment.NewLine);

            if (consumer["SocialNetworks"].Type == JTokenType.Null || string.IsNullOrEmpty(consumer["SocialNetworks"].ToString()))
            {
                SocialMediaLabel.IsVisible = true;
                SocialMediaLabel.Text = "No social media";
            }
            else if (consumer["SocialNetworks"] is JArray)
            {
                JToken[] arr = consumer["SocialNetworks"].ToArray();
                int column = 0;

                foreach (var a in arr)
                {
                    Button button = new Button
                    {
                        Text = a["Type"].ToString(),
                        Margin = new Thickness(15, 10, 0, 0),
                        BackgroundColor = Color.White,
                        TextColor = Color.FromHex("#11a0dc"),
                        FontSize = 12,
                        CornerRadius = 18,
                        BorderWidth = 2,
                        BorderColor = Color.FromHex("#11a0dc"),
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start
                    };
                    HomeGrid.Children.Add(button, column, 15);
                    column++;

                    button.Clicked += async (sender, args) => await Browser.OpenAsync(a["Url"].ToString(), BrowserLaunchMode.SystemPreferred);
                }
            }

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

        // populates the booking tab
        private async void PopulateBookingTab(JObject result)
        {
            string[] promotion_ids = Promotions.GetPromotionIDs(result);

            var dateStart = DateTime.Now;
            var dateStartStr = dateStart.ToString("yyyy-MM-ddTHH:mm:ss");

            var dateEnd = DateTime.Now.AddDays(7.00);
            var dateEndStr = dateEnd.ToString("yyyy-MM-ddTHH:mm:ss");

            string url = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/AvailabilityForDateRangeV2?";

            JObject r = await Requests.APICallPost(url, token, dateStartStr, dateEndStr, partysize);

            var capacity = 0;

            var checkAvail = r["AvailableDates"].ToString();

            if (r != null && checkAvail.Length > 2)
            {
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
                    promotions.Clear();
                    foreach (var pr in array_promotions)
                    {
                        var valid = pr["ValidityPeriods"].First;

                        promotions.Add(new Promotion
                        {
                            Name = pr["Name"].ToString(),
                            Description = pr["Description"].ToString(),
                            StartTime = valid["StartTime"].ToString(),
                            EndTime = valid["EndTime"].ToString(),
                            StartDate = Convert.ToDateTime(valid["StartDate"].ToString()).Date.ToString("dd/MM/yyyy"),
                            EndDate = Convert.ToDateTime(valid["EndDate"].ToString()).Date.ToString("dd/MM/yyyy"),
                        });
                    }
                }
            }
            else
            {
                AvailabilityView.IsVisible = false;
                NoAvailabilityLabel.IsVisible = true;
            }

            Promotions.GetAvailablePromotions(url, token, r, capacity);

            AvailabilityView.ItemsSource = availableTimes;
        }


        // populates the reviews tab
        private void PopulateReviewsTab(JObject result)
        {
            overallReviews = "Reviews (" + Utils.GetRestaurantField(result, "NumberOfReviews") + ")";
            reviews.Clear();

            foreach (JToken review in result["Reviews"].ToArray())
            {
                reviews.Add(new Review
                {
                    Name = review["ReviewedBy"].ToString(),
                    Content = review["Review"].ToString(),
                    Score = Utils.GetRestaurantField((JObject)review, "AverageScore", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    ReviewDate = review["ReviewDateTime"].ToString(),
                    VisitDate = review["VisitDateTime"].ToString(),
                    LikelyToRecommend = Utils.GetRestaurantField((JObject)review, "Answer1", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    FoodAndDrink = Utils.GetRestaurantField((JObject)review, "Answer2", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    Service = Utils.GetRestaurantField((JObject)review, "Answer3", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    Atmosphere = Utils.GetRestaurantField((JObject)review, "Answer4", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    Value = Utils.GetRestaurantField((JObject)review, "Answer5", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                    ScoreNumber = review["AverageScore"].ToString()
                });
            }
            ReviewsView.ItemsSource = reviews;
        }

        // populates the app with all data
        private async void GetRestaurantData(string url, string token)
        {
            JArray r = await Requests.APICallGet(url, token);
            result = (JObject)r.First;

            // gets restaurant json object in the consumer api
            var consumerUrl = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename;
            JArray restaurant = await Requests.APICallGet(consumerUrl, token);
            consumer = (JObject)restaurant.First;

            PopulateHomeTab(result);
            PopulateBookingTab(result);
            SetMenu(consumer);
            PopulateReviewsTab(result);

            Indicator1.IsVisible = false;
            Indicator2.IsVisible = false;
            Indicator3.IsVisible = false;
            Indicator4.IsVisible = false;

            Indicator1.IsRunning = false;
            Indicator2.IsRunning = false;
            Indicator3.IsRunning = false;
            Indicator4.IsRunning = false;

            Frame1.IsVisible = false;
            Frame2.IsVisible = false;
            Frame3.IsVisible = false;
            Frame4.IsVisible = false;
        }

        //method do download pdf from url
        public Stream DownloadPdfStream(string URL)
        {
            var uri = new System.Uri(URL);
            var client = new WebClient();

            //Returns the PDF document stream from the given URL
            return client.OpenRead(uri);
        }

        //method to get menu for restaurant
        private void SetMenu(JObject json)
        {
            if (json["Menus"].Type == JTokenType.Null || string.IsNullOrEmpty(json["Menus"].ToString()) || !json["Menus"].Any())
            {
                MenuLabel.Text = "No Menus Currently Available.";
            }
            else
            {
                var pdfUrl = json["Menus"][0]["StorageUrl"].ToString();
                //Provide the PDF document URL in the below overload.
                Stream documenStream = DownloadPdfStream(pdfUrl);
                //Loads the PDF document as Stream to PDF viewer control
                pdfViewerControl.LoadDocument(documenStream);
            }
        }

        // event triggered when the floating action button is clicked
        private void FloatingButton_Clicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new MainPage());
        }

        // event triggered when the phone button is clicked
        private void PhoneButton_Clicked(object sender, EventArgs e)
        {
            PhoneDialer.Open(Utils.GetRestaurantField(consumer, "ReservationPhoneNumber"));
        }

        // event triggered when the email button is clicked
        private async void EmailButton_Clicked(object sender, EventArgs e)
        {
            var message = new EmailMessage
            {
                Subject = "",
                Body = "",
                To = new List<string> { Utils.GetRestaurantField(consumer, "EmailAddress") },
            };
            await Email.ComposeAsync(message);
        }

        // event triggered when the website button is clicked
        private async void WebsiteButton_Clicked(object sender, EventArgs e)
        {
            await Browser.OpenAsync(Utils.GetRestaurantField(consumer, "Website"), BrowserLaunchMode.SystemPreferred);
        }

        // event triggered when a review is tapped
        private async void ReviewsView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            var review = e.Item as Review;
            await Navigation.PushPopupAsync(new ReviewsPopup(review));
        }

        public void BookTimeSlot(Object Sender, EventArgs e)
        {
            Button b = (Button)Sender;
            string dateTime = b.BindingContext as string;
            Booking.Makebooking(micrositename, dateTime.Split(',')[0], dateTime.Split(',')[1], partysize);
        }

        private void OnSliderValueChanged(object sender, ValueChangedEventArgs args)
        {
            int value = (int)args.NewValue;
            sliderValueLabel.Text = "Party Size of " + value.ToString();
        }

        private void ChangePartySize(object sender, EventArgs e)
        {
            partysize = (int)partySizeSlider.Value;
            sliderValueLabel.Text = "Party Size of " + partysize.ToString();

            promotions.Clear();
            availableTimes.Clear();
            PopulateBookingTab(result);
        }
    }
}
 