using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Views;
using Android.Widget;
using Plugin.Permissions;
using Plugin.Connectivity;
using Plugin.Permissions.Abstractions;
using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LogoScanner.MainPage), typeof(LogoScanner.Droid.CameraPage))]

namespace LogoScanner.Droid
{
    public class CameraPage : PageRenderer, TextureView.ISurfaceTextureListener
    {
        // constructor
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

        [Obsolete]
        protected override async void OnElementChanged(ElementChangedEventArgs<Page> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null || Element == null)
                return;

            // exception handling for the camera permission
            try
            {
                activity = this.Context as Activity;
                view = activity.LayoutInflater.Inflate(Resource.Layout.CameraLayout, this, false);
                cameraType = CameraFacing.Back;

                textureView = view.FindViewById<TextureView>(Resource.Id.textureView);
                textureView.SurfaceTextureListener = this;
                textureView.Click += FocusOnTouch;

                takePhotoButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.takePhotoButton);
                takePhotoButton.Click += TakePhotoButtonTapped;

                cameraRectangle = view.FindViewById<global::Android.Widget.Button>(Resource.Id.cameraRectangle);
                cameraRectangle.Click += FocusOnTouch;

                toggleFlashButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.toggleFlashButton);
                toggleFlashButton.Click += ToggleFlashButtonTapped;

                AddView(view);
            }
            catch (Exception)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Camera Permission Not Granted", "OK");
            }
        }

        // method to set the size of the camera to equal to the whole viewport
        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            var msw = MeasureSpec.MakeMeasureSpec(r - l, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(b - t, MeasureSpecMode.Exactly);

            view.Measure(msw, msh);
            view.Layout(0, 0, r - l, b - t);
        }

        [Obsolete]
        public async void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            // exception handling for the camera permission and won't pass until granted
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                while (status != PermissionStatus.Granted)
                {
                    if (await CrossPermissions.Current.ShouldShowRequestPermissionRationaleAsync(Permission.Camera))
                    {
                        await App.Current.MainPage.DisplayAlert("Camera Permission", "Access to Camera Required", "OK");
                        while (status != PermissionStatus.Granted)
                        {
                            var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                            status = results[Permission.Camera];
                        }
                    }
                    else
                    {
                        var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                        status = results[Permission.Camera];
                    }
                }

                if (status == PermissionStatus.Granted)
                {
                    camera = global::Android.Hardware.Camera.Open((int)cameraType);
                    textureView.LayoutParameters = new FrameLayout.LayoutParams(width, height);
                    surfaceTexture = surface;
                    camera.SetPreviewTexture(surface);
                    PrepareAndStartCamera();
                }
            }
            catch (Exception)
            {
                await App.Current.MainPage.DisplayAlert("Error", "Please Restart App", "OK");
                var activity = (Activity)Forms.Context;
                activity.FinishAffinity();
            }
        }

        [Obsolete]
        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            camera.StopPreview();
            camera.Release();
            return true;
        }

        [Obsolete]
        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
            PrepareAndStartCamera();
        }

        [Obsolete]
        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        // method to check to rotation of the screen and use to overcome a bug with the certain hardware 
        [Obsolete]
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

        // method for flash button and make sure it off after the taking a photo
        [Obsolete]
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

        // method for manual focus when touch the screen
        [Obsolete]
        private void FocusOnTouch(object sender, EventArgs e)
        {
            camera.AutoFocus(null);
        }

        // method to capture the photo
        [Obsolete]
        private async void TakePhotoButtonTapped(object sender, EventArgs e)
        {
            var current = CrossConnectivity.Current.IsConnected;

            // check the internet connection to use the ResDiary API
            if (!current)
            {
                await App.Current.MainPage.DisplayAlert("Connection Error", "Please connect to the internet", "OK");
            }
            else
            {
                try
                {
                    var parameters = camera.GetParameters();
                    parameters.FlashMode = global::Android.Hardware.Camera.Parameters.FlashModeOff;
                    camera.SetParameters(parameters);
                    camera.StopPreview();
                    DialogService.ShowLoading("Scanning Logo");

                    // crop the image into the sqaure in order to make the prediction more accuracy
                    var image = CropImage(textureView.Bitmap);
                    using (var imageStream = new MemoryStream())
                    {
                        await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 50, imageStream);
                        image.Recycle();
                        imageBytes = imageStream.ToArray();
                    }

                    // send the image to CustomVision in form of bytes
                    var results = await CustomVisionService.PredictImageContentsAsync(imageBytes);
                    String resultInString = results.ToString();

                    if (resultInString.Length > 0)
                    {
                        // if the logo appeared more than 1 result than use the Geolocation
                        if (Geolocation.HasMoreOptions(resultInString))
                        {
                            DialogService.ShowLoading("More Restaurants Available");
                            resultInString = await Geolocation.GetCloserOptionAsync(resultInString);
                        }
                        var navigationPage = new NavigationPage(new RestaurantPage(resultInString));

                        DialogService.HideLoading();
                        camera.StartPreview();
                        await App.Current.MainPage.Navigation.PushModalAsync(navigationPage, true);
                    }
                    else
                    {
                        DialogService.HideLoading();
                        camera.StartPreview();

                        await App.Current.MainPage.DisplayAlert("Restaurant Not Found", "Please re-scan the Logo", "OK");
                    }
                }
                catch (Exception)
                {
                    camera.StopPreview();
                    camera.Release();
                    camera = global::Android.Hardware.Camera.Open((int)cameraType);
                    camera.SetPreviewTexture(surfaceTexture);

                    PrepareAndStartCamera();
                }
            }
        }

        // method to crop the image
        private static Bitmap CropImage(Bitmap image)
        {
            var resizedBitmap = Bitmap.CreateBitmap(image, image.Width / 4 - image.Width / 8,
                image.Height / 4 + image.Height / 20, image.Height / 3, image.Height / 3);
            return resizedBitmap;
        } 
    }
}