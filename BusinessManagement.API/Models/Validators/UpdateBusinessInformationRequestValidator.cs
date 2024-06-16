using App.Models.DTO.Requests;

namespace App.Models.Validators
{
    public class UpdateBusinessInformationRequestValidator : AbstractValidator<UpdateBusinessInformationRequest>
    {
        public UpdateBusinessInformationRequestValidator() 
        {
            RuleFor(x => x.BusinessUuid).NotEmpty().NotEmptyGuid();
            RuleFor(x => x.BusinessOwnerUuid).NotEmpty().NotEmptyGuid();
            RuleFor(x => x.BusinessFullname).NotEmpty();
            RuleFor(x => x.BusinessDisplayName).NotEmpty();
            RuleFor(x => x.BusinessStructureTypeId).NotEmpty();
            RuleFor(x => x.CountryCode).NotEmpty().Length(2);
            RuleFor(x => x.BusinessIndustry).NotEmpty();
        }
    }
}
