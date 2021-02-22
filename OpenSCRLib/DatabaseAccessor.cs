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
                    var records = col.FindAll();
                    if (records.Count() > 0)
                    {
                        nextCameraChannel = records.Max(x => x.CameraChannel);
                        nextCameraChannel += 1;
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
            List<CameraSetting> cameraSettings = new List<CameraSetting>();

            try
            {
                using (var liteDb = new LiteDatabase($"Filename={DataBaseFileName}; Mode=Exclusive;"))
                {
                    var col = liteDb.GetCollection<CameraSetting>(CameraSettingTable);
                    cameraSettings = col.FindAll().ToList();
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
    }
}
