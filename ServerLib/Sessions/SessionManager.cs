using System.Net;

namespace ServerBrains.Sessions;
public class SessionManager
{
  /// <summary>
  /// Track all sessions.
  /// </summary>
  protected Dictionary<IPAddress, Session> sessionMap = new Dictionary<IPAddress, Session>();

  // TODO: We need a way to remove very old sessions so that the server doesn't accumulate thousands of stale endpoints.

  public SessionManager()
  {
    sessionMap = new Dictionary<IPAddress, Session>();
  }

  /// <summary>
  /// Creates or returns the existing session for this remote endpoint.
  /// </summary>
  public Session GetSession(IPEndPoint remoteEndPoint)
  {
    // The port is always changing on the remote endpoint, so we can only use IP portion.
    // Session session = sessionMap.CreateOrGet(remoteEndPoint.Address);
    Session session = sessionMap[remoteEndPoint.Address];
    return session;
  }
}