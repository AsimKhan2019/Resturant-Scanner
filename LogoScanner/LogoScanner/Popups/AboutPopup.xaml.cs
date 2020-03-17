using System;
using Rg.Plugins.Popup.Services;

namespace LogoScanner
{
    public partial class AboutPopup : Rg.Plugins.Popup.Pages.PopupPage
    {
        public AboutPopup(string fullDescription)
        {
            InitializeComponent();

            FullDescriptionLabel.Text = fullDescription;
        }

        private async void OnClose(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
