using System;

namespace WebView2Extension
{
    /// <summary>
    /// Main entry point for the WebView2 extension
    /// </summary>
    public static class WebViewExtension
    {
        /// <summary>
        /// Shows a WebView2 window with the provided access token
        /// </summary>
        /// <param name="accessToken">The JWT access token from MSAL</param>
        /// <param name="userName">The authenticated user name</param>
        /// <param name="tokenExpiry">Token expiration time</param>
        public static void ShowTokenWebView(string accessToken, string userName, string tokenExpiry)
        {
            try
            {
                var webViewForm = new TokenWebView(accessToken, userName, tokenExpiry);
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
            return "1.0.0";
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