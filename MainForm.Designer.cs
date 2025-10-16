namespace MSALAuthApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnLogin;
        private System.Windows.Forms.Button btnLogout;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnWebViewExtension;
        private System.Windows.Forms.TextBox txtToken;
        private System.Windows.Forms.Label lblTitle;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnLogin = new System.Windows.Forms.Button();
            this.btnLogout = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnWebViewExtension = new System.Windows.Forms.Button();
            this.txtToken = new System.Windows.Forms.TextBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.Location = new System.Drawing.Point(12, 9);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(234, 20);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "MSAL Authentication Demo";
            
            // 
            // btnLogin
            // 
            this.btnLogin.Location = new System.Drawing.Point(16, 45);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(100, 35);
            this.btnLogin.TabIndex = 1;
            this.btnLogin.Text = "Login";
            this.btnLogin.UseVisualStyleBackColor = true;
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            
            // 
            // btnLogout
            // 
            this.btnLogout.Enabled = false;
            this.btnLogout.Location = new System.Drawing.Point(130, 45);
            this.btnLogout.Name = "btnLogout";
            this.btnLogout.Size = new System.Drawing.Size(100, 35);
            this.btnLogout.TabIndex = 2;
            this.btnLogout.Text = "Logout";
            this.btnLogout.UseVisualStyleBackColor = true;
            this.btnLogout.Click += new System.EventHandler(this.btnLogout_Click);
            
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(244, 45);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(100, 35);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            
            // 
            // btnWebViewExtension
            // 
            this.btnWebViewExtension.Enabled = true;
            this.btnWebViewExtension.Location = new System.Drawing.Point(358, 45);
            this.btnWebViewExtension.Name = "btnWebViewExtension";
            this.btnWebViewExtension.Size = new System.Drawing.Size(120, 35);
            this.btnWebViewExtension.TabIndex = 5;
            this.btnWebViewExtension.Text = "WebView2 Ext";
            this.btnWebViewExtension.UseVisualStyleBackColor = true;
            this.btnWebViewExtension.Click += new System.EventHandler(this.btnWebViewExtension_Click);
            
            // 
            // txtToken
            // 
            this.txtToken.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtToken.Location = new System.Drawing.Point(16, 95);
            this.txtToken.Multiline = true;
            this.txtToken.Name = "txtToken";
            this.txtToken.ReadOnly = true;
            this.txtToken.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtToken.Size = new System.Drawing.Size(756, 347);
            this.txtToken.TabIndex = 4;
            this.txtToken.Text = "Click Login to authenticate with Azure AD and view the access token here.\r\n\r\nThe 'WebView2 Ext' button will automatically find and use cached tokens without requiring explicit token passing.";
            
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 461);
            this.Controls.Add(this.txtToken);
            this.Controls.Add(this.btnWebViewExtension);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnLogout);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MSAL Authentication Demo";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}