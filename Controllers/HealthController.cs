using Microsoft.AspNetCore.Mvc;

namespace NurseryManagementSystem.API.Controllers
{
    /// <summary>
    /// Health check endpoint for monitoring and orchestration
    /// </summary>
    [ApiController]
    [Route("health")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HealthController : ControllerBase
    {
        /// <summary>
        /// Health check endpoint
        /// </summary>
        /// <returns>Returns OK status if service is healthy</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new 
            { 
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "Nursery Management System API",
                version = "1.0"
            });
        }

        /// <summary>
        /// Ready check endpoint (checks database connectivity)
        /// </summary>
        /// <returns>Returns OK if service is ready to serve requests</returns>
        [HttpGet("ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Ready([FromServices] AppDbContext dbContext)
        {
            try
            {
                // Check database connectivity
                var canConnect = await dbContext.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                        new { status = "not ready", reason = "Database connection failed" });
                }

                return Ok(new 
                { 
                    status = "ready",
                    timestamp = DateTime.UtcNow,
                    database = "connected"
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { status = "not ready", reason = "Database not accessible" });
            }
        }

        /// <summary>
        /// Live check endpoint (quick response)
        /// </summary>
        /// <returns>Returns OK if service is running</returns>
        [HttpGet("live")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Live()
        {
            return Ok(new 
            { 
                status = "live",
                timestamp = DateTime.UtcNow
            });
        }
    }
}
