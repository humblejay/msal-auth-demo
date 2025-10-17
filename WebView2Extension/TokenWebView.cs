using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebView2Extension
{
    /// <summary>
    /// WebView2 form that displays token information and enables API calls
    /// </summary>
    public partial class TokenWebView : Form
    {
        private WebView2 webView2;
        private string accessToken;
        private string userName;
        private string tokenExpiry;

        public TokenWebView(string token, string user, string expiry)
        {
            accessToken = token;
            userName = user;
            tokenExpiry = expiry;
            
            InitializeComponent();
            InitializeWebView();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            
            // Form
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 700);
            this.Name = "TokenWebView";
            this.Text = "WebView2 Token Extension - API Ready";
            this.StartPosition = FormStartPosition.CenterScreen;
            
            // WebView2
            this.webView2 = new WebView2()
            {
                Name = "webView2",
                Dock = DockStyle.Fill
            };
            
            this.Controls.Add(this.webView2);
            this.ResumeLayout(false);
        }

        private async void InitializeWebView()
        {
            try
            {
                // Wait for WebView2 to be ready
                await webView2.EnsureCoreWebView2Async(null);

                // Create HTML content with token information and API demo
                string htmlContent = CreateTokenDisplayHtml();

                // Navigate to the HTML content
                webView2.NavigateToString(htmlContent);

                // Add JavaScript objects to expose token to web content
                webView2.CoreWebView2.AddWebResourceRequestedFilter("*", Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);
                
                // Wait a moment for the page to fully load
                await Task.Delay(1000);
                
                // Inject token into JavaScript global scope with immediate test
                string jsCode = string.Format(@"
                    window.msalToken = {{
                        accessToken: '{0}',
                        userName: '{1}',
                        tokenExpiry: '{2}'
                    }};
                    console.log('MSAL Token injected into WebView2:', window.msalToken);
                    
                    // Immediately test token after injection
                    if (typeof testTokenAvailability === 'function') {{
                        testTokenAvailability();
                    }}
                ", 
                    accessToken.Replace("'", "\\'")
                           .Replace("\r", "\\r")
                           .Replace("\n", "\\n"),
                    userName.Replace("'", "\\'")
                           .Replace("\r", "\\r")
                           .Replace("\n", "\\n"),
                    tokenExpiry.Replace("'", "\\'")
                              .Replace("\r", "\\r")
                              .Replace("\n", "\\n"));
                
                await webView2.CoreWebView2.ExecuteScriptAsync(jsCode);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error initializing WebView2: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CreateTokenDisplayHtml()
        {
            var html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html lang='en'>");
            html.AppendLine("<head>");
            html.AppendLine("    <meta charset='UTF-8'>");
            html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            html.AppendLine("    <title>MSAL Token - WebView2 Extension</title>");
            html.AppendLine("    <style>");
            html.AppendLine("        body { font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; margin: 20px; background: #f5f5f5; }");
            html.AppendLine("        .container { max-width: 900px; margin: 0 auto; background: white; padding: 30px; border-radius: 8px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }");
            html.AppendLine("        .header { text-align: center; margin-bottom: 30px; }");
            html.AppendLine("        .header h1 { color: #0078d4; margin: 0; }");
            html.AppendLine("        .header p { color: #666; margin: 5px 0 0 0; }");
            html.AppendLine("        .info-section { margin-bottom: 25px; }");
            html.AppendLine("        .info-label { font-weight: bold; color: #333; margin-bottom: 5px; }");
            html.AppendLine("        .info-value { background: #f8f9fa; padding: 10px; border-radius: 4px; border-left: 4px solid #0078d4; font-family: 'Courier New', monospace; word-break: break-all; }");
            html.AppendLine("        .token-section { margin-bottom: 25px; }");
            html.AppendLine("        .token-display { background: #f8f9fa; padding: 15px; border-radius: 4px; border-left: 4px solid #28a745; max-height: 200px; overflow-y: auto; font-family: 'Courier New', monospace; font-size: 12px; word-break: break-all; }");
            html.AppendLine("        .api-section { margin-top: 30px; padding: 20px; background: #e7f3ff; border-radius: 6px; }");
            html.AppendLine("        .btn { background: #0078d4; color: white; border: none; padding: 10px 20px; border-radius: 4px; cursor: pointer; margin: 5px; }");
            html.AppendLine("        .btn:hover { background: #106ebe; }");
            html.AppendLine("        .btn-success { background: #28a745; }");
            html.AppendLine("        .btn-success:hover { background: #218838; }");
            html.AppendLine("        .api-result { margin-top: 15px; padding: 15px; background: white; border-radius: 4px; border: 1px solid #ddd; white-space: pre-wrap; font-family: 'Courier New', monospace; font-size: 12px; }");
            html.AppendLine("        .status-ok { color: #28a745; }");
            html.AppendLine("        .status-error { color: #dc3545; }");
            html.AppendLine("    </style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            html.AppendLine("    <div class='container'>");
            html.AppendLine("        <div class='header'>");
            html.AppendLine("            <h1>🔐 WebView2 Token Extension</h1>");
            html.AppendLine("            <p>MSAL Token Retrieved from Cache - Ready for API Calls</p>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <div class='info-section'>");
            html.AppendLine("            <div class='info-label'>👤 Authenticated User:</div>");
            html.AppendLine("            <div class='info-value'>" + System.Web.HttpUtility.HtmlEncode(userName) + "</div>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <div class='info-section'>");
            html.AppendLine("            <div class='info-label'>⏰ Token Expires:</div>");
            html.AppendLine("            <div class='info-value'>" + System.Web.HttpUtility.HtmlEncode(tokenExpiry) + "</div>");
            html.AppendLine("        </div>");
            
            html.AppendLine("        <div class='token-section'>");
            html.AppendLine("            <div class='info-label'>🎟️ Access Token (JWT):</div>");
            html.AppendLine("            <div class='token-display' id='tokenDisplay'>" + System.Web.HttpUtility.HtmlEncode(accessToken) + "</div>");
            html.AppendLine("        </div>");

            html.AppendLine("        <div class='api-section'>");
            html.AppendLine("            <h3>🚀 API Demo - Token Ready for Use</h3>");
            html.AppendLine("            <p>The access token was autonomously retrieved from MSAL cache and is available in JavaScript as <code>window.msalToken</code> for API calls without re-authentication.</p>");
            html.AppendLine("            <button class='btn' onclick='testTokenAvailability()'>Test Token Availability</button>");
            html.AppendLine("            <button class='btn btn-success' onclick='callMicrosoftGraphAPI()'>Call Microsoft Graph API</button>");
            html.AppendLine("            <button class='btn' onclick='copyTokenToClipboard()'>Copy Token to Clipboard</button>");
            html.AppendLine("            <div id='apiResult' class='api-result' style='display: none;'></div>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            // JavaScript functions
            html.AppendLine("    <script>");
            html.AppendLine("        function showResult(message, isSuccess = true) {");
            html.AppendLine("            const resultDiv = document.getElementById('apiResult');");
            html.AppendLine("            resultDiv.style.display = 'block';");
            html.AppendLine("            resultDiv.className = 'api-result ' + (isSuccess ? 'status-ok' : 'status-error');");
            html.AppendLine("            resultDiv.textContent = message;");
            html.AppendLine("        }");
            html.AppendLine("");
            html.AppendLine("        function testTokenAvailability() {");
            html.AppendLine("            if (window.msalToken && window.msalToken.accessToken) {");
            html.AppendLine("                const tokenLength = window.msalToken.accessToken.length;");
            html.AppendLine("                showResult(`✅ SUCCESS: Token available!\\nToken Length: ${tokenLength} characters\\nUser: ${window.msalToken.userName}\\nExpiry: ${window.msalToken.tokenExpiry}`, true);");
            html.AppendLine("            } else {");
            html.AppendLine("                showResult('❌ ERROR: Token not available in WebView2', false);");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("");
            html.AppendLine("        async function callMicrosoftGraphAPI() {");
            html.AppendLine("            if (!window.msalToken || !window.msalToken.accessToken) {");
            html.AppendLine("                showResult('❌ ERROR: No token available', false);");
            html.AppendLine("                return;");
            html.AppendLine("            }");
            html.AppendLine("");
            html.AppendLine("            try {");
            html.AppendLine("                showResult('🔄 Calling Microsoft Graph API...', true);");
            html.AppendLine("                ");
            html.AppendLine("                const response = await fetch('https://graph.microsoft.com/v1.0/me', {");
            html.AppendLine("                    method: 'GET',");
            html.AppendLine("                    headers: {");
            html.AppendLine("                        'Authorization': `Bearer ${window.msalToken.accessToken}`,");
            html.AppendLine("                        'Content-Type': 'application/json'");
            html.AppendLine("                    }");
            html.AppendLine("                });");
            html.AppendLine("");
            html.AppendLine("                if (response.ok) {");
            html.AppendLine("                    const userData = await response.json();");
            html.AppendLine("                    showResult(`✅ SUCCESS: Microsoft Graph API Call\\n\\nUser Profile Data:\\n${JSON.stringify(userData, null, 2)}`, true);");
            html.AppendLine("                } else {");
            html.AppendLine("                    const errorText = await response.text();");
            html.AppendLine("                    showResult(`❌ ERROR: API call failed\\nStatus: ${response.status}\\nResponse: ${errorText}`, false);");
            html.AppendLine("                }");
            html.AppendLine("            } catch (error) {");
            html.AppendLine("                showResult(`❌ ERROR: ${error.message}`, false);");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("");
            html.AppendLine("        function copyTokenToClipboard() {");
            html.AppendLine("            if (window.msalToken && window.msalToken.accessToken) {");
            html.AppendLine("                navigator.clipboard.writeText(window.msalToken.accessToken).then(() => {");
            html.AppendLine("                    showResult('✅ SUCCESS: Token copied to clipboard!', true);");
            html.AppendLine("                }).catch(err => {");
            html.AppendLine("                    showResult('❌ ERROR: Failed to copy token to clipboard', false);");
            html.AppendLine("                });");
            html.AppendLine("            } else {");
            html.AppendLine("                showResult('❌ ERROR: No token available to copy', false);");
            html.AppendLine("            }");
            html.AppendLine("        }");
            html.AppendLine("");
            html.AppendLine("        // Token will be injected and tested automatically by C# code");
            html.AppendLine("    </script>");
            html.AppendLine("</body>");
            html.AppendLine("</html>");

            return html.ToString();
        }
    }
}