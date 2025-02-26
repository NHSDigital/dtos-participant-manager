namespace ParticipantManager.Shared;

using Serilog.Core;
using Serilog.Events;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Serilog.Enrichers.Sensitive;

public class NhsNumberHashingPolicy : IDestructuringPolicy
{
  private const string NHSPattern =@"\b\d{10}\b";
  private static readonly Regex NhsNumberPattern = new Regex(NHSPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
  private static readonly bool DisableHashing = Environment.GetEnvironmentVariable("DISABLE_NHS_HASHING")?.ToLower() == "false";

  public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory, out LogEventPropertyValue result)
  {
    Console.WriteLine("Destruct");
    var properties = value.GetType().GetProperties().Where(p => p.PropertyType == typeof(string));

    var structureProperties = new List<LogEventProperty>();

    // Hash property values that match NHS Number format
    foreach (var property in properties)
    {
      string? propertyValue = property.GetValue(value) as string;

      if (!string.IsNullOrEmpty(propertyValue) && NhsNumberPattern.IsMatch(propertyValue))
      {
        string loggedValue = $"[HASHED:{DataMasking.HashNhsNumber(propertyValue)}]";
        if( DisableHashing )
        {
          loggedValue = propertyValue;
        }

        structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue(loggedValue)));
      }
      else
      {
        structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue(propertyValue)));
      }
    }

    result = new StructureValue(structureProperties);
    return true;
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

public class NhsNumberRegexMaskOperator : RegexMaskingOperator
{
  private const string NHSPattern =@"\d{10}";
  public NhsNumberRegexMaskOperator() : base(NHSPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
  {
  }
}
