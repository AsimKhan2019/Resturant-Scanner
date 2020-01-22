using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Hardware;
using Android.Views;
using Android.Widget;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.IO;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(LogoScanner.MainPage), typeof(LogoScanner.Droid.CameraPage))]
namespace LogoScanner.Droid
{
	public class CameraPage : PageRenderer, TextureView.ISurfaceTextureListener
	{
		//constructor
		public CameraPage(Context context) : base(context)
		{

		}

		[Obsolete]
		global::Android.Hardware.Camera camera;

		global::Android.Widget.Button takePhotoButton;
		global::Android.Widget.Button toggleFlashButton;
		global::Android.Widget.Button cameraRectangle;

		Activity activity;
		CameraFacing cameraType;
		TextureView textureView;
		SurfaceTexture surfaceTexture;
		global::Android.Views.View view;

		bool flashOn;

		byte[] imageBytes;

		[Obsolete]
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
				textureView.Click += FocusOnTouch;

				takePhotoButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.takePhotoButton);
				takePhotoButton.Click += TakePhotoButtonTapped;

				cameraRectangle = view.FindViewById<global::Android.Widget.Button>(Resource.Id.cameraRectangle);
				cameraRectangle.Click += FocusOnTouch;

				toggleFlashButton = view.FindViewById<global::Android.Widget.Button>(Resource.Id.toggleFlashButton);
				toggleFlashButton.Click += ToggleFlashButtonTapped;

				AddView(view);
			}
			catch (System.Exception)
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

		[Obsolete]
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
			catch (System.Exception)
			{

				await App.Current.MainPage.DisplayAlert("Runtime error", "Please reopen the app", "OK");
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

		public void OnSurfaceTextureUpdated(SurfaceTexture surface)
		{

		}

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

		[Obsolete]
		private void FocusOnTouch(object sender, EventArgs e)
		{
			camera.AutoFocus(null);
		}

		[Obsolete]
		private async void TakePhotoButtonTapped(object sender, EventArgs e)
		{
			camera.StopPreview();
			DialogService.ShowLoading("Capturing Every Pixel");

			var image = CropImage(textureView.Bitmap);
			using (var imageStream = new MemoryStream())
			{
				await image.CompressAsync(Bitmap.CompressFormat.Jpeg, 50, imageStream);
				image.Recycle();
				imageBytes = imageStream.ToArray();
			}

			var navigationPage = new NavigationPage(new Page1(imageBytes));
			//var results = await CustomVisionService.PredictImageContentsAsync(imageBytes, (new CancellationTokenSource()).Token);
			//var navigationPage = new NavigationPage(new RestaurantPage(results.ToString()));

			DialogService.HideLoading();
			camera.StartPreview();
			await App.Current.MainPage.Navigation.PushModalAsync(navigationPage, false);
		}

		private static Bitmap CropImage(Bitmap image)
		{
			var resizedbitmap1 = Bitmap.CreateBitmap(image, image.Width/4 -40, image.Height / 4 + 20, 480, 480);
			return resizedbitmap1;
		}
	}

}
