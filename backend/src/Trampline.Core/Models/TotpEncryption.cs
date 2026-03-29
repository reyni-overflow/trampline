using System.Security.Cryptography;
using System.Text;

namespace Trampline.Core.Models;

public static class TotpEncryption
{
    private static byte[]? _key;

    public static void Initialize(string masterKey)
    {
        _key = SHA256.HashData(Encoding.UTF8.GetBytes("totp_encryption:" + masterKey));
    }

    public static string? Encrypt(string? plaintext)
    {
        if (string.IsNullOrEmpty(plaintext) || _key == null) return plaintext;

        var iv = RandomNumberGenerator.GetBytes(16);
        using var aes = Aes.Create();
        aes.Key = _key;

        var encrypted = aes.EncryptCbc(Encoding.UTF8.GetBytes(plaintext), iv);
        var result = new byte[iv.Length + encrypted.Length];
        iv.CopyTo(result, 0);
        encrypted.CopyTo(result, iv.Length);

        return Convert.ToBase64String(result);
    }

    public static string? Decrypt(string? ciphertext)
    {
        if (string.IsNullOrEmpty(ciphertext) || _key == null) return ciphertext;

        try
        {
            var data = Convert.FromBase64String(ciphertext);
            if (data.Length < 17) return ciphertext;

            var iv = data[..16];
            var encrypted = data[16..];

            using var aes = Aes.Create();
            aes.Key = _key;

            return Encoding.UTF8.GetString(aes.DecryptCbc(encrypted, iv));
        }
        catch
        {
            return ciphertext;
        }
    }
}
