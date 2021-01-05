using OpenSCRLib;
using System;
using System.ServiceModel.Discovery;
using System.Xml;

namespace OptionViews.Models
{
    public class WsDiscoveryClient : IDisposable
    {
        private DiscoveryClient discoveryClient;

        private object userState;

        private Action<CameraDeviceInfo> deviceDiscoveriedAction;

        public WsDiscoveryClient(Action<CameraDeviceInfo> deviceDiscoveriedAction)
        {
            this.deviceDiscoveriedAction = deviceDiscoveriedAction;

            var endPoint = new UdpDiscoveryEndpoint(DiscoveryVersion.WSDiscoveryApril2005);
            discoveryClient = new DiscoveryClient(endPoint);
            discoveryClient.FindProgressChanged += DiscoveryClient_FindProgressChanged;
            discoveryClient.FindCompleted += DiscoveryClient_FindCompleted;

            this.userState = null;
        }

        public void Dispose()
        {
            if (this.userState != null)
            {
                this.discoveryClient.CancelAsync(this.userState);
                this.userState = null;
            }
        }

        public void FindNetworkVideoTransmitterAsync()
        {
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
                    var deviceInfo = new CameraDeviceInfo();
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

                    this.deviceDiscoveriedAction(deviceInfo);
                }
            }
        }

        private void DiscoveryClient_FindCompleted(object sender, FindCompletedEventArgs e)
        {
            if (this.userState != null)
                this.userState = null;
        }
    }
}
