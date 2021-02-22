using LiteDB;
using System;

namespace OpenSCRLib
{
    public class CameraSetting : IDisposable
    {
        [BsonCtor]
        public CameraSetting() { }

        [BsonCtor]
        public CameraSetting(ObjectId id, int cameraChannel, string cameraName, NetworkCameraSetting networkCameraSettings, UsbCameraSetting usbCameraSettings)
        {
            this.Id = id;

            this.CameraChannel = cameraChannel;

            this.CameraName = cameraName;

            this.NetworkCameraSettings = networkCameraSettings;

            this.UsbCameraSettings = usbCameraSettings;
        }

        public ObjectId Id { get; set; }

        public int CameraChannel { get; set; }

        public string CameraName { get; set; }

        public NetworkCameraSetting NetworkCameraSettings { get; set; }

        public UsbCameraSetting UsbCameraSettings { get; set; }

        public void Dispose() { }

        public override string ToString()
        {
            return $"CameraSetting(Id={this.Id}, CameraChannel={this.CameraChannel}, CameraName={this.CameraName}, NetworkCameraSettings={this.NetworkCameraSettings}, UsbCameraSettings={this.UsbCameraSettings}";
        }
    }
}
