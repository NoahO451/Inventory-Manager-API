namespace App.Models.Validators
{
    public static class ValidationExtensions
    {
        /// <summary>
        /// Validation fails if the guid is empty, in the form of: 00000000-0000-0000-0000-000000000000
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, Guid> NotEmptyGuid<T>(this IRuleBuilder<T, Guid> ruleBuilder)
        {
            return ruleBuilder.Must(guid => guid != Guid.Empty)
                         .WithMessage("GUID must not be empty.");
        }

        /// <summary>
        /// Validation fails if the nullable guid is empty, in the form of: 00000000-0000-0000-0000-000000000000
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, Guid?> NotEmptyGuid<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
        {
            return ruleBuilder.Must(guid => guid == null || guid != Guid.Empty)
                         .WithMessage("If provided, GUID must not be empty.");
        }

        /// <summary>
        /// Validation fails if the auth0 id provided is not in the form of: provider|45fe478e7e87f7a8c0a6684a. 
        /// The provider can be any IdP such as Google or Auth0. The alphanumeric string must contain 24 characters.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, string> ValidAuth0Id<T>(this IRuleBuilderOptions<T, string> ruleBuilder)
        {
            return ruleBuilder.Must(auth0Id => IsValidAuth0Id(auth0Id))
                              .WithMessage("Invalid Auth0 ID format.");
        }

        /// <summary>
        /// Ensure that identity provider and alpha numeric string exists.
        /// </summary>
        /// <param name="auth0Id"></param>
        /// <returns>Boolean result if an Auth0 id is valid</returns>
        private static bool IsValidAuth0Id(string auth0Id)
        {
            var parts = auth0Id.Split('|');
            if (parts.Length != 2)
            {
                return false;
            }

            var provider = parts[0];
            var id = parts[1];

            return !string.IsNullOrWhiteSpace(provider) && !string.IsNullOrWhiteSpace(id) && id.Length == 24;
        }
    }
}
