using Microsoft.Identity.Client;
using System;
using System.IO;
using System.Threading.Tasks;

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
                .Build();
            
            // Try to get token silently from cache
            var accounts = await app.GetAccountsAsync();
            if (accounts != null)
            {
                foreach (var account in accounts)
                {
                    try
                    {
                        var result = await app.AcquireTokenSilent(
                            new[] { "https://graph.microsoft.com/.default" },
                            account)
                        .ExecuteAsync();
                        
                        return result;
                    }
                    catch (MsalUiRequiredException)
                    {
                        // Token expired or not available
                        continue;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading MSAL cache: {ex.Message}");
        }
        
        return null;
    }
}

// Usage:
// var reader = new MSALTokenCacheReader("your-client-id", "https://login.microsoftonline.com/common");
// var result = await reader.GetCachedTokenAsync();
// if (result != null) { /* use result.AccessToken */ }