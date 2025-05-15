using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

public class BlobStorageService
{
    private readonly string _sasUrl;
    private readonly string _containerName;

    public BlobStorageService(IConfiguration configuration)
    {
        _sasUrl = configuration["AzureBlobStorage:SasUrl"]
            ?? throw new ArgumentNullException("AzureBlobStorage:SasUrl is missing or empty.");

        _containerName = configuration["AzureBlobStorage:ContainerName"]
            ?? throw new ArgumentNullException("AzureBlobStorage:ContainerName is missing or empty.");
    }

    public async Task<string> UploadFileAsync(IFormFile file, string fileName)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is null or empty.");

        try
        {
            var containerClient = new BlobContainerClient(new Uri(_sasUrl));
            var blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            Console.WriteLine("Uploading to blob: " + blobClient.Uri);
            return blobClient.Uri.ToString(); // SAS-protected blob URL
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Azure Blob upload failed: {ex.Message}", ex);
        }
    }
}
