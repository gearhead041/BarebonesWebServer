using ServerBrains;
using ServerBrains.Routes;
using ServerBrains.Actions;

string websitePath = Server.GetWebsitePath();
Server.onError = Server.ErrorHandler!;
Server.router.AddRoute(new Route()
{
  Verb = Router.POST,
  Path = "/demo/redirect",
  Action = Actions.RedirectMe
});

Server.Start(websitePath);
Console.ReadLine();
