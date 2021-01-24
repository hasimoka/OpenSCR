using LiteDB;

namespace OpenSCRLib
{
    public class CameraSetting
    {
        [BsonCtor]
        public CameraSetting() { }

        [BsonCtor]
        public CameraSetting(ObjectId id, int cameraChannel, string cameraName, NetworkCameraSetting networkCameraSetting, UsbCameraSetting usbCameraSetting)
        {
            this.Id = id;

            this.CameraChannel = cameraChannel;

            this.CameraName = cameraName;

            this.NetowrkCameraSetting = networkCameraSetting;

            this.UsbCameraSetting = usbCameraSetting;
        }

        public ObjectId Id { get; set; }

        public int CameraChannel { get; set; }

        public string CameraName { get; set; }

        public NetworkCameraSetting NetowrkCameraSetting { get; set; }

        public UsbCameraSetting UsbCameraSetting { get; set; }

        public override string ToString()
        {
            return $"CameraSetting(Id={this.Id}, CameraChannel={this.CameraChannel}, CameraName={this.CameraName}, NetworkCameraSetting={this.NetowrkCameraSetting}, UsbCameraSetting={this.UsbCameraSetting}";
        }
    }
}
