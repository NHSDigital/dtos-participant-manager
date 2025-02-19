namespace ParticipantManager.Experience.API;

public static class PathwayInfoUrlMapper
{
  private static readonly Dictionary<string, string> PathwayUrls = new()
      {
          { "Breast Screening Regular", "https://www.nhs.uk/conditions/breast-screening-mammogram/" },
          { "Cervical Screening", "https://www.nhs.uk/conditions/cervical-screening/" },
          { "Bowel Screening", "https://www.nhs.uk/conditions/bowel-cancer/" }
      };

  public static string GetUrl(string pathwayName)
  {
      return PathwayUrls.TryGetValue(pathwayName, out var url) ? url : "https://example.com/default-screening";
  }
}

