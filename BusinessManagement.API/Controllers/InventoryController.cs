using App.Helpers;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IInventoryService inventoryService, ILogger<InventoryController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        [HttpGet("inventory-item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryItem([FromQuery]Guid id)
        {
            try
            {
                var inventoryItem = await _inventoryService.GetInventoryItem(id);

                if (inventoryItem == null)
                {
                   _logger.LogWarning("{trace} InventoryItem was null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return Ok(inventoryItem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("inventory-items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInventoryItems([FromQuery] Guid userId, Guid businessId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                List<GetAllInventoryItemsResponse> inventoryItems = await _inventoryService.GetAllInventoryItems(userId, businessId);

                if (inventoryItems == null) 
                {
                    _logger.LogWarning("{trace} inventoryItems were null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return Ok(inventoryItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("add-inventory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddInventoryItem(AddInventoryItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            try
            {
                var response = await _inventoryService.AddInventoryItem(request);

                if (response == null || !response.Success)
                {
                    _logger.LogWarning("{trace} response was null", LogHelper.TraceLog());
                    return BadRequest();
                }

                return CreatedAtAction(nameof(AddInventoryItem), new { id = response.Data });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
