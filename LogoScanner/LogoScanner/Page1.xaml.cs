using System.IO;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace LogoScanner
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Page1 : ContentPage
    {
        byte[] Img { get; set; }
        public Page1(byte[] _img)
        {
            InitializeComponent();
            Img = _img;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            image.Source = ImageSource.FromStream(() => new MemoryStream(Img));
        }
    }

}