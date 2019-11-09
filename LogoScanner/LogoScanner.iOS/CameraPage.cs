using System;
using AVFoundation;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;


/*
 * AVFoundation Reference: http://red-glasses.com/index.php/tutorials/ios4-take-photos-with-live-video-preview-using-avfoundation/
 * Additional Camera Settings Reference: http://stackoverflow.com/questions/4550271/avfoundation-images-coming-in-unusably-dark
 * Custom Renderers: http://blog.xamarin.com/using-custom-uiviewcontrollers-in-xamarin.forms-on-ios/
 */

[assembly: ExportRenderer(typeof(LogoScanner.MainPage), typeof(LogoScanner.iOS.CameraPage))]
namespace LogoScanner.iOS
{
    public class CameraPage : PageRenderer
    {
        AVCaptureSession captureSession;
        AVCaptureDeviceInput captureDeviceInput;
        UIButton cameraRectangle;
        UIButton toggleFlashButton;
        UIView liveCameraStream;
        AVCaptureStillImageOutput stillImageOutput;
        UIButton takePhotoButton;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupUserInterface();
            SetupEventHandlers();

            AuthorizeCameraUse();
            SetupLiveCameraStream();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        public async void AuthorizeCameraUse()
        {
            var authorizationStatus = AVCaptureDevice.GetAuthorizationStatus(AVMediaType.Video);

            if (authorizationStatus != AVAuthorizationStatus.Authorized)
            {
                await AVCaptureDevice.RequestAccessForMediaTypeAsync(AVMediaType.Video);
            }
        }

        public void SetupLiveCameraStream()
        {
            captureSession = new AVCaptureSession();

            var videoPreviewLayer = new AVCaptureVideoPreviewLayer(captureSession)
            {
                Frame = liveCameraStream.Bounds
            };
            liveCameraStream.Layer.AddSublayer(videoPreviewLayer);

            var captureDevice = AVCaptureDevice.DefaultDeviceWithMediaType(AVMediaType.Video);
            ConfigureCameraForDevice(captureDevice);
            captureDeviceInput = AVCaptureDeviceInput.FromDevice(captureDevice);

            var dictionary = new NSMutableDictionary();
            dictionary[AVVideo.CodecKey] = new NSNumber((int)AVVideoCodec.JPEG);
            stillImageOutput = new AVCaptureStillImageOutput()
            {
                OutputSettings = new NSDictionary()
            };

            captureSession.AddOutput(stillImageOutput);
            captureSession.AddInput(captureDeviceInput);
            captureSession.StartRunning();
        }

        public async void CapturePhoto()
        {
            var videoConnection = stillImageOutput.ConnectionFromMediaType(AVMediaType.Video);
            var sampleBuffer = await stillImageOutput.CaptureStillImageTaskAsync(videoConnection);

            // var jpegImageAsBytes = AVCaptureStillImageOutput.JpegStillToNSData (sampleBuffer).ToArray ();
            var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);
            // var image = new UIImage (jpegImageAsNsData);
            // var image2 = new UIImage (image.CGImage, image.CurrentScale, UIImageOrientation.UpMirrored);
            // var data = image2.AsJPEG ().ToArray ();

            // SendPhoto (data);
            SendPhoto(jpegImageAsNsData.ToArray());
        }

        public void ConfigureCameraForDevice(AVCaptureDevice device)
        {
            var error = new NSError();
            if (device.IsFocusModeSupported(AVCaptureFocusMode.ContinuousAutoFocus))
            {
                device.LockForConfiguration(out error);
                device.FocusMode = AVCaptureFocusMode.ContinuousAutoFocus;
                device.UnlockForConfiguration();
            }
            else if (device.IsExposureModeSupported(AVCaptureExposureMode.ContinuousAutoExposure))
            {
                device.LockForConfiguration(out error);
                device.ExposureMode = AVCaptureExposureMode.ContinuousAutoExposure;
                device.UnlockForConfiguration();
            }
            else if (device.IsWhiteBalanceModeSupported(AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance))
            {
                device.LockForConfiguration(out error);
                device.WhiteBalanceMode = AVCaptureWhiteBalanceMode.ContinuousAutoWhiteBalance;
                device.UnlockForConfiguration();
            }
        }

        public void ToggleFlash()
        {
            var device = captureDeviceInput.Device;

            var error = new NSError();
            if (device.HasFlash)
            {
                if (device.FlashMode == AVCaptureFlashMode.On)
                {
                    device.LockForConfiguration(out error);
                    device.FlashMode = AVCaptureFlashMode.Off;
                    device.UnlockForConfiguration();

                    toggleFlashButton.SetBackgroundImage(UIImage.FromFile("NoFlashButton.png"), UIControlState.Normal);
                }
                else
                {
                    device.LockForConfiguration(out error);
                    device.FlashMode = AVCaptureFlashMode.On;
                    device.UnlockForConfiguration();

                    toggleFlashButton.SetBackgroundImage(UIImage.FromFile("FlashButton.png"), UIControlState.Normal);
                }
            }
        }

        public AVCaptureDevice GetCameraForOrientation(AVCaptureDevicePosition orientation)
        {
            var devices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);

            foreach (var device in devices)
            {
                if (device.Position == orientation)
                {
                    return device;
                }
            }

            return null;
        }

        private void SetupUserInterface()
        {
            var centerButtonX = View.Bounds.GetMidX();
            var centerX = View.Bounds.GetMidX();
            var centerY = View.Bounds.GetMidY();
            var bottomButtonY = View.Bounds.Bottom - 100;
            var topButtonY = View.Bounds.Top + 45;

            liveCameraStream = new UIView()
            {
                Frame = new CGRect(0f, 0f, View.Bounds.Width, View.Bounds.Height)
            };

            takePhotoButton = new UIButton()
            {
                Frame = new CGRect(centerButtonX - 35, bottomButtonY, 70, 70)
            };
            takePhotoButton.SetBackgroundImage(UIImage.FromFile("TakePhotoButton.png"), UIControlState.Normal);

            cameraRectangle = new UIButton()
            {
                Frame = new CGRect(centerX - 125, centerY - 125, 250, 250)
            };
            cameraRectangle.SetBackgroundImage(UIImage.FromFile("CameraRectangle.png"), UIControlState.Normal);

            toggleFlashButton = new UIButton()
            {
                Frame = new CGRect(centerButtonX - 20, topButtonY, 40, 40)
            };
            toggleFlashButton.SetBackgroundImage(UIImage.FromFile("NoFlashButton.png"), UIControlState.Normal);

            View.Add(liveCameraStream);
            View.Add(takePhotoButton);
            View.Add(cameraRectangle);
            View.Add(toggleFlashButton);
        }

        private void SetupEventHandlers()
        {
            takePhotoButton.TouchUpInside += (object sender, EventArgs e) => {
                CapturePhoto();
            };

            cameraRectangle.TouchUpInside += (object sender, EventArgs e) =>
            {
                UpdateFocusIfNeeded();
            };
            
            toggleFlashButton.TouchUpInside += (object sender, EventArgs e) => {
                ToggleFlash();
            };
        }

        public async void SendPhoto(byte[] image)
        {
            var navigationPage = new NavigationPage(new RestaurantPage());

            await App.Current.MainPage.Navigation.PushModalAsync(navigationPage, false);
        }
    }
}