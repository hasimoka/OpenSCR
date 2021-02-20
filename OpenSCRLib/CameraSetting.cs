using LiteDB;
using System;

namespace OpenSCRLib
{
    public class CameraSetting : IDisposable
    {
        [BsonCtor]
        public CameraSetting() { }

        [BsonCtor]
        public CameraSetting(ObjectId id, int cameraChannel, string cameraName, NetworkCameraSetting networkCameraSetting, UsbCameraSetting usbCameraSetting)
        {
            this.Id = id;

            this.CameraChannel = cameraChannel;

            this.CameraName = cameraName;

            this.NetworkCameraSetting = networkCameraSetting;

            this.UsbCameraSetting = usbCameraSetting;
        }

        public ObjectId Id { get; set; }

        public int CameraChannel { get; set; }

        public string CameraName { get; set; }

        public NetworkCameraSetting NetworkCameraSetting { get; set; }

        public UsbCameraSetting UsbCameraSetting { get; set; }

        public void Dispose() { }

        public override string ToString()
        {
            return $"CameraSetting(Id={this.Id}, CameraChannel={this.CameraChannel}, CameraName={this.CameraName}, NetworkCameraSetting={this.NetworkCameraSetting}, UsbCameraSetting={this.UsbCameraSetting}";
        }
    }
}
