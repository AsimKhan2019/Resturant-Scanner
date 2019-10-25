using System.ComponentModel;
using System.Net.Http;
using System.Text;
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

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            string result;
            string credentials;

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
    }
}
