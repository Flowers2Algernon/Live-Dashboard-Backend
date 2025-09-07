using Microsoft.AspNetCore.Mvc;
using LERD.Application.Interfaces;
using LERD.Shared.DTOs;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionsController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _subscriptionService.GetAllAsync(page, pageSize);
                
                return Ok(new ApiResponse<PagedResult<SubscriptionDto>>
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var subscription = await _subscriptionService.GetByIdAsync(id);
                if (subscription == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Subscription not found"
                    });
                }
                
                return Ok(new ApiResponse<SubscriptionDto>
                {
                    Success = true,
                    Data = subscription
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSubscriptionRequest request)
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
                var subscription = await _subscriptionService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = subscription.Id }, 
                    new ApiResponse<SubscriptionDto>
                    {
                        Success = true,
                        Data = subscription
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

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSubscriptionRequest request)
        {
            try
            {
                var subscription = await _subscriptionService.UpdateAsync(id, request);
                return Ok(new ApiResponse<SubscriptionDto>
                {
                    Success = true,
                    Data = subscription
                });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new ApiResponse<object>
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _subscriptionService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Subscription not found"
                    });
                }
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Subscription cancelled successfully"
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

        // Organisation-specific subscription endpoints
        [HttpGet("organisation/{organisationId}")]
        public async Task<IActionResult> GetByOrganisation(Guid organisationId)
        {
            try
            {
                var subscriptions = await _subscriptionService.GetSubscriptionsByOrganisationAsync(organisationId);
                
                return Ok(new ApiResponse<IEnumerable<SubscriptionDto>>
                {
                    Success = true,
                    Data = subscriptions
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

        [HttpGet("organisation/{organisationId}/active")]
        public async Task<IActionResult> GetActiveByOrganisation(Guid organisationId)
        {
            try
            {
                var subscription = await _subscriptionService.GetActiveSubscriptionByOrganisationAsync(organisationId);
                if (subscription == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "No active subscription found for this organisation"
                    });
                }
                
                return Ok(new ApiResponse<SubscriptionDto>
                {
                    Success = true,
                    Data = subscription
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

        [HttpGet("organisation/{organisationId}/status")]
        public async Task<IActionResult> CheckOrganisationSubscriptionStatus(Guid organisationId)
        {
            try
            {
                var status = await _subscriptionService.CheckSubscriptionStatusAsync(organisationId);
                
                return Ok(new ApiResponse<SubscriptionStatusResponse>
                {
                    Success = true,
                    Data = status
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

        // Admin endpoint for creating/updating organisation subscriptions
        [HttpPost("organisation/{organisationId}")]
        public async Task<IActionResult> CreateOrUpdateOrganisationSubscription(
            Guid organisationId, 
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
                var subscription = await _subscriptionService.CreateOrUpdateOrganisationSubscriptionAsync(organisationId, request);
                
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

        // Utility endpoint for updating expired subscriptions
        [HttpPost("update-expired")]
        public async Task<IActionResult> UpdateExpiredSubscriptions()
        {
            try
            {
                var updatedCount = await _subscriptionService.UpdateExpiredSubscriptionsAsync();
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Data = new { UpdatedCount = updatedCount },
                    Message = $"Updated {updatedCount} expired subscriptions"
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