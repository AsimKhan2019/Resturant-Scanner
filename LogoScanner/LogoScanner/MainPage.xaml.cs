using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LogoScanner
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        //attributes
        public MainPageModel Model { get; set; }
        public MainPage()
        {
            InitializeComponent();
            Model = new MainPageModel(Navigation);
            BindingContext = Model;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Model.TakePhotoAsync();
        }
    }
}
