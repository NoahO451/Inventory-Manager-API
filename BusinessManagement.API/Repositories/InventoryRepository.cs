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
        Task<InventoryItem> GetInventoryItem(Guid uuid);
        Task<List<InventoryItem>> RetrieveAllInventoryItems(int businessId);
        Task CreateInventoryItem(InventoryItem request);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private DataContext _context;

        public InventoryRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<InventoryItem> GetInventoryItem(Guid uuid)
        {
            InventoryItem? result = null;

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                string sql = """
                        SELECT 
                        ii.inventory_item_uuid,
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
                    WHERE ii.inventory_item_uuid = @Id;
                    """;

                result = await connection.QuerySingleAsync<InventoryItem>(sql, new { Id = uuid });
            }

            return result;
        }

        public async Task<List<InventoryItem>> RetrieveAllInventoryItems(int businessId)
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

                var result = await connection.QueryAsync<InventoryItem>(sql, new { BusinessId = businessId });
                inventoryItems = result.ToList();
            }

            return inventoryItems;
        }

        public async Task CreateInventoryItem(InventoryItem request, Guid businessUuid)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    string insertInventoryItemSql = """
                        INSERT INTO inventory_item (inventory_item_uuid, name, description, sku, cost, serial_number, purchased_date, supplier, brand, model, quantity, reorder_quantity, location, expiration_date, category, packaging, item_weight, is_listed, is_lot, notes)
                        VALUES (@InventoryItemUuid, @Name, @Description, @Sku, @Cost, @SerialNumber, @PurchasedDate, @Supplier, @Brand, @Model, @Quantity, @ReorderQuantity, @Location, @ExpirationDate, @Category, @Packaging, @ItemWeight, @IsListed, @IsLot, @Notes)
                        RETURNING inventory_item_id; 
                        """;

                    long inventoryItemId = await connection.QueryFirstAsync<long>(insertInventoryItemSql, request);

                    // Because our join tables use the primary key of the table, we need top get the business' primary key
                    // by selecting based on the GUID we are passed in.
                    string getBusinessIdSQL = """
                        SELECT business_id 
                        FROM business b
                        WHERE b.business_uuid = @BusinessId
                        """;

                    long businessId = await connection.QueryFirstAsync<int>(getBusinessIdSQL, businessUuid);


                    string insertBusinessInventoryItemSql = """
                        INSERT INTO business_inventory_item (inventory_item_id, business_id)
                        VALUES (@InventoryItemId, @BusinessId);
                        """;

                    await connection.ExecuteAsync(insertBusinessInventoryItemSql, new { InventoryItemId = inventoryItemId, BusinessId = businessId });

                    transaction.Commit();
                }
            }
        }
    }
}