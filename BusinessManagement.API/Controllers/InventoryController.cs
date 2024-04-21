using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [ApiController]
    [Route("api/inventory-items")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryController> _logger;
        private readonly IValidator<UpdateInventoryItemRequest> _updateInventoryValidator;
        private readonly IValidator<AddInventoryItemRequest> _addInventoryValidator;

        public InventoryController(IInventoryService inventoryService,
            ILogger<InventoryController> logger,
            IValidator<UpdateInventoryItemRequest> updateInventoryValidator,
            IValidator<AddInventoryItemRequest> addInventoryValidator)
        {
            _inventoryService = inventoryService;
            _updateInventoryValidator = updateInventoryValidator;
            _addInventoryValidator = addInventoryValidator;
            _logger = logger;
        }

        [HttpGet("{uuid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetInventoryItem(Guid uuid)
        {
            if (Guid.Empty == uuid)
            {
                _logger.LogWarning("{trace} uuid was empty", LogHelper.TraceLog());
                return BadRequest();
            }

            try
            {
                var inventoryItem = await _inventoryService.GetInventoryItem(uuid);

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

        [HttpGet("user/{userId}/business/{businessId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllInventoryItems(Guid userId, Guid businessId)
        {
            if (Guid.Empty == userId || Guid.Empty == businessId)
            {
                _logger.LogWarning("{trace} a uuid was empty", LogHelper.TraceLog());
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

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddInventoryItem(AddInventoryItemRequest request)
        {
            ValidationResult validationResult = _addInventoryValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{trace} validation failed: {err}",
                    LogHelper.TraceLog(), LogHelper.ErrorList(validationResult));

                return BadRequest(validationResult.Errors);
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

        [HttpDelete("{uuid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveInventoryItem(Guid uuid)
        {
            try
            {
                var response = await _inventoryService.RemovedItemResults(uuid);

                if (response == null || !response.Success) 
                {
                    _logger.LogWarning("{trace} response was null", LogHelper.TraceLog());
                    return BadRequest(response?.ErrorMessage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateInventoryItem(UpdateInventoryItemRequest request)
        {
            ValidationResult validationResult = _updateInventoryValidator.Validate(request);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning("{trace} validation failed: {err}",
                    LogHelper.TraceLog(), LogHelper.ErrorList(validationResult)); 

                return BadRequest(validationResult.Errors);
            }

            try
            {
                var response = await _inventoryService.UpdatedItemResults(request);

                if (response == null || !response.Success)
                {
                    _logger.LogWarning("{trace} response was null", LogHelper.TraceLog());
                    return BadRequest(response?.ErrorMessage);
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}