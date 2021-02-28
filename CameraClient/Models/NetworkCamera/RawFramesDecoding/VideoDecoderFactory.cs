namespace CameraClient.Models.NetworkCamera.RawFramesDecoding
{
    public abstract class VideoDecoderFactory
    {
        protected VideoCodecId _codecId;

        public VideoDecoderFactory(VideoCodecId codecId)
        {
            _codecId = codecId;
        }

        public abstract IVideoDecoder CreateVideoDecoder();

        public static VideoDecoderFactory getFactory(VideoCodecId codecId)
        {
            if (codecId == VideoCodecId.MJPEG)
            {
                return new MjpegDecoderFactory(codecId);
            }
            else if (codecId == VideoCodecId.H264)
            {
                return new H264DecoderFactory(codecId);
            }

            return null;
        }
    }

    public class MjpegDecoderFactory : VideoDecoderFactory
    {
        public MjpegDecoderFactory(VideoCodecId codecId) : base(codecId){}

        public override IVideoDecoder CreateVideoDecoder()
        {
            return MjpegVideoDecoder.CreateDecoder(_codecId);
        }
    }

    public class H264DecoderFactory : VideoDecoderFactory
    {
        public H264DecoderFactory(VideoCodecId codecId): base(codecId){}

        public override IVideoDecoder CreateVideoDecoder()
        {
            return H264VideoDecoder.CreateDecoder(_codecId);
        }
    }
}
