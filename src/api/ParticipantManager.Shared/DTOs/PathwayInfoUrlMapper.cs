namespace ParticipantManager.Shared.DTOs;

public static class PathwayInfoUrlMapper
{
  private static readonly Dictionary<string, string> PathwayUrls = new()
  {
    { "Breast Screening Routine", "https://www.nhs.uk/conditions/breast-screening-mammogram/" },
    { "Cervical Screening Routine", "https://www.nhs.uk/conditions/cervical-screening/" },
    { "Bowel Screening Routine", "https://www.nhs.uk/conditions/bowel-cancer/" }
  };

  public static string GetUrl(string pathwayName)
  {
    return PathwayUrls.TryGetValue(pathwayName, out var url) ? url : "https://example.com/default-screening";
  }
}
