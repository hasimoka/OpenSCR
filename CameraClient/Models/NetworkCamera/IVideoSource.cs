using System;
using CameraClient.Models.NetworkCamera.RawFramesDecoding.DecodedFrames;

namespace CameraClient.Models.NetworkCamera
{
    public interface IVideoSource
    {
        event EventHandler<DecodedVideoFrame> FrameDecoded;
    }
}