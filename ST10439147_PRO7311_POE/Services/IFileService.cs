namespace ST10439147_PRO7311_POE.Services
{
    public interface IFileService
    {
      
        // Validates and saves an uploaded PDF to the server's UploadedFiles folder.
        // Returns the saved relative web path.        
        Task<string> SaveSignedAgreement(IFormFile file);

        // Returns the absolute file path if it exists, null otherwise.
        string? GetFilePath(string storedPath);
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------