using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MyFunctions.Functions;

public class AddImageToBlob
{
    private readonly ILogger<AddImageToBlob> _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private const string ContainerName = "products";

    public AddImageToBlob(ILogger<AddImageToBlob> logger)
    {
        _logger = logger;
        var connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    [Function("AddImageBlob")]
    public async Task<HttpResponseData> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload-image")] HttpRequestData req)
    {
        try
        {
            _logger.LogInformation("UploadImage function triggered");

            // Read the entire request body
            byte[] fileBytes;
            string fileName;
            string contentType;

            using (var memoryStream = new MemoryStream())
            {
                await req.Body.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            if (fileBytes.Length == 0)
            {
                _logger.LogWarning("No file provided in request");
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "No file provided" });
                return badResponse;
            }

            contentType = req.Headers.Contains("Content-Type")
                ? req.Headers.GetValues("Content-Type").FirstOrDefault() ?? "application/octet-stream"
                : "application/octet-stream";

            fileName = "uploaded-image";
            if (req.Headers.Contains("X-File-Name"))
            {
                fileName = req.Headers.GetValues("X-File-Name").FirstOrDefault() ?? fileName;
            }

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension))
            {
                extension = GetExtensionFromContentType(contentType);
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            if (string.IsNullOrEmpty(extension) || Array.IndexOf(allowedExtensions, extension) == -1)
            {
                var badResponse = req.CreateResponse(HttpStatusCode.BadRequest);
                await badResponse.WriteAsJsonAsync(new { error = "Invalid file type!" });
                return badResponse;
            }

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            _logger.LogInformation($"Uploading file: {uniqueFileName}");

            // Get container client
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

            // Get blob client
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Determine final content type
            var finalContentType = GetContentType(extension);

            // Upload the file
            using (var stream = new MemoryStream(fileBytes))
            {
                await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = finalContentType });
            }

            var imageUrl = blobClient.Uri.ToString();
            _logger.LogInformation($"File uploaded successfully: {imageUrl}");

            // Return success response with URL
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new
            {
                success = true,
                imageUrl = imageUrl,
                fileName = uniqueFileName
            });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading image: {ex.Message}");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = $"Upload failed: {ex.Message}" });
            return errorResponse;
        }
    }

    private string GetContentType(string extension)
    {
        switch (extension)
        {
            case ".jpg":
            case ".jpeg":
                return "image/jpeg";
            case ".png":
                return "image/png";
            case ".gif":
                return "image/gif";
            case ".webp":
                return "image/webp";
            default:
                return "application/octet-stream";
        }
    }

    private string GetExtensionFromContentType(string contentType)
    {
        if (contentType.Contains("jpeg") || contentType.Contains("jpg"))
            return ".jpg";
        if (contentType.Contains("png"))
            return ".png";
        if (contentType.Contains("gif"))
            return ".gif";
        if (contentType.Contains("webp"))
            return ".webp";

        return ".jpg"; // default
    }
}