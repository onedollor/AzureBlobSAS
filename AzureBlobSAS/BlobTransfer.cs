using System;
using System.IO;

namespace AzureBlobSAS
{
    public class FileChecker
    {
        public static bool CheckFileExist(string filename, string blobContentHash)
        {
            if (File.Exists(filename))
            {
                string md5 = CalculateMD5(filename);
                return md5.Equals(blobContentHash);
            }
            else 
            {
                return false;
            }
        }

        public static string CalculateMD5(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
