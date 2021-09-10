using System;
using System.Threading.Tasks;

using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AzureBlobSAS
{
    public class PullBlob
    {
        public static async Task DownloadContainerTask(string localDownloadPath, string sasUristring)
        {
            Uri sasUri = new Uri(sasUristring);

            BlobContainerClient blobContainerClient = new BlobContainerClient(sasUri);

            await foreach (BlobItem blob in blobContainerClient.GetBlobsAsync())
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

                if (!FileChecker.CheckFileExist(localFilePath, BitConverter.ToString(prop.ContentHash).Replace("-", "").ToLowerInvariant()))
                {
                    using (var file = File.Open(localFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var blobClient = blobContainerClient.GetBlobClient(blob.Name);
                        await blobClient.DownloadToAsync(file);
                    }
                }
            }
        }
    }
}
