using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(LogoScanner.iOS.RestaurantPageRenderer))]
namespace LogoScanner.iOS
{
    public class RestaurantPageRenderer : TabbedRenderer
    {
        /*protected override void OnElementChanged(ElementChangedEventArgs<TabbedPage> e)
        {
            base.OnElementChanged(e);

            Element.CurrentPageChanged += HandleCurrentPageChanged;
        }

        void HandleCurrentPageChanged(object sender, EventArgs e)
        {
            var currentNavigationPage = Element.CurrentPage as NavigationPage;
            if (!(currentNavigationPage.RootPage is PhrasesPage))
                return;

            var tabLayout = (TabLayout)ViewGroup.GetChildAt(1);

            for (int i = 0; i < tabLayout.TabCount; i++)
            {
                var currentTab = tabLayout.GetTabAt(i);
                var currentTabText = currentTab.Text;

                if (currentTabText.Equals("Play") || currentTabText.Equals("Pause"))
                {
                    Device.BeginInvokeOnMainThread(() => UpdateTab(currentTabText, currentTab, currentNavigationPage));
                    break;
                }
            }
        }

        void CurrentPageChanged(object sender, EventArgs e)
        {

        }*/


    }
}

