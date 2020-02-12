using System;
using Rg.Plugins.Popup.Services;

namespace LogoScanner
{
    public partial class ReviewsPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public ReviewsPopup(Review review)
        {
            InitializeComponent();

            BindingContext = review;
        }

        private async void OnClose(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
