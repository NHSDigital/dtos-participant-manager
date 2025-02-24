using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

public class NhsNumberHashingPolicy : IDestructuringPolicy
{
  private static readonly Regex NhsNumberPattern = new Regex(@"\b\d{10}\b", RegexOptions.Compiled);
  //private static readonly bool DisableHashing = Environment.GetEnvironmentVariable("DISABLE_NHS_HASHING")?.ToLower() == "false";
  public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
  {
    if (value is string strValue && NhsNumberPattern.IsMatch(strValue))
    {
      string loggedValue = $"[HASHED:{DataMasking.HashNhsNumber(strValue)}]";
      // if( DisableHashing )
      // {
      //   loggedValue = strValue;
      // }

      result = new ScalarValue(loggedValue);
      return true;
    }
    result = null;
    return false;
  }
}

public static class DataMasking
{
    public static string HashNhsNumber(string nhsNumber)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = Encoding.UTF8.GetBytes(nhsNumber);
            byte[] hashBytes = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hashBytes);
        }
    }
}
