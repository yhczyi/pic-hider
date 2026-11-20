using System.Security.Cryptography;
using System.Text;

namespace PicHider.Core;

/// <summary>PBKDF2-HMAC-SHA256 派生密钥，AES-256-GCM 加解密。</summary>
public static class PayloadCrypto
{
    public const int SaltSize = 16;
    public const int NonceSize = 12;
    public const int TagSize = 16;
    public const int Iterations = 100_000;
    public const int KeySize = 32;

    public static bool IsEmptyPassword(string? password) => string.IsNullOrEmpty(password);

    public static byte[] DeriveKey(string password, ReadOnlySpan<byte> salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(password),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            KeySize);

    /// <summary>返回 密文 + 16 字节 tag。</summary>
    public static byte[] Encrypt(byte[] key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> plain, ReadOnlySpan<byte> aad)
    {
        byte[] cipher = new byte[plain.Length + TagSize];
        using var aes = new AesGcm(key, TagSize);
        aes.Encrypt(nonce, plain, cipher.AsSpan(0, plain.Length), cipher.AsSpan(plain.Length), aad);
        return cipher;
    }

    /// <summary>密码错误或数据被篡改时抛 WrongPassword。</summary>
    public static byte[] Decrypt(byte[] key, ReadOnlySpan<byte> nonce, ReadOnlySpan<byte> cipherAndTag, ReadOnlySpan<byte> aad)
    {
        if (cipherAndTag.Length < TagSize)
        {
            throw new StegoException(StegoError.WrongPassword, "密码错误或数据已被篡改。");
        }

        byte[] plain = new byte[cipherAndTag.Length - TagSize];
        try
        {
            using var aes = new AesGcm(key, TagSize);
            aes.Decrypt(nonce, cipherAndTag[..^TagSize], cipherAndTag[^TagSize..], plain, aad);
            return plain;
        }
        catch (Exception ex) when (ex is CryptographicException or ArgumentException)
        {
            throw new StegoException(StegoError.WrongPassword, "密码错误或数据已被篡改。", ex);
        }
    }
}
