using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE.Extensions;
using ST10439147_PRO7311_POE.Services;

// Represents an API controller that manages client resources, providing endpoints to retrieve, create, update, and delete clients.
// This controller exposes RESTful endpoints for client management operations.
// Access to certain actions, such as creating, updating, or deleting clients, may be restricted based on the user's role (e.g., manager
// or admin). All endpoints are prefixed with 'api/v1/clients'.
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
    // Retrieves a list of all clients. This endpoint is accessible to all authenticated users.
    // Returns a 200 OK response containing the list of clients.
    [HttpGet("All")]
    public async Task<IActionResult> All()
    {
        var clients = await _clientService.GetAllClients();
        return Ok(clients);
    }

    // GET api/v1/clients/Get/{id}
    // Retrieves a specific client by its unique identifier. This endpoint is accessible to all authenticated users.
    // If the client with the specified ID exists, returns a 200 OK response containing the client data; otherwise, returns a 404 Not Found response.
    [HttpGet("Get/{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var client = await _clientService.GetClient(id);
        return client == null ? NotFound() : Ok(client);
    }

    // POST api/v1/clients/Create
    // Creates a new client resource. This endpoint is restricted to users with a manager role.
    // Accepts a client object in the request body and attempts to create a new client. If the user does not have the required role, returns a 200 OK response with an access denied message; otherwise, returns a 200 OK response containing the result of the creation operation.
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] ST10439147_PRO7311_POE.Models.Clients client)
    {
        if (!this.IsManager())
            return Ok(new { Message = "Access denied.", Status = false });

        var result = await _clientService.CreateClient(client);
        return Ok(result);
    }

    // PUT api/v1/clients/Update/{id}
    // Updates an existing client resource identified by its unique identifier. This endpoint is restricted to users with a manager role.
    // Accepts an updated client object in the request body and attempts to update the existing client. If the user does not have the required role, returns a 200 OK response with an access denied message; if the client with the specified ID does not exist, returns a 404 Not Found response; otherwise, returns a 200 OK response containing the result of the update operation.
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
    // Deletes an existing client resource identified by its unique identifier. This endpoint is restricted to users with an admin role.
    // If the user does not have the required role, returns a 200 OK response with an access denied message; if the client with the specified ID does not exist, returns a 404 Not Found response; otherwise, returns a 200 OK response containing the result of the deletion operation.
    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!this.IsAdmin())
            return Ok(new { Message = "Access denied – Admins only.", Status = false });

        var result = await _clientService.DeleteClient(id);
        return result.Status ? Ok(result) : NotFound(result);
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------