using System.Security.Cryptography;
using System.Text;

namespace ParticipantManager.Shared;

public static class DataMasking
{
    public static string HashNhsNumber(string nhsNumber)
    {
        using (var sha256 = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(nhsNumber);
            var hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
