using App.Models.DTO.Responses;
using App.Repositories;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/businesses")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly ILogger<BusinessController> _logger;
        public BusinessController(IBusinessService businessService, ILogger<BusinessController> logger)
        {
            _businessService = businessService;
            _logger = logger;
        }

        [HttpPost("createBusiness")]
        [Authorize("create:new-business")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateNewBusiness(CreateNewBusinessResponse request)
        {
            try
            {
                var result = await _businessService.CreateNewBusiness(request);

                if (result == null || !result.Success)
                {
                    _logger.LogWarning("{trace} New business creation failed", LogHelper.TraceLog());
                    return BadRequest(result?.ErrorMessage ?? "New business creation failed");
                }

                return CreatedAtAction(nameof(CreateNewBusiness), result.Data);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
