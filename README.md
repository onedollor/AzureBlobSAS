# AzureBlobSAS

### App.config

```xml
  <appSettings>
    <add key="SasUri" value="https://{accountname}.blob.core.windows.net/{container}?{blob access token}&amp;comp=list&amp;restype=continer"/>
  </appSettings>
```

### PushBlob
```c#
  string sasUristring = ConfigurationManager.AppSettings.Get("SasUri");
  string localDownloadPath = @"C:\data\azure";

  Uri sasUri = new Uri(sasUristring);
  BlobContainerClient container = new BlobContainerClient(sasUri);

  Task task;
  
  string localFilePath = @"G:\data\AAPL.csv";
  //A path start from "/" e.g. "/folder_A/folder_B/blob_x.txt"
  string blobName = @"/data/stock/AAPL.csv"; 
  bool overwrite = true;
  
  task = PushBlob.UploadAsync(container, @"C:\data\AAPL.csv", overwrite, @"/data/stock/AAPL.csv");
  Console.WriteLine("s task.Wait();");
  task.Wait();
  Console.WriteLine("f task.Wait();");
  Console.ReadLine();

```

### PullBlob
```c#
  task = PullBlob.DownloadBlobsHierarchicalListing(container, localDownloadPath, DateTime.MinValue);
  Console.WriteLine("s task.Wait();");
  task.Wait();
  Console.WriteLine("f task.Wait();");

  Console.ReadLine();
```
