using LAB_12.DTOs;
using LAB_12.Services;
using Microsoft.AspNetCore.Mvc;

namespace LAB_12.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TripsController : ControllerBase
    {
        private readonly IDbService _dbService;

        public TripsController(IDbService dbService)
        {
            _dbService = dbService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetTrips([FromQuery] int page = 1 , [FromQuery] int pageSize = 10)
        {
            var result = await _dbService.GetTrips(page, pageSize);
            return Ok(result);
        }

        [HttpPost("{idTrip}/clients")]
        public async Task<IActionResult> AddClientToTrip([FromRoute] int idTrip, [FromBody] AddClientDTO client)
        {
            try
            {
                await _dbService.AddClientToTrip(idTrip, client);
                return Ok("Client added to Trip");
            }
            catch (KeyNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (InvalidOperationException e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
