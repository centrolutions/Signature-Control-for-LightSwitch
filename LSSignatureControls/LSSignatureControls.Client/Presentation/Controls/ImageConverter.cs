using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.IO;
using System.Windows.Media.Imaging;
using ImageTools.IO.Png;
using ImageTools.IO.Jpeg;
using ImageTools;

namespace LSSignatureControls.Presentation.Controls
{
    /// <summary>
    /// Converts between WriteableBitmap and a Byte array
    /// </summary>
    public class ImageConverter : IValueConverter
    {
        /// <summary>
        /// 
        /// </summary>
        public ImageEncoding ImageType { get; set; }

        /// <summary>
        /// Converts encoded image bytes to a WriteableBitmap object
        /// </summary>
        /// <param name="value">The encoded byte array to convert</param>
        /// <param name="targetType">ignored</param>
        /// <param name="parameter">ignored</param>
        /// <param name="culture">ignored</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            //convert bytes to WriteableBitmap
            try
            {
                var bytes = (byte[])value;
                return FromByteArray(bytes);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a WriteableBitmap object to encoded image bytes
        /// </summary>
        /// <param name="value">The WriteableBitmap object to convert</param>
        /// <param name="targetType">ignored</param>
        /// <param name="parameter">ignored</param>
        /// <param name="culture">ignored</param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            //convert WriteableBitmap to encoded bytes (PNG or JPG)
            try
            {
                var writeable = (WriteableBitmap)value;
                return ToByteArray(writeable, this.ImageType);
            }
            catch
            {
                return null;
            }
        }

        static byte[] ToByteArray(WriteableBitmap bmp, ImageEncoding imageType)
        {
            //if the image is empty, there are no bytes
            if (bmp == null || (bmp.PixelWidth == 0 && bmp.PixelHeight == 0))
                return null;

            using (var encoded = (imageType == ImageEncoding.Png) ? GetPngStream(bmp) : GetJpgStream(bmp))
            using (var ms = new MemoryStream())
            {
                encoded.CopyTo(ms);
                return ms.GetBuffer();
            }
        }
        static WriteableBitmap FromByteArray(byte[] buffer)
        {
            if (buffer == null || buffer.Length == 0)
                return null;

            using (var ms = new MemoryStream(buffer))
            {
                var bmp = new BitmapImage();
                bmp.SetSource(ms);
                var writeable = new WriteableBitmap(bmp);
                return writeable;
            }
        }

        static Stream GetPngStream(WriteableBitmap bmp)
        {
#if TRIAL
            bmp = AddTrialTextToImage(bmp);
#endif
            PngEncoder enc = new PngEncoder();
            enc.IsWritingUncompressed = false;
            var ms = new MemoryStream();
            enc.Encode(bmp.ToImage(), ms);
            ms.Position = 0;
            return ms;
        }

        static Stream GetJpgStream(WriteableBitmap bmp)
        {
#if TRIAL
            bmp = AddTrialTextToImage(bmp);
#endif
            JpegEncoder enc = new JpegEncoder();
            enc.Quality = 90;
            var ms = new MemoryStream();
            enc.Encode(bmp.ToImage(), ms);
            ms.Position = 0;
            return ms;
        }

#if TRIAL
        static WriteableBitmap AddTrialTextToImage(WriteableBitmap bmp)
        {
            bmp.Invalidate();
            var txt = new TextBlock();
            txt.Text = LSSignatureControls.Presentation.Resources.Strings.TrialVersionWatermark;
            txt.FontSize = FindProperDiagonalFontSize(txt.Text, bmp.PixelWidth, bmp.PixelHeight);
            txt.Foreground = new SolidColorBrush(Colors.Red);

            var rotation = new RotateTransform() { Angle = 45 };
            var translate = new TranslateTransform() { X = 0, Y = 0 };
            for (var i = txt.ActualHeight * -4; i < txt.ActualHeight * 3; i += txt.ActualHeight)
            {
                translate.Y = i;
                translate.X = i * -1;
                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(rotation);
                transformGroup.Children.Add(translate);
                bmp.Render(txt, transformGroup);
            }

            bmp.Invalidate();
            return bmp;
        }

        static double FindProperDiagonalFontSize(string text, double photoWidth, double photoHeight)
        {
            var txt = new TextBlock();
            txt.Text = text;
            txt.Foreground = new SolidColorBrush(Colors.Black);
            txt.FontSize = 10;
            var targetLength = HypotenuseLength(photoWidth, photoHeight);

            while (txt.ActualWidth < targetLength)
            {
                txt.FontSize += 4;
            }

            return txt.FontSize;
        }

        static double HypotenuseLength(double sideA, double sideB)
        {
            return Math.Sqrt(Math.Pow(sideA, 2) + Math.Pow(sideB, 2));
        }
#endif
    }
}
