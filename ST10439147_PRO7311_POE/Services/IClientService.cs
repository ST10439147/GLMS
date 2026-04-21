using ST10439147_PRO7311_POE.Models;

namespace ST10439147_PRO7311_POE.Services
{
    public interface IClientService
    {
        Task<List<Clients>> GetAllClients();
        Task<Clients?> GetClient(int id);
        Task<ClientResponseDto> CreateClient(Clients client);
        Task<ClientResponseDto> UpdateClient(int id, Clients updated);
        Task<ClientResponseDto> DeleteClient(int id);
    }

    // response DTO – mirrors the pattern used by ContractService
    public class ClientResponseDto
    {
        public Clients? Client { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Status { get; set; }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------