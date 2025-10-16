using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebView2Extension
{
    /// <summary>
    /// Reads tokens from MSAL cache autonomously without requiring token passing from main app
    /// </summary>
    public class MSALTokenCacheReader
    {
        private readonly string _clientId;
        private readonly string _authority;
        
        public MSALTokenCacheReader(string clientId, string authority)
        {
            _clientId = clientId;
            _authority = authority;
        }
        
        public async Task<AuthenticationResult> GetCachedTokenAsync()
        {
            try
            {
                // Create MSAL client with same config as main app
                var app = PublicClientApplicationBuilder
                    .Create(_clientId)
                    .WithAuthority(_authority)
                    .WithRedirectUri("http://localhost")
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();
                
                // Try to get token silently from cache
                var accounts = await app.GetAccountsAsync();
                if (accounts != null && accounts.Any())
                {
                    foreach (var account in accounts)
                    {
                        try
                        {
                            var result = await app.AcquireTokenSilent(
                                new[] { "https://graph.microsoft.com/User.Read" },
                                account)
                            .ExecuteAsync();
                            
                            return result;
                        }
                        catch (MsalUiRequiredException)
                        {
                            // Token expired or not available, try next account
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error reading MSAL cache: " + ex.Message);
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets token information from MSAL cache
        /// </summary>
        /// <returns>Token information or null if not available</returns>
        public async Task<TokenInfo> GetTokenInfoAsync()
        {
            var result = await GetCachedTokenAsync();
            if (result != null)
            {
                return new TokenInfo
                {
                    AccessToken = result.AccessToken,
                    UserName = result.Account != null ? result.Account.Username : "Unknown User",
                    TokenExpiry = result.ExpiresOn
                };
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// Container for token information
    /// </summary>
    public class TokenInfo
    {
        public string AccessToken { get; set; }
        public string UserName { get; set; }
        public DateTimeOffset TokenExpiry { get; set; }
        
        public bool IsValid 
        { 
            get { return !string.IsNullOrEmpty(AccessToken); } 
        }
        
        public string TokenExpiryString 
        { 
            get { return TokenExpiry.ToString("yyyy-MM-dd HH:mm:ss zzz"); } 
        }
    }
}