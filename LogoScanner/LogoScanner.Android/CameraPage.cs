using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Views;
using Android.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using System.Threading;
using System.IO;
using Plugin.Permissions.Abstractions;
using Plugin.Permissions;
using Xamarin.Essentials;

[assembly: ExportRenderer(typeof(LogoScanner.MainPage), typeof(LogoScanner.Droid.CameraPage))]
namespace LogoScanner.Droid
{
    /*
    * Display Camera Stream: http://developer.xamarin.com/recipes/android/other_ux/textureview/display_a_stream_from_the_camera/
    * Camera Rotation: http://stackoverflow.com/questions/3841122/android-camera-preview-is-sideways
    */

    public class CameraPage : PageRenderer, TextureView.ISurfaceTextureListener
    {
        //constructor
        public CameraPage(Context context) : base(context)
        {
        }

        [Obsolete]
        private global::Android.Hardware.Camera camera;

        private global::Android.Widget.Button takePhotoButton;
        private global::Android.Widget.Button toggleFlashButton;
        private global::Android.Widget.Button cameraRectangle;

        private Activity activity;
        private CameraFacing cameraType;
        private TextureView textureView;
        private SurfaceTexture surfaceTexture;
        private global::Android.Views.View view;

        private bool flashOn;

        private byte[] imageBytes;

        protected override async void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            try
            {
                activity = this.Context as Activity;
                view = activity.LayoutInflater.Inflate(Resource.Layout.CameraLayout, this, false);
                cameraType = CameraFacing.Back;

                textureView = view.FindViewById<TextureView>(Resource.Id.textureView);
                textureView.SurfaceTextureListener = this;
                textureView.Click += focusOnTouch;

                takePhotoButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.takePhotoButton);
                takePhotoButton.Click += TakePhotoButtonTapped;

                cameraRectangle = view.FindViewById<global::Android.Widget.Button>(Resource.Id.cameraRectangle);
                cameraRectangle.Click += focusOnTouch;

                toggleFlashButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.toggleFlashButton);
                toggleFlashButton.Click += ToggleFlashButtonTapped;

                AddView(view);
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Camera Permission Not Granted", "OK");
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        public async void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                if (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                    {
                        await App.Current.MainPage.DisplayAlert("Camera Permission", "Allow us to access your camera", "OK");
                    }
                    var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                    status = results[Permission.Camera];
                }

                if (status == PermissionStatus.Granted)
                {
                    camera = global::Android.Hardware.Camera.Open((int)cameraType);
                    textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
                    surfaceTexture = surface;
                    camera.SetPreviewTexture(surface);
                    PrepareAndStartCamera();
                }
                else if (status != PermissionStatus.Unknown)
                {
                    await App.Current.MainPage.DisplayAlert("Permission unknown", "Please allow your camera", "OK");
                }
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert("Runtime error", "Please reopen the app", "OK");
            }
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            camera.StopPreview();
            camera.Release();

            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            PrepareAndStartCamera();
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        private void PrepareAndStartCamera()
        {
            camera.StopPreview();

            var display = activity.WindowManager.DefaultDisplay;
            if (display.Rotation == SurfaceOrientation.Rotation0)
            {
                camera.SetDisplayOrientation(90);
            }

            if (display.Rotation == SurfaceOrientation.Rotation270)
            {
                camera.SetDisplayOrientation(180);
            }

            camera.StartPreview();
        }

        private void ToggleFlashButtonTapped(object sender, EventArgs e)
        {
            flashOn = !flashOn;
            if (flashOn)
            {
                if (cameraType == CameraFacing.Back)
                {
                    toggleFlashButton.SetBackgroundResource(Resource.Drawable.FlashButton);
                    cameraType = CameraFacing.Back;

                    camera.StopPreview();
                    camera.Release();
                    camera = global::Android.Hardware.Camera.Open((int)cameraType);
                    var parameters = camera.GetParameters();
                    parameters.FlashMode = global::Android.Hardware.Camera.Parameters.FlashModeTorch;
                    camera.SetParameters(parameters);
                    camera.SetPreviewTexture(surfaceTexture);
                    PrepareAndStartCamera();
                }
            }
            else
            {
                toggleFlashButton.SetBackgroundResource(Resource.Drawable.NoFlashButton);
                camera.StopPreview();
                camera.Release();

                camera = global::Android.Hardware.Camera.Open((int)cameraType);
                var parameters = camera.GetParameters();
                parameters.FlashMode = global::Android.Hardware.Camera.Parameters.FlashModeOff;
                camera.SetParameters(parameters);
                camera.SetPreviewTexture(surfaceTexture);
                PrepareAndStartCamera();
            }
        }

        private void focusOnTouch(object sender, EventArgs e)
        {
            camera.AutoFocus(null);
        }

        [Obsolete]
        private async void TakePhotoButtonTapped(object sender, EventArgs e)
        {
            var current = Connectivity.NetworkAccess;

            if (current == NetworkAccess.Unknown || current != NetworkAccess.Internet)
            {
                await App.Current.MainPage.DisplayAlert("Error", "No Internet Connection Available", "OK");
            }
            else
            {
                camera.StopPreview();
                DialogService.ShowLoading("Capturing Every Pixel");

                var image = textureView.Bitmap;
                using (var imageStream = new MemoryStream())
                {
                    await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 50, imageStream);
                    image.Recycle();
                    imageBytes = imageStream.ToArray();
                }

                var results = await CustomVisionService.PredictImageContentsAsync(imageBytes, (new CancellationTokenSource()).Token);
                var navigationPage = new NavigationPage(new RestaurantPage(results.ToString()));

                DialogService.HideLoading();
                camera.StartPreview();
                await App.Current.MainPage.Navigation.PushModalAsync(navigationPage, false);
            }
        }
    }
}