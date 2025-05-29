using LAB_12.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LAB_12.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public ClientsController(IDbService dbService)
        {
            _dbService = dbService;
        }

        [HttpDelete("{clientId}")]
        public async Task<IActionResult> RemoveClient(int clientId)
        {
            try
            {
                await _dbService.RemoveClient(clientId);
                return Ok("Client Removed");
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
