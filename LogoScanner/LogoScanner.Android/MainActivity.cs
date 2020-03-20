using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Plugin.CurrentActivity;
using Acr.UserDialogs;
using ImageCircle.Forms.Plugin.Droid;
using Plugin.Permissions;
using SuaveControls.FloatingActionButton.Droid.Renderers;
using Android.Content.Res;
using LogoScanner.Themes;

namespace LogoScanner.Droid
{
    [Activity(Label = "LogoScanner", Icon = "@mipmap/icon", Theme = "@style/MainTheme", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Rg.Plugins.Popup.Popup.Init(this, savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            global::Xamarin.FormsMaps.Init(this, savedInstanceState);

            ImageCircleRenderer.Init();
            UserDialogs.Init(this);
            FloatingActionButtonRenderer.Initialize();

            // plugin for access to an Android Application’s current Activity that is being displayed
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            LoadApplication(new App());

            SetAppTheme();
        }

        // for checking the user permission
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        private void SetAppTheme()
        {
            // Ensure the device is running Android Froyo or higher because UIMode was added in Android Froyo, API 8.0
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Froyo)
            {
                var uiModeFlags = CrossCurrentActivity.Current.AppContext.Resources.Configuration.UiMode & UiMode.NightMask;

                switch (uiModeFlags)
                {
                    case UiMode.NightYes:
                        App.Current.Resources = new DarkTheme();
                        break;

                    case UiMode.NightNo:
                        App.Current.Resources = new LightTheme();
                        break;
                }
            }
            else
            {
                App.Current.Resources = new LightTheme();
            }
        }
    }
}