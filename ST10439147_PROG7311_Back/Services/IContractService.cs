using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Models;
namespace ST10439147_PRO7311_POE_Back.Services
{
    
    // IContractService defines the contract for managing contracts and their associated service requests within the application.
    // It provides methods for creating, retrieving, updating, and deleting contracts, as well as managing their statuses and associated service requests.
    // Searching and filtering contracts based on criteria such as start date and status is also supported. Additionally, it includes functionality for handling file uploads related to signed agreements.
    
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
        Task<ServiceRequestResponseDto> SetServiceRequestStatus(int requestId, ServiceRequestStatus newStatus);
        Task<ServiceRequestResponseDto> DeleteServiceRequest(int requestId);
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------