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
            Task dl = dl_task();
            dl.Wait();
        }

        static async Task dl_task()
        {
            string uriString = "";
            string localDownloadPath = @"G:\data\azure";

            Uri sasUri = new Uri(uriString);

            BlobContainerClient blobContainerClient = new BlobContainerClient(sasUri);

            foreach (BlobItem blob in blobContainerClient.GetBlobs())
            {
                string fileName = blob.Name;
                BlobItemProperties prop = blob.Properties;
                string localFilePath = Path.Combine(localDownloadPath, fileName);

                if(download_track(localDownloadPath, fileName, prop.ContentHash)) 
                {
                    using (var file = File.Open(localFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                    {
                        var blobClient = blobContainerClient.GetBlobClient(blob.Name);
                        await blobClient.DownloadToAsync(file);

                    }
                }
            }
        }

        static bool download_track(string dl_path, string filename, byte[] hash_code)
        { 

        }

    }
}
