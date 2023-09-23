namespace ServerBrains.Sessions;

/// <summary>
/// Sessions are associated with client IP.
/// </summary>
public class Session
{
  public DateTime LastConnection { get; set; }
  public bool Authorized { get; set; }

  /// <summary>
  /// Can be used  by controllers to add additional information that needs to persist
  /// in the session
  /// </summary>
  /// <value></value>
  public Dictionary<string, string> Objects { get; set; }

  public Session()
  {
    Objects = new();
    UpdateLastConnectionTime();
  }

  public void UpdateLastConnectionTime()
  {
    LastConnection = DateTime.Now;
  }

  /// <summary>
  /// Returns true if the last request exceeds the specified expiration time in seconds.
  /// </summary>
  public bool IsExpired(int expirationInSeconds)
  {
    return (DateTime.Now - LastConnection).TotalSeconds > expirationInSeconds;
  }
}
