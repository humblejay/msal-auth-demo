# MSAL Token Cache Analysis

## Overview
This document analyzes how the MSALAuthApp discovers and retrieves cached tokens from Microsoft Authentication Library (MSAL) cache locations.

## Token Discovery Strategy

### Successful Cache Location
✅ **VERIFIED**: After testing with actual login, tokens are successfully stored and retrieved from:
**`%LocalAppData%\Microsoft\IdentityCache`**

The **"SharedCache" strategy** successfully finds cached tokens, but they are stored in the Microsoft Identity cache location rather than the `.IdentityService` directory.

### Cache Strategy Priority Order
The application tries multiple strategies in this order:

1. **SharedCache** ✅ (Most successful)
   - Uses: `CacheOptions.EnableSharedCacheOptions`
   - Redirect URI: `http://localhost`
   - This is the primary strategy that successfully finds tokens

2. **DefaultCache**
   - Uses: Standard MSAL cache without shared options
   - Redirect URI: `http://localhost`

3. **EmbeddedWebView**
   - Uses: `EnableSharedCacheOptions`
   - Redirect URI: `https://login.microsoftonline.com/common/oauth2/nativeclient`

4. **MobileRedirect**
   - Uses: `EnableSharedCacheOptions`
   - Redirect URI: `msal{clientId}://auth`

## Cache Directory Locations

### Primary MSAL Cache Locations (Windows)
The application searches these directories in order:

1. `%LocalAppData%\.IdentityService` - Expected shared cache location
2. `%LocalAppData%\Microsoft\MSAL` - Standard MSAL cache
3. `%AppData%\Microsoft\MSAL` - Roaming MSAL cache  
4. `%LocalAppData%\Packages` - UWP app cache location

### ✅ ACTUAL Cache Location (Verified)
After testing with successful login, tokens are **actually stored in**:
- **`%LocalAppData%\Microsoft\IdentityCache`** - Contains .bin files with encrypted token data

This location is automatically discovered by the cache discovery code and is one of the standard MSAL cache locations.

## Implementation Details

### Main Application Configuration
```csharp
_app = PublicClientApplicationBuilder
    .Create(ClientId)
    .WithAuthority("https://login.microsoftonline.com/" + TenantId)
    .WithRedirectUri("http://localhost")
    .WithCacheOptions(CacheOptions.EnableSharedCacheOptions) // Key setting
    .Build();
```

### Token Retrieval Process
1. Application tries each cache strategy sequentially
2. For each strategy, it creates an MSAL client with specific configuration
3. Calls `GetAccountsAsync()` to find cached accounts
4. Attempts `AcquireTokenSilent()` for each account
5. Returns first successful token or null if none found

## Key Findings

### ✅ Confirmed Behavior
- Tokens are successfully found using MSAL's cache discovery mechanism
- **Actual cache location**: `%LocalAppData%\Microsoft\IdentityCache` (verified with login test)
- Cache files are stored as encrypted .bin files with hashed directory names
- Multiple applications can access the same cached tokens when using `EnableSharedCacheOptions`
- The cache discovery code successfully finds tokens despite them not being in `.IdentityService`

### 🔧 Technical Configuration
- **Framework**: .NET Framework 4.8
- **MSAL Version**: 4.54.1
- **Client Type**: Public Client Application
- **Cache Type**: Shared Cache (cross-application)

## Usage in WebView2 Extension

The WebView2Extension can autonomously discover and use cached tokens without requiring token passing from the main application, thanks to:

1. **Shared cache access** - Both applications use the same cache location
2. **Multiple strategy fallback** - Tries different configurations to maximize compatibility
3. **Autonomous operation** - No dependency on main application state

## Verification

### ✅ TESTED: Cache Discovery Verification
After successful login with MainForm, cache files can be found at:
```
%LocalAppData%\Microsoft\IdentityCache\1\UD\{user_hash}\{environment_hash}\{client_hash}.bin
%LocalAppData%\Microsoft\IdentityCache\1\AppMetadata\{environment_hash}\{client_hash}.bin
```

To verify cache discovery programmatically, run the `CacheDiscoveryTest.RunCacheDiscoveryTest()` method which will:
- List all discovered cache directories
- Test each cache strategy
- Report which strategy successfully retrieves tokens
- Display token information (user, expiry, etc.)

### Manual Verification
After logging in with the application, you can manually verify cache files exist:
```powershell
# Check for cache files
Get-ChildItem "$env:LOCALAPPDATA\Microsoft\IdentityCache" -Recurse -File
```

## Security Considerations

- Shared cache allows cross-application token access for same client ID
- Tokens are encrypted and tied to the user's Windows profile
- Cache location is protected by Windows file system permissions
- Token expiry is automatically handled by MSAL

---

**Generated**: 2025-10-17  
**Project**: MSALAuthApp  
**Purpose**: Document MSAL token cache discovery mechanism