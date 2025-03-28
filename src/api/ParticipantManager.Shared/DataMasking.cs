using System.Security.Cryptography;
using System.Text;

namespace ParticipantManager.Shared;

public static class DataMasking
{
    public static string HashNhsNumber(string nhsNumber)
    {
        var bytes = Encoding.UTF8.GetBytes(nhsNumber);
        var hashBytes = SHA256.HashData(bytes);
        return Convert.ToBase64String(hashBytes);
    }
}
