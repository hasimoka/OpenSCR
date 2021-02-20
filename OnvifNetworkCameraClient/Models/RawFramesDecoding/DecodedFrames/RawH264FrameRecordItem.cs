using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnvifNetworkCameraClient.Models.RawFramesDecoding.DecodedFrames
{
    public class RawH264FrameRecordItem
    {
        public RawH264FrameRecordItem(DateTime timestamp, byte[] frameSegment, byte[] spsPpsSegment)
        {
            this.Timestamp = timestamp;

            if (frameSegment != null)
                this.FrameSegment = new ArraySegment<byte>(frameSegment);
            else
                this.FrameSegment = new ArraySegment<byte>();

            if (spsPpsSegment != null)
                this.SpsPpsSegment = new ArraySegment<byte>(spsPpsSegment);
            else
                this.SpsPpsSegment = new ArraySegment<byte>();

        }

        public DateTime Timestamp { get; }

        public ArraySegment<byte> FrameSegment { get; }

        public ArraySegment<byte> SpsPpsSegment { get; }
    }
}
