using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Extensions;
using ST10439147_PRO7311_POE_Back.Models;
using ST10439147_PRO7311_POE_Back.Services;


namespace ST10439147_PRO7311_POE.Controllers
{

    // This controller manages contract-related operations, including creation, retrieval, updating, deletion,
    // searching, and handling of signed agreements and service requests. Access is controlled based on user roles.
    // Managers and Admins can create, update, set status, delete contracts, and upload agreements.
    // All logged-in users can view contracts, search, download agreements, and submit service requests.
    // Admins have exclusive rights to delete contracts and service requests.
    // The controller uses dependency injection to access contract and file services, and returns standardized response DTOs for operations.
    // All endpoints are protected with role-based access checks, and appropriate HTTP status codes are returned for success, unauthorized access, and not found scenarios.
    // The controller is decorated with [ApiController] for automatic model validation and response formatting, and [Route] to define the base URL pattern for all endpoints.

    [ApiController]
    [Route("api/v1/[controller]")]
    [IgnoreAntiforgeryToken]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IFileService _fileService;

        public ContractController(IContractService contractService, IFileService fileService)
        {
            _contractService = contractService;
            _fileService = fileService;
        }

        // Helper method to create a standardized denial response
        private static ContractResponseDto Deny(string message) =>
            new() { Message = message, Status = false };

        // Manager+ required: create a new contract
        // Only users with Manager or Admin roles can create contracts. The endpoint accepts a Contract object in the request body,
        // and returns a ContractResponseDto indicating success or failure of the creation operation.
        [HttpPost("Create")]
        public async Task<ActionResult<ContractResponseDto>> Create([FromBody] Contract contract)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            var result = await _contractService.CreateContract(contract);
            return Ok(result);
        }

        // All logged-in users can view a specific contract by ID. The endpoint checks if the user is logged in, and if so, retrieves the contract details using the contract service. If the contract is found, it returns the contract data; otherwise, it returns a NotFound response.
        // The endpoint is protected to ensure that only authenticated users can access contract details, and it provides appropriate feedback if the user is not logged in or if the contract does not exist.
        [HttpGet("Get/{id}")]
        public async Task<ActionResult<ContractResponseDto>> Get(int id)
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var result = await _contractService.GetContract(id);
            return result == null ? NotFound() : Ok(result);
        }

        // All logged-in users can view all contracts. The endpoint checks if the user is logged in, and if so, retrieves a list of all contracts using the contract service. It returns the list of contracts in the response. If the user is not logged in, it returns a denial message indicating that access is denied and prompting the user to log in.
        // This endpoint allows authenticated users to access the complete list of contracts, providing a way to view all existing contracts in the system. It ensures that only logged-in users can access this information, maintaining security and privacy of contract data.
        // The response includes all contracts, which may be filtered or paginated in a real-world application to improve performance and usability, especially if there are a large number of contracts.
        [HttpGet("All")]
        public async Task<ActionResult<List<Contract>>> All()
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var contracts = await _contractService.GetAllContracts();
            return Ok(contracts);
        }

        // Manager+ required: update an existing contract
        // Only users with Manager or Admin roles can update contracts. The endpoint accepts a contract ID as a route parameter and a Contract object in the request body containing the updated contract details. It checks if the user has the necessary role, and if so, it calls the contract service to perform the update operation. The response is a ContractResponseDto indicating the success or failure of the update.
        // This endpoint allows authorized users to modify existing contract information, ensuring that only those with appropriate permissions can make changes to contract data. The contract service handles the logic for updating the contract in the database, and the response provides feedback on whether the update was successful or if there were any issues.
        // The endpoint also ensures that the user is informed if they do not have the necessary permissions to perform the update, maintaining security and proper access control within the application.
        [HttpPut("Update/{id}")]
        public async Task<ActionResult<ContractResponseDto>> Update(int id, [FromBody] Contract contract)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            var result = await _contractService.UpdateContract(id, contract);
            return Ok(result);
        }

        // Manager+ required: set contract status
        // Only users with Manager or Admin roles can set the status of a contract. The endpoint accepts a contract ID and a ContractStatus value as route parameters. It checks if the user has the necessary role, and if so, it calls the contract service to update the contract's status. The response is a ContractResponseDto indicating the success or failure of the status update.
        // This endpoint allows authorized users to change the status of a contract, which is an important aspect of contract management. The contract service handles the logic for updating the status in the database, and the response provides feedback on whether the operation was successful or if there were any issues. The endpoint also ensures that only users with the appropriate permissions can perform this action, maintaining security and proper access control within the application.
        [HttpPut("SetStatus/{id}/{status}")]
        public async Task<ActionResult<ContractResponseDto>> SetStatus(int id, ContractStatus status)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            var result = await _contractService.SetContractStatus(id, status);
            return Ok(result);
        }

        // Admin only: delete a contract
        // Only users with Admin roles can delete contracts. The endpoint accepts a contract ID as a route parameter, checks if the user is an Admin, and if so, it calls the contract service to delete the contract. The response is a ContractResponseDto indicating the success or failure of the deletion operation. If the user is not an Admin, it returns a denial message indicating that access is denied and prompting the user to log in with appropriate credentials.
        // This endpoint allows authorized users to remove contracts from the system, ensuring that only those with the highest level of permissions can perform such a critical action. The contract service handles the logic for deleting the contract from the database, and the response provides feedback on whether the deletion was successful or if there were any issues. The endpoint also ensures that users without the necessary permissions are informed of their access limitations, maintaining security and proper access control within the application.
        // The endpoint returns a NotFound response if the contract to be deleted does not exist, providing clear feedback to the user about the outcome of their deletion attempt.
        [HttpDelete("Delete/{id}")]
        public async Task<ActionResult<ContractResponseDto>> Delete(int id)
        {
            if (!this.IsAdmin())
                return Ok(Deny("Access denied – Admins only."));

            var result = await _contractService.DeleteContract(id);
            return result == null ? NotFound() : Ok(result);
        }

        // All logged-in users can search for contracts based on optional criteria such as start date range and status. The endpoint accepts query parameters for startFrom, startTo, and status, and checks if the user is logged in. If the user is authenticated, it calls the contract service to perform the search based on the provided criteria and returns the list of matching contracts. If the user is not logged in, it returns a denial message indicating that access is denied and prompting the user to log in.
        // This endpoint allows users to filter and find specific contracts based on their start dates and status, providing a more efficient way to navigate through potentially large numbers of contracts. The search functionality enhances the user experience by allowing users to quickly locate relevant contracts without having to manually sift through all available contracts. The endpoint ensures that only authenticated users can perform searches, maintaining security and proper access control within the application.
        // The search criteria are optional, meaning that users can search using any combination of the parameters or even without any parameters to retrieve all contracts. The contract service handles the logic for applying the search filters and returning the appropriate results based on the provided criteria.
        [HttpGet("Search")]
        public async Task<ActionResult<List<Contract>>> Search(
            [FromQuery] DateTime? startFrom,
            [FromQuery] DateTime? startTo,
            [FromQuery] ContractStatus? status)
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var results = await _contractService.SearchContracts(startFrom, startTo, status);
            return Ok(results);
        }

        // Manager+ required: upload a signed agreement for a specific contract
        // Only users with Manager or Admin roles can upload signed agreements. The endpoint accepts a contract ID as a route parameter and an IFormFile representing the signed agreement in the request body. It checks if the user has the necessary role, and if so, it saves the file using the file service and updates the contract with the path to the saved agreement. The response is a ContractResponseDto indicating the success or failure of the upload operation.
        // This endpoint allows authorized users to associate a signed agreement document with a specific contract, facilitating the management of contract documentation. The file service handles the logic for saving the uploaded file to the server, and the contract service updates the contract record with the file path and name. The endpoint also provides feedback on whether the upload was successful or if there were any issues, such as invalid file formats or errors during file saving.
        [HttpPost("UploadAgreement/{contractId}")]
        [RequestSizeLimit(20 * 1024 * 1024)]          // ← hard cap at 20 MB
        [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
        public async Task<ActionResult<ContractResponseDto>> UploadAgreement(
            int contractId, IFormFile file)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            try
            {
                var savedPath = await _fileService.SaveSignedAgreement(file);
                var result = await _contractService.SaveSignedAgreement(
                    contractId, savedPath, file.FileName);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ContractResponseDto { Message = ex.Message, Status = false });
            }
        }

        // All logged-in users can download the signed agreement for a specific contract. The endpoint accepts a contract ID as a route parameter, checks if the user is logged in, and retrieves the contract details using the contract service. If a signed agreement is found, it retrieves the file path from the file service, reads the file bytes, and returns the file as a downloadable response with the appropriate content type and filename. If the user is not logged in, if there is no signed agreement on file, or if the file cannot be found on the server, it returns appropriate error responses.
        // This endpoint allows users to access the signed agreement associated with a contract, facilitating easy retrieval of important contract documents. It ensures that only authenticated users can download agreements, maintaining security and proper access control. The endpoint also provides clear feedback in cases where the agreement is not available or if there are issues with file retrieval, enhancing the user experience.
        // The file is returned with a content type of "application/pdf" and the original filename if available, or a default name of "agreement.pdf" if not. This allows users to easily identify and save the downloaded agreement on their devices.
        [HttpGet("DownloadAgreement/{contractId}")]
        public async Task<IActionResult> DownloadAgreement(int contractId)
        {
            if (!this.IsLoggedIn())
                return Unauthorized("Please log in to download agreements.");

            var result = await _contractService.GetContract(contractId);
            if (result?.Contract?.SignedAgreementPath == null)
                return NotFound("No signed agreement on file for this contract.");

            var fullPath = _fileService.GetFilePath(result.Contract.SignedAgreementPath);
            if (fullPath == null)
                return NotFound("File not found on server.");

            var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(fileBytes, "application/pdf",
                result.Contract.SignedAgreementFileName ?? "agreement.pdf");
        }

        // All logged-in users can submit a service request related to a contract. The endpoint accepts a ServiceRequest object in the request body, which contains details about the service request being made. It checks if the user is logged in, and if so, it calls the contract service to create the service request. The response is a ServiceRequestResponseDto indicating the success or failure of the submission.
        // This endpoint allows users to request services or actions related to a specific contract, facilitating communication and issue resolution between users and contract managers. The service request can be used for various purposes, such as requesting changes, reporting issues, or asking for clarifications regarding the contract. The contract service handles the logic for creating and storing the service request in the database, and the response provides feedback on whether the submission was successful or if there were any issues.
        // The endpoint ensures that only authenticated users can submit service requests, maintaining security and proper access control within the application. It also provides a standardized response format to inform users about the outcome of their request submission.
        [HttpPost("SubmitRequest")]
        public async Task<ActionResult<ServiceRequestResponseDto>> SubmitRequest(
            [FromBody] ServiceRequest request)
        {
            if (!this.IsLoggedIn())
                return Ok(new ServiceRequestResponseDto { Message = "Access denied – please log in.", Status = false });

            var result = await _contractService.CreateServiceRequest(request);
            return Ok(result);
        }

        // All logged-in users can view service requests for a specific contract. The endpoint accepts a contract ID as a route parameter, checks if the user is logged in, and retrieves the list of service requests associated with that contract using the contract service. The response is a list of ServiceRequest objects. If the user is not logged in, it returns a denial message indicating that access is denied and prompting the user to log in.
        // This endpoint allows users to see all service requests that have been submitted for a particular contract, providing visibility into ongoing issues, requests, or actions related to the contract. It ensures that only authenticated users can access this information, maintaining security and proper access control within the application. The contract service handles the logic for retrieving the relevant service requests from the database based on the provided contract ID.
        // The response includes all service requests for the specified contract, which may be filtered or sorted in a real-world application to improve usability, especially if there are many service requests associated with a contract.
        // The endpoint returns a list of service requests, allowing users to track the status and details of each request, facilitating better communication and management of contract-related issues.
        [HttpGet("ServiceRequests/{contractId}")]
        public async Task<ActionResult<List<ServiceRequest>>> GetServiceRequests(int contractId)
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var results = await _contractService.GetServiceRequestsByContract(contractId);
            return Ok(results);
        }

        // Manager+ required: update a service request's status
        // Only users with Manager or Admin roles can update the status of a service request. The endpoint accepts a service request ID and a ServiceRequestStatus value as route parameters. It checks if the user has the necessary role, and if so, it calls the contract service to update the status of the specified service request. The response is a ServiceRequestResponseDto indicating the success or failure of the status update operation.
        // This endpoint allows authorized users to manage the lifecycle of service requests by updating their status, which is crucial for tracking the progress and resolution of issues or requests related to contracts. The contract service handles the logic for updating the status in the database, and the response provides feedback on whether the operation was successful or if there were any issues. The endpoint also ensures that only users with the appropriate permissions can perform this action, maintaining security and proper access control within the application.
        [HttpPut("SetRequestStatus/{requestId}/{status}")]
        public async Task<ActionResult<ServiceRequestResponseDto>> SetRequestStatus(
            int requestId, ServiceRequestStatus status)
        {
            if (!this.IsManager())
                return Ok(new ServiceRequestResponseDto
                {
                    Message = "Access denied – Managers and Admins only.",
                    Status = false
                });

            var result = await _contractService.SetServiceRequestStatus(requestId, status);
            return Ok(result);
        }

        // Admin only: delete a service request (must be Rejected or Completed first)
        // Only users with Admin roles can delete service requests. The endpoint accepts a service request ID as a route parameter, checks if the user is an Admin, and if so, it calls the contract service to delete the specified service request. The response is a ServiceRequestResponseDto indicating the success or failure of the deletion operation. If the user is not an Admin, it returns a denial message indicating that access is denied and prompting the user to log in with appropriate credentials.
        // This endpoint allows authorized users to remove service requests from the system, ensuring that only those with the highest level of permissions can perform such a critical action.
        // The contract service handles the logic for deleting the service request from the database, and the response provides feedback on whether the deletion was successful or if there were any issues. The endpoint also ensures that users without the necessary permissions are informed of their access limitations, maintaining security and proper access control within the application.
        [HttpDelete("DeleteRequest/{requestId}")]
        public async Task<ActionResult<ServiceRequestResponseDto>> DeleteRequest(int requestId)
        {
            if (!this.IsAdmin())
                return Ok(new ServiceRequestResponseDto
                {
                    Message = "Access denied – Admins only.",
                    Status = false
                });

            var result = await _contractService.DeleteServiceRequest(requestId);
            return Ok(result);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------