using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
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
            var cacheStrategies = GetCacheDiscoveryStrategies();
            
            foreach (var strategy in cacheStrategies)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Trying cache strategy: {strategy.Name}");
                    
                    var app = strategy.CreateApp(_clientId, _authority);
                    var accounts = await app.GetAccountsAsync();
                    
                    if (accounts != null && accounts.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"Found {accounts.Count()} accounts with strategy: {strategy.Name}");
                        
                        foreach (var account in accounts)
                        {
                            try
                            {
                                var result = await app.AcquireTokenSilent(
                                    new[] { "https://graph.microsoft.com/User.Read" },
                                    account)
                                .ExecuteAsync();
                                
                                System.Diagnostics.Debug.WriteLine($"✅ SUCCESS: Token retrieved using strategy: {strategy.Name}");
                                return result;
                            }
                            catch (MsalUiRequiredException)
                            {
                                System.Diagnostics.Debug.WriteLine($"Token expired for account {account.Username} with strategy: {strategy.Name}");
                                continue;
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"No accounts found with strategy: {strategy.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error with strategy {strategy.Name}: {ex.Message}");
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Gets different cache discovery strategies to try
        /// </summary>
        private List<CacheStrategy> GetCacheDiscoveryStrategies()
        {
            return new List<CacheStrategy>
            {
                // Strategy 1: Shared cache (most common)
                new CacheStrategy
                {
                    Name = "SharedCache",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("http://localhost")
                        .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                        .Build()
                },
                
                // Strategy 2: Default cache
                new CacheStrategy
                {
                    Name = "DefaultCache",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("http://localhost")
                        .Build()
                },
                
                // Strategy 3: Try with different redirect URIs that apps might use
                new CacheStrategy
                {
                    Name = "EmbeddedWebView",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                        .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                        .Build()
                },
                
                // Strategy 4: Mobile/UWP style redirect
                new CacheStrategy
                {
                    Name = "MobileRedirect",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri($"msal{clientId}://auth")
                        .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                        .Build()
                }
            };
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
        
        /// <summary>
        /// Discovers MSAL cache locations and attempts to find tokens
        /// </summary>
        public Task<List<string>> DiscoverAvailableCaches()
        {
            var discoveredCaches = new List<string>();
            
            try
            {
                // Common MSAL cache locations
                var cachePaths = new List<string>
                {
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ".IdentityService"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "MSAL"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Microsoft", "MSAL"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages")
                };
                
                foreach (var basePath in cachePaths)
                {
                    if (Directory.Exists(basePath))
                    {
                        System.Diagnostics.Debug.WriteLine($"Found cache directory: {basePath}");
                        discoveredCaches.Add(basePath);
                        
                        // Look for client-specific subdirectories
                        try
                        {
                            var subdirs = Directory.GetDirectories(basePath);
                            foreach (var subdir in subdirs)
                            {
                                if (subdir.Contains(_clientId) || subdir.Contains("msal"))
                                {
                                    System.Diagnostics.Debug.WriteLine($"Found client-specific cache: {subdir}");
                                    discoveredCaches.Add(subdir);
                                }
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            System.Diagnostics.Debug.WriteLine($"Access denied to: {basePath}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error discovering caches: {ex.Message}");
            }
            
            return Task.FromResult(discoveredCaches);
        }
    }
    
    /// <summary>
    /// Represents a cache discovery strategy
    /// </summary>
    public class CacheStrategy
    {
        public string Name { get; set; }
        public Func<string, string, IPublicClientApplication> CreateApp { get; set; }
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