using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE_Back.DTOs;
using ST10439147_PRO7311_POE_Back.Extensions;
using ST10439147_PRO7311_POE_Back.Models;
using ST10439147_PRO7311_POE_Back.Services;

namespace ST10439147_PRO7311_POE.Controllers
{
    /// <summary>
    /// Manages the full lifecycle of contracts and their associated service requests.
    /// Managers and Admins can create, update, and change status; all authenticated users can read.
    /// Admins have exclusive rights to delete contracts and service requests.
    /// </summary>
    [ApiController]
    [Route("api/contracts")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public class ContractController : ControllerBase
    {
        private readonly IContractService _contractService;
        private readonly IFileService _fileService;

        public ContractController(IContractService contractService, IFileService fileService)
        {
            _contractService = contractService;
            _fileService = fileService;
        }

        private static ContractResponseDto Deny(string message) =>
            new() { Message = message, Status = false };

        // ── Contracts ────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all contracts, with optional filtering by start-date range and/or status.
        /// </summary>
        /// <param name="startFrom">Earliest start date (inclusive). Omit to ignore.</param>
        /// <param name="startTo">Latest start date (inclusive). Omit to ignore.</param>
        /// <param name="status">Filter to a specific status. Omit to return all statuses.</param>
        /// <returns>A list of matching contracts.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<Contract>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<Contract>>> GetAll(
            [FromQuery] DateTime? startFrom,
            [FromQuery] DateTime? startTo,
            [FromQuery] ContractStatus? status)
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var results = await _contractService.SearchContracts(startFrom, startTo, status);
            return Ok(results);
        }

        /// <summary>Returns a single contract by its ID.</summary>
        /// <param name="id">The contract ID.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractResponseDto>> GetById(int id)
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var result = await _contractService.GetContract(id);
            return result == null ? NotFound() : Ok(result);
        }

        /// <summary>Creates a new contract. Requires Manager or Admin role.</summary>
        /// <param name="contract">Contract data.</param>
        [HttpPost]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ContractResponseDto>> Create([FromBody] Contract contract)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            var result = await _contractService.CreateContract(contract);
            return Ok(result);
        }

        /// <summary>Updates an existing contract's details. Requires Manager or Admin role.</summary>
        /// <param name="id">The contract ID to update.</param>
        /// <param name="contract">Updated contract data.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ContractResponseDto>> Update(int id, [FromBody] Contract contract)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            var result = await _contractService.UpdateContract(id, contract);
            return Ok(result);
        }

        /// <summary>
        /// Changes the status of a contract (e.g. approve/decline/put on hold).
        /// Requires Manager or Admin role.
        /// </summary>
        /// <param name="id">The contract ID.</param>
        /// <param name="body">Object containing the new <see cref="ContractStatus"/> value.</param>
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ContractResponseDto>> SetStatus(
            int id, [FromBody] StatusUpdateDto body)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            var result = await _contractService.SetContractStatus(id, body.Status);
            return Ok(result);
        }

        /// <summary>Deletes a contract and its service requests. Requires Admin role.</summary>
        /// <param name="id">The contract ID to delete.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ContractResponseDto>> Delete(int id)
        {
            if (!this.IsAdmin())
                return Ok(Deny("Access denied – Admins only."));

            var result = await _contractService.DeleteContract(id);
            return result == null ? NotFound() : Ok(result);
        }

        // ── Signed Agreements ────────────────────────────────────────────────────

        /// <summary>
        /// Uploads a signed PDF agreement and associates it with the contract.
        /// Requires Manager or Admin role. Maximum file size: 20 MB.
        /// </summary>
        /// <param name="contractId">The contract to attach the agreement to.</param>
        /// <param name="file">The signed PDF file.</param>
        [HttpPost("{contractId:int}/agreement")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ContractResponseDto), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ContractResponseDto>> UploadAgreement(
            int contractId, IFormFile file)
        {
            if (!this.IsManager())
                return Ok(Deny("Access denied – Managers and Admins only."));

            try
            {
                var savedPath = await _fileService.SaveSignedAgreement(file);
                var result = await _contractService.SaveSignedAgreement(contractId, savedPath, file.FileName);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ContractResponseDto { Message = ex.Message, Status = false });
            }
        }

        /// <summary>Downloads the signed PDF agreement for a contract.</summary>
        /// <param name="contractId">The contract whose agreement to download.</param>
        [HttpGet("{contractId:int}/agreement")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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

        // ── Service Requests ─────────────────────────────────────────────────────

        /// <summary>Returns all service requests for the given contract.</summary>
        /// <param name="contractId">The contract ID.</param>
        [HttpGet("{contractId:int}/requests")]
        [ProducesResponseType(typeof(List<ServiceRequest>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<ServiceRequest>>> GetRequests(int contractId)
        {
            if (!this.IsLoggedIn())
                return Ok(Deny("Access denied – please log in."));

            var results = await _contractService.GetServiceRequestsByContract(contractId);
            return Ok(results);
        }

        /// <summary>
        /// Submits a new service request against an Active contract.
        /// The contract must be in Active status.
        /// </summary>
        /// <param name="request">Service request details including ContractId and description.</param>
        [HttpPost("requests")]
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceRequestResponseDto>> SubmitRequest(
            [FromBody] ServiceRequest request)
        {
            if (!this.IsLoggedIn())
                return Ok(new ServiceRequestResponseDto { Message = "Access denied – please log in.", Status = false });

            var result = await _contractService.CreateServiceRequest(request);
            return Ok(result);
        }

        /// <summary>
        /// Updates the status of a service request (e.g. approve or decline).
        /// Requires Manager or Admin role.
        /// </summary>
        /// <param name="requestId">The service request ID.</param>
        /// <param name="body">Object containing the new <see cref="ServiceRequestStatus"/> value.</param>
        [HttpPatch("requests/{requestId:int}/status")]
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<ServiceRequestResponseDto>> SetRequestStatus(
            int requestId, [FromBody] RequestStatusUpdateDto body)
        {
            if (!this.IsManager())
                return Ok(new ServiceRequestResponseDto
                {
                    Message = "Access denied – Managers and Admins only.",
                    Status = false
                });

            var result = await _contractService.SetServiceRequestStatus(requestId, body.Status);
            return Ok(result);
        }

        /// <summary>
        /// Deletes a service request. The request must be Rejected or Completed first.
        /// Requires Admin role.
        /// </summary>
        /// <param name="requestId">The service request ID to delete.</param>
        [HttpDelete("requests/{requestId:int}")]
        [ProducesResponseType(typeof(ServiceRequestResponseDto), StatusCodes.Status200OK)]
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
