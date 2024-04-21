namespace App.Models.Validators
{
    public static class ValidationExtensions
    {
        /// <summary>
        /// Checks for guids in the form of: 00000000-0000-0000-0000-000000000000
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
        /// Checks for nullable guids in the form of: 00000000-0000-0000-0000-000000000000
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruleBuilder"></param>
        /// <returns></returns>
        public static IRuleBuilderOptions<T, Guid?> NotEmptyGuid<T>(this IRuleBuilder<T, Guid?> ruleBuilder)
        {
            return ruleBuilder.Must(guid => guid == null || guid != Guid.Empty)
                         .WithMessage("If provided, GUID must not be empty.");
        }
    }
}
