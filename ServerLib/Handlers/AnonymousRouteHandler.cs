using ServerBrains.Sessions;

namespace ServerBrains.Handlers;

/// <summary>
/// Page is always visible
/// </summary>
public class AnonymousRouteHandler : RouteHandler
{
  public AnonymousRouteHandler(Func<Session, Dictionary<string, string>, string>
  handler) : base(handler)
  {
  }

  public override string Handle(Session session, Dictionary<string, string> parms)
    => handler(session, parms);
  
  

}