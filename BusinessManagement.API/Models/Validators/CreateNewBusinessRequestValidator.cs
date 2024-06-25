using App.Models.DTO.Requests;
using App.Models.DTO.Responses;

namespace App.Models.Validators
{
    public class CreateNewBusinessRequestValidator : AbstractValidator<CreateNewBusinessRequest>
    {
        public CreateNewBusinessRequestValidator() 
        {
            RuleFor(x => x.BusinessOwnerUuid).NotEmpty().NotEmptyGuid();
            RuleFor(x => x.BusinessFullname).NotEmpty();
            RuleFor(x => x.BusinessStructureTypeId).NotEmpty();
            RuleFor(x => x.CountryCode).NotEmpty().Length(2);
            RuleFor(x => x.BusinessIndustry).NotEmpty();
        }
    }
}
