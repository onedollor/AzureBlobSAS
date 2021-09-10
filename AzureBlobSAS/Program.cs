using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobSAS
{
    class Program
    {
        static void Main(string[] args)
        {
            string sasUristring = ConfigurationManager.AppSettings.Get("SasUri");
            string localDownloadPath = @"G:\data\azure";

            Task dl = DownloadContainerTask(localDownloadPath, sasUristring);
            dl.Wait();
        }

        static async Task DownloadContainerTask(string localDownloadPath, string sasUristring)
        {
            Uri sasUri = new Uri(sasUristring);

            BlobContainerClient blobContainerClient = new BlobContainerClient(sasUri);

            Azure.Pageable<BlobItem> blobs = blobContainerClient.GetBlobs();

            foreach (BlobItem blob in blobs)
            {
                string fileName = blob.Name;
                BlobItemProperties prop = blob.Properties;
                string localFilePath = Path.Combine(localDownloadPath, fileName);

                if (fileName.IndexOf("/") >= 0) 
                {
                    string folder = fileName.Substring(0, fileName.LastIndexOf("/"));
                    string localFolder = Path.Combine(localDownloadPath, folder);
                    if (!Directory.Exists(localFolder)) 
                    {
                        Directory.CreateDirectory(localFolder);
                    }
                }

                if(!FileExist(localFilePath, BitConverter.ToString(prop.ContentHash).Replace("-", "").ToLowerInvariant()))
                {
                    using (var file = File.Open(localFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var blobClient = blobContainerClient.GetBlobClient(blob.Name);
                        await blobClient.DownloadToAsync(file);
                    }
                }
            }
        }

        static bool FileExist(string filename, string blobContentHash)
        {
            string md5 = CalculateMD5(filename);

            return md5.Equals(blobContentHash);
        }

        static string CalculateMD5(string filename)
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
