using System;
using System.Collections.Generic;
using System.IO;

public static class ConfigHelper
{
    private static readonly string SecretsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "secrets.config");
    private static Dictionary<string, string> _secrets;

    private static void EnsureLoaded()
    {
        if (_secrets != null) return;
        _secrets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        try
        {
            if (File.Exists(SecretsFile))
            {
                foreach (var line in File.ReadAllLines(SecretsFile))
                {
                    var trimmed = line?.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#")) continue;
                    var idx = trimmed.IndexOf('=');
                    if (idx > 0)
                    {
                        var key = trimmed.Substring(0, idx).Trim();
                        var value = trimmed.Substring(idx + 1).Trim();
                        _secrets[key] = value;
                    }
                }
            }
        }
        catch
        {
            // swallow - secrets file is optional
        }
    }

    public static string Get(string key)
    {
        // Priority: environment variable -> secrets file -> null
        var env = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrEmpty(env)) return env;

        EnsureLoaded();
        if (_secrets != null && _secrets.TryGetValue(key, out var v)) return v;

        return null;
    }

    public static string GetClientId() => Get("ClientId");
    public static string GetTenantId() => Get("TenantId");
}
