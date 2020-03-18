using LogoScanner;
using LogoScanner.Helpers;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Extensions;
using Syncfusion.Pdf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
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
        public static List<Review> reviews = new List<Review>();
        public static ObservableCollection<AvailableTime> availableTimes = new ObservableCollection<AvailableTime>();

        private string micrositename;
        private string overallReviews;
        private string token;

        private JObject consumer;
        private JObject result;

        private static int partySize = 3;
        private static int slotNumber = 3;

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
                BookingTab.IconImageSource = "BookingIcon.png";
                MenuTab.IconImageSource = "MenuIcon.png";
                ReviewsTab.IconImageSource = "ReviewIcon.png";

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

            while (request.message.Equals("Unable to Connect to Internet", StringComparison.InvariantCulture))
            {
                await DisplayAlert("Error", request.message, "OK"); // displays an error message to the user

                if (request.message == "Unable to Connect to Internet")
                {
                    request = await Requests.ConnectToResDiary();
                }
            }

            if (request.status.Equals("Success", StringComparison.InvariantCulture)) // if connection to api is successful
            {
                JArray hasSummary = await Requests.APICallGet("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/HasMicrositeSummary", request.message);
                JObject result = (JObject)hasSummary.First;

                if (result["Result"] != null)
                {
                    var datestart = DateTime.Now;
                    var datestartstr = datestart.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture);

                    var dateend = DateTime.Now.AddDays(7.00);
                    var dateendstr = dateend.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.CurrentCulture);

                    JArray setupData = await Requests.APICallGet("https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/Setup?date=" + datestartstr + "&channelCode=ONLINE", token);
                    var data = (JObject)setupData.First;

                    if (data["OnlinePartySizeDefault"] != null)
                        partySize = (int)data["OnlinePartySizeDefault"];

                    PartyButton.Text = partySize + " PERSONS";
                    SlotButton.Text = slotNumber + " SLOTS";

                    SetUpPartyPicker(data);
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
            if (result["PricePoint"].Type != JTokenType.Null) price = Int32.Parse(result["PricePoint"].ToString(), CultureInfo.CurrentCulture);
            PriceLabel.Text = Utils.GetRestaurantField(result, "PricePoint", "£", price);

            HomeStars.Value = Convert.ToDouble(Utils.GetRestaurantField(result, "AverageReviewScore"));
            DescriptionLabel.Text = Utils.GetRestaurantField(consumer, "ShortDescription");

            var viewMoreTap = new TapGestureRecognizer();
            viewMoreTap.Tapped += async (s, e) =>
            {
                await Navigation.PushPopupAsync(new AboutPopup(Utils.GetRestaurantField(consumer, "Description")));
            };
            ViewMoreLabel.GestureRecognizers.Clear();
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
                        TextColor = Color.FromHex("#11a0dc"),
                        FontSize = 12,
                        CornerRadius = 18,
                        BorderWidth = 2,
                        BorderColor = Color.FromHex("#11a0dc"),
                        VerticalOptions = LayoutOptions.Start,
                        HorizontalOptions = LayoutOptions.Start
                    };
                    button.SetDynamicResource(Button.BackgroundColorProperty, "BarBackgroundColor");
                    HomeGrid.Children.Add(button, column, 15);
                    column++;

                    button.Clicked += async (sender, args) => await Browser.OpenAsync(a["Url"].ToString(), BrowserLaunchMode.SystemPreferred);
                }
            }

            double latitude = Convert.ToDouble(result["Latitude"].ToString(), CultureInfo.CurrentCulture);
            double longitude = Convert.ToDouble(result["Longitude"].ToString(), CultureInfo.CurrentCulture);
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
            string[] promotionIds = Promotions.GetPromotionIDs(result);

            var dateStart = DateTime.Now;
            var dateStartStr = dateStart.ToString("yyyy-MM-ddTHH:mm:ss");

            var dateEnd = DateTime.Now.AddDays(7.00);
            var dateEndStr = dateEnd.ToString("yyyy-MM-ddTHH:mm:ss");

            string url = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/AvailabilityForDateRangeV2?";

            JObject r = await Requests.APICallPost(url, token, dateStartStr, dateEndStr, partySize);

            var checkAvail = r["AvailableDates"].ToString();

            if (r != null && checkAvail.Length > 2)
            {
                AvailabilityView.IsVisible = true;
                NoAvailabilityLabel.IsVisible = false;

                if (promotionIds.Length > 0)
                {
                    string promotions_url = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/Promotion?";
                    StringBuilder builder = new StringBuilder();

                    builder.Append(promotions_url);
                    foreach (string id in promotionIds)
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

            Promotions.GetAvailablePromotions(r, slotNumber);

            AvailabilityView.ItemsSource = availableTimes;
        }

        // populates the reviews tab
        private async void PopulateReviewsTab()
        {
            var reviewsUrl = "https://api.rdbranch.com/api/ConsumerApi/v1/Restaurant/" + this.micrositename + "/Reviews?sortBy=Newest&page=1&pageSize=100";
            JArray reviewsCall = await Requests.APICallGet(reviewsUrl, token);
            JObject reviewsResponse = (JObject)reviewsCall.First;

            overallReviews = "Reviews (" + Utils.GetRestaurantField(reviewsResponse, "TotalRows") + ")";
            reviews.Clear();

            if (int.Parse(Utils.GetRestaurantField(reviewsResponse, "TotalRows")) == 0)
            {
                ReviewsView.IsVisible = false;
                ReviewsLabel.Text = "No Reviews Currently Available.";
            }
            else
            {
                foreach (JToken review in reviewsResponse["Data"].ToArray())
                {
                    reviews.Add(new Review
                    {
                        Name = review["ReviewedBy"].ToString(),
                        Content = review["Review"].ToString(),
                        Score = Convert.ToDouble(Utils.GetRestaurantField((JObject)review, "AverageScore")),
                        ReviewDate = review["ReviewDateTime"].ToString(),
                        VisitDate = review["VisitDateTime"].ToString(),
                        LikelyToRecommend = Utils.GetRestaurantField((JObject)review, "Answer1", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                        FoodAndDrink = Utils.GetRestaurantField((JObject)review, "Answer2", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                        Service = Utils.GetRestaurantField((JObject)review, "Answer3", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                        Atmosphere = Utils.GetRestaurantField((JObject)review, "Answer4", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                        Value = Utils.GetRestaurantField((JObject)review, "Answer5", "★", (int)Math.Round(Double.Parse(review["AverageScore"].ToString()), 0, MidpointRounding.AwayFromZero)),
                        ScoreNumber = review["AverageScore"].ToString(),
                    }); ;
                }
                ReviewsView.ItemsSource = reviews;
            }
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
            PopulateMenuTab(consumer);
            PopulateReviewsTab();

            SlotPicker.ItemsSource = Enumerable.Range(1, 10).ToList();
            SlotPicker.IsVisible = false;

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
        public byte[] DownloadPdfStream(string URL)
        {
            var uri = new System.Uri(URL);
            var client = new WebClient();

            //Returns the PDF document from the given URL
            return client.DownloadData(uri);
        }

        //method to get menu for restaurant
        private void PopulateMenuTab(JObject json)
        {
            if (json["Menus"].Type == JTokenType.Null || string.IsNullOrEmpty(json["Menus"].ToString()) || !json["Menus"].Any())
            {
                pdfViewerControl.IsVisible = false;
                MenuLabel.Text = "No Menus Currently Available.";
            }
            else
            {
                int menuLength = json["Menus"].Count();
                //Create a new PDF document
                PdfDocument document = new PdfDocument();
                var listMenu = new List<byte[]>();

                for (int i = 0; i < menuLength; i++)
                {
                    //Provide the PDF document URL in the below overload.
                    var pdfUrl = json["Menus"][i]["StorageUrl"].ToString();

                    try
                    {
                        //Returns the PDF document from the given URL
                        var documenStream = DownloadPdfStream(pdfUrl);
                        listMenu.Add(documenStream);
                    }
                    catch (WebException wex)
                    {
                    }
                }

                PdfMergeOptions mergeOptions = new PdfMergeOptions();

                //Enable Optimize Resources
                mergeOptions.OptimizeResources = true;

                //Merge the documents
                PdfDocumentBase.Merge(document, mergeOptions, listMenu.ToArray());

                //Save the PDF document to stream
                MemoryStream stream = new MemoryStream();

                document.Save(stream);

                //Close the documents
                document.Close(true);

                pdfViewerControl.LoadDocument(stream);
            }
        }

        // event triggered when the floating action button is clicked
        private async void FloatingButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync();
            await Navigation.PushModalAsync(new MainPage());
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
        private async void ReviewsView_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            var review = e.ItemData as Review;
            await Navigation.PushPopupAsync(new ReviewsPopup(review));
        }

        // event triggered when a timeslot is tapped
        private void AvailabilityView_ItemTapped(object sender, Syncfusion.ListView.XForms.ItemTappedEventArgs e)
        {
            var slot = e.ItemData as AvailableTime;
            string dateTime = slot.DateTime as string;
            Booking.Makebooking(micrositename, dateTime.Split(',')[0], dateTime.Split(',')[1], partySize);
        }

        private void Slot_Clicked(object sender, EventArgs e)
        {
            SlotPicker.IsVisible = true;
            SlotPicker.Focus();
            SlotPicker.SelectedIndex = slotNumber - 1;
            SlotPicker.IsVisible = false;
        }

        private void SlotPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SlotPicker.SelectedItem != null) slotNumber = (int)SlotPicker.SelectedItem;

            if (slotNumber == 1)
                SlotButton.Text = slotNumber + " SLOT";
            else
                SlotButton.Text = slotNumber + " SLOTS";

            promotions.Clear();
            availableTimes.Clear();
            PopulateBookingTab(result);
        }

        private void Party_Clicked(object sender, EventArgs e)
        {
            PartySizePicker.IsVisible = true;
            PartySizePicker.Focus();
            PartySizePicker.SelectedIndex = partySize - 1;
            PartySizePicker.IsVisible = false;
        }

        private void PartySizePicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PartySizePicker.SelectedItem != null) partySize = (int)PartySizePicker.SelectedItem;

            if (partySize == 1)
                PartyButton.Text = partySize + " PERSON";
            else
                PartyButton.Text = partySize + " PERSONS";

            promotions.Clear();
            availableTimes.Clear();
            PopulateBookingTab(result);
        }

        private void SetUpPartyPicker(JObject data)
        {
            var acceptableCoversList = new List<int>();

            if (data["MaxOnlinePartySize"] != null && data["MinOnlinePartySize"] != null)
                acceptableCoversList = Enumerable.Range((int)data["MinOnlinePartySize"], (int)data["MaxOnlinePartySize"] - (int)data["MinOnlinePartySize"] + 1).ToList();
            else
                acceptableCoversList = Enumerable.Range(1, 10).ToList();

            PartySizePicker.ItemsSource = acceptableCoversList;
            PartySizePicker.IsVisible = false;
        }
    }
}