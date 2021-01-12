using RtspClientSharp.RawFrames;
using RtspClientSharp.RawFrames.Video;
using OnvifNetworkCameraClient.Models.RawFramesDecoding;
using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using OnvifNetworkCameraClient.Models.RawFramesReceiving;
using System;
using System.Collections.Generic;

namespace OnvifNetworkCameraClient.Models
{
    class RealtimeVideoSource : IVideoSource, IDisposable
    {
        private IRawFramesSource _rawFramesSource;

        private readonly Dictionary<VideoCodecId, IVideoDecoder> _videoDecodersMap =
            new Dictionary<VideoCodecId, IVideoDecoder>();

        public event EventHandler<DecodedVideoFrame> FrameDecoded;

        public void SetRawFramesSource(IRawFramesSource rawFramesSource)
        {
            if (_rawFramesSource != null)
            {
                _rawFramesSource.FrameReceived -= OnFrameReceived;
                DropAllVideoDecoders();
            }

            _rawFramesSource = rawFramesSource;

            if (rawFramesSource == null)
                return;

            rawFramesSource.FrameReceived += OnFrameReceived;
        }

        public void Dispose()
        {
            DropAllVideoDecoders();
        }

        private void DropAllVideoDecoders()
        {
            foreach (IVideoDecoder decoder in _videoDecodersMap.Values)
                decoder.Dispose();

            _videoDecodersMap.Clear();
        }

        private void OnFrameReceived(object sender, RawFrame rawFrame)
        {
            if (!(rawFrame is RawVideoFrame rawVideoFrame))
                return;

            // Rawフレーム -> Bitmapへ変換する
            IVideoDecoder decoder = GetDecoderForFrame(rawVideoFrame);

            var decodedFrame = decoder.TryDecode(rawVideoFrame);

            if (decodedFrame != null)
            {
                FrameDecoded?.Invoke(this, decodedFrame);
            }
        }

        private IVideoDecoder GetDecoderForFrame(RawVideoFrame videoFrame)
        {
            VideoCodecId codecId = DetectCodecId(videoFrame);
            if (!_videoDecodersMap.TryGetValue(codecId, out IVideoDecoder decoder))
            {
                var factory = VideoDecoderFactory.getFactory(codecId);
                if (factory != null)
                    decoder = factory.CreateVideoDecoder();

                if (decoder != null)
                    _videoDecodersMap.Add(codecId, decoder);
            }

            return decoder;
        }

        private VideoCodecId DetectCodecId(RawVideoFrame videoFrame)
        {
            if (videoFrame is RawJpegFrame)
                return VideoCodecId.MJPEG;
            if (videoFrame is RawH264Frame)
                return VideoCodecId.H264;

            throw new ArgumentOutOfRangeException(nameof(videoFrame));
        }
    }
}