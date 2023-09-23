using ServerBrains.Sessions;

namespace ServerBrains.Handlers;

/// <summary>
/// Page is only visible to authorized users.
/// </summary>
public class AuthenticatedRouteHandler : RouteHandler
{
  public AuthenticatedRouteHandler(Func<Session, Dictionary<string, string>, string>
  handler) : base(handler)
  {
  }

  public override string Handle(Session session, Dictionary<string, string> parms)
  {
    string ret;

    if (session.Authorized)
    {
      ret = handler(session, parms);
    }
    else
    {
      ret = Server.onError(ServerError.NotAuthorized)!;
    }

    return ret;
  }
}