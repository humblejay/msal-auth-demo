using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebView2Extension
{
    /// <summary>
    /// Main entry point for the WebView2 extension
    /// </summary>
    public static class WebViewExtension
    {
    // Azure AD app registration details are loaded from environment or local secrets
    private static readonly string ClientId = ConfigHelper.GetClientId();
    private static readonly string TenantId = ConfigHelper.GetTenantId();
    private static readonly string Authority = !string.IsNullOrEmpty(TenantId) ? "https://login.microsoftonline.com/" + TenantId : null;
        
        /// <summary>
        /// Shows a WebView2 window with autonomous token retrieval from MSAL cache
        /// </summary>
        public static async void ShowTokenWebView()
        {
            try
            {
                // Read token from MSAL cache silently with enhanced discovery
                var cacheReader = new MSALTokenCacheReader(ClientId, Authority);
                
                // First, discover available caches
                var discoveredCaches = await cacheReader.DiscoverAvailableCaches();
                System.Diagnostics.Debug.WriteLine($"Cache Discovery: Found {discoveredCaches.Count} potential cache locations");
                
                var tokenInfo = await cacheReader.GetTokenInfoAsync();
                
                if (tokenInfo == null || !tokenInfo.IsValid)
                {
                    var cacheInfo = string.Join("\n", discoveredCaches.Take(5)); // Show first 5
                    var message = $"No cached token available. Please login in the host application first.\n\n" +
                                $"Cache Discovery Results:\n{cacheInfo}\n\n" +
                                $"Found {discoveredCaches.Count} potential cache locations.\n" +
                                $"Check Debug Output for detailed discovery logs.";
                    
                    System.Windows.Forms.MessageBox.Show(
                        message,
                        "Token Not Found - Cache Discovery Results",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Warning);
                    return;
                }

                var webViewForm = new TokenWebView(tokenInfo.AccessToken, tokenInfo.UserName, tokenInfo.TokenExpiryString);
                webViewForm.Show();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(
                    "Error loading WebView2 extension: " + ex.Message,
                    "WebView2 Extension Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Gets the version of this extension
        /// </summary>
        /// <returns>Extension version</returns>
        public static string GetVersion()
        {
            return "1.1.0";
        }

        /// <summary>
        /// Gets the extension name
        /// </summary>
        /// <returns>Extension name</returns>
        public static string GetName()
        {
            return "WebView2 Token Extension";
        }
    }
}
