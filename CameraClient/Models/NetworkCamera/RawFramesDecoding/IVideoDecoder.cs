using CameraClient.Models.NetworkCamera.RawFramesDecoding.DecodedFrames;
using RtspClientSharp.RawFrames.Video;

namespace CameraClient.Models.NetworkCamera.RawFramesDecoding
{
    public enum VideoCodecId
    {
        MJPEG = 1,
        H264 = 2,
    }

    public interface IVideoDecoder
    {
        DecodedVideoFrame TryDecode(RawVideoFrame rawVideoFrame);

        void SetOutputFolder(string outputFolder);

        void Dispose();
    }
}
