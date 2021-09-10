using System;
using System.Threading.Tasks;

using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;

namespace AzureBlobSAS
{
    public class PullBlob
    {
        public static async Task DownloadBlobsFlatListing(BlobContainerClient container, string localDownloadPath, DateTime lastModifiedDate, int segmentSize=32, bool skipDeleted=true)
        {
            if (null == lastModifiedDate) lastModifiedDate = DateTime.MinValue;

            try 
            {
                // Call the listing operation and return pages of the specified size.
                var resultSegment = container.GetBlobsAsync()
                    .AsPages(default, segmentSize);

                // Enumerate the blobs returned for each page.
                await foreach (Azure.Page<BlobItem> blobPage in resultSegment)
                {
                    foreach (BlobItem blob in blobPage.Values)
                    {
                        if (skipDeleted && !blob.Deleted)
                        {
                            string fileName = blob.Name;
                            BlobItemProperties prop = blob.Properties;
                            string filePath = Path.Combine(localDownloadPath, fileName);

                            if (fileName.IndexOf("/") >= 0)
                            {
                                string folder = fileName.Substring(0, fileName.LastIndexOf("/"));
                                string localFolder = Path.Combine(localDownloadPath, folder);
                                if (!Directory.Exists(localFolder))
                                {
                                    Directory.CreateDirectory(localFolder);
                                }
                                
                                Directory.SetCreationTimeUtc(localFolder, DateTime.UtcNow);
                            }

                            if (blob.Properties.LastModified >= lastModifiedDate) 
                            {
                                await PullBlob.DownloadBlob(container, blob, filePath);
                            }
                        }
                    }
                }
            }
            catch (Azure.RequestFailedException e)
            {
                throw e;
            }
        }

        public static async Task DownloadBlobsHierarchicalListing(BlobContainerClient container, string localDownloadPath, DateTime lastModifiedDate, string prefix = null, int? segmentSize = 32, bool skipDeleted = true)
        {
            if (null == lastModifiedDate) lastModifiedDate = DateTime.MinValue;

            try
            {
                if (prefix == "/") 
                {
                    prefix = null;
                }

                // Call the listing operation and return pages of the specified size.
                var resultSegment = container.GetBlobsByHierarchyAsync(prefix: prefix, delimiter: "/")
                    .AsPages(default, segmentSize);

                // Enumerate the blobs returned for each page.
                await foreach (Azure.Page<BlobHierarchyItem> blobPage in resultSegment)
                {
                    // A hierarchical listing may return both virtual directories and blobs.
                    foreach (BlobHierarchyItem blobhierarchyItem in blobPage.Values)
                    {
                        if (blobhierarchyItem.IsPrefix)
                        {
                            string localFolder = Path.Combine(localDownloadPath, blobhierarchyItem.Prefix);

                            if (!Directory.Exists(localFolder))
                            {
                                Directory.CreateDirectory(localFolder);
                            }

                            Directory.SetCreationTimeUtc(localFolder, DateTime.UtcNow);

                            // Call recursively with the prefix to traverse the virtual directory.
                            await DownloadBlobsHierarchicalListing(container, localDownloadPath, lastModifiedDate, blobhierarchyItem.Prefix, segmentSize);
                        }
                        else
                        {
                            // Download the blob.
                            string filePath = Path.Combine(localDownloadPath, blobhierarchyItem.Blob.Name);

                            if (skipDeleted && !blobhierarchyItem.Blob.Deleted)
                            {
                                if (blobhierarchyItem.Blob.Properties.LastModified >= lastModifiedDate)
                                {
                                    await PullBlob.DownloadBlob(container, blobhierarchyItem.Blob, filePath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Azure.RequestFailedException e)
            {
                throw e;
            }
        }

        public static async Task SyncBlobsByHierarchicalListing(BlobContainerClient container, string localDownloadPath, string prefix = null, int? segmentSize = 32)
        {
            DateTime lastAccessTimeUtc = DateTime.UtcNow;

            await PullBlob.DownloadBlobsHierarchicalListing(container, localDownloadPath, DateTime.MinValue, prefix, segmentSize, true);

            PullBlob.ClearLocalFiles(localDownloadPath, lastAccessTimeUtc);
        }

        private static void ClearLocalFiles(string localDownloadPath, DateTime lastAccessTimeUtc) 
        {
            string[] files = Directory.GetFiles(localDownloadPath, "*.*", SearchOption.AllDirectories);

            foreach (string filePath in files) 
            {
                if (File.GetLastAccessTimeUtc(filePath) < lastAccessTimeUtc) 
                {
                    File.Delete(filePath);
                }
            }


            string[] dirs = Directory.GetDirectories(localDownloadPath, "*", SearchOption.AllDirectories);

            foreach (string dir in dirs)
            {
                IEnumerable<string> items = Directory.EnumerateFileSystemEntries(dir);

                using (IEnumerator<string> item = items.GetEnumerator())
                {
                    if (!item.MoveNext())
                    {
                        Directory.Delete(dir);
                    }
                }
            }
        }

        private static async Task DownloadBlob(BlobContainerClient container, BlobItem blob, string filePath, bool checkExist=true)
        {
            if (checkExist && !FileChecker.CheckFileExist(filePath, BitConverter.ToString(blob.Properties.ContentHash).Replace("-", "").ToLowerInvariant()))
            {
                using (var file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var blobClient = container.GetBlobClient(blob.Name);
                    await blobClient.DownloadToAsync(file);
                }
            }

            File.SetLastAccessTimeUtc(filePath, DateTime.UtcNow);
        }
    }
}
