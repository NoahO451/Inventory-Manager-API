using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpGet("inventory-item")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryItem([FromQuery] Guid id)
        {
            try
            {
                var inventoryItem = await _inventoryService.GetInventoryItem(id);

                if (inventoryItem == null)
                {
                    return BadRequest();
                }

                return Ok(inventoryItem);
            }
            catch (Exception)
            {
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
                    return BadRequest();
                }

                return Ok(inventoryItems);
            }
            catch (Exception)
            {
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
                    return BadRequest();
                }

                return CreatedAtAction(nameof(AddInventoryItem), new { id = response.Data });
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete("remove-inventory-item")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveInventoryItem([FromQuery] Guid uuid)
        {
            try
            {
                var response = await _inventoryService.RemovedItemResults(uuid);

                if (response == null || !response.Success) 
                { 
                    return BadRequest(response?.ErrorMessage);
                }

                return NoContent();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
