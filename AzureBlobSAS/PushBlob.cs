using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AzureBlobSAS
{
    public class PushBlob
    {
        public static async Task UploadAsync(BlobContainerClient container , string localFilePath, bool overwrite = false, string blobName = null) 
        {
            // Get a connection string to our Azure Storage account.  You can
            // obtain your connection string from the Azure Portal (click
            // Access Keys under Settings in the Portal Storage account blade)
            // or using the Azure CLI with:
            //
            //     az storage account show-connection-string --name <account_name> --resource-group <resource_group>
            //
            // And you can provide the connection string to your application
            // using an environment variable.

            // Get a reference to a blob named "sample-file" in a container named "sample-container"

            //@blobName a path start from / e.g. /folder_A/folder_B/blob_x.txt

            if (null == blobName) 
            {
                blobName = Path.GetFileName(localFilePath);
            }

            BlobClient blob = container.GetBlobClient(blobName);

            // Upload local file
            await blob.UploadAsync(localFilePath, overwrite);
        }

        public static async Task UploadAsync(BlobContainerClient container, string localFilePath, BlobUploadOptions options, string blobName = null)
        {
            if (null == blobName)
            {
                blobName = Path.GetFileName(localFilePath);
            }

            BlobClient blob = container.GetBlobClient(blobName);

            // Upload local file
            await blob.UploadAsync(localFilePath, options);
        }

        public static async Task UploadAsync(BlobContainerClient container, BinaryData content, BlobUploadOptions options, string blobName)
        {
            BlobClient blob = container.GetBlobClient(blobName);

            // Upload local file
            await blob.UploadAsync(content, options);
        }

        public static async Task UploadAsync(BlobContainerClient container, Stream content, BlobUploadOptions options, string blobName)
        {
            BlobClient blob = container.GetBlobClient(blobName);

            // Upload local file
            await blob.UploadAsync(content, options);
        }
    }
}
