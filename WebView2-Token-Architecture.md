# WebView2 Token Sharing Architecture & Security Model

## Question
How is the token accessible to WebView2? Is it pulling from sessionStorage, localStorage or memoryStorage or some other means? Or is the security context passed through client to the DLL and then WebView2?

## 🔐 Token Sharing Architecture

### **Method Used: Direct Memory Passing**

```
┌─────────────────┐    In-Memory     ┌─────────────────┐    JavaScript    ┌──────────────────┐
│   WinForms      │    Variables     │  WebView2 DLL   │    Injection     │   WebView2       │
│   (Main App)    │ ───────────────→ │   Extension     │ ──────────────→  │   JavaScript     │
│                 │                  │                 │                  │                  │
│ • MSAL Auth     │                  │ • Token Params │                  │ • window.msalToken│
│ • Token Storage │                  │ • HTML Creation │                  │ • API Calls      │
└─────────────────┘                  └─────────────────┘                  └──────────────────┘
```

### **Step-by-Step Token Flow:**

1. **WinForms Storage** (MainForm.cs):
```csharp
// Token stored in memory variables
private string _currentAccessToken;
private string _currentUserName;
private DateTimeOffset _currentTokenExpiry;

// After MSAL authentication
_currentAccessToken = result.AccessToken;
_currentUserName = result.Account.Username;
_currentTokenExpiry = result.ExpiresOn;
```

2. **DLL Parameter Passing** (MainForm.cs → WebView2Extension.dll):
```csharp
// Direct method call with token parameters
showMethod.Invoke(null, new object[] 
{
    _currentAccessToken,        // JWT token as string
    _currentUserName,          // Username as string
    _currentTokenExpiry.ToString() // Expiry as string
});
```

3. **WebView2 JavaScript Injection** (TokenWebView.cs):
```csharp
// Token injected directly into JavaScript global scope
string jsCode = string.Format(@"
    window.msalToken = {{
        accessToken: '{0}',
        userName: '{1}',
        tokenExpiry: '{2}'
    }};
", accessToken, userName, tokenExpiry);

await webView2.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(jsCode);
```

### **🔍 Security Context Details:**

**NOT using any browser storage:**
- ❌ No localStorage
- ❌ No sessionStorage  
- ❌ No cookies
- ❌ No indexedDB
- ❌ No browser cache

**Instead using:**
- ✅ **Direct memory variables** in WinForms process
- ✅ **Assembly method invocation** to pass parameters
- ✅ **JavaScript injection** into WebView2's V8 context
- ✅ **Process memory** - token exists only in application memory

### **🛡️ Security Implications:**

**Advantages:**
- **No persistence**: Token never stored on disk or browser storage
- **Process isolation**: Token exists only in application memory space
- **Direct control**: Full control over token lifecycle
- **No cross-site access**: Token only available to our WebView2 instance

**Security boundaries:**
- Token lives in .NET process memory
- Passed as parameters between assemblies
- Injected into isolated WebView2 context
- Available only to our JavaScript code

### **🔄 Alternative Approaches We Could Use:**

1. **Browser Storage Methods:**
```javascript
// Could store in localStorage (persistent)
localStorage.setItem('msalToken', token);

// Could store in sessionStorage (session only)
sessionStorage.setItem('msalToken', token);
```

2. **WebView2 Host-Web Communication:**
```csharp
// Could use WebView2's postMessage API
webView2.CoreWebView2.PostWebMessageAsString(tokenJson);
```

3. **Custom Protocol Handler:**
```csharp
// Could intercept custom URLs like msal://token
webView2.CoreWebView2.CustomSchemeRegistrations
```

### **🎯 Why We Chose Direct Injection:**

- **Simplicity**: Straightforward implementation
- **Security**: No persistent storage exposure
- **Performance**: No additional HTTP requests or storage operations
- **Control**: Complete lifecycle management
- **Demonstration**: Clear token sharing pattern

## Conclusion

The token is essentially passed **"hand-to-hand"** from WinForms → DLL → WebView2 JavaScript, staying in memory throughout the entire process without ever touching browser storage systems!

This approach demonstrates a secure, controlled method of sharing authentication state between different UI technologies (WinForms and WebView2) while maintaining security boundaries and avoiding persistent storage exposure.

---
*Generated: 2025-10-16*
*Project: MSAL Authentication Demo with WebView2 Extension*
*Repository: https://github.com/humblejay/msal-auth-demo*