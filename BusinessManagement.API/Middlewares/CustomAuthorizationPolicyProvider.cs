using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace App.Middlewares
{
    public class CustomAuthorizationPolicyProvider : IAuthorizationPolicyProvider
    {
        private DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

        public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        public async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // Create a policy with a single requirement that checks if the user has the given permission
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build();

            return await Task.FromResult(policy);
        }

        public async Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        {
            return await _fallbackPolicyProvider.GetDefaultPolicyAsync();
        }

        public async Task<AuthorizationPolicy> GetFallbackPolicyAsync()
        {
            return await _fallbackPolicyProvider.GetFallbackPolicyAsync();
        }
    }
}
