using System;
using System.IO;
using UnityEngine;

namespace Bakery.Translate
{
    public static class FileManager
    {
        public static void Create(string fileName, string path, string content)
        {
            string filePath = Path.Combine(path, fileName);

            File.WriteAllText(filePath, content);

            Debug.Log($"File created at: {filePath}");
        }

        public static bool OpenFile(string fileName, string path, out string raw)
        {
            try
            {
                string filePath = Path.Combine(path, fileName);
                raw = File.ReadAllText(filePath);
                return true;
            }
            catch (Exception e)
            {
                raw = string.Empty;

                if (e is FileNotFoundException)
                    return false;

                Debug.LogError("OpenFile:" + e);
            }
            return false;
        }
    }
}
