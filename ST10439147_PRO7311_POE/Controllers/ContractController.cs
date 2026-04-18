using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Models;
using ST10439147_PRO7311_POE.Services;

[ApiController]
[Route("api/v1/[controller]")]
[IgnoreAntiforgeryToken]
public class ContractController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly IFileService _fileService;
    private readonly LoginDTO _loginDto;

    public ContractController(IContractService contractService, IFileService fileService, LoginDTO loginDto)
    {
        _contractService = contractService;
        _fileService = fileService;
        _loginDto = loginDto;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────
    private bool IsLoggedIn => _loginDto.AccessLevel > 0;
    private bool IsManager => _loginDto.AccessLevel == (int)UserRole.Manager
                              || _loginDto.AccessLevel == (int)UserRole.Admin;
    private bool IsAdmin => _loginDto.AccessLevel == (int)UserRole.Admin;

    private static ContractResponseDto Deny(string message) =>
        new() { Message = message, Status = false };

    // ── Endpoints ─────────────────────────────────────────────────────────────

    // Manager+ required: create a contract
    [HttpPost("Create")]
    public async Task<ActionResult<ContractResponseDto>> Create([FromBody] Contract contract)
    {
        if (!IsManager)
            return Ok(Deny("Access denied – Managers and Admins only."));

        var result = await _contractService.CreateContract(contract);
        return Ok(result);
    }

    // Any logged-in user: view a single contract
    [HttpGet("Get/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Get(int id)
    {
        if (!IsLoggedIn)
            return Ok(Deny("Access denied – please log in."));

        var result = await _contractService.GetContract(id);
        return result == null ? NotFound() : Ok(result);
    }

    // Any logged-in user: view all contracts
    [HttpGet("All")]
    public async Task<ActionResult<List<Contract>>> All()
    {
        if (!IsLoggedIn)
            return Ok(Deny("Access denied – please log in."));

        var contracts = await _contractService.GetAllContracts();
        return Ok(contracts);
    }

    // Manager+ required: edit a contract
    [HttpPut("Update/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Update(int id, [FromBody] Contract contract)
    {
        if (!IsManager)
            return Ok(Deny("Access denied – Managers and Admins only."));

        var result = await _contractService.UpdateContract(id, contract);
        return Ok(result);
    }

    // Manager+ required: change contract status
    [HttpPut("SetStatus/{id}/{status}")]
    public async Task<ActionResult<ContractResponseDto>> SetStatus(int id, ContractStatus status)
    {
        if (!IsManager)
            return Ok(Deny("Access denied – Managers and Admins only."));

        var result = await _contractService.SetContractStatus(id, status);
        return Ok(result);
    }

    // Admin only: delete a contract
    [HttpDelete("Delete/{id}")]
    public async Task<ActionResult<ContractResponseDto>> Delete(int id)
    {
        if (!IsAdmin)
            return Ok(Deny("Access denied – Admins only."));

        var result = await _contractService.DeleteContract(id);
        return result == null ? NotFound() : Ok(result);
    }

    // Any logged-in user: search/filter contracts
    [HttpGet("Search")]
    public async Task<ActionResult<List<Contract>>> Search(
        [FromQuery] DateTime? startFrom,
        [FromQuery] DateTime? startTo,
        [FromQuery] ContractStatus? status)
    {
        if (!IsLoggedIn)
            return Ok(Deny("Access denied – please log in."));

        var results = await _contractService.SearchContracts(startFrom, startTo, status);
        return Ok(results);
    }

    // Manager+ required: upload signed agreement PDF
    [HttpPost("UploadAgreement/{contractId}")]
    public async Task<ActionResult<ContractResponseDto>> UploadAgreement(
        int contractId, IFormFile file)
    {
        if (!IsManager)
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

    // Any logged-in user: download a signed agreement
    [HttpGet("DownloadAgreement/{contractId}")]
    public async Task<IActionResult> DownloadAgreement(int contractId)
    {
        if (!IsLoggedIn)
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

    // Any logged-in user: submit a service request
    [HttpPost("SubmitRequest")]
    public async Task<ActionResult<ServiceRequestResponseDto>> SubmitRequest(
        [FromBody] ServiceRequest request)
    {
        if (!IsLoggedIn)
            return Ok(new ServiceRequestResponseDto { Message = "Access denied – please log in.", Status = false });

        var result = await _contractService.CreateServiceRequest(request);
        return Ok(result);
    }

    // Any logged-in user: view service requests for a contract
    [HttpGet("ServiceRequests/{contractId}")]
    public async Task<ActionResult<List<ServiceRequest>>> GetServiceRequests(int contractId)
    {
        if (!IsLoggedIn)
            return Ok(Deny("Access denied – please log in."));

        var results = await _contractService.GetServiceRequestsByContract(contractId);
        return Ok(results);
    }
}