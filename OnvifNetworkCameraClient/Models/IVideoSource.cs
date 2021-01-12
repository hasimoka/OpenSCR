using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using System;

namespace OnvifNetworkCameraClient.Models
{
    public interface IVideoSource
    {
        event EventHandler<DecodedVideoFrame> FrameDecoded;
    }
}