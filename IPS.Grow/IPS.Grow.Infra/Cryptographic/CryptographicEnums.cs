namespace IPS.Grow.Infra.Cryptographic;

public enum EncodingType
{
    Unicode,
    [Obsolete("UTF7 is obseleted, consider using UTF8 instead")]
    UTF7,
    UTF8,
    UTF32
}

public enum HashAlgorithmType
{
    MD5,
    SHA1,
    SHA256,
    SHA384,
    SHA512
}
