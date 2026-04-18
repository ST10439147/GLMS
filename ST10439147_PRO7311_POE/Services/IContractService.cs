using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Models;
namespace ST10439147_PRO7311_POE.Services
{
    public interface IContractService
    {
        // Contracts
        Task<ContractResponseDto> CreateContract(Contract contract);
        Task<ContractResponseDto?> GetContract(int id);
        Task<List<Contract>> GetAllContracts();
        Task<ContractResponseDto> UpdateContract(int id, Contract contract);
        Task<ContractResponseDto> SetContractStatus(int contractId, ContractStatus newStatus);
        Task<ContractResponseDto?> DeleteContract(int id);

        // Search/Filter (LINQ - Rubric requirement)
        Task<List<Contract>> SearchContracts(DateTime? startFrom, DateTime? startTo, ContractStatus? status);

        // File upload
        Task<ContractResponseDto> SaveSignedAgreement(int contractId, string filePath, string originalFileName);

        // Service Requests
        Task<ServiceRequestResponseDto> CreateServiceRequest(ServiceRequest request);
        Task<List<ServiceRequest>> GetServiceRequestsByContract(int contractId);
    }
}
