using Microsoft.AspNetCore.Mvc;
using LERD.Application.Interfaces;
using LERD.Shared.DTOs;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/organizations")]
    public class OrganisationsSubscriptionController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;
        private readonly ISubscriptionService _subscriptionService;

        public OrganisationsSubscriptionController(
            IOrganisationService organisationService, 
            ISubscriptionService subscriptionService)
        {
            _organisationService = organisationService;
            _subscriptionService = subscriptionService;
        }

        // API endpoint from documentation: /api/organizations/{id}/subscription
        [HttpGet("{id}/subscription")]
        public async Task<IActionResult> GetOrganisationWithSubscription(Guid id)
        {
            try
            {
                var result = await _subscriptionService.GetOrganisationWithSubscriptionAsync(id);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Organisation not found"
                    });
                }
                
                return Ok(new ApiResponse<OrganisationWithSubscriptionDto>
                {
                    Success = true,
                    Data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }

        // API endpoint from documentation: /api/organizations/{id}/subscription (POST)
        [HttpPost("{id}/subscription")]
        public async Task<IActionResult> CreateOrUpdateOrganisationSubscription(
            Guid id, 
            [FromBody] CreateSubscriptionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            try
            {
                // Validate organisation exists
                var organisation = await _organisationService.GetByIdAsync(id);
                if (organisation == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Organisation not found"
                    });
                }

                // Set the organisation ID from the route
                request.OrganisationId = id;
                
                var subscription = await _subscriptionService.CreateOrUpdateOrganisationSubscriptionAsync(id, request);
                
                return Ok(new ApiResponse<SubscriptionDto>
                {
                    Success = true,
                    Data = subscription,
                    Message = "Organisation subscription created/updated successfully"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
        }
    }
}