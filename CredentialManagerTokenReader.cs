using System;
using System.Runtime.InteropServices;
using System.Text;

public class CredentialManagerTokenReader
{
    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CredRead(string target, int type, int reservedFlag, out IntPtr credentialPtr);

    [DllImport("advapi32.dll", SetLastError = true)]
    private static extern bool CredFree(IntPtr cred);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct CREDENTIAL
    {
        public int Flags;
        public int Type;
        public IntPtr TargetName;
        public IntPtr Comment;
        public long LastWritten;
        public int CredentialBlobSize;
        public IntPtr CredentialBlob;
        public int Persist;
        public int AttributeCount;
        public IntPtr Attributes;
        public IntPtr TargetAlias;
        public IntPtr UserName;
    }

    public static string ReadCredential(string targetName)
    {
        try
        {
            IntPtr credPtr;
            if (CredRead(targetName, 1 /* CRED_TYPE_GENERIC */, 0, out credPtr))
            {
                var cred = (CREDENTIAL)Marshal.PtrToStructure(credPtr, typeof(CREDENTIAL));
                
                if (cred.CredentialBlobSize > 0)
                {
                    byte[] credentialBlob = new byte[cred.CredentialBlobSize];
                    Marshal.Copy(cred.CredentialBlob, credentialBlob, 0, cred.CredentialBlobSize);
                    
                    CredFree(credPtr);
                    return Encoding.Unicode.GetString(credentialBlob);
                }
                
                CredFree(credPtr);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading credential: {ex.Message}");
        }
        
        return null;
    }

    // Try common MSAL credential target names
    public static string FindMSALToken(string clientId, string username = null)
    {
        string[] possibleTargets = {
            $"MSALCache_{clientId}",
            $"MSAL.{clientId}",
            username != null ? $"MSAL.{clientId}.{username}" : null,
            "Microsoft.Developer.IdentityService"
        };

        foreach (var target in possibleTargets)
        {
            if (target == null) continue;
            
            var credential = ReadCredential(target);
            if (!string.IsNullOrEmpty(credential))
            {
                return credential;
            }
        }
        
        return null;
    }
}

// Usage:
// string token = CredentialManagerTokenReader.FindMSALToken("your-client-id", "user@domain.com");