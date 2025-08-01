using System.Security.Cryptography;
using System.Text;

namespace MinimalApi.Services;

public static class PasswordHashService
{
    /// <summary>
    /// Gera hash da senha usando PBKDF2 com salt
    /// </summary>
    public static string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[32];
        rng.GetBytes(salt);
        
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        
        var hashBytes = new byte[64];
        Array.Copy(salt, 0, hashBytes, 0, 32);
        Array.Copy(hash, 0, hashBytes, 32, 32);
        
        return Convert.ToBase64String(hashBytes);
    }
    
    /// <summary>
    /// Verifica se a senha fornecida corresponde ao hash armazenado
    /// </summary>
    public static bool VerifyPassword(string password, string hash)
    {
        var hashBytes = Convert.FromBase64String(hash);
        var salt = new byte[32];
        Array.Copy(hashBytes, 0, salt, 0, 32);
        
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
        var computedHash = pbkdf2.GetBytes(32);
        
        for (int i = 0; i < 32; i++)
        {
            if (hashBytes[i + 32] != computedHash[i])
                return false;
        }
        
        return true;
    }
}
