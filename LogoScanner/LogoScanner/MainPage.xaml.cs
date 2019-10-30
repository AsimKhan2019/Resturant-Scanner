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

            if (request.status.Equals("Succcess"))
            {
                // TODO do something with token
            }
            else
            {
                // TODO create warning pop up
            }
        }
    }
}
