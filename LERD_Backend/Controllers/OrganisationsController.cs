using Microsoft.AspNetCore.Mvc;
using LERD.Application.Interfaces;
using LERD.Shared.DTOs;

namespace LERD_Backend.Controllers
{
    [ApiController]
    [Route("api/organizations")]
    public class OrganisationsController : ControllerBase
    {
        private readonly IOrganisationService _organisationService;

        public OrganisationsController(IOrganisationService organisationService)
        {
            _organisationService = organisationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] bool includeCount = false)
        {
            try
            {
                // 如果没有显式请求includeCount，则检查service是否有重载方法
                PagedResult<OrganisationDto> result;
                if (includeCount)
                {
                    // 强制获取准确的总数（可能较慢）
                    result = await _organisationService.GetAllAsync(page, pageSize, true);
                }
                else
                {
                    // 使用快速模式（估计总数或不获取总数）
                    result = await _organisationService.GetAllAsync(page, pageSize);
                }
                
                return Ok(new ApiResponse<PagedResult<OrganisationDto>>
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
                var organisation = await _organisationService.GetByIdAsync(id);
                if (organisation == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Organisation not found"
                    });
                }
                
                return Ok(new ApiResponse<OrganisationDto>
                {
                    Success = true,
                    Data = organisation
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
        public async Task<IActionResult> Create([FromBody] CreateOrganisationRequest request)
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
                var organisation = await _organisationService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = organisation.Id }, 
                    new ApiResponse<OrganisationDto>
                    {
                        Success = true,
                        Data = organisation
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
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrganisationRequest request)
        {
            try
            {
                var organisation = await _organisationService.UpdateAsync(id, request);
                return Ok(new ApiResponse<OrganisationDto>
                {
                    Success = true,
                    Data = organisation
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
                var deleted = await _organisationService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Organisation not found"
                    });
                }
                
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Organisation deleted successfully"
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