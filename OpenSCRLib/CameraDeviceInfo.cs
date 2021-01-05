using System;
using System.Net;

namespace OpenSCRLib
{
    public class CameraDeviceInfo
    {
        public Uri EndpointUri { get; set; }

        public string Name { get; set; }

        public string Location { get; set; }

        public string Address
        {
            get
            {
                return this.EndpointUri == null ? string.Empty : Dns.GetHostAddresses(this.EndpointUri.Host)[0].ToString(); ;
            }
        }

        public CameraDeviceInfo()
        {
            this.EndpointUri = null;
            this.Name = string.Empty;
            this.Location = string.Empty;
        }
    }
}