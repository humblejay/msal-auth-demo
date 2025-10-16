using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Identity.Client;

namespace MSALAuthApp
{
    public partial class MainForm : Form
    {
        private IPublicClientApplication _app;
        private string[] _scopes = { "https://graph.microsoft.com/User.Read" };
        
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
    }
}