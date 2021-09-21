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

            Uri sasUri = new Uri(sasUristring);

            BlobContainerClient container = new BlobContainerClient(sasUri);

            int a;


            /* or 
            string containerUri = "https://{accountname}.blob.core.windows.net/{container}";
            string signature = "sp=racwdl&amp;st=2021-09-01T12:18:54Z&amp;se=2021-09-30T20:18:54Z&amp;spr=https&amp;sv=2020-08-04&amp;sr=c&amp;sig=xxxxxxxxxxxxxxxxxxxxxxxxxxxx";

            BlobContainerClient container = new BlobContainerClient(containerUri, signature);
            */

            Task task;
            
            string localFilePath = @"G:\data\AAPL.csv";
            string blobName = @"/data/stock/AAPL.csv";
            bool overwrite = true;

            task = PushBlob.UploadAsync(container, localFilePath, new BlobUploadOptions(), blobName, overwrite);
            Console.WriteLine("s task.Wait();");
            task.Wait();
            Console.WriteLine("f task.Wait();");
            Console.ReadLine();

            //task = PullBlob.DownloadBlobsFlatListing(container, localDownloadPath, DateTime.Now.AddMinutes(-30));
            //Console.WriteLine("s task.Wait();");
            //task.Wait();
            //Console.WriteLine("f task.Wait();");

            //Console.ReadLine();

            //task = PullBlob.DownloadBlobsFlatListing(container, localDownloadPath, DateTime.MinValue);
            //Console.WriteLine("s task.Wait();");
            //task.Wait();
            //Console.WriteLine("f task.Wait();");

            //Console.ReadLine();

            task = PullBlob.DownloadBlobsHierarchicalListing(container, localDownloadPath, DateTime.MinValue);
            Console.WriteLine("s task.Wait();");
            task.Wait();
            Console.WriteLine("f task.Wait();");

            Console.ReadLine();

            task = PullBlob.SyncBlobsByHierarchicalListing(container, localDownloadPath, DateTime.MinValue);
            Console.WriteLine("s task.Wait();");
            task.Wait();
            Console.WriteLine("f task.Wait();");

            Console.ReadLine();


        }
    }
}
