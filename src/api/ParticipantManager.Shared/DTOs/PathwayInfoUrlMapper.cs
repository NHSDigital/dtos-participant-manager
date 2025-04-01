namespace ParticipantManager.Shared.DTOs;

public static class PathwayInfoUrlMapper
{
    #pragma warning disable S1075 // URIs should not be hardcoded
    private static readonly Dictionary<string, string> PathwayUrls = new()
    {
        { "Breast Screening Routine", "https://www.nhs.uk/conditions/breast-screening-mammogram/" },
        { "Cervical Screening Routine", "https://www.nhs.uk/conditions/cervical-screening/" },
        { "Bowel Screening Routine", "https://www.nhs.uk/conditions/bowel-cancer/" }
    };
    #pragma warning restore S1075

    public static string GetUrl(string pathwayName)
    {
        return PathwayUrls.TryGetValue(pathwayName, out var url) ? url : "https://www.nhs.uk/conditions/nhs-screening/";
    }
}
