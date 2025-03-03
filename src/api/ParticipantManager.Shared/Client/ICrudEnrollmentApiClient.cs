namespace ParticipantManager.Shared;

public interface ICrudEnrollmentApiClient
{
  Task CreateEnrollmentAsync(string nhsNumber);
}
