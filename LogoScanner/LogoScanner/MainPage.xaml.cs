using System;
using System.ComponentModel;
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

            var request = await Requests.ConnectToResDiary();

            if (request.status.Equals("Success"))
            {
                // TODO do something with token
            }
            else
            {
                await DisplayAlert("Error", request.message, "OK");
            }
        }
    }
}
