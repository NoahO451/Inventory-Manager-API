using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using App.Repositories;
using Azure;

namespace App.Services
{
    public interface IInventoryService
    {
        Task<GetInventoryItemResponse> GetInventoryItem(long id);
        Task<List<GetAllInventoryItemsResponse>> GetAllInventoryItems(GetAllInventoryItemsRequest request);
        Task<ApiResponse<long>> AddInventoryItem(AddInventoryItemRequest request);
    }

    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<GetInventoryItemResponse> GetInventoryItem(long id)
        {
            InventoryItem item = await _inventoryRepository.GetInventoryItem(id);

            if (item == null)
            {
                return new GetInventoryItemResponse();
            }

            var response = new GetInventoryItemResponse
            {
                InventoryItemId = item.InventoryItemId,
                Name = item.Name,
                Description = item.Description,
                SKU = item.SKU,
                Cost = item.Cost,
                SerialNumber = item.SerialNumber,
                PurchasedDate = item.PurchasedDate,
                Supplier = item.Supplier,
                Brand = item.Brand,
                Model = item.Model,
                Quantity = item.Quantity,
                ReorderQuantity = item.ReorderQuantity,
                Location = item.Location,
                ExpirationDate = item.ExpirationDate,
                Category = item.Category,
                Packaging = item.Packaging,
                ItemWeight = item.ItemWeight,
                IsListed = item.IsListed,
                IsLot = item.IsLot,
                Notes = item.Notes
            };

            return response;
        }

        public async Task<List<GetAllInventoryItemsResponse>> GetAllInventoryItems(GetAllInventoryItemsRequest request)
        {

            List<InventoryItem> inventoryItems = await _inventoryRepository.RetrieveAllInventoryItems(request);

            List<GetAllInventoryItemsResponse> response = new List<GetAllInventoryItemsResponse>();

            if (inventoryItems == null || inventoryItems.Count == 0)
            {
                return response;
            }

            foreach (InventoryItem item in inventoryItems)
            {
                response.Add(new GetAllInventoryItemsResponse
                {
                    InventoryItemId = item.InventoryItemId,
                    Name = item.Name,
                    Description = item.Description,
                    SKU = item.SKU,
                    Cost = item.Cost,
                    SerialNumber = item.SerialNumber,
                    PurchasedDate = item.PurchasedDate,
                    Supplier = item.Supplier,
                    Brand = item.Brand,
                    Model = item.Model,
                    Quantity = item.Quantity,
                    ReorderQuantity = item.ReorderQuantity,
                    Location = item.Location,
                    ExpirationDate = item.ExpirationDate,
                    Category = item.Category,
                    Packaging = item.Packaging,
                    ItemWeight = item.ItemWeight,
                    IsListed = item.IsListed,
                    IsLot = item.IsLot,
                    Notes = item.Notes
                });
            }

            return response;
        }

        public async Task<ApiResponse<long>> AddInventoryItem(AddInventoryItemRequest request)
        {
            try
            {
                // VALIDATE REQUEST HERE

                long inventoryId = await _inventoryRepository.CreateInventoryItem(request);

                ApiResponse<long> apiResponse = new ApiResponse<long>() { Data = inventoryId };

                if (inventoryId != -1 && await _inventoryRepository.GetInventoryItem(inventoryId) != null)
                {
                    apiResponse.Success = true;
                    return apiResponse;
                }

                apiResponse.Success = false;
                return apiResponse;
            }
            catch (Exception)
            {
                return new ApiResponse<long>() { Success = false};
            }
        }

    }
}
