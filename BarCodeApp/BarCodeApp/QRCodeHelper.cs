using Microsoft.Devices;
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ZXing;

namespace BarCodeApp
{
    public static class QRCodeHelper
    {
        public static WriteableBitmap CreateQRCode(BarcodeFormat barcodeFormat, string inputText)
        {
            BarcodeWriter qrCodeWriter = new BarcodeWriter();
            qrCodeWriter.Renderer = new ZXing.Rendering.WriteableBitmapRenderer() { Foreground = Color.FromArgb(0, 0, 0, 0) };
            qrCodeWriter.Format = BarcodeFormat.QR_CODE;
            qrCodeWriter.Options.Height = 400;
            qrCodeWriter.Options.Width = 400;
            qrCodeWriter.Options.Margin = 1;
            WriteableBitmap qrCodeImage = qrCodeWriter.Write(inputText);
            return qrCodeImage;
        }

        public static string GetQRCodeValue(WriteableBitmap image)
        {
            BarcodeReader barcodeReader = new BarcodeReader();
            barcodeReader.Options.TryHarder = true;
            Result result = barcodeReader.Decode(image);
            if (result != null)
                return result.Text;
            return string.Empty;
        }
    }
}
