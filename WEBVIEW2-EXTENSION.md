# WebView2 Extension for Token Sharing

This feature branch demonstrates how to create a WebView2 extension that can access MSAL tokens from the main WinForms application without requiring re-authentication.

## 🌟 Features

- **Token Sharing**: Pass JWT access tokens from WinForms to WebView2
- **Dynamic DLL Loading**: Load WebView2 extension on-demand
- **JavaScript Integration**: Token available as `window.msalToken` in WebView2
- **API Ready**: Make authenticated API calls directly from WebView2
- **Microsoft Graph Demo**: Built-in example calling Graph API

## 🏗️ Architecture

```
┌─────────────────────┐    Load DLL    ┌──────────────────────┐
│   WinForms App      │───────────────→│  WebView2Extension  │
│   (MSALAuthApp)     │    Pass Token  │      (DLL)           │
│                     │───────────────→│                      │
│ - MSAL Auth         │                │ - WebView2 Form      │
│ - Token Storage     │                │ - Token Injection    │
│ - Extension Loader  │                │ - API Calls          │
└─────────────────────┘                └──────────────────────┘
                                                 │
                                                 ▼
                                        ┌──────────────────┐
                                        │   JavaScript     │
                                        │  window.msalToken│
                                        │ - accessToken    │
                                        │ - userName       │
                                        │ - tokenExpiry    │
                                        └──────────────────┘
```

## 🚀 How It Works

### 1. Main Application Flow
1. User authenticates via MSAL in WinForms app
2. Access token is stored in memory
3. "WebView2 Ext" button becomes enabled
4. User clicks button to load extension

### 2. Extension Loading Process
1. Main app locates `WebView2Extension.dll`
2. Uses `Assembly.LoadFrom()` to dynamically load DLL
3. Calls `WebViewExtension.ShowTokenWebView()` with token data
4. Extension creates new WebView2 window

### 3. Token Injection
1. WebView2 form initializes with token data
2. Token is injected into JavaScript global scope as `window.msalToken`
3. HTML page loads with token display and API demo
4. JavaScript can access token for authenticated API calls

## 📁 Project Structure

```
MSALAuthApp/
├── MainForm.cs              # Main app with extension loader
├── bin/Debug/
│   ├── MSALAuthApp.exe      # Main application
│   └── WebView2Extension.dll # Extension DLL
└── WebView2Extension/       # Extension source code
    ├── WebViewExtension.cs   # Main extension entry point
    ├── TokenWebView.cs       # WebView2 form implementation
    └── WebView2Extension.csproj
```

## 🛠️ Key Components

### WebViewExtension.cs
```csharp
public static class WebViewExtension
{
    public static void ShowTokenWebView(string accessToken, string userName, string tokenExpiry)
    {
        var webViewForm = new TokenWebView(accessToken, userName, tokenExpiry);
        webViewForm.Show();
    }
}
```

### Token Injection
```csharp
// Inject token into JavaScript global scope
string jsCode = string.Format(@"
    window.msalToken = {{
        accessToken: '{0}',
        userName: '{1}',
        tokenExpiry: '{2}'
    }};
", accessToken, userName, tokenExpiry);

await webView2.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(jsCode);
```

### JavaScript API Calls
```javascript
// Call Microsoft Graph API using injected token
const response = await fetch('https://graph.microsoft.com/v1.0/me', {
    method: 'GET',
    headers: {
        'Authorization': `Bearer ${window.msalToken.accessToken}`,
        'Content-Type': 'application/json'
    }
});
```

## 🎯 Use Cases

1. **Hybrid Applications**: Mix WinForms authentication with web-based UI
2. **API Testing**: Use WebView2 as an API testing interface
3. **Token Debugging**: Inspect and validate JWT tokens
4. **External API Integration**: Call third-party APIs with Azure AD tokens
5. **Single Sign-On**: Share authentication across different UI technologies

## 🔧 Building and Running

### Prerequisites
- .NET Framework 4.8
- WebView2 Runtime (automatically installed on Windows 11)
- Microsoft.Web.WebView2 NuGet package

### Build Commands
```bash
# Build WebView2 Extension DLL
msbuild WebView2Extension\WebView2Extension.csproj -p:Configuration=Debug

# Copy extension to main app directory
copy "WebView2Extension\bin\Debug\WebView2Extension.dll" "bin\Debug\"
copy "WebView2Extension\bin\Debug\Microsoft.Web.WebView2.*.dll" "bin\Debug\"

# Build main application
msbuild MSALAuthApp.csproj -p:Configuration=Debug
```

### Usage
1. Run `bin\Debug\MSALAuthApp.exe`
2. Click **Login** to authenticate with Azure AD
3. After successful login, click **WebView2 Ext**
4. WebView2 window opens with token information
5. Use the API demo buttons to test token functionality

## 🔐 Security Considerations

- **Token Exposure**: Token is available in JavaScript - ensure secure context
- **Memory Management**: Token stored in memory until logout
- **HTTPS Only**: Use HTTPS for production API calls
- **Token Validation**: Verify token expiry before API calls
- **Secure Storage**: Consider encrypted storage for sensitive scenarios

## 🐛 Troubleshooting

### Common Issues

**Extension DLL Not Found**
- Ensure WebView2Extension.dll is in the same directory as MSALAuthApp.exe
- Check that all WebView2 dependencies are copied

**WebView2 Runtime Missing**
- Install Microsoft Edge WebView2 Runtime
- Download from: https://developer.microsoft.com/microsoft-edge/webview2/

**Token Not Available in JavaScript**
- Check browser developer tools console for injection errors
- Verify token is not null/empty when passed to extension

**API Calls Failing**
- Ensure token has correct scopes for target API
- Check network connectivity and CORS policies
- Verify token has not expired

## 🚀 Future Enhancements

- **Token Refresh**: Automatic token refresh when expired
- **Multiple Scopes**: Support for different API scopes
- **Custom APIs**: Templates for different API integrations
- **Offline Mode**: Cache API responses for offline use
- **Extension Marketplace**: Plugin architecture for multiple extensions