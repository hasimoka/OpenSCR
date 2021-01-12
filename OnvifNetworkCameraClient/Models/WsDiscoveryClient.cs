using OpenSCRLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Discovery;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OnvifNetworkCameraClient.Models
{
    public class WsDiscoveryClient : IWsDiscoveryClient
    {
        private DiscoveryClient discoveryClient;

        private object userState;

        private Action<IpCameraDeviceInfo> deviceDiscoveriedAction;

        public WsDiscoveryClient()
        {

            var endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
            this.discoveryClient = new DiscoveryClient(endPoint);
            this.discoveryClient.FindProgressChanged += DiscoveryClient_FindProgressChanged;
            this.discoveryClient.FindCompleted += DiscoveryClient_FindCompleted;

            this.userState = null;

            this.deviceDiscoveriedAction = null;
        }

        public void Dispose()
        {
            if (this.userState != null)
            {
                this.discoveryClient.CancelAsync(this.userState);
                this.userState = null;
            }
        }

        public void FindNetworkVideoTransmitterAsync(Action<IpCameraDeviceInfo> deviceDiscoveriedAction)
        {
            this.deviceDiscoveriedAction = deviceDiscoveriedAction;

            var endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
            this.discoveryClient = new DiscoveryClient(endPoint);
            this.discoveryClient.FindProgressChanged += DiscoveryClient_FindProgressChanged;
            this.discoveryClient.FindCompleted += DiscoveryClient_FindCompleted;

            FindCriteria findCriteria = new FindCriteria();
            findCriteria.Duration = TimeSpan.MaxValue;
            findCriteria.MaxResults = int.MaxValue;
            // Edit: optionally specify contract type, ONVIF v1.0
            findCriteria.ContractTypeNames.Add(
                new XmlQualifiedName(
                    "NetworkVideoTransmitter",
                    "http://www.onvif.org/ver10/network/wsdl"));

            if (this.userState != null)
            {
                this.discoveryClient.CancelAsync(this.userState);
            }

            this.userState = new object();
            this.discoveryClient.FindAsync(findCriteria, this.userState);
        }

        private void DiscoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            foreach (var uri in e.EndpointDiscoveryMetadata.ListenUris)
            {
                if (uri.HostNameType == UriHostNameType.IPv4)
                {
                    var deviceInfo = new IpCameraDeviceInfo();
                    deviceInfo.EndpointUri = uri;

                    foreach (var scope in e.EndpointDiscoveryMetadata.Scopes)
                    {
                        if (scope.Segments[1].Replace("/", string.Empty).ToLower() == "name")
                        {
                            deviceInfo.Name = scope.Segments[2].Replace("/", string.Empty);
                        }
                        else if (scope.Segments[1].Replace("/", string.Empty).ToLower() == "location")
                        {
                            deviceInfo.Location = scope.Segments[2].Replace("/", string.Empty);
                        }
                    }

                    if (this.deviceDiscoveriedAction != null)
                        this.deviceDiscoveriedAction(deviceInfo);
                }
            }
        }

        private void DiscoveryClient_FindCompleted(object sender, FindCompletedEventArgs e)
        {
            this.discoveryClient.Close();
            this.discoveryClient = null;

            if (this.userState != null)
                this.userState = null;
        }
    }
}
