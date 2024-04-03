using App.Helpers;
using App.Models;
using App.Models.DTO.Requests;
using App.Models.DTO.Responses;
using Azure.Core;
using Dapper;

namespace App.Repositories
{
    public interface IInventoryRepository
    {
        Task<InventoryItem> GetInventoryItem(long id);
        Task<List<InventoryItem>> RetrieveAllInventoryItems(GetAllInventoryItemsRequest request);
        Task<long> CreateInventoryItem(AddInventoryItemRequest request);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private DataContext _context;

        public InventoryRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<InventoryItem> GetInventoryItem(long id)
        {
            InventoryItem result = new InventoryItem();

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                string sql = """
                        SELECT 
                        ii.inventory_item_id,
                        ii.name,
                        ii.description,
                        ii.sku,
                        ii.cost,
                        ii.serial_number,
                        ii.purchased_date,
                        ii.supplier,
                        ii.brand,
                        ii.model,
                        ii.quantity,
                        ii.reorder_quantity,
                        ii.location,
                        ii.expiration_date,
                        ii.category,
                        ii.packaging,
                        ii.item_weight,
                        ii.is_listed,
                        ii.is_lot,
                        ii.notes
                    FROM 
                        inventory_item ii
                    WHERE ii.inventory_item_id = @Id;
                    """;

                result = await connection.QuerySingleAsync<InventoryItem>(sql, new { Id = id });
            }

            return result;
        }

        public async Task<List<InventoryItem>> RetrieveAllInventoryItems(GetAllInventoryItemsRequest request)
        {
            List<InventoryItem> inventoryItems = new List<InventoryItem>();

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                string sql = """
                        SELECT 
                        ii.inventory_item_id,
                        ii.name,
                        ii.description,
                        ii.sku,
                        ii.cost,
                        ii.serial_number,
                        ii.purchased_date,
                        ii.supplier,
                        ii.brand,
                        ii.model,
                        ii.quantity,
                        ii.reorder_quantity,
                        ii.location,
                        ii.expiration_date,
                        ii.category,
                        ii.packaging,
                        ii.item_weight,
                        ii.is_listed,
                        ii.is_lot,
                        ii.notes,
                        b.business_id,
                        b.business_name 
                    FROM 
                        inventory_item ii
                    LEFT JOIN 
                        business_inventory_item bii ON ii.inventory_item_id = bii.inventory_item_id
                    LEFT JOIN 
                        business b ON bii.business_id = b.business_id
                    WHERE bii.business_id = @BusinessId;
                    """;

                var result = await connection.QueryAsync<InventoryItem>(sql, new { BusinessId = request.BusinessId });
                inventoryItems = result.ToList();
            }

            return inventoryItems;
        }

        public async Task<long> CreateInventoryItem(AddInventoryItemRequest request)
        {
            long inventoryItemId = -1;

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    string insertInventoryItemSql = """
                        INSERT INTO inventory_item (name, description, sku, cost, serial_number, purchased_date, supplier, brand, model, quantity, reorder_quantity, location, expiration_date, category, packaging, item_weight, is_listed, is_lot, notes)
                        VALUES (@Name, @Description, @Sku, @Cost, @SerialNumber, @PurchasedDate, @Supplier, @Brand, @Model, @Quantity, @ReorderQuantity, @Location, @ExpirationDate, @Category, @Packaging, @ItemWeight, @IsListed, @IsLot, @Notes)
                        RETURNING inventory_item_id; 
                        """;

                    inventoryItemId = await connection.QueryFirstAsync<long>(insertInventoryItemSql, request);

                    string insertBusinessInventoryItemSql = """
                        INSERT INTO business_inventory_item (inventory_item_id, business_id)
                        VALUES (@InventoryItemId, @BusinessId);
                        """;

                    await connection.ExecuteAsync(insertBusinessInventoryItemSql, new { InventoryItemId = inventoryItemId, BusinessId = request.BusinessId });

                    transaction.Commit();
                }
            }

            return inventoryItemId;
        }
    }
}