using App.Models.DTO.Requests;

namespace App.Models.Validators
{
    public class NewUserSignupRequestValidator : AbstractValidator<NewUserSignupRequest>
    {
        public NewUserSignupRequestValidator()
        {
            RuleFor(x => x.FullAuth0Id).NotEmpty().ValidAuth0Id();
            RuleFor(x => x.FullName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
        }
    }
}
