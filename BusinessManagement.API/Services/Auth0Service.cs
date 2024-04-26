using App.Helpers;
using App.Models.DTO;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;

namespace App.Services
{
    public interface IAuth0Service
    {
        Task<ServiceResult> UpdateAuth0UserEmail(string auth0Id, string emailAddress);
    }

    /// <summary>
    /// Handles calls to Auth0s APIs
    /// </summary>
    public class Auth0Service : IAuth0Service
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<Auth0Service> _logger;

        private const string api_version = $"/api/v2";

        public Auth0Service(IConfiguration configuration, IHttpClientFactory httpClientFactory, ILogger<Auth0Service> logger)
        {
            _configuration = configuration;
            _httpClient = httpClientFactory.CreateClient("Auth0Domain");
            _logger = logger;
        }
        
        /// <summary>
        /// Updates user's Auth0 email. Intended for use while updating a user's email in our database.
        /// </summary>
        /// <param name="fullAuth0Id"></param>
        /// <param name="emailAddress"></param>
        /// <returns></returns>
        public async Task<ServiceResult> UpdateAuth0UserEmail(string fullAuth0Id, string emailAddress)
        {
            try
            {
                string? token = await GenerateManagementToken();

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning("{trace} Auth0 management JWT null or whiteSpace", LogHelper.TraceLog());
                    return ServiceResult.FailureResult("Auth0 management JWT null or whiteSpace.");
                }

                Auth0Settings? settings = _configuration.GetSection("Auth0Settings").Get<Auth0Settings>();

                if (settings == null)
                {
                    _logger.LogError("{trace} Auth0 configuration settings null or whiteSpace", LogHelper.TraceLog());
                    return ServiceResult.FailureResult("Auth0 configuration settings null or whiteSpace.");
                }

                var request = new HttpRequestMessage(HttpMethod.Patch, $"{api_version}/users/{fullAuth0Id}");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var content = new StringContent($"{{\"email\":\"{emailAddress}\"}}", null, "application/json");
                request.Content = content;

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return ServiceResult.SuccessResult();
                }

                return ServiceResult.FailureResult("Failed to update auth0 user email.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{trace} Exception thrown", LogHelper.TraceLog());
                return ServiceResult.FailureResult("Exception updating Auth0 user email", ex);
            }
        }

        /// <summary>
        /// Obtain an Auth0 Management API JWT
        /// </summary>
        /// <returns>JWT token</returns>
        private async Task<string?> GenerateManagementToken()
        {
            Auth0Settings? settings = _configuration.GetSection("Auth0Settings").Get<Auth0Settings>();
            string? ManagementClientId = _configuration.GetValue<string>("ManagementClientId");
            string? ManagementClientSecret = _configuration.GetValue<string>("ManagementClientSecret");

            if (string.IsNullOrEmpty(ManagementClientId) || string.IsNullOrEmpty(ManagementClientSecret))
                return null;

            string path = "/oauth/token";

            var request = new HttpRequestMessage(HttpMethod.Post, path);
            request.Headers.Add("Accept", "application/json");
            var jsonContent = $"{{\"client_id\":\"{ManagementClientId}\",\"client_secret\":\"{ManagementClientSecret}\",\"audience\":\"https://dev-1tpta51o17o23r4e.us.auth0.com/api/v2/\",\"grant_type\":\"client_credentials\"}}";
            var content = new StringContent(jsonContent, null, "application/json");

            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();

                JsonNode? obj = JsonNode.Parse(responseContent);
                string? token = obj?["access_token"]?.GetValue<string>();

                if (!string.IsNullOrWhiteSpace(token))
                {
                    return token;
                }
            }
            return null;
        }
    }
}
