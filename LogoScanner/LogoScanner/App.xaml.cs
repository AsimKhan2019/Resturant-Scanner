using Xamarin.Essentials;
using Xamarin.Forms;

namespace LogoScanner
{
    public partial class App : Application
    {
        public static Location location = null;

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override async void OnStart()
        {
            // Handle when your app starts
            location = await Geolocation.GetMyLocation();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}