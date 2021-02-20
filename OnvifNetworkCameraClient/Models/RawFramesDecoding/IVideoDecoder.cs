using RtspClientSharp.RawFrames.Video;
using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding
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
