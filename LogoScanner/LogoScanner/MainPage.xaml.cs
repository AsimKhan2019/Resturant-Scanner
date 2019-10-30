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

            TestLabel.Text = request.message;
        }
    }
}
