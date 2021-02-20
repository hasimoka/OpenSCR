using RtspClientSharp.RawFrames.Video;
using OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames;
using System;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding
{
    class H264VideoDecoder : IVideoDecoder
    {
        private const string LibraryName = "openh264-1.7.0-win64.dll";

        private readonly OpenH264Lib.Decoder _decoder;
        private readonly VideoCodecId _videoCodecId;

        private H264FrameRecorder recorder;

        private byte[] _extraData = new byte[0];
        private bool _disposed;

        private H264VideoDecoder(VideoCodecId videoCodecId, OpenH264Lib.Decoder decoder)
        {
            _videoCodecId = videoCodecId;
            _decoder = decoder;

            this.recorder = null;
        }

        ~H264VideoDecoder()
        {
            this.Dispose();
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
            byte[] spsPpsFrame = null;
            if (rawVideoFrame is RawH264IFrame)
            {
                var rawH264IFrame = (RawH264IFrame)rawVideoFrame;
                spsPpsFrame = rawH264IFrame.SpsPpsSegment.Array.Skip(rawH264IFrame.SpsPpsSegment.Offset).Take(rawH264IFrame.SpsPpsSegment.Count).ToArray();
                _decoder.Decode(spsPpsFrame, spsPpsFrame.Length);
            }

            var frame = rawVideoFrame.FrameSegment.Array.Skip(rawVideoFrame.FrameSegment.Offset).Take(rawVideoFrame.FrameSegment.Count).ToArray();
            var frameImage = _decoder.Decode(frame, frame.Length);

            // DatetimeのTimezoneをJSTに変更する
            var jstZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            var jstTimestamp = TimeZoneInfo.ConvertTimeFromUtc(rawVideoFrame.Timestamp, jstZoneInfo);

            this.recorder.AddFrame(new RawH264FrameRecordItem(jstTimestamp, frame, spsPpsFrame));

            return new DecodedVideoFrame(frameImage);
        }

        public void SetOutputFolder(string outputFolder)
        {
            if (!string.IsNullOrEmpty(outputFolder))
            {
                if (this.recorder != null)
                {
                    this.recorder.Stop();
                    this.recorder = null;
                }

                this.recorder = new H264FrameRecorder();
                this.recorder.Start(outputFolder);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (recorder != null)
            {
                this.recorder.Stop();
                this.recorder = null;
            }

            _disposed = true;
            _decoder.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}