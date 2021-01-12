using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using RtspClientSharp.RawFrames.Video;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding
{
    public class MjpegVideoDecoder : IVideoDecoder
    {
        private VideoCodecId codecId;

        public MjpegVideoDecoder(VideoCodecId videoCodecId)
        {
            this.codecId = videoCodecId;
        }

        public static IVideoDecoder CreateDecoder(VideoCodecId videoCodecId)
        {
            return new MjpegVideoDecoder(videoCodecId);
        }

        public void Dispose()
        {
        }

        public DecodedVideoFrame TryDecode(RawVideoFrame rawVideoFrame)
        {
            DecodedVideoFrame decodedVideoFrame = null;

            using (var stream = new MemoryStream(rawVideoFrame.FrameSegment.Array))
            {
                var decoder = new JpegBitmapDecoder(stream, BitmapCreateOptions.None, BitmapCacheOption.None);
                decodedVideoFrame = new DecodedVideoFrame(this.BitmapFromSource(decoder.Frames[0]));
            }

            return decodedVideoFrame;
        }

        private Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            // convert image format
            var src = new FormatConvertedBitmap();
            src.BeginInit();
            src.Source = bitmapsource;
            src.DestinationFormat = System.Windows.Media.PixelFormats.Bgra32;
            src.EndInit();

            // copy to bitmap
            Bitmap bitmap = new Bitmap(src.PixelWidth, src.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            src.CopyPixels(System.Windows.Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bitmap.UnlockBits(data);

            return bitmap;
        }
    }
}
