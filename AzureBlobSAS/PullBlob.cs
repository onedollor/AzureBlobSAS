using System;
using System.Threading.Tasks;

using System.IO;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Collections.Generic;
using Azure;

namespace AzureBlobSAS
{
    public class PullBlob
    {
        public static int MAX_RERUN_COUNT { get; set; } = 3;
        public static long BLOCK_SIZE { get; set; } = 1024 * 1024 * 100;
        public static long MAX_SINGLE_FILE_SIZE { get; set; } = (long)5000 * (long)1000000000;

        //
        // Summary:
        //     Download all blobs from a container by flatlisting blobs.
        //
        // Parameters:
        //   container:
        //     A Azure.Storage.Blobs.BlobContainerClient referencing the block blob that
        //     includes the name of the account, the name of the container, and the name of
        //     the blob.
        //
        //   localDownloadPath:
        //     local folder for store downloaded blobs(files)
        //
        //   lastModifiedDate:
        //     filter blob by blob lastModifiedDate >= this parameter before download.
        //
        //   segmentSize:
        //     The size of Azure.Page`1s that should be requested (from service operations that
        //     support it).
        //
        //   skipDeleted:
        //     do not download blob if Deleted is true
        //
        // Returns:
        //     a task.
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

        //
        // Summary:
        //     Download all blobs from a container by hierarchical blobs.
        //
        // Parameters:
        //   container:
        //     A Azure.Storage.Blobs.BlobContainerClient referencing the block blob that
        //     includes the name of the account, the name of the container, and the name of
        //     the blob.
        //
        //   localDownloadPath:
        //     local folder for store downloaded blobs(files)
        //
        //   lastModifiedDate:
        //     filter blob by blob lastModifiedDate >= this parameter before download.
        //
        //   prefix:
        //     Specifies a string that filters the results to return only blobs whose name begins
        //     with the specified prefix.
        //
        //   segmentSize:
        //     The size of Azure.Page`1s that should be requested (from service operations that
        //     support it).
        //
        //   skipDeleted:
        //     do not download blob if Deleted is true
        //
        // Returns:
        //     a task.
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

        //
        // Summary:
        //     Sync a local folder with a blob container.
        //
        // Parameters:
        //   container:
        //     A Azure.Storage.Blobs.BlobContainerClient referencing the block blob that
        //     includes the name of the account, the name of the container, and the name of
        //     the blob.
        //
        //   localDownloadPath:
        //     local folder for store downloaded blobs(files)
        //
        //   lastModifiedDate:
        //     filter blob by blob lastModifiedDate >= this parameter before download.
        //
        //   prefix:
        //     Specifies a string that filters the results to return only blobs whose name begins
        //     with the specified prefix.
        //
        //   segmentSize:
        //     The size of Azure.Page`1s that should be requested (from service operations that
        //     support it).
        //
        //   skipDeleted:
        //     do not download blob if Deleted is true
        //
        // Returns:
        //     a task.
        public static async Task SyncBlobsByHierarchicalListing(BlobContainerClient container, string localDownloadPath, DateTime lastModifiedDate, string prefix = null, int? segmentSize = 32)
        {
            DateTime lastAccessTimeUtc = DateTime.UtcNow;

            await PullBlob.DownloadBlobsHierarchicalListing(container, localDownloadPath, lastModifiedDate, prefix, segmentSize, true);

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


            //string[] dirs = Directory.GetDirectories(localDownloadPath, "*", SearchOption.AllDirectories);

            //foreach (string dir in dirs)
            //{
            //    IEnumerable<string> items = Directory.EnumerateFileSystemEntries(dir);

            //    using (IEnumerator<string> item = items.GetEnumerator())
            //    {
            //        if (!item.MoveNext())
            //        {
            //            Directory.Delete(dir);
            //        }
            //    }
            //}
        }

        private static async Task DownloadBlob(BlobContainerClient container, BlobItem blob, string filePath, bool checkExist=true)
        {
            if (!checkExist || !FileChecker.CheckFileExist(filePath, BitConverter.ToString(blob.Properties.ContentHash).Replace("-", "").ToLowerInvariant()))
            {
                using (var file = File.Open(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    var blobClient = container.GetBlobClient(blob.Name);
                    await blobClient.DownloadToAsync(file);
                }
            }

            File.SetLastAccessTimeUtc(filePath, DateTime.UtcNow);
        }

        private static async Task RangeDownloadBlob(BlobContainerClient container, BlobItem blob, string filePath, bool checkExist = true, int rerunCount = 0)
        {
            if (rerunCount > MAX_RERUN_COUNT)
            {
                throw new Exception(String.Format("RangeDownloadBlob Failed after max retry({0})", MAX_RERUN_COUNT));
            }

            if (!checkExist || !FileChecker.CheckFileExist(filePath, BitConverter.ToString(blob.Properties.ContentHash).Replace("-", "").ToLowerInvariant()))
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }

                    string blobContentHash = BitConverter.ToString(blob.Properties.ContentHash).Replace("-", "").ToLowerInvariant();

                    string tempFilePath = filePath + "." + blobContentHash;
                    BlobClient blobClient = container.GetBlobClient(blob.Name);
                    long? blobLen = blob.Properties.ContentLength;

                    long dlLen = 0;

                    if (File.Exists(tempFilePath))
                    {
                        FileInfo fInfo = new FileInfo(tempFilePath);
                        dlLen = fInfo.Length;
                    }

                    long maxLoopControl = (long)((MAX_SINGLE_FILE_SIZE - dlLen) / BLOCK_SIZE);

                    using (FileStream outputStream = new FileStream(tempFilePath, FileMode.Append))
                    {
                        using StreamWriter writer = new StreamWriter(outputStream)
                        {
                            AutoFlush = false
                        };

                        while (dlLen < blobLen && maxLoopControl > 0)
                        {
                            HttpRange range = new HttpRange(dlLen, BLOCK_SIZE); //100MB
                            Task<Response<BlobDownloadStreamingResult>> dlResult = blobClient.DownloadStreamingAsync(range);
                            dlResult.Wait();

                            using (Stream dataStream = dlResult.Result.Value.Content)
                            {
                                dataStream.CopyTo(writer.BaseStream);
                                writer.Flush();

                                dlLen += dataStream.Position;
                            }

                            maxLoopControl--;
                        }

                        writer.Close();
                    }

                    if (FileChecker.CheckFileExist(tempFilePath, blobContentHash))
                    {
                        File.Move(tempFilePath, filePath);
                        File.Delete(tempFilePath);
                    }
                    else
                    {
                        File.Delete(tempFilePath);
                        await RangeDownloadBlob(container, blob, filePath, checkExist, ++rerunCount);
                    }
                }
                catch (Azure.RequestFailedException e)
                {
                    throw e;
                }
            }

        }
    }
}
//GetBlockBlobClientCore