using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Identity.Client;

namespace MSALAuthApp
{
    public partial class MainForm : Form
    {
        private IPublicClientApplication _app;
        private string[] _scopes = { "https://graph.microsoft.com/User.Read" };
        private string _currentAccessToken;
        private string _currentUserName;
        private DateTimeOffset _currentTokenExpiry;
        
        // Azure AD app registration details
        private const string ClientId = "b08336ab-2b1a-48ab-b583-c49161fc6055";
        private const string TenantId = "bd80183f-c644-44a3-aa23-fd0979b821db";

        public MainForm()
        {
            InitializeComponent();
            InitializeMSAL();
        }

        private void InitializeMSAL()
        {
            try
            {
                if (string.IsNullOrEmpty(ClientId) || string.IsNullOrEmpty(TenantId))
                {
                    MessageBox.Show("Client ID or Tenant ID is not configured. Please check the configuration.", "Configuration Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                _app = PublicClientApplicationBuilder
                    .Create(ClientId)
                    .WithAuthority("https://login.microsoftonline.com/" + TenantId)
                    .WithRedirectUri("http://localhost") // This will use system browser
                    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions)
                    .Build();
            }
            catch (Exception ex)
            {
                _app = null;
                MessageBox.Show("Error initializing MSAL: " + ex.Message + "\n\nPlease check your Azure AD configuration.", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                if (_app == null)
                {
                    MessageBox.Show("MSAL is not initialized. Please restart the application.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                if (btnLogin != null) btnLogin.Enabled = false;
                if (txtToken != null) txtToken.Text = "Authenticating...";
                
                var result = await _app.AcquireTokenInteractive(_scopes)
                    .WithPrompt(Prompt.SelectAccount)
                    .ExecuteAsync();

                if (txtToken != null)
                {
                    txtToken.Text = "Access Token:\r\n" + result.AccessToken + "\r\n\r\n" +
                                   "User: " + result.Account.Username + "\r\n" +
                                   "Token Expires: " + result.ExpiresOn;
                }
                
                // Store token details for WebView2 extension
                _currentAccessToken = result.AccessToken;
                _currentUserName = result.Account.Username;
                _currentTokenExpiry = result.ExpiresOn;
                
                if (btnLogout != null) btnLogout.Enabled = true;
                MessageBox.Show("Login successful!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (MsalException msalEx)
            {
                if (msalEx.ErrorCode != "authentication_canceled")
                {
                    if (txtToken != null) txtToken.Text = "Authentication error: " + msalEx.Message;
                    MessageBox.Show("Authentication failed: " + msalEx.Message, "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (txtToken != null) txtToken.Text = "Authentication was canceled by user.";
                }
            }
            catch (Exception ex)
            {
                if (txtToken != null) txtToken.Text = "Error: " + ex.Message;
                MessageBox.Show("An error occurred: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (btnLogin != null) btnLogin.Enabled = true;
            }
        }

        private async void btnLogout_Click(object sender, EventArgs e)
        {
            try
            {
                if (_app == null)
                {
                    MessageBox.Show("MSAL is not initialized. Please restart the application.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                var accounts = await _app.GetAccountsAsync();
                
                foreach (var account in accounts)
                {
                    await _app.RemoveAsync(account);
                }

                if (txtToken != null) txtToken.Text = "Logged out successfully.";
                if (btnLogout != null) btnLogout.Enabled = false;
                
                // Clear stored token details
                _currentAccessToken = null;
                _currentUserName = null;
                _currentTokenExpiry = DateTimeOffset.MinValue;
                
                MessageBox.Show("Logout successful!", "Success", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Logout error: " + ex.Message, "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            if (txtToken != null)
            {
                txtToken.Clear();
            }
        }

        private void btnWebViewExtension_Click(object sender, EventArgs e)
        {
            try
            {
                // Load the WebView2 extension DLL
                string extensionPath = Path.Combine(Application.StartupPath, "WebView2Extension.dll");
                
                if (!File.Exists(extensionPath))
                {
                    MessageBox.Show("WebView2Extension.dll not found. Please ensure the extension is built and available.", 
                        "Extension Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Load and call the autonomous extension
                Assembly extensionAssembly = Assembly.LoadFrom(extensionPath);
                Type extensionType = extensionAssembly.GetType("WebView2Extension.WebViewExtension");
                
                if (extensionType != null)
                {
                    MethodInfo showMethod = extensionType.GetMethod("ShowTokenWebView");
                    if (showMethod != null)
                    {
                        // Call the extension without parameters - it will read token from MSAL cache
                        showMethod.Invoke(null, null);
                    }
                    else
                    {
                        MessageBox.Show("ShowTokenWebView method not found in extension.", "Extension Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("WebViewExtension class not found in extension.", "Extension Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading WebView2 extension: " + ex.Message, "Extension Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}