// Shared Token Manager - accessible from both main app and extension
public static class TokenManager
{
    private static string _currentAccessToken;
    private static string _currentUserName;
    private static DateTimeOffset _currentTokenExpiry;
    private static readonly object _lock = new object();

    // Main app sets the token
    public static void SetToken(string accessToken, string userName, DateTimeOffset expiry)
    {
        lock (_lock)
        {
            _currentAccessToken = accessToken;
            _currentUserName = userName;
            _currentTokenExpiry = expiry;
        }
    }

    // Extension DLL gets the token
    public static TokenInfo GetToken()
    {
        lock (_lock)
        {
            return new TokenInfo
            {
                AccessToken = _currentAccessToken,
                UserName = _currentUserName,
                TokenExpiry = _currentTokenExpiry
            };
        }
    }

    public static bool IsTokenAvailable()
    {
        lock (_lock)
        {
            return !string.IsNullOrEmpty(_currentAccessToken);
        }
    }
}

public class TokenInfo
{
    public string AccessToken { get; set; }
    public string UserName { get; set; }
    public DateTimeOffset TokenExpiry { get; set; }
}

// Usage in Main App:
// TokenManager.SetToken(result.AccessToken, result.Account.Username, result.ExpiresOn);

// Usage in Extension:
// var tokenInfo = TokenManager.GetToken();
// if (tokenInfo != null && !string.IsNullOrEmpty(tokenInfo.AccessToken))
// {
//     // Use token in WebView2
// }