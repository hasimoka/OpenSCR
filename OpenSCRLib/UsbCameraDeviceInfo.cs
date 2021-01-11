using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace OpenSCRLib
{
    public class UsbCameraDeviceInfo
    {
        public string DevicePath { get; }

        public string Name { get; }

        public UsbCameraDeviceInfo(string devicePath, string name)
        {
            this.DevicePath = devicePath;
            this.Name = name;
        }
    }
}
