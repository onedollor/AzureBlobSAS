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

            Task dl;


            //dl = PullBlob.DownloadBlobsFlatListing(container, localDownloadPath, DateTime.Now.AddMinutes(-30));
            //Console.WriteLine("s dl.Wait();");
            //dl.Wait();
            //Console.WriteLine("f dl.Wait();");

            //Console.ReadLine();

            //dl = PullBlob.DownloadBlobsFlatListing(container, localDownloadPath, DateTime.MinValue);
            //Console.WriteLine("s dl.Wait();");
            //dl.Wait();
            //Console.WriteLine("f dl.Wait();");

            //Console.ReadLine();

            dl = PullBlob.DownloadBlobsHierarchicalListing(container, localDownloadPath, DateTime.MinValue);
            Console.WriteLine("s dl.Wait();");
            dl.Wait();
            Console.WriteLine("f dl.Wait();");

            Console.ReadLine();

            dl = PullBlob.SyncBlobsByHierarchicalListing(container, localDownloadPath);
            Console.WriteLine("s dl.Wait();");
            dl.Wait();
            Console.WriteLine("f dl.Wait();");

            Console.ReadLine();


        }
    }
}
