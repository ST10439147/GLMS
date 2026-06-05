using ST10439147_PRO7311_POE_Back.Models;

namespace ST10439147_PRO7311_POE_Back.Services
{
    public interface IClientService
    {
        Task<List<Clients>> GetAllClients();
        Task<Clients?> GetClient(int id);
        Task<ClientResponseDto> CreateClient(Clients client);
        Task<ClientResponseDto> UpdateClient(int id, Clients updated);
        Task<ClientResponseDto> PatchClient(int id, PatchClientDto patch);
        Task<ClientResponseDto> DeleteClient(int id);
    }

    // response DTO – mirrors the pattern used by ContractService
    public class ClientResponseDto
    {
        public Clients? Client { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
    }

    /// <summary>Request body for PATCH /api/clients/{id} — all fields optional.</summary>
    public class PatchClientDto
    {
        /// <summary>New client name. Omit to keep the existing value.</summary>
        public string? Name { get; set; }
        /// <summary>New contact details. Omit to keep the existing value.</summary>
        public string? ContactDetails { get; set; }
        /// <summary>New region. Omit to keep the existing value.</summary>
        public ClientRegion? Region { get; set; }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------