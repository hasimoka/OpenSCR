using LiteDB;

namespace OpenSCRLib
{
    public class UsbCameraSetting
    {
        [BsonCtor]
        public UsbCameraSetting() { }

        [BsonCtor]
        public UsbCameraSetting(string devicePath, int cameraHeight, int cameraWidth, int frameRate)
        {
            this.DevicePath = devicePath;

            this.CameraHeight = cameraHeight;

            this.CameraWidth = cameraWidth;

            this.FrameRate = frameRate;
        }

        public string DevicePath { get; set; }

        public int CameraHeight { get; set; }

        public int CameraWidth { get; set; }

        public int FrameRate { get; set; }
    }
}
