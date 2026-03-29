using System.Security.Cryptography;

namespace Trampline.Application.Services;

public interface ITotpService
{
    (string secret, string uri) GenerateSetup(string email, string issuer = "Trampline");
    bool Verify(string secret, string code, int windowSize = 1);
}

public class TotpService : ITotpService
{
    private const int SecretLength = 20;
    private const int Period = 30;
    private const int Digits = 6;
    private static readonly int[] Pow10 = { 1, 10, 100, 1000, 10000, 100000, 1000000, 10000000, 100000000 };

    public (string secret, string uri) GenerateSetup(string email, string issuer = "Trampline")
    {
        var secretBytes = RandomNumberGenerator.GetBytes(SecretLength);
        var base32Secret = Base32Encode(secretBytes);
        var encodedEmail = Uri.EscapeDataString(email);
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var uri = $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={base32Secret}&issuer={encodedIssuer}&digits={Digits}&period={Period}";
        return (base32Secret, uri);
    }

    public bool Verify(string secret, string code, int windowSize = 1)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != Digits)
            return false;

        var secretBytes = Base32Decode(secret);
        var currentTimeStep = GetCurrentTimeStep();

        for (var i = -windowSize; i <= windowSize; i++)
        {
            var totp = ComputeTotp(secretBytes, currentTimeStep + i);
            if (ConstantTimeEquals(totp, code))
                return true;
        }

        return false;
    }

    private static long GetCurrentTimeStep()
    {
        var unixSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return unixSeconds / Period;
    }

    private static string ComputeTotp(byte[] secret, long timeStep)
    {
        var timeBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
            Array.Reverse(timeBytes);

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(timeBytes);

        var offset = hash[^1] & 0x0F;
        var binary =
            ((hash[offset] & 0x7F) << 24) |
            ((hash[offset + 1] & 0xFF) << 16) |
            ((hash[offset + 2] & 0xFF) << 8) |
            (hash[offset + 3] & 0xFF);

        var otp = binary % Pow10[Digits];
        return otp.ToString().PadLeft(Digits, '0');
    }

    private static bool ConstantTimeEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;
        var diff = 0;
        for (var i = 0; i < a.Length; i++)
            diff |= a[i] ^ b[i];
        return diff == 0;
    }

    private static string Base32Encode(byte[] data)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var result = new char[(data.Length * 8 + 4) / 5];
        var idx = 0;
        int buffer = data[0], bitsLeft = 8, dataIdx = 1;

        while (bitsLeft > 0 || dataIdx < data.Length)
        {
            if (bitsLeft < 5)
            {
                if (dataIdx < data.Length)
                {
                    buffer <<= 8;
                    buffer |= data[dataIdx++] & 0xFF;
                    bitsLeft += 8;
                }
                else
                {
                    var pad = 5 - bitsLeft;
                    buffer <<= pad;
                    bitsLeft += pad;
                }
            }

            var index = (buffer >> (bitsLeft - 5)) & 0x1F;
            bitsLeft -= 5;
            result[idx++] = alphabet[index];
        }

        return new string(result, 0, idx);
    }

    private static byte[] Base32Decode(string encoded)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        encoded = encoded.TrimEnd('=').ToUpperInvariant();

        var output = new byte[encoded.Length * 5 / 8];
        var bitBuffer = 0;
        var bitsInBuffer = 0;
        var outputIdx = 0;

        foreach (var c in encoded)
        {
            var val = alphabet.IndexOf(c);
            if (val < 0) continue;

            bitBuffer = (bitBuffer << 5) | val;
            bitsInBuffer += 5;

            if (bitsInBuffer >= 8)
            {
                output[outputIdx++] = (byte)(bitBuffer >> (bitsInBuffer - 8));
                bitsInBuffer -= 8;
            }
        }

        return output[..outputIdx];
    }
}
