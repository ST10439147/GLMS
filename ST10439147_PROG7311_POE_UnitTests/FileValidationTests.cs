using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using ST10439147_PRO7311_POE_Back.Services;

namespace ST10439147_PRO7311_POE.Tests
{
    // Tests the FileService upload validation rules:
    //   - Only .pdf extension is accepted.
    //   - File must contain valid PDF magic bytes (%PDF).
    //   - Empty/null files are rejected.
    
    public class FileValidationTests
    {
        private static readonly byte[] ValidPdfHeader = { 0x25, 0x50, 0x44, 0x46 }; // %PDF

        private static FileService CreateFileService()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());
            return new FileService(envMock.Object, new Mock<ILogger<FileService>>().Object);
        }

        private static IFormFile MakeFormFile(string fileName, byte[] content)
        {
            var stream = new MemoryStream(content);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(content.Length);
            fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns<Stream, CancellationToken>((s, _) =>
                    {
                        stream.Position = 0;
                        return stream.CopyToAsync(s);
                    });
            return fileMock.Object;
        }

        [Fact]
        public async Task SaveSignedAgreement_ValidPdf_SavesFile()
        {
            var content = ValidPdfHeader.Concat(new byte[100]).ToArray();
            var file = MakeFormFile("agreement.pdf", content);

            var path = await CreateFileService().SaveSignedAgreement(file);

            Assert.False(string.IsNullOrWhiteSpace(path));
            Assert.Contains("uploads/agreements", path);
        }

        [Fact]
        public async Task SaveSignedAgreement_ExeFile_ThrowsInvalidOperation()
        {
            var content = new byte[] { 0x4D, 0x5A, 0x00, 0x00 }; // MZ (EXE) header
            var file = MakeFormFile("malware.exe", content);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateFileService().SaveSignedAgreement(file));

            Assert.Contains(".exe", ex.Message);
            Assert.Contains("Only .pdf", ex.Message);
        }

        [Theory]
        [InlineData("document.docx")]
        [InlineData("spreadsheet.xlsx")]
        [InlineData("script.js")]
        public async Task SaveSignedAgreement_NonPdfExtension_ThrowsInvalidOperation(string fileName)
        {
            var content = ValidPdfHeader.Concat(new byte[10]).ToArray();
            var file = MakeFormFile(fileName, content);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateFileService().SaveSignedAgreement(file));
        }

        [Fact]
        public async Task SaveSignedAgreement_NullFile_ThrowsInvalidOperation()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => CreateFileService().SaveSignedAgreement(null!));
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------