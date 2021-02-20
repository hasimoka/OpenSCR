using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnvifNetworkCameraClient.Models
{
    public class H264FrameParser
    {
        public static int GetNalUnitCount(ArraySegment<byte> buffer)
        {
            var nalUnitCount = 0;

            var startPos = 0;
            do
            {
                int nalStart, nalEnd;
                var findTargetSpan = new ArraySegment<byte>(buffer.ToArray(), startPos, buffer.Count - startPos);
                var size = FindNalUnit(findTargetSpan, out nalStart, out nalEnd);
                if (size != 0)
                {
                    nalUnitCount += 1;
                    startPos += nalEnd;
                }
                else
                {
                    break;
                }
            } while (startPos < buffer.Count);

            return nalUnitCount;
        }

        public static int FindNalUnit(ArraySegment<byte> buffer, out int nalStart, out int nalEnd)
        {
            // find start
            nalStart = 0;
            nalEnd = 0;

            if (buffer.Count == 0)
                return 0;

            int i = 0;
            while ( // ( next_bits( 24 ) != 0x000001 && next_bits( 32 ) != 0x00000001 )
                (buffer.Array[i] != 0 || buffer.Array[i + 1] != 0 || buffer.Array[i + 2] != 0x01) &&
                (buffer.Array[i] != 0 || buffer.Array[i + 1] != 0 || buffer.Array[i + 2] != 0 || buffer.Array[i + 3] != 0x01))
            {
                i++; // skip leading zero
                if (i + 4 >= buffer.Count) { return 0; } // did not find nal start
            }

            if (buffer.Array[i] != 0 || buffer.Array[i + 1] != 0 || buffer.Array[i + 2] != 0x01) // ( next_bits( 24 ) != 0x000001 )
            {
                i++;
            }

            if (buffer.Array[i] != 0 || buffer.Array[i + 1] != 0 || buffer.Array[i + 2] != 0x01) { /* error, should never happen */ return 0; }
            i += 3;

            nalStart = i;

            while (   //( next_bits( 24 ) != 0x000000 && next_bits( 24 ) != 0x000001 )
                (buffer.Array[i] != 0 || buffer.Array[i + 1] != 0 || buffer.Array[i + 2] != 0) &&
                (buffer.Array[i] != 0 || buffer.Array[i + 1] != 0 || buffer.Array[i + 2] != 0x01)
                )
            {
                i++;
                // FIXME the next line fails when reading a nal that ends exactly at the end of the data
                if (i + 3 >= buffer.Count)
                {
                    nalEnd = buffer.Count;
                    return -1;
                } // did not find nal end, stream ended first
            }

            nalEnd = i;

            return (nalEnd - nalStart);
        }

    }
}
