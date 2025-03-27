using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;

namespace ParticipantManager.Shared;

public class NhsNumberHashingPolicy : IDestructuringPolicy
{
    private const string NhsNumberPattern = @"\b\d{10}\b";

    private static readonly Regex NhsNumberRegex =
        new(NhsNumberPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly bool DisableHashing =
        Environment.GetEnvironmentVariable("DISABLE_NHS_HASHING")?.ToLower() == "false";

    public bool TryDestructure(object value, ILogEventPropertyValueFactory propertyValueFactory,
        out LogEventPropertyValue result)
    {
        var properties = value.GetType().GetProperties();

        var structureProperties = new List<LogEventProperty>();

        // Hash property values that match NHS Number format
        foreach (var property in properties)
        {
            var propertyValue = property.GetValue(value)?.ToString();

            if (!string.IsNullOrEmpty(propertyValue) && NhsNumberRegex.IsMatch(propertyValue))
            {
                var hashedNhsNumberValue = $"[HASHED:{DataMasking.HashNhsNumber(propertyValue)}]";
                if (DisableHashing) hashedNhsNumberValue = propertyValue;

                structureProperties.Add(new LogEventProperty(property.Name, new ScalarValue(hashedNhsNumberValue)));
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
