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

This project no longer requires editing `MainForm.cs` to inject secrets. Instead the application loads `ClientId` and `TenantId` at runtime using a small helper (`ConfigHelper`) with the following priority:

1. Environment variables `ClientId` and `TenantId`
2. Local `secrets.config` file in the project root (ignored by git)

Use one of these options to provide your Azure AD values (recommended: environment variables):

- Environment variables (PowerShell example):

```powershell
[Environment]::SetEnvironmentVariable('ClientId','<your-client-id>','User')
[Environment]::SetEnvironmentVariable('TenantId','<your-tenant-id>','User')

# Export into the current session so you can run the app immediately without restarting
$env:ClientId = '<your-client-id>'
$env:TenantId = '<your-tenant-id>'
```

- Local `secrets.config` (developer convenience — do NOT commit):

Create `secrets.config` in the project root with:

```xml
<secrets>
   <add key="ClientId" value="your-client-id" />
   <add key="TenantId" value="your-tenant-id" />
</secrets>
```

If neither is present the app will report missing configuration at startup.

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

## Environment and setup (required)

Before building and running this project, ensure the following are available on the machine used for development and testing:

- Operating system: Windows 10 or Windows 11 (this is a WinForms desktop app)
- .NET Framework 4.8 Developer Pack / Targeting Pack installed (required for building)
- Visual Studio Build Tools or full Visual Studio (MSBuild). Prefer Visual Studio 2019/2022 or the corresponding Build Tools so the Roslyn compiler supports modern C# features used in the project.
- NuGet CLI (or Visual Studio's package restore) to restore `packages.config` dependencies
- Microsoft Edge WebView2 Runtime installed (runtime required if you use the WebView2 extension features)

Recommended quick checks and install links:

- .NET Framework 4.8 Developer Pack: https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48
- Visual Studio Build Tools (MSBuild): https://visualstudio.microsoft.com/downloads/ (select "Build Tools for Visual Studio")
- WebView2 Runtime: https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section
- NuGet CLI: https://www.nuget.org/downloads

How to restore, build and run (PowerShell)

1) Open a PowerShell prompt with Developer Tools (or ensure MSBuild path is available):

```powershell
# From the repository root
Set-Location -Path 'C:\code\MSALAuthApp'

# Restore NuGet packages (packages.config style)
nuget restore -PackagesDirectory packages

# Use the provided rebuild.bat which prefers VS Build Tools MSBuild and copies extension artifacts
.\rebuild.bat

# Run the built exe
.\bin\Debug\MSALAuthApp.exe
```

Note: If you prefer to run MSBuild directly, point it at the solution or project and specify Configuration=Debug. Example (PowerShell):

```powershell
& 'C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\MSBuild.exe' 'C:\code\MSALAuthApp\MSALAuthApp.sln' /p:Configuration=Debug
```

How to provide ClientId and TenantId (recommended - do NOT check secrets into source control)

The app reads `ClientId` and `TenantId` from environment variables first. If those are not present it falls back to a local `secrets.config` file (which is ignored by git). Use one of these approaches:

1) Environment variables (recommended for local dev):

```powershell
# Set for current user (survives logoffs). After setting, open a new shell or export to the current session.
[Environment]::SetEnvironmentVariable('ClientId','<your-client-id>','User')
[Environment]::SetEnvironmentVariable('TenantId','<your-tenant-id>','User')

# Export into the current session so you can run the app immediately without restarting
$env:ClientId = '<your-client-id>'
$env:TenantId = '<your-tenant-id>'
```

2) Local secrets file (developer convenience): create a file called `secrets.config` in the project root with the following contents — do NOT commit this file to source control (a template `secrets.config.template` is included):

```xml
<secrets>
   <add key="ClientId" value="your-client-id" />
   <add key="TenantId" value="your-tenant-id" />
</secrets>
```

If neither method is present the app will log an error and prompt for missing configuration when started.

Extra notes

- If you see build errors about the .NET Framework reference assemblies (MSB3644), install the .NET Framework 4.8 Developer Pack / Targeting Pack and then rebuild.
- If you see compiler errors about newer C# syntax (for example interpolation `$"..."` reported as unexpected), ensure MSBuild is from a recent Visual Studio/Build Tools installation (Visual Studio 2019/2022 or newer).
- The WebView2 extension requires the WebView2 runtime installed on the machine. If you don't intend to use WebView2, the main app will still run but WebView2 extension features will be unavailable.
