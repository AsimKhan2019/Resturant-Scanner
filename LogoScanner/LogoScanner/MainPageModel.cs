using Plugin.Media;
using Plugin.Media.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace LogoScanner
{
    public class MainPageModel
    {
        //constructor
        public MainPageModel(INavigation navigation)
        {
            //_navigation = navigation;
            Photo = new Command(TakePhotoAsync);

        }

        //attributes
        public INavigation _navigation;
        public Command Photo { get; set; }

        public async void TakePhotoAsync()
        {
            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await App.Current.MainPage.DisplayAlert("No Camera", "No camera avaialble.", "OK");
                return;
            }
            var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
            {
                Directory = "Pictures",
                Name = DateTime.Now + "image.jpg"
            });
            if (file == null)
                return;

            await App.Current.MainPage.DisplayAlert("File Location", file.Path, "OK");
        }
    }
}
