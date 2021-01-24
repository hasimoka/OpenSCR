using System;

namespace OpenSCRLib
{
    public class NetworkCameraEndpoint
    {
        public string DeviceName { get; set; }

        public Uri EndpointUri { get; set; }

        public string Location { get; set; }

        public override string ToString()
        {
            return $"NetworkCameraEndpoint(DeviceName={this.DeviceName}, EndpointUri={this.EndpointUri}, Location={this.Location})";
        }
    }
}
