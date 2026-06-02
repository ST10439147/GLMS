using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10439147_PRO7311_POE_Back.Extensions;
using ST10439147_PRO7311_POE_Back.Models;
using ST10439147_PRO7311_POE_Back.Services;

namespace ST10439147_PRO7311_POE_Back.Controllers
{
    /// <summary>
    /// Manages client resources. Managers and Admins can create and update clients;
    /// only Admins can delete. All authenticated users may read.
    /// </summary>
    [ApiController]
    [Route("api/clients")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        /// <summary>Returns all clients ordered by name.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<Clients>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _clientService.GetAllClients();
            return Ok(clients);
        }

        /// <summary>Returns a single client by ID.</summary>
        /// <param name="id">The client ID.</param>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Clients), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _clientService.GetClient(id);
            return client == null ? NotFound() : Ok(client);
        }

        /// <summary>Creates a new client. Requires Manager or Admin role.</summary>
        /// <param name="client">Client data.</param>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Create([FromBody] Clients client)
        {
            if (!this.IsManager())
                return Ok(new { Message = "Access denied – Managers and Admins only.", Status = false });

            var result = await _clientService.CreateClient(client);
            return Ok(result);
        }

        /// <summary>Updates an existing client. Requires Manager or Admin role.</summary>
        /// <param name="id">The client ID to update.</param>
        /// <param name="updated">Updated client data.</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromBody] Clients updated)
        {
            if (!this.IsManager())
                return Ok(new { Message = "Access denied – Managers and Admins only.", Status = false });

            var result = await _clientService.UpdateClient(id, updated);
            return result.Status ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Deletes a client. Requires Admin role.
        /// All contracts must be in a terminal status and all service requests resolved first.
        /// </summary>
        /// <param name="id">The client ID to delete.</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            if (!this.IsAdmin())
                return Ok(new { Message = "Access denied – Admins only.", Status = false });

            var result = await _clientService.DeleteClient(id);
            return result.Status ? Ok(result) : NotFound(result);
        }
    }
}
//--------------------------------------DDDDDoooooo End of File DDDDDoooooo--------------------------------------