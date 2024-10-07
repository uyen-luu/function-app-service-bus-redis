using System.Security.Cryptography;
using System.Text;

namespace IPS.Grow.Infra.Cryptographic;

public class EncodingFactory
{
    public static byte[] HashPassword256(string password)
    {
        var encoding = new UnicodeEncoding();
        return SHA256.HashData(encoding.GetBytes(password));
    }

    public static byte[] ComputeHash(string input, HashAlgorithmType hashAlgorithmType, EncodingType encodingType = EncodingType.Unicode)
    {
        var byteArray = GetBytes(input, encodingType);
        using HashAlgorithm algorithm = CreateHashAlgorithm(hashAlgorithmType);
        return algorithm.ComputeHash(byteArray);
    }

    public static byte[] GetBytes(string input, EncodingType encodingType = EncodingType.Unicode)
        => encodingType switch
        {
            EncodingType.UTF7 => Encoding.UTF7.GetBytes(input),
            EncodingType.UTF8 => Encoding.UTF8.GetBytes(input),
            EncodingType.UTF32 => Encoding.UTF32.GetBytes(input),
            _ => Encoding.Unicode.GetBytes(input),
        };

    public static string GetHashString(string input, HashAlgorithmType hashAlgorithmType, EncodingType encodingType = EncodingType.Unicode, bool upperCase = false)
    {
        var hashedBytes = ComputeHash(input, hashAlgorithmType, encodingType)!;
        string hex = BitConverter.ToString(hashedBytes).Replace("-", "");
        return upperCase ? hex.ToUpper() : hex.ToLower();
    }

    private static HashAlgorithm CreateHashAlgorithm(HashAlgorithmType hashAlgorithmType)
    {
        return hashAlgorithmType switch
        {
            HashAlgorithmType.MD5 => MD5.Create(),
            HashAlgorithmType.SHA1 => SHA1.Create(),
            HashAlgorithmType.SHA256 => SHA256.Create(),
            HashAlgorithmType.SHA384 => SHA384.Create(),
            HashAlgorithmType.SHA512 => SHA512.Create(),
            _ => throw new ArgumentException("Unsupported hash algorithm type")
        };
    }
}
