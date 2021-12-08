using System;
using System.IO;
using VideoDown.Model;

namespace VideoDown.Utility
{
    public static class PathHelper
    {
        public static HttpReturnData Save(string saveDirName, string title, string id)
        {
            var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Video/{saveDirName}");
            var path = SavePathProgress($"{saveDirName}-{title}-{id}.mp4").Replace(" ", "");

            var saveFile = Path.Combine(saveDir, path);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            if (!File.Exists(saveFile))
            {
                return new HttpReturnData { Success = true, Message = saveFile };
            }
            return new HttpReturnData { Success = false, Message = "文件重复" };
        }

        public static HttpReturnData Save1(string saveDirName, string title, string id)
        {
            var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"Video/{saveDirName}");
            var path = SavePathProgress($"{saveDirName}-{title}-{id}.mp3").Replace(" ", "");

            var saveFile = Path.Combine(saveDir, path);
            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }
            if (!File.Exists(saveFile))
            {
                return new HttpReturnData { Success = true, Message = saveFile };
            }
            return new HttpReturnData { Success = false, Message = "文件重复" };
        }

        private static string SavePathProgress(string saveFile)
        {
            return saveFile.Replace("\\", "").Replace("/", "").Replace(":", "").Replace("?", "").Replace("\"", "").Replace("<", "").Replace(">", "").Replace("|", "").Replace("*", "").Trim().Replace(" ", "");
        }

        #region 打开目录

        public static void Open()
        {
            var saveDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video");

            if (!Directory.Exists(saveDir))
            {
                Directory.CreateDirectory(saveDir);
            }

            System.Diagnostics.Process.Start(saveDir);
        }

        #endregion 打开目录
    }
}