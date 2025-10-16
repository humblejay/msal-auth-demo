using System;
using System.Threading.Tasks;

public class TokenExtractionStrategy
{
    private readonly string _clientId;
    private readonly string _authority;
    private readonly string _username;

    public TokenExtractionStrategy(string clientId, string authority = null, string username = null)
    {
        _clientId = clientId;
        _authority = authority ?? "https://login.microsoftonline.com/common";
        _username = username;
    }

    public async Task<TokenResult> GetTokenAsync()
    {
        // Method 1: MSAL Cache (Most reliable)
        try
        {
            var msalReader = new MSALTokenCacheReader(_clientId, _authority);
            var result = await msalReader.GetCachedTokenAsync();
            if (result != null)
            {
                return new TokenResult
                {
                    AccessToken = result.AccessToken,
                    Username = result.Account?.Username,
                    ExpiresOn = result.ExpiresOn,
                    Method = "MSAL Cache"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MSAL Cache failed: {ex.Message}");
        }

        // Method 2: Windows Credential Manager
        try
        {
            string token = CredentialManagerTokenReader.FindMSALToken(_clientId, _username);
            if (!string.IsNullOrEmpty(token))
            {
                return new TokenResult
                {
                    AccessToken = token,
                    Username = _username,
                    Method = "Credential Manager"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Credential Manager failed: {ex.Message}");
        }

        // Method 3: Registry
        try
        {
            string token = BrowserTokenExtractor.ExtractFromRegistry(_clientId);
            if (!string.IsNullOrEmpty(token))
            {
                return new TokenResult
                {
                    AccessToken = token,
                    Username = _username,
                    Method = "Registry"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Registry extraction failed: {ex.Message}");
        }

        // Method 4: File System Cache
        try
        {
            string token = BrowserTokenExtractor.ExtractFromFileCache(_clientId, _username);
            if (!string.IsNullOrEmpty(token))
            {
                return new TokenResult
                {
                    AccessToken = token,
                    Username = _username,
                    Method = "File Cache"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"File cache extraction failed: {ex.Message}");
        }

        // Method 5: Browser Cookies (Least reliable)
        try
        {
            string token = BrowserTokenExtractor.ExtractFromChromeCookies();
            if (!string.IsNullOrEmpty(token))
            {
                return new TokenResult
                {
                    AccessToken = token,
                    Username = _username,
                    Method = "Browser Cookies"
                };
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Browser extraction failed: {ex.Message}");
        }

        return new TokenResult
        {
            AccessToken = null,
            Method = "No token found"
        };
    }
}

public class TokenResult
{
    public string AccessToken { get; set; }
    public string Username { get; set; }
    public DateTimeOffset? ExpiresOn { get; set; }
    public string Method { get; set; }
    public bool IsValid => !string.IsNullOrEmpty(AccessToken);
}

// Usage:
// var strategy = new TokenExtractionStrategy("your-client-id", "https://login.microsoftonline.com/common", "user@domain.com");
// var tokenResult = await strategy.GetTokenAsync();
// 
// if (tokenResult.IsValid)
// {
//     Console.WriteLine($"Token found via {tokenResult.Method}");
//     // Use tokenResult.AccessToken
// }
// else
// {
//     Console.WriteLine("No token available - user needs to re-authenticate");
// }