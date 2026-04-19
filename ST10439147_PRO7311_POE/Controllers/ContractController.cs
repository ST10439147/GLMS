using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Extensions;
using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;

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

    private static ContractResponseDto Deny(string message) =>
        new() { Message = message, Status = false };

    [HttpPost("Create")]
    public async Task<ActionResult<ContractResponseDto>> Create([FromBody] Contract contract)
    {
        if (!this.IsManager())
            return Ok(Deny("Access denied – Managers and Admins only."));

        var result = await _contractService.CreateContract(contract);
        return Ok(result);
    }

    [HttpGet("Get/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Get(int id)
    {
        if (!this.IsLoggedIn())
            return Ok(Deny("Access denied – please log in."));

        var result = await _contractService.GetContract(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("All")]
    public async Task<ActionResult<List<Contract>>> All()
    {
        if (!this.IsLoggedIn())
            return Ok(Deny("Access denied – please log in."));

        var contracts = await _contractService.GetAllContracts();
        return Ok(contracts);
    }

    [HttpPut("Update/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Update(int id, [FromBody] Contract contract)
    {
        if (!this.IsManager())
            return Ok(Deny("Access denied – Managers and Admins only."));

        var result = await _contractService.UpdateContract(id, contract);
        return Ok(result);
    }

    [HttpPut("SetStatus/{id}/{status}")]
    public async Task<ActionResult<ContractResponseDto>> SetStatus(int id, ContractStatus status)
    {
        if (!this.IsManager())
            return Ok(Deny("Access denied – Managers and Admins only."));

        var result = await _contractService.SetContractStatus(id, status);
        return Ok(result);
    }

    [HttpDelete("Delete/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Delete(int id)
    {
        if (!this.IsAdmin())
            return Ok(Deny("Access denied – Admins only."));

        var result = await _contractService.DeleteContract(id);
        return result == null ? NotFound() : Ok(result);
    }

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

    [HttpPost("UploadAgreement/{contractId}")]
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

    [HttpPost("SubmitRequest")]
    public async Task<ActionResult<ServiceRequestResponseDto>> SubmitRequest(
        [FromBody] ServiceRequest request)
    {
        if (!this.IsLoggedIn())
            return Ok(new ServiceRequestResponseDto { Message = "Access denied – please log in.", Status = false });

        var result = await _contractService.CreateServiceRequest(request);
        return Ok(result);
    }

    [HttpGet("ServiceRequests/{contractId}")]
    public async Task<ActionResult<List<ServiceRequest>>> GetServiceRequests(int contractId)
    {
        if (!this.IsLoggedIn())
            return Ok(Deny("Access denied – please log in."));

        var results = await _contractService.GetServiceRequestsByContract(contractId);
        return Ok(results);
    }

    // Manager+ required: update a service request's status
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