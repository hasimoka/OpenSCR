using System;
using System.Drawing;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames
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