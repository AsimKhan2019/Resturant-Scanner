using System;
using LogoScanner.Themes;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ContentPage), typeof(LogoScanner.iOS.PageRenderer))]

namespace LogoScanner.iOS
{
    public class PageRenderer : Xamarin.Forms.Platform.iOS.PageRenderer
    {
        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null) return;

            try
            {
                SetTheme();
            }
            catch (Exception ex)
            {
                if (ex.Source != null)
                    Console.WriteLine("Exception source: {0}", ex.Source);
                throw;
            }
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (TraitCollection.UserInterfaceStyle != previousTraitCollection.UserInterfaceStyle)
                SetTheme();
        }

        private void SetTheme()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(12, 0)) // support iOS 13 and up
            {
                if (TraitCollection.UserInterfaceStyle == UIUserInterfaceStyle.Dark)
                    App.Current.Resources = new DarkTheme();
                else
                    App.Current.Resources = new LightTheme();
            }
            else
            {
                App.Current.Resources = new LightTheme();
            }
        }
    }
}