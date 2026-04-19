namespace ST10439147_PRO7311_POE.Services
{
    public interface IFileService
    {
        /// <summary>
        /// Validates and saves an uploaded PDF to the server's UploadedFiles folder.
        /// Returns the saved relative web path.
        /// </summary>
        Task<string> SaveSignedAgreement(IFormFile file);

        /// <summary>Returns the absolute file path if it exists, null otherwise.</summary>
        string? GetFilePath(string storedPath);
    }

    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<FileService> _logger;

        // PDF magic bytes: every valid PDF file starts with "%PDF"
        private static readonly byte[] PdfMagicBytes = { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        public FileService(IWebHostEnvironment env, ILogger<FileService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string> SaveSignedAgreement(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException("No file was uploaded.");

            // ── Extension check ─────────────────────────────────────────────
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
                throw new InvalidOperationException(
                    $"Invalid file type '{extension}'. Only .pdf files are allowed.");

            // ── Magic bytes check (more reliable than ContentType) ───────────
            // Browsers and HTTP clients often send "application/octet-stream"
            // instead of "application/pdf", so we validate the file content
            // itself by checking the PDF signature bytes (%PDF).
            using (var peekStream = file.OpenReadStream())
            {
                var header = new byte[4];
                var bytesRead = await peekStream.ReadAsync(header, 0, 4);
                if (bytesRead < 4 || !header.SequenceEqual(PdfMagicBytes))
                    throw new InvalidOperationException(
                        "The uploaded file is not a valid PDF.");
            }

            // ── UUID naming to prevent overwrites ────────────────────────────
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "agreements");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Saved signed agreement: {FileName} → {Path}",
                file.FileName, fullPath);

            // Return the relative web path stored in DB
            return Path.Combine("uploads", "agreements", uniqueFileName)
                       .Replace("\\", "/");
        }

        public string? GetFilePath(string storedPath)
        {
            var fullPath = Path.Combine(_env.WebRootPath, storedPath);
            return File.Exists(fullPath) ? fullPath : null;
        }
    }
}