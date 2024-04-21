using App.Models.DTO.Requests;

namespace App.Models.Validators
{
    public class UpdateInventoryItemRequestValidator : AbstractValidator<UpdateInventoryItemRequest>
    {
        public UpdateInventoryItemRequestValidator()
        {
            // Not sure if this should be here or if we should only check in the domain object. Need to test performance. 
            RuleFor(x => x.InventoryItemUuid).NotEmptyGuid();
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.ReorderQuantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0);
            RuleFor(x => x.ItemWeightG).GreaterThanOrEqualTo(0);
            RuleFor(x => x.CustomPackageUuid).NotEmptyGuid();
            RuleFor(x => x.BusinessUuid).NotEmptyGuid();
            RuleFor(x => x.UserUuid).NotEmptyGuid();
        }
    }
}
