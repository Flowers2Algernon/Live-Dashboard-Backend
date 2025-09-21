using Microsoft.AspNetCore.Mvc;
using LERD.Application.Interfaces;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;

        public TestController(IOrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        [HttpGet("organizations-simple")]
        public async Task<IActionResult> GetOrganizationsSimple()
        {
            try
            {
                var result = await _organisationService.GetAllAsync(1, 10);
                return Ok(new
                {
                    Success = true,
                    TotalCount = result.TotalCount,
                    ItemCount = result.Items.Count(),
                    FirstItem = result.Items.FirstOrDefault(),
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray(),
                    Timestamp = DateTime.Now
                });
            }
        }
    }
}
