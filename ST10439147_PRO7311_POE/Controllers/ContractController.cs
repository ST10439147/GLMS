using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;

[ApiController]
[Route("api/v1/[controller]")]
public class ContractController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IFileService _fileService;

    public ContractController(IContractService contractService, IFileService fileService)
    {
        _contractService = contractService;
        _fileService = fileService;
    }

    [HttpPost("Create")]
    public async Task<ActionResult<ContractResponseDto>> Create([FromBody] Contract contract)
    {
        var result = await _contractService.CreateContract(contract);
        return Ok(result);
    }

    [HttpGet("Get/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Get(int id)
    {
        var result = await _contractService.GetContract(id);
        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("All")]
    public async Task<ActionResult<List<Contract>>> All()
    {
        var contracts = await _contractService.GetAllContracts();
        return Ok(contracts);
    }

    [HttpPut("Update/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Update(int id, [FromBody] Contract contract)
    {
        var result = await _contractService.UpdateContract(id, contract);
        return Ok(result);
    }

    [HttpPut("SetStatus/{id}/{status}")]
    public async Task<ActionResult<ContractResponseDto>> SetStatus(int id, ContractStatus status)
    {
        var result = await _contractService.SetContractStatus(id, status);
        return Ok(result);
    }

    [HttpDelete("Delete/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Delete(int id)
    {
        var result = await _contractService.DeleteContract(id);
        return result == null ? NotFound() : Ok(result);
    }

    // ── Search/Filter (LINQ) ───────────────────────────────────────────────────
    [HttpGet("Search")]
    public async Task<ActionResult<List<Contract>>> Search(
        [FromQuery] DateTime? startFrom,
        [FromQuery] DateTime? startTo,
        [FromQuery] ContractStatus? status)
    {
        var results = await _contractService.SearchContracts(startFrom, startTo, status);
        return Ok(results);
    }

    // ── File upload: PDF Signed Agreement ──────────────────────────────────────
    [HttpPost("UploadAgreement/{contractId}")]
    public async Task<ActionResult<ContractResponseDto>> UploadAgreement(
        int contractId, IFormFile file)
    {
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

    // ── File download ──────────────────────────────────────────────────────────
    [HttpGet("DownloadAgreement/{contractId}")]
    public async Task<IActionResult> DownloadAgreement(int contractId)
    {
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

    // ── Service Requests ───────────────────────────────────────────────────────
    [HttpPost("SubmitRequest")]
    public async Task<ActionResult<ServiceRequestResponseDto>> SubmitRequest(
        [FromBody] ServiceRequest request)
    {
        var result = await _contractService.CreateServiceRequest(request);
        return Ok(result);
    }

    [HttpGet("ServiceRequests/{contractId}")]
    public async Task<ActionResult<List<ServiceRequest>>> GetServiceRequests(int contractId)
    {
        var results = await _contractService.GetServiceRequestsByContract(contractId);
        return Ok(results);
    }
}
