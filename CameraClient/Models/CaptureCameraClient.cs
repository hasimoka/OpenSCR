using OpenSCRLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using CameraClient.Models.NetworkCamera;
using CameraClient.Models.UsbCamera;
using HalationGhost;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace CameraClient.Models
{
    public class CaptureCameraClient: BindableModelBase, IDisposable
    {
        private const string NoImageFilePath = @".\Images\no_image_g.png";

        private readonly CameraSetting _cameraSettings;

        private readonly ICameraClient _cameraClient;

        private readonly RawH264Player _rawH264Player;

        public CaptureCameraClient(CameraSetting cameraSettings)
        {
            _cameraSettings = cameraSettings;

            if (_cameraSettings.NetworkCameraSettings != null)
            {
                _cameraClient = new NetworkCameraClient(cameraSettings);
            }
            else if (_cameraSettings.UsbCameraSettings != null)
            {
                _cameraClient = new UsbCameraClient(cameraSettings);
            }
            else
            {
                throw new ArgumentException("カメラ種類が設定されていないカメラ設定が引数に指定されました。");
            }

            FrameImage = _cameraClient.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposable);

            // NoImageの画像を読み込む
            using (var stream = new MemoryStream(File.ReadAllBytes(NoImageFilePath)))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                NoFrameImage = bitmap;
            }

            _rawH264Player = new RawH264Player(cameraSettings.RecordedFrameFolder);
            PlayerFrameImage = _rawH264Player.FrameImage
                .ToReactivePropertySlimAsSynchronized(x => x.Value)
                .AddTo(Disposable);

            _cameraClient.StartCapture(_cameraSettings);
        }

        public ReactivePropertySlim<BitmapSource> FrameImage { get; }

        public ReactivePropertySlim<BitmapSource> PlayerFrameImage { get; }

        public BitmapSource NoFrameImage { get; }

        public int CameraChannel => _cameraSettings?.CameraChannel ?? -1;

        public string CameraName => _cameraSettings?.CameraName ?? string.Empty;

        public new void Dispose()
        {
            _cameraSettings?.Dispose();
            _cameraClient?.Dispose();

            base.Dispose();
        }

        public void StartCapture()
        {
            _cameraClient?.StartCapture(_cameraSettings);
        }

        public void StopCapture()
        {
            _cameraClient?.StopCapture();
        }

        public void ClearFrameImage()
        {
            _cameraClient?.ClearFrameImage();
        }

        public LinkedList<(DateTime startFrameTime, DateTime endFrameTime)> SearchRecordedTerms(
            DateTime searchStartTime, DateTime searchEndTime)
        {
            return _rawH264Player.SearchRecordedTerms(searchStartTime, searchEndTime);
        }

        public void Play(DateTime playTime)
        {
            _rawH264Player.Play(playTime);
        }

        public void Stop()
        {
            _rawH264Player.Stop();
        }
    }
}
