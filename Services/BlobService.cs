using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Extensions.Configuration;

namespace GazeManager.Services
{
    public class BlobService
    {
        private readonly string connectionStr;

        public BlobService(IConfiguration configuration)
        {
            this.connectionStr = configuration["AppConfig:Azure:StorageAccount"];
        }

        public async Task<string> UploadFile(string containerName, string fileName, IFormFile file)
        {
            try
            {
                CloudStorageAccount storageAccount;
                if (CloudStorageAccount.TryParse(connectionStr, out storageAccount))
                {
                    CloudBlobClient client = storageAccount.CreateCloudBlobClient();
                    CloudBlobContainer container = client.GetContainerReference(containerName);
                    await container.CreateIfNotExistsAsync();
                    await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
                    var blob = container.GetBlockBlobReference(fileName.ToLower());
                    await blob.UploadFromStreamAsync(file.OpenReadStream());
                    return blob.Uri.AbsoluteUri + "?v=" + DateTime.Now;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return null;
        }

    }
}