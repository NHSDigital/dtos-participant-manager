using System.Text.RegularExpressions;
using Serilog.Enrichers.Sensitive;

namespace ParticipantManager.Shared;

public class NhsNumberRegexMaskOperator()
    : RegexMaskingOperator(NhsNumberPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled)
{
    private const string NhsNumberPattern = @"\b\d{10}\b";
}
