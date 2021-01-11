using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSCRLib
{
    public class UsbCameraVideoInfo
    {
        public UsbCameraVideoInfo(int width, int height, short bitCount)
        {
            this.Width = width;
            this.Height = height;
            this.BitCount = bitCount;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public short BitCount { get; set; }

        public override string ToString()
        {
            return $"Width: {this.Width}, Height: {this.Height}, BitCount: {this.BitCount}";
        }
    }
}
