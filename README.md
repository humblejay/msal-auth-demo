# MSAL Authentication Demo

A simple .NET Framework 4.8 WinForms application demonstrating Azure AD authentication using MSAL (Microsoft Authentication Library).

## Features

- **Login**: Authenticates user via system browser using Azure AD/Entra ID
- **Logout**: Clears the authentication session
- **Clear**: Clears the token display area
- **Token Display**: Shows the received access token and user information

## Setup Instructions

### 1. Azure AD App Registration

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to **Azure Active Directory** > **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: MSAL Demo App
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Platform = Public client/native, URI = `http://localhost`
5. Click **Register**
6. Note down the **Application (client) ID** and **Directory (tenant) ID**

### 2. Configure the Application

1. Open `MainForm.cs`
2. Replace the placeholder values:
   ```csharp
   private const string ClientId = "your-client-id"; // Replace with your actual client ID
   private const string TenantId = "your-tenant-id"; // Replace with your actual tenant ID
   ```

### 3. Install Dependencies

First, you need to install the MSAL NuGet package. You have two options:

#### Option A: Using NuGet Package Manager (Visual Studio)
1. Right-click on the project in Solution Explorer
2. Select "Manage NuGet Packages"
3. Install these exact versions for compatibility:
   - Microsoft.Identity.Client 4.54.1
   - Microsoft.IdentityModel.Abstractions 6.22.0
   - Microsoft.IdentityModel.Logging 6.22.0
   - Microsoft.IdentityModel.Tokens 6.22.0

#### Option B: Using NuGet CLI (Recommended)
```
nuget.exe install Microsoft.Identity.Client -Version 4.54.1 -OutputDirectory packages
nuget.exe install Microsoft.IdentityModel.Abstractions -Version 6.22.0 -OutputDirectory packages
nuget.exe install Microsoft.IdentityModel.Logging -Version 6.22.0 -OutputDirectory packages
nuget.exe install Microsoft.IdentityModel.Tokens -Version 6.22.0 -OutputDirectory packages
```

#### Option C: Restore from packages.config
The packages.config file is configured with compatible versions:
```
nuget restore -PackagesDirectory packages
```

### 4. Build and Run

1. Build the solution (Ctrl+Shift+B)
2. Run the application (F5)
3. Click **Login** to authenticate
4. Your default browser will open for Azure AD authentication
5. After successful login, the access token will be displayed

## How It Works

1. **MSAL Initialization**: Creates a `PublicClientApplication` with your Azure AD app details
2. **Interactive Authentication**: Uses `AcquireTokenInteractive()` to open system browser
3. **Token Display**: Shows the JWT access token, username, and expiration time
4. **Account Management**: Handles login/logout state and account caching

## Key MSAL Concepts

- **PublicClientApplication**: For desktop/mobile apps that can't securely store secrets
- **Interactive Flow**: Opens browser for user authentication
- **Scopes**: Requests specific permissions (User.Read for basic profile)
- **Token Caching**: MSAL automatically caches tokens for performance

## Troubleshooting

- Ensure you have .NET Framework 4.8 installed
- Verify your client ID and tenant ID are correct
- Check that the redirect URI matches exactly: `http://localhost`
- Make sure the Azure AD app is configured for public client flows