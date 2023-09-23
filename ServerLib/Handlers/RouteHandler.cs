using ServerBrains.Sessions;
namespace ServerBrains.Handlers;

/// <summary>
/// Base Class of Route Handlers
/// </summary>
public abstract class RouteHandler
{
  protected Func<Session, Dictionary<string, string>, string> handler;
  public RouteHandler(Func<Session, Dictionary<string, string>, string> handler)
  {
    this.handler = handler;
  }

  public abstract string Handle(Session session, Dictionary<string, string> parms);
}