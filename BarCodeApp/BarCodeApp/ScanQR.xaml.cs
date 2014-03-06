using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Devices;
using ZXing;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Input;

namespace BarCodeApp
{
    public partial class ScanQR : PhoneApplicationPage
    {
        private PhotoCamera phoneCamera;
        private WriteableBitmap previewBuffer;
        private DispatcherTimer scanTimer;

        public ScanQR()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            phoneCamera = new PhotoCamera();
            phoneCamera.Initialized += PhoneCamera_Initialized;
            phoneCamera.AutoFocusCompleted += PhoneCamera_AutoFocusCompleted;
            CameraButtons.ShutterKeyHalfPressed += CameraButtons_ShutterKeyHalfPressed;
            VideoBrush.SetSource(phoneCamera);
            scanTimer = new DispatcherTimer();
            scanTimer.Interval = TimeSpan.FromMilliseconds(250);
            scanTimer.Tick += ScanForBarcode;
            CameraCanvas.Tap += new EventHandler<GestureEventArgs>(Focus_Tapped);
        }

        private void Focus_Tapped(object sender, GestureEventArgs e)
        {
            if (phoneCamera != null && phoneCamera.IsFocusAtPointSupported)
            {
                Point tapLocation = e.GetPosition(CameraCanvas);

                FocusBrackets.SetValue(Canvas.LeftProperty, tapLocation.X - 30);
                FocusBrackets.SetValue(Canvas.TopProperty, tapLocation.Y - 28);

                double focusXPercentage = tapLocation.X / CameraCanvas.ActualWidth;
                double focusYPercentage = tapLocation.Y / CameraCanvas.ActualWidth;

                FocusBrackets.Visibility = Visibility.Visible;
                phoneCamera.FocusAtPoint(focusXPercentage, focusYPercentage);

            }
        }

        private void CameraButtons_ShutterKeyHalfPressed(object sender, EventArgs e)
        {
            phoneCamera.Focus();
        }

        private void PhoneCamera_AutoFocusCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(delegate()
            {
                FocusBrackets.Visibility = Visibility.Collapsed;
            });
        }

        private void PhoneCamera_Initialized(object sender, CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    phoneCamera.FlashMode = FlashMode.Off;
                    previewBuffer = new WriteableBitmap((int)phoneCamera.PreviewResolution.Width, (int)phoneCamera.PreviewResolution.Height);
                    scanTimer.Start();
                });
            }
        }

        private void ScanForBarcode(object sender, EventArgs e)
        {
            phoneCamera.GetPreviewBufferArgb32(previewBuffer.Pixels);
            previewBuffer.Invalidate();
            string value = QRCodeHelper.GetQRCodeValue(previewBuffer);
            if (!string.IsNullOrWhiteSpace(value))
            {
                VibrateController.Default.Start(TimeSpan.FromMilliseconds(100));
                BarcodeDataText.Text = value;
                scanTimer.Stop();
                phoneCamera.Dispose();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            scanTimer.Stop();
            scanTimer.Tick -= ScanForBarcode;
            phoneCamera.Initialized -= PhoneCamera_Initialized;
            phoneCamera.AutoFocusCompleted -= PhoneCamera_AutoFocusCompleted;
            CameraButtons.ShutterKeyHalfPressed -= CameraButtons_ShutterKeyHalfPressed;
            phoneCamera.Dispose();
            base.OnNavigatedFrom(e);
        }
    }
}