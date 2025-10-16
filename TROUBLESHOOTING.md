# MSAL Authentication App - Troubleshooting Guide

## Common Issues and Solutions

### 1. Object Reference Not Set to an Instance of an Object

**Possible Causes:**
- MSAL initialization failed
- UI controls not properly initialized
- Missing Azure AD configuration

**Solutions:**
- Check that Client ID and Tenant ID are properly configured
- Restart the application if MSAL initialization failed
- Ensure all UI controls are loaded before clicking buttons

### 2. MSAL Initialization Errors

**Error: "Client ID or Tenant ID is not configured"**
- Verify the ClientId and TenantId constants in MainForm.cs
- Ensure the values are not empty or null

**Error: "Authority validation failed"**
- Check that the Tenant ID is correct
- Verify network connectivity
- Ensure the Azure AD tenant is accessible

### 3. Authentication Errors

**Error: "AADSTS700016: Application not found"**
- The Client ID is incorrect or the app registration doesn't exist
- Verify the Client ID in Azure Portal > App Registrations

**Error: "AADSTS50011: Redirect URI mismatch"**
- The redirect URI in Azure AD doesn't match "http://localhost"
- Update the app registration to include this redirect URI

**Error: "AADSTS65001: User or administrator has not consented"**
- The application needs admin consent for the requested scopes
- Have an admin consent to the application permissions

### 4. Runtime Errors

**Error: "Could not load file or assembly"**
- Missing dependencies (MSAL or System.Threading.Tasks.Extensions)
- Run the application from the bin\Debug directory
- Ensure all DLLs are in the same folder as the executable

### 5. Network/Firewall Issues

**Browser doesn't open or authentication fails:**
- Check corporate firewall settings
- Verify internet connectivity
- Try using a different browser as default
- Check proxy settings

### 6. Performance Issues

**Application takes long to load:**
- MSAL is downloading token cache or metadata
- This is normal on first run or after cache expiration
- Subsequent runs should be faster

## Debugging Tips

1. **Check the application logs**: Look for error messages in MessageBox dialogs
2. **Verify Azure AD configuration**: Double-check Client ID, Tenant ID, and redirect URI
3. **Test network connectivity**: Ensure you can access login.microsoftonline.com
4. **Run as administrator**: Sometimes needed for localhost redirect handling
5. **Clear browser cache**: Old authentication state might interfere

## Getting Help

If you continue to have issues:
1. Note the exact error message
2. Check the Azure AD sign-in logs in the Azure Portal
3. Verify your app registration settings match the application configuration
4. Test with a simple console application first to isolate MSAL issues

## Azure AD App Registration Checklist

✅ **Application (client) ID** is correctly copied  
✅ **Directory (tenant) ID** is correctly copied  
✅ **Redirect URI** is set to `http://localhost`  
✅ **Platform** is configured as "Mobile and desktop applications"  
✅ **API permissions** include Microsoft Graph User.Read (delegated)  
✅ **Allow public client flows** is enabled