using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ClassLibrary.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "products";
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _functionAppUrl;
        private readonly string _functionApiKey;

        public BlobService(string connectionString, IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient();
            _functionAppUrl = configuration["FunctionAppUrl"] ?? throw new ArgumentNullException("FunctionAppUrl is not configured.");
            _functionApiKey = configuration["FunctionApiKey"] ?? throw new ArgumentNullException("FunctionApiKey is not configured.");
        }

        public async Task<string> UploadsAsync(Stream fileStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream);
            return blobClient.Uri.ToString();
        }

        public async Task<string> UploadsAsyncFunction(Stream fileStream, string fileName, string contentType)
        {
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            var fileBytes = memoryStream.ToArray();
            var content = new ByteArrayContent(fileBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            content.Headers.Add("X-File-Name", fileName);
            string requestUrl = $"{_functionAppUrl}upload-image?code={_functionApiKey}";
            var response = await _httpClient.PostAsync(requestUrl, content);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<ImageResponse>();
            if (result?.ImageUrl == null)
            {
                throw new Exception("Failed to get image URL from Azure Function");
            }
            return result.ImageUrl;
        }

        public async Task DeleteBlobAsync(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1];
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
        }
    }

    public class ImageResponse
    {
        public bool Success { get; set; }
        public string? ImageUrl { get; set; }
        public string? FileName { get; set; }
    }
}
