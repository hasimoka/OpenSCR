using RtspClientSharp.RawFrames.Video;
using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using System;
using System.Linq;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding
{
    class H264VideoDecoder : IVideoDecoder
    {
        private const string LibraryName = "openh264-1.7.0-win32.dll";

        private readonly OpenH264Lib.Decoder _decoder;
        private readonly VideoCodecId _videoCodecId;


        private byte[] _extraData = new byte[0];
        private bool _disposed;

        private H264VideoDecoder(VideoCodecId videoCodecId, OpenH264Lib.Decoder decoder)
        {
            _videoCodecId = videoCodecId;
            _decoder = decoder;
        }

        ~H264VideoDecoder()
        {
            Dispose();
        }

        public static IVideoDecoder CreateDecoder(VideoCodecId videoCodecId)
        {
            var decoder = new OpenH264Lib.Decoder(LibraryName);

            if (decoder == null)
                throw new DecoderException(
                    $"An error occurred while creating video decoder for {videoCodecId} codec");

            return new H264VideoDecoder(videoCodecId, decoder);
        }

        public DecodedVideoFrame TryDecode(RawVideoFrame rawVideoFrame)
        {
            if (rawVideoFrame is RawH264IFrame)
            {
                var rawH264IFrame = (RawH264IFrame)rawVideoFrame;
                var spsPpsFrame = rawH264IFrame.SpsPpsSegment.Array.Skip(rawH264IFrame.SpsPpsSegment.Offset).Take(rawH264IFrame.SpsPpsSegment.Count).ToArray();
                _decoder.Decode(spsPpsFrame, spsPpsFrame.Length);
            }

            var frame = rawVideoFrame.FrameSegment.Array.Skip(rawVideoFrame.FrameSegment.Offset).Take(rawVideoFrame.FrameSegment.Count).ToArray();
            var image = _decoder.Decode(frame, frame.Length);

            if (image == null)
                return null;

            return new DecodedVideoFrame(image);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _decoder.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}