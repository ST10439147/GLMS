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
}
