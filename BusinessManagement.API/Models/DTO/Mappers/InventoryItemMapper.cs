using App.Models.DTO.Requests;
using App.Models.ValueObjects;

namespace App.Models.DTO.Mappers
{
    public class InventoryItemMapper
    {
        public static InventoryItem FromRequest(AddInventoryItemRequest req)
        {
            var item = new Item(req.Name, req.Description, req.Cost, req.Quantity, req.ExpirationDate, req.Category, req.ItemWeightG);
            var itemDetail = new ItemDetail(req.SKU, req.SerialNumber, req.Supplier, req.Brand, req.Model);

            // Map properties from the request to the InventoryItem constructor
            return new InventoryItem(
                Guid.NewGuid(),
                req.PurchaseDate,
                req.ReorderQuantity,
                req.CustomPackageId,
                req.Location,
                req.IsListed,
                req.IsLot,
                req.Notes,
                item,
                itemDetail
            );
        }
    }
}
