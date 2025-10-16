using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebView2Extension
{
    /// <summary>
    /// Test class for cache discovery scenarios
    /// </summary>
    public class CacheDiscoveryTest
    {
        private const string TestClientId = "b08336ab-2b1a-48ab-b583-c49161fc6055";
        private const string TestAuthority = "https://login.microsoftonline.com/bd80183f-c644-44a3-aa23-fd0979b821db";
        
        /// <summary>
        /// Test all cache discovery strategies
        /// </summary>
        public static async Task<string> RunCacheDiscoveryTest()
        {
            var results = new List<string>();
            results.Add("🔍 MSAL Cache Discovery Test");
            results.Add("================================");
            results.Add($"Client ID: {TestClientId}");
            results.Add($"Authority: {TestAuthority}");
            results.Add("");
            
            try
            {
                var cacheReader = new MSALTokenCacheReader(TestClientId, TestAuthority);
                
                // Test 1: Cache directory discovery
                results.Add("📁 Testing Cache Directory Discovery:");
                var discoveredCaches = await cacheReader.DiscoverAvailableCaches();
                
                if (discoveredCaches.Any())
                {
                    results.Add($"✅ Found {discoveredCaches.Count} potential cache locations:");
                    foreach (var cache in discoveredCaches.Take(10))
                    {
                        results.Add($"   • {cache}");
                    }
                }
                else
                {
                    results.Add("❌ No cache directories found");
                }
                
                results.Add("");
                
                // Test 2: Strategy testing
                results.Add("🔄 Testing Cache Strategies:");
                var strategies = GetTestStrategies();
                
                foreach (var strategy in strategies)
                {
                    try
                    {
                        results.Add($"Testing: {strategy.Name}");
                        var app = strategy.CreateApp(TestClientId, TestAuthority);
                        var accounts = await app.GetAccountsAsync();
                        
                        if (accounts != null && accounts.Any())
                        {
                            results.Add($"   ✅ Found {accounts.Count()} accounts:");
                            foreach (var account in accounts.Take(3))
                            {
                                results.Add($"      - {account.Username} (Environment: {account.Environment})");
                            }
                        }
                        else
                        {
                            results.Add($"   ⚠️ No accounts found");
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add($"   ❌ Error: {ex.Message}");
                    }
                    results.Add("");
                }
                
                // Test 3: Actual token retrieval
                results.Add("🎟️ Testing Token Retrieval:");
                var tokenInfo = await cacheReader.GetTokenInfoAsync();
                
                if (tokenInfo != null && tokenInfo.IsValid)
                {
                    results.Add("✅ SUCCESS: Token retrieved!");
                    results.Add($"   User: {tokenInfo.UserName}");
                    results.Add($"   Expires: {tokenInfo.TokenExpiryString}");
                    results.Add($"   Token Length: {tokenInfo.AccessToken.Length} characters");
                }
                else
                {
                    results.Add("❌ No valid token found with any strategy");
                }
                
            }
            catch (Exception ex)
            {
                results.Add($"❌ FATAL ERROR: {ex.Message}");
                results.Add($"Stack Trace: {ex.StackTrace}");
            }
            
            results.Add("");
            results.Add("Test completed at: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            
            return string.Join("\n", results);
        }
        
        /// <summary>
        /// Get test strategies for comprehensive testing
        /// </summary>
        private static List<CacheStrategy> GetTestStrategies()
        {
            return new List<CacheStrategy>
            {
                new CacheStrategy
                {
                    Name = "SharedCache + localhost redirect",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("http://localhost")
                        .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                        .Build()
                },
                
                new CacheStrategy
                {
                    Name = "Default cache + localhost redirect",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("http://localhost")
                        .Build()
                },
                
                new CacheStrategy
                {
                    Name = "SharedCache + native redirect",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                        .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                        .Build()
                },
                
                new CacheStrategy
                {
                    Name = "Default + native redirect",
                    CreateApp = (clientId, authority) => PublicClientApplicationBuilder
                        .Create(clientId)
                        .WithAuthority(authority)
                        .WithRedirectUri("https://login.microsoftonline.com/common/oauth2/nativeclient")
                        .Build()
                }
            };
        }
    }
}