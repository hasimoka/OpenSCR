using System.Drawing;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames
{
    public class DecodedVideoFrame
    {
        public Bitmap Image { get; }

        public DecodedVideoFrame(Bitmap frameImage)
        {
            Image = frameImage;
        }
    }
}