using System;
using Android.Content;
using Android.Support.Design.Widget;
using LogoScanner.Droid;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(TabbedPage), typeof(LogoScanner.Droid.RestaurantPageRenderer))]
namespace LogoScanner.Droid
{
    public class RestaurantPageRenderer : TabbedPageRenderer, TabLayout.IOnTabSelectedListener
    {
        public RestaurantPageRenderer(Context context) : base(context)
        {

        }

        void TabLayout.IOnTabSelectedListener.OnTabSelected(TabLayout.Tab tab)
        {
            if (tab == null) return;

            switch (tab.Text)
            {
                case "Home":
                    tab.SetIcon(Resource.Drawable.HomeIcon);
                    break;
                case "Menu":
                    tab.SetIcon(Resource.Drawable.MenuIcon);
                    break;
                case "Reviews":
                    tab.SetIcon(Resource.Drawable.ReviewIcon);
                    break;
                case "Scan":
                    tab.SetIcon(Resource.Drawable.ScanIcon);
                    break;
            }
        }

        void TabLayout.IOnTabSelectedListener.OnTabUnselected(TabLayout.Tab tab)
        {
            if (tab == null) return;

            switch (tab.Text)
            {
                case "Home":
                    tab.SetIcon(Resource.Drawable.HomeIconFilled);
                    break;
                case "Menu":
                    tab.SetIcon(Resource.Drawable.MenuIconFilled);
                    break;
                case "Reviews":
                    tab.SetIcon(Resource.Drawable.ReviewIconFilled);
                    break;
                case "Scan":
                    tab.SetIcon(Resource.Drawable.ScanIconFilled);
                    break;
            }
        }
    }
}

