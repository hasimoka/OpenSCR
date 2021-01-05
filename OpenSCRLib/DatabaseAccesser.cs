using LiteDB;
using System;
using System.IO;

namespace OpenSCRLib
{
    public class DatabaseAccesser
    {
        private const string FILE_NAME = ".\\DB\\openSCR.db";

        private const string CAMERA_LOGIN_INFO_TABLE = "camera_login_information";

        private const int CAMERA_LOGIN_INFO_ID = 1;

        public DatabaseAccesser()
        {
            // DBファイルを格納するフォルダの存在チェック／初期作成
            if (!Directory.Exists(Path.GetDirectoryName(FILE_NAME)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FILE_NAME));
            }

            //// DBファイルの存在チェック／初期作成
            //if (!File.Exists(FILE_NAME))
            //{
            //    File.Create(FILE_NAME).Close();
            //}
        }

        public CameraLoginInfo GetCameraLoginInfo()
        {
            CameraLoginInfo result = null;

            try
            {
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraLoginInfo>(CAMERA_LOGIN_INFO_TABLE);
                    var record = col.FindOne(x => x.Id == CAMERA_LOGIN_INFO_ID);

                    result = record;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public void SetCameraLoginInfo(CameraLoginInfo cameraLoginInfo)
        {
            try
            {
                using (var litedb = new LiteDatabase($"Filename={FILE_NAME}; Mode=Exclusive;"))
                {
                    var col = litedb.GetCollection<CameraLoginInfo>(CAMERA_LOGIN_INFO_TABLE);
                    var record = col.FindOne(x => x.Id == CAMERA_LOGIN_INFO_ID);

                    if (record == null)
                    {
                        col.Insert(cameraLoginInfo);
                    }
                    else
                    {
                        record.Name = cameraLoginInfo.Name;
                        record.Password = cameraLoginInfo.Password;

                        col.Update(record);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
