using App.Models.DTO.Requests;

namespace App.Models.Validators
{
    public class AddInventoryItemRequestValidator : AbstractValidator<AddInventoryItemRequest>
    {
        public AddInventoryItemRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().Length(1, 50);
            RuleFor(x => x.Description).Length(1, 500);
            RuleFor(x => x.SKU).Length(1, 16);
            RuleFor(x => x.Cost).GreaterThanOrEqualTo(0);
            RuleFor(x => x.SerialNumber).Length(1, 50);
            RuleFor(x => x.Supplier).Length(1, 100);
            RuleFor(x => x.Brand).Length(1, 50);
            RuleFor(x => x.Model).Length(1, 50);
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ReorderQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Location).Length(1, 50);
            RuleFor(x => x.Category).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CustomPackageUuid).NotEmptyGuid();
            RuleFor(x => x.ItemWeightG).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Notes).Length(1, 500);
            RuleFor(x => x.BusinessUuid).NotEmpty().NotEmptyGuid();
            RuleFor(x => x.UserUuid).NotEmpty().NotEmptyGuid();
        }
    }
}