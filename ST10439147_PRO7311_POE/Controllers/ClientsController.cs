using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.Extensions;
using ST10439147_PRO7311_POE.Services;

[ApiController]
[Route("api/v1/[controller]")]
[IgnoreAntiforgeryToken]
public class ClientsController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientsController(IClientService clientService)
    {
        _clientService = clientService;
    }

    // GET api/v1/clients/All
    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var clients = await _clientService.GetAllClients();
        return Ok(clients);
    }

    // GET api/v1/clients/Get/{id}
    [HttpGet("Get/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var client = await _clientService.GetClient(id);
        return client == null ? NotFound() : Ok(client);
    }

    // POST api/v1/clients/Create
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ST10439147_PRO7311_POE.Models.Clients client)
    {
        if (!this.IsManager())
            return Ok(new { Message = "Access denied.", Status = false });

        var result = await _clientService.CreateClient(client);
        return Ok(result);
    }

    // PUT api/v1/clients/Update/{id}
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] ST10439147_PRO7311_POE.Models.Clients updated)
    {
        if (!this.IsManager())
            return Ok(new { Message = "Access denied.", Status = false });

        var result = await _clientService.UpdateClient(id, updated);
        return result.Status ? Ok(result) : NotFound(result);
    }

    // DELETE api/v1/clients/Delete/{id}
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!this.IsAdmin())
            return Ok(new { Message = "Access denied – Admins only.", Status = false });

        var result = await _clientService.DeleteClient(id);
        return result.Status ? Ok(result) : NotFound(result);
    }
}