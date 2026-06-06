namespace ST10439147_PRO7311_POE_Back.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        private const long MaxFileSizeBytes = 20 * 1024 * 1024; // 20 MB

        // PDF magic bytes: every valid PDF file starts with "%PDF"
        // This is a simple check to prevent non-PDF files from being uploaded with a .pdf extension.
        // Ref (magic bytes validation): https://code-maze.com/aspnetcore-how-to-validate-file-upload-extensions/
        // Ref (magic number concept): https://www.aspsnippets.com/questions/291996/Validate-file-extension-with-magic-number-using-C-and-VBNet-in-ASPNet/
        // Ref (MS Q&A on magic check): https://learn.microsoft.com/en-us/answers/questions/726816/check-a-valid-file-using-magic-check
        private static readonly byte[] PdfMagicBytes = { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        // Validates the uploaded file and saves it to the server, returning the relative path for database storage.
        // Throws InvalidOperationException if the file is invalid (e.g. wrong type, not a real PDF).
        // The caller is responsible for catching exceptions and returning appropriate HTTP responses.
        // The file is saved with a unique name to prevent overwriting existing files, and the original filename is not used for storage to enhance security.
        public async Task<string> SaveSignedAgreement(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file was uploaded.");

            if (file.Length > MaxFileSizeBytes)
                throw new InvalidOperationException(
                    $"File size {file.Length / (1024 * 1024):F1} MB exceeds the 20 MB limit.");

            // Checking the extension is a quick initial filter, but we must also check the file header to ensure it's a valid PDF.
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                throw new InvalidOperationException(
                    $"Invalid file type '{extension}'. Only .pdf files are allowed.");

            using var inputStream = file.OpenReadStream();
            var header = new byte[4];
            // Read the first 4 bytes to check for the PDF magic number.
            var bytesRead = await inputStream.ReadAsync(header, 0, 4);
            if (bytesRead < 4 || !header.SequenceEqual(PdfMagicBytes))
                throw new InvalidOperationException(
                    "The uploaded file is not a valid PDF.");

            // Save the file to uploads/agreements with a unique name to prevent collisions.
            // WebRootPath is null when no wwwroot folder exists (API-only host), so fall back to ContentRootPath.
            var rootPath = _env.WebRootPath ?? _env.ContentRootPath;
            var uploadsFolder = Path.Combine(rootPath, "uploads", "agreements");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var outputStream = new FileStream(fullPath, FileMode.Create))
            {
                // Write the magic bytes we already read, then stream the remainder.
                // This works regardless of whether the source stream is seekable.
                await outputStream.WriteAsync(header, 0, bytesRead);
                await inputStream.CopyToAsync(outputStream);
            }

            _logger.LogInformation("Saved signed agreement: {FileName} → {Path}",
                file.FileName, fullPath);

            // Return the relative web path stored in DB
            return Path.Combine("uploads", "agreements", uniqueFileName)
                       .Replace("\\", "/");
        }
        // Given a stored relative path (e.g. "uploads/agreements/abc123.pdf"), return the absolute file path if it exists, or null if the file is missing.
        public string? GetFilePath(string storedPath)
        {
            var rootPath = _env.WebRootPath ?? _env.ContentRootPath;
            var fullPath = Path.Combine(rootPath, storedPath);
            return File.Exists(fullPath) ? fullPath : null;
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------