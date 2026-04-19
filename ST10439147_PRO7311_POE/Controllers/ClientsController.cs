using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ST10439147_PRO7311_POE.Data;
using ST10439147_PRO7311_POE.DTOs;
using ST10439147_PRO7311_POE.Models;

[ApiController]
[Route("api/v1/[controller]")]
[IgnoreAntiforgeryToken]
public class ClientsController : ControllerBase
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory;
    private readonly LoginDTO _loginDto;

    public ClientsController(IDbContextFactory<AppDbContext> dbFactory, LoginDTO loginDto)
    {
        _dbFactory = dbFactory;
        _loginDto = loginDto;
    }

    private bool IsLoggedIn => _loginDto.AccessLevel > 0;
    private bool IsManager => _loginDto.AccessLevel == (int)UserRole.Manager
                              || _loginDto.AccessLevel == (int)UserRole.Admin;
    private bool IsAdmin => _loginDto.AccessLevel == (int)UserRole.Admin;

    // GET api/v1/clients/All
    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        using var ctx = _dbFactory.CreateDbContext();
        var clients = await ctx.Clients.OrderBy(c => c.Name).ToListAsync();
        return Ok(clients);
    }

    // GET api/v1/clients/Get/{id}
    [HttpGet("Get/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        using var ctx = _dbFactory.CreateDbContext();
        var client = await ctx.Clients.FindAsync(id);
        return client == null ? NotFound() : Ok(client);
    }

    // POST api/v1/clients/Create
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ST10439147_PRO7311_POE.Models.Clients client)
    {
        if (!IsManager) return Ok(new { Message = "Access denied.", Status = false });
        using var ctx = _dbFactory.CreateDbContext();
        client.Id = 0;
        ctx.Clients.Add(client);
        await ctx.SaveChangesAsync();
        return Ok(new { Client = client, Message = $"Client '{client.Name}' created.", Status = true });
    }

    // PUT api/v1/clients/Update/{id}
    [HttpPut("Update/{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ST10439147_PRO7311_POE.Models.Clients updated)
    {
        if (!IsManager) return Ok(new { Message = "Access denied.", Status = false });
        using var ctx = _dbFactory.CreateDbContext();
        var existing = await ctx.Clients.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Name = updated.Name;
        existing.ContactDetails = updated.ContactDetails;
        existing.Region = updated.Region;
        await ctx.SaveChangesAsync();
        return Ok(new { Client = existing, Message = $"Client '{existing.Name}' updated.", Status = true });
    }

    // DELETE api/v1/clients/Delete/{id}
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!IsAdmin) return Ok(new { Message = "Access denied – Admins only.", Status = false });
        using var ctx = _dbFactory.CreateDbContext();
        var client = await ctx.Clients.FindAsync(id);
        if (client == null) return NotFound();
        ctx.Clients.Remove(client);
        await ctx.SaveChangesAsync();
        return Ok(new { Message = $"Client '{client.Name}' deleted.", Status = true });
    }
}
