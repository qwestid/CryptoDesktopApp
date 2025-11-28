using System;

namespace CryptoDesktopApp.Models;

public class CryptoContainer
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CryptoProvider Provider { get; set; }
    public string Reader { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime CreatedAt { get; set; }
}

public enum CryptoProvider
{
    CryptoPro,
    VipNet,
    Unknown
}

public class CertificateInfo
{
    public string Subject { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string Thumbprint { get; set; } = string.Empty;
    public CryptoProvider Provider { get; set; }
}

public class SignResult
{
    public bool Success { get; set; }
    public byte[]? Signature { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
}
