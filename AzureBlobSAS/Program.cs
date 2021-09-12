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

            Task task;

            task = PushBlob.UploadAsync(container, @"G:\data\AAPL.csv", false, @"/data/stock/AAPL.csv");
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
