// using Azure;
// using Azure.Storage.Blobs;
// using Azure.Storage.Blobs.Models;
// using Azure.Storage.Sas;
// using Microsoft.AspNetCore.Components.Forms;
//
// namespace MoviePortal.Services;
//
// public class AzureBlobService
// {
//     private readonly BlobServiceClient _blobClient;
//
//     public AzureBlobService(IConfiguration config)
//     {
//         var conn = config["ConnectionStrings:AzureBlobStorage"];
//         _blobClient = new BlobServiceClient(conn);
//     }
//     
//     private async Task<(string Container, string BlobName, string PublicUrl, string? ContentType, long? Size, string? ETag)>
//         UploadImageFromStreamAsync(string containerName, Stream stream, string filename, string? contentType, long size, string prefix)
//     {
//         var container = _blobClient.GetBlobContainerClient(containerName);
//         await container.CreateIfNotExistsAsync(PublicAccessType.Blob); // ảnh: public
//
//         var ext = Path.GetExtension(filename);
//         var blobName = $"{prefix}/{Guid.NewGuid():N}{ext}";
//         var blob = container.GetBlobClient(blobName);
//
//         var res = await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType ?? "application/octet-stream" });
//         return (containerName, blobName, blob.Uri.ToString(), contentType, size, res.Value.ETag.ToString());
//     }
//     
//     // IBrowserFile (Blazor)
//     public Task<(string Container, string BlobName, string PublicUrl, string? ContentType, long? Size, string? ETag)>
//         UploadImageAsync(string containerName, IBrowserFile file, string prefix)
//         => UploadImageFromStreamAsync(containerName, file.OpenReadStream(1L * 1024 * 1024 * 1024), file.Name, file.ContentType, file.Size, prefix);
//     
//     // IFormFile (MVC/endpoint form-data)
//     public Task<(string Container, string BlobName, string PublicUrl, string? ContentType, long? Size, string? ETag)>
//         UploadImageAsync(string containerName, IFormFile file, string prefix)
//         => UploadImageFromStreamAsync(containerName, file.OpenReadStream(), file.FileName, file.ContentType, file.Length, prefix);
//
//     // IBrowserFile (Blazor)
//     public Task<(string Container, string BlobName, string? ContentType, long? Size, string? ETag)>
//         UploadVideoAsync(string containerName, IBrowserFile file, string prefix)
//         => UploadVideoFromStreamAsync(containerName, file.OpenReadStream(10L * 1024 * 1024 * 1024), file.Name, file.ContentType, file.Size, prefix);
//
//     // IFormFile (MVC/endpoint form-data)
//     public Task<(string Container, string BlobName, string? ContentType, long? Size, string? ETag)>
//         UploadVideoAsync(string containerName, IFormFile file, string prefix)
//         => UploadVideoFromStreamAsync(containerName, file.OpenReadStream(), file.FileName, file.ContentType, file.Length, prefix);
//
//     private async Task<(string Container, string BlobName, string? ContentType, long? Size, string? ETag)>
//         UploadVideoFromStreamAsync(string containerName, Stream stream, string filename, string? contentType, long size, string prefix)
//     {
//         var container = _blobClient.GetBlobContainerClient(containerName);
//         await container.CreateIfNotExistsAsync(); // video: private
//
//         var ext = Path.GetExtension(filename);
//         var blobName = $"{prefix}/{Guid.NewGuid():N}{ext}";
//         var blob = container.GetBlobClient(blobName);
//
//         var res = await blob.UploadAsync(stream, new BlobHttpHeaders { ContentType = contentType ?? "application/octet-stream" });
//         return (containerName, blobName, contentType, size, res.Value.ETag.ToString());
//     }
//
//     // Phát video: SINH SAS tạm (không lưu vào DB)
//     public Uri GetReadSasUri(string containerName, string blobName, TimeSpan lifetime)
//     {
//         // Lấy BlobClient từ service client đã tạo bằng connection string
//         var blob = _blobClient.GetBlobContainerClient(containerName).GetBlobClient(blobName);
//
//         var sas = new BlobSasBuilder
//         {
//             BlobContainerName = containerName,
//             BlobName = blobName,
//             Resource = "b",
//             ExpiresOn = DateTimeOffset.UtcNow.Add(lifetime)
//         };
//         sas.SetPermissions(BlobSasPermissions.Read);
//
//         // Nếu BlobClient được tạo bằng shared key (connection string có AccountKey), sẽ generate SAS được
//         if (blob.CanGenerateSasUri)
//             return blob.GenerateSasUri(sas);
//
//         // Trường hợp dùng AAD (không có key) → gợi ý dùng User Delegation SAS (bên dưới)
//         throw new InvalidOperationException(
//             "BlobClient cannot generate SAS. If you are using AAD/Managed Identity, use User Delegation SAS instead.");
//     }
//
//     // Xóa blob: nên dùng container + blobName
//     public async Task DeleteAsync(string containerName, string blobName)
//         => await _blobClient.GetBlobContainerClient(containerName)
//             .GetBlobClient(blobName)
//             .DeleteIfExistsAsync();
//     
//     public async Task<bool> CheckContainerAsync(string containerName, bool createIfMissing = false, CancellationToken ct = default)
//     {
//         var container = _blobClient.GetBlobContainerClient(containerName);
//
//         if (createIfMissing)
//             await container.CreateIfNotExistsAsync(cancellationToken: ct);
//
//         var exists = await container.ExistsAsync(ct);
//         if (!exists.Value) return false;
//
//         // thêm 1 lệnh nhẹ để chắc chắn có quyền
//         try
//         {
//             var _ = await container.GetPropertiesAsync(cancellationToken: ct);
//             return true;
//         }
//         catch (RequestFailedException)
//         {
//             return false;
//         }
//     }
// }