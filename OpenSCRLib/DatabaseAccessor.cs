using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSCRLib
{
    public class DatabaseAccessor
    {
        private const string DataBaseFileName = ".\\DB\\openSCR.db";

        private const string CameraSettingTable = "camera_settings";

        private const string RecordedFrameBaseFolderTable = "recorded_frame_base_folders";

        public DatabaseAccessor()
        {
            // DBファイルを格納するフォルダの存在チェック／初期作成
            if (!Directory.Exists(Path.GetDirectoryName(DataBaseFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(DataBaseFileName));
            }
        }

        public int GetNextCameraChannel()
        {
            var nextCameraChannel = 0;

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<CameraSetting>(CameraSettingTable);
                    var records = col.FindAll().ToArray();
                    if (records.Any())
                    {
                        var cameraChannels = records.OrderBy(x => x.CameraChannel).Select(x => x.CameraChannel).ToArray();
                        nextCameraChannel = cameraChannels.Where(x => !cameraChannels.Contains(x+1)).Select(x=>x+1).Min();
                    }
                    else
                    {
                        nextCameraChannel = 1;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return nextCameraChannel;
        }

        public List<CameraSetting> FindCameraSettings()
        {
            var cameraSettings = new List<CameraSetting>();

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<CameraSetting>(CameraSettingTable);
                    cameraSettings = col.FindAll().OrderBy(x => x.CameraChannel).ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return cameraSettings;
        }

        public CameraSetting FindCameraSetting(int cameraChannel)
        {
            CameraSetting cameraSetting = null;

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<CameraSetting>(CameraSettingTable);
                    cameraSetting = col.FindOne(x => x.CameraChannel == cameraChannel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return cameraSetting;
        }

        public bool UpdateOrInsertCameraSetting(CameraSetting cameraSetting)
        {
            var result = false;

            try 
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<CameraSetting>(CameraSettingTable);
                    result = col.Upsert(cameraSetting);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public bool DeleteCameraSetting(CameraSetting cameraSetting)
        {
            var result = false;

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<CameraSetting>(CameraSettingTable);
                    result = col.Delete(cameraSetting.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public bool UpdateOrInsertRecordedFrameBaseFolder(string baseFolder)
        {
            var result = false;

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var recordedFrameBaseFolder = new RecordedFrameBaseFolder {BaseFolder = baseFolder};

                    var col = liteDb.GetCollection<RecordedFrameBaseFolder>(RecordedFrameBaseFolderTable);
                    result = col.Upsert(recordedFrameBaseFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public string GetRecordedFrameBaseFolder()
        {
            var baseFolder = string.Empty;

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<RecordedFrameBaseFolder>(RecordedFrameBaseFolderTable);
                    var recordedFrameBaseFolder = col.FindOne(x => x.Id == RecordedFrameBaseFolder.FixedId);
                    if (recordedFrameBaseFolder != null)
                        baseFolder = recordedFrameBaseFolder.BaseFolder;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return baseFolder;
        }
    }
}
