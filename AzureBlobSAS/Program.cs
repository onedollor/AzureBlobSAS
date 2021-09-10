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

            Task dl = PullBlob.DownloadContainerTask(localDownloadPath, sasUristring);
            dl.Wait();
        }
    }
}
