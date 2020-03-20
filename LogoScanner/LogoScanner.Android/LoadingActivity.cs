using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.App;

namespace LogoScanner.Droid
{
    [Activity(Theme = "@style/launchtheme", MainLauncher = true, NoHistory = true)]
    public class LoadingActivity : AppCompatActivity
    {
        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);
        }

        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        public override void OnBackPressed()
        {
        }
    }
}