using LiteDB;
using System;
using System.IO;

namespace OpenSCRLib
{
    public class CameraSetting : IDisposable
    {
        [BsonCtor]
        public CameraSetting() { }

        [BsonCtor]
        public CameraSetting(ObjectId id, int cameraChannel, string cameraName, string recordedFrameFolder, NetworkCameraSetting networkCameraSettings, UsbCameraSetting usbCameraSettings)
        {
            Id = id;

            CameraChannel = cameraChannel;

            CameraName = cameraName;

            RecordedFrameFolder = recordedFrameFolder;

            NetworkCameraSettings = networkCameraSettings;

            UsbCameraSettings = usbCameraSettings;
        }

        public ObjectId Id { get; set; }

        public int CameraChannel { get; set; }

        public string CameraName { get; set; }

        public string RecordedFrameFolder { get; set; }

        public NetworkCameraSetting NetworkCameraSettings { get; set; }

        public UsbCameraSetting UsbCameraSettings { get; set; }

        public static string GetRecordedFrameFolder(string baseFolder, int cameraChannel)
        {
            if (string.IsNullOrEmpty(baseFolder))
                baseFolder = @".\Movies";

            return Path.Combine(baseFolder, $"{cameraChannel:00}");
        }

        public void Dispose() { }

        public override string ToString()
        {
            return $"CameraSetting(Id={Id}, CameraChannel={CameraChannel}, CameraName={CameraName}, RecordedFrameFolder={RecordedFrameFolder} NetworkCameraSettings={NetworkCameraSettings}, UsbCameraSettings={UsbCameraSettings})";
        }
    }
}
