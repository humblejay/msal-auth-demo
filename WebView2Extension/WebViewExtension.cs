using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Identity.Client;

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
                void Log(string message)
                {
                    try
                    {
                        var logPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "webview_extension_debug.log");
                        var line = DateTimeOffset.Now.ToString("o") + " " + message + Environment.NewLine;
                        System.IO.File.AppendAllText(logPath, line);
                    }
                    catch { /* swallow logging errors */ }
                }

                Log("ShowTokenWebView invoked.");

                if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(TenantId))
                {
                    MessageBox.Show("Client ID or Tenant ID is not configured for the extension. Please set environment variables ClientId and TenantId or a local secrets.config.",
                        "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log("Configuration error: ClientId or TenantId missing.");
                    return;
                }

                var authority = !string.IsNullOrEmpty(TenantId) ? "https://login.microsoftonline.com/" + TenantId : Authority;

                // Create an independent in-memory-only PublicClientApplication for the extension
                var app = PublicClientApplicationBuilder
                    .Create(ClientId)
                    .WithAuthority(authority)
                    .WithRedirectUri("http://localhost")
                    .Build();

                Log($"Created PCA for clientId={ClientId} authority={authority}");

                string[] scopes = new[] { "https://graph.microsoft.com/User.Read" };

                AuthenticationResult result = null;

                try
                {
                    var accounts = await app.GetAccountsAsync();
                    Log($"GetAccountsAsync returned {accounts?.Count() ?? 0} accounts");
                    var account = accounts.FirstOrDefault();
                    if (account != null)
                    {
                        try
                        {
                            result = await app.AcquireTokenSilent(scopes, account).ExecuteAsync();
                            Log($"AcquireTokenSilent succeeded for account={account.Username}");
                        }
                        catch (MsalUiRequiredException)
                        {
                            // Silent failed, will fall back to interactive
                            Log("AcquireTokenSilent threw MsalUiRequiredException; interactive required.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log("Error while attempting silent token acquisition: " + ex.ToString());
                }

                if (result == null)
                {
                    Log("Attempting AcquireTokenInteractive (system browser)");
                    try
                    {
                        result = await app
                            .AcquireTokenInteractive(scopes)
                            // Avoid forcing account selection so system browser SSO (cookies) can be reused
                            //.WithPrompt(Prompt.SelectAccount)
                            .WithUseEmbeddedWebView(false)
                            .ExecuteAsync();
                        Log($"AcquireTokenInteractive succeeded for account={result?.Account?.Username}");
                    }
                    catch (Exception ex)
                    {
                        Log("AcquireTokenInteractive failed: " + ex.ToString());
                        throw;
                    }
                }

                if (result == null)
                {
                    Log("Result is null after token acquisition attempts.");
                    MessageBox.Show("Unable to retrieve token in extension.", "Token Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                Log("Token acquired, showing WebView form.");

                var webViewForm = new TokenWebView(result.AccessToken, result.Account.Username, result.ExpiresOn.ToString());
                webViewForm.Show();
            }
            catch (MsalException msalEx)
            {
                MessageBox.Show("MSAL error in extension: " + msalEx.Message, "MSAL Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error loading WebView2 extension: " + ex.Message,
                    "WebView2 Extension Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
