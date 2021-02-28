using System.Drawing;

namespace CameraClient.Models.NetworkCamera.RawFramesDecoding.DecodedFrames
{
    public class DecodedVideoFrame
    {
        public DecodedVideoFrame(Bitmap frameImage)
        {
            this.FrameImage = frameImage;
        }

        public Bitmap FrameImage { get; }
    }
}