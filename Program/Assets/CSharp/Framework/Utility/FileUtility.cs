using System;
using System.Collections.Generic;
using System.IO;

namespace lhFramework.Infrastructure.Utility
{
    public class FileUtility
    {
        public static void CopyEntireDir(string sourcePath, string destPath)
        {
            try
            {
                //Now Create all of the directories
                foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                   SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(sourcePath, destPath));

                //Copy all the files & Replaces any files with the same name
                foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                   SearchOption.AllDirectories))
                    File.Copy(newPath, newPath.Replace(sourcePath, destPath), true);
            }
            catch(Exception exc)
            {
                UnityEngine.Debug.Log(exc);
            }
        }
    }
}
