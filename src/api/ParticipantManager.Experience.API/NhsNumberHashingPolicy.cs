namespace ParticipantManager.Experience.API;

using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Serilog.Enrichers.Sensitive;

public class NhsNumberHashingPolicy : IDestructuringPolicy
{
  private static readonly Regex NhsNumberPattern = new Regex(@"\b\d{10}\b", RegexOptions.Compiled);
  //private static readonly bool DisableHashing = Environment.GetEnvironmentVariable("DISABLE_NHS_HASHING")?.ToLower() == "false";
  public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
  {
    Console.WriteLine("TryDestructure start");
    var properties = value.GetType().GetProperties().Where(p => p.PropertyType == typeof(string));

    foreach (var property in properties)
    {
      string? propertyValue = property.GetValue(value) as string;

      if (!string.IsNullOrEmpty(propertyValue) && NhsNumberPattern.IsMatch(propertyValue))
      {
        string loggedValue = $"[HASHED:{DataMasking.HashNhsNumber(propertyValue)}]";
        // if( DisableHashing )
        // {
        //   loggedValue = strValue;
        // }

        result = new ScalarValue(loggedValue);
        return true;
      }
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

public class NhsNumberRegexMask : RegexMaskingOperator
{
  public NhsNumberRegexMask(string regexString) : base(regexString)
  {
  }
}
