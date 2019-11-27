﻿using System;
using System.Threading.Tasks;
using AVFoundation;
using CoreGraphics;
using Foundation;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
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

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupUserInterface();
            SetupEventHandlers();

            if (await AuthorizeCameraUse()) SetupLiveCameraStream();
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        }

        public async Task<Boolean> AuthorizeCameraUse()
        {
            var cameraStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);

            if (cameraStatus != PermissionStatus.Granted)
            {
                var results = await CrossPermissions.Current.RequestPermissionsAsync(new[] { Permission.Camera });
                cameraStatus = results[Permission.Camera];
            }

            if (cameraStatus == PermissionStatus.Granted)
            {
                return true;
            }
            else
            {
                await App.Current.MainPage.DisplayAlert("Permission", "Please allow camera access to continue.", "OK");                    
                CrossPermissions.Current.OpenAppSettings();
            }

            return false;
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
            var jpegImageAsNsData = AVCaptureStillImageOutput.JpegStillToNSData(sampleBuffer);

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
            var bottomButtonY = View.Bounds.Bottom - 165;
            var topButtonY = View.Bounds.Top + 100;

            liveCameraStream = new UIView()
            {
                Frame = new CGRect(0f, 0f, View.Frame.Size.Width, View.Bounds.Height)
            };
            liveCameraStream.BackgroundColor = UIColor.Black;

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