using Azure.Storage.Files.Shares;
using Azure;
using System.Drawing.Text;
using Azure.Storage.Files.Shares.Models;
using ClassLibrary.Models;

namespace ClassLibrary.Services
{
    public class FileService
    {
        private readonly String _connectionString;
        private readonly String _fileShareName;
        public FileService(String connectionString, string fileShareName) 
        {
            _connectionString = connectionString ??
                throw new ArgumentException(nameof(connectionString));
                _fileShareName = fileShareName ?? throw new ArgumentException(nameof(fileShareName));
        }

        public async Task UploadFileAsync(string directoryName, string fileName, Stream fileStream)
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);

                var directoryClient = shareClient.GetDirectoryClient(directoryName);
                await directoryClient.CreateIfNotExistsAsync();

                var fileClient = directoryClient.GetFileClient(fileName);

                await fileClient.CreateAsync(fileStream.Length);
                await fileClient.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream); 
            }
            catch (Exception ex)
            {
                throw new Exception("Error uploading file:" + ex.Message, ex);
            }
        }

        public async Task<Stream> DownloadFileAsync(string directoryName, string fileName)
        {
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);
                var directoryClient = shareClient.GetDirectoryClient(directoryName);
                var fileClient = directoryClient.GetFileClient(fileName);
                var downloadInfo = await fileClient.DownloadAsync();
                return downloadInfo.Value.Content;
            }
            catch(Exception ex)
            {
                throw new Exception("Error dowloading file:" + ex.Message, ex);
            }
        }

        public async Task<List<Contract>> ListFileAsync(string directoryName)
        {
            var contracts = new List<Contract>();
            try
            {
                var serviceClient = new ShareServiceClient(_connectionString);
                var shareClient = serviceClient.GetShareClient(_fileShareName);

                var directoryClient = shareClient.GetDirectoryClient(directoryName);
                await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
                {
                    if (!item.IsDirectory)
                    {
                        var fileClient = directoryClient.GetFileClient(item.Name);
                        var properties = await fileClient.GetPropertiesAsync();
                        contracts.Add(new Contract
                        {
                            Name = item.Name,
                            Size = properties.Value.ContentLength
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error listing files:" + ex.Message, ex);
            }
            return contracts;
        }

    }
}
