using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenSCRLib
{
    public class DatabaseAccesser
    {
        private const string FILE_NAME = ".\\DB\\openSCR.db";

        private const string CAMERA_SETTING_TABLE = "camera_settings";

        public DatabaseAccesser()
        {
            // DBファイルを格納するフォルダの存在チェック／初期作成
            if (!Directory.Exists(Path.GetDirectoryName(FILE_NAME)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FILE_NAME));
            }
        }

        public int GetNextCameraChannel()
        {
            var nextCameraChannel = 0;

            try
            {
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraSetting>(CAMERA_SETTING_TABLE);
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
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraSetting>(CAMERA_SETTING_TABLE);
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
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraSetting>(CAMERA_SETTING_TABLE);
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
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraSetting>(CAMERA_SETTING_TABLE);
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
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraSetting>(CAMERA_SETTING_TABLE);
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
