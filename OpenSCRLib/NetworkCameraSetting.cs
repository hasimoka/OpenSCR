using LiteDB;

namespace OpenSCRLib
{
    public class NetworkCameraSetting
    {
        [BsonCtor]
        public NetworkCameraSetting() { }

        [BsonCtor]
        public NetworkCameraSetting(string userName, string password, bool isLoggedIn, string ipAddress, string encoding, string productName, string profileToken, string streamUri, int cameraHeight, int cameraWidth)
        {
            this.UserName = userName;

            this.Password = password;

            this.IsLoggedIn = isLoggedIn;

            this.IpAddress = ipAddress;

            this.Encoding = encoding;

            this.ProductName = productName;

            this.ProfileToken = profileToken;

            this.StreamUri = streamUri;

            this.CameraHeight = cameraHeight;

            this.CameraWidth = cameraWidth;
        }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool IsLoggedIn { get; set; }

        public string IpAddress { get; set; }

        public string Encoding { get; set; }

        public string ProductName { get; set; }

        public string ProfileToken { get; set; }

        public string StreamUri { get; set; }

        public int CameraHeight { get; set; }

        public int CameraWidth { get; set; }

        public override string ToString()
        {
            return $"NetworkCameraSetting(UserName={this.UserName}, Password={this.Password}, IsLoggedIn={this.IsLoggedIn}, IpAddress={this.IpAddress}, Encoding={this.Encoding}, ProductName={this.ProductName}, ProfileToken={this.ProfileToken}, StreamUri={this.StreamUri}, CameraHeight={this.CameraHeight}, CameraWidth={this.CameraWidth})";
        }
    }
}
