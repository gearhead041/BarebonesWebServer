using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;


namespace ServerBrains;
/// <summary>
///  A lean and mean web server. Try it if you dare.
/// </summary> 
public class Server
{
  private static HttpListener listener;
  private static Router router = new Router();
  /// <summary>
  /// Returns a list of IP addresses assigned to localhsot network 
  /// devices.
  /// </summary>
  /// 
  private static List<IPAddress> GetLocalHostIPs()
  {
    IPHostEntry host;
    host = Dns.GetHostEntry(Dns.GetHostName());
    List<IPAddress> ret = host.AddressList.Where(ip =>
    ip.AddressFamily == AddressFamily.InterNetwork).ToList();
    return ret;
  }

  private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
  {
    HttpListener listener = new HttpListener();
    listener.Prefixes.Add("http://localhost/");
    localhostIPs.ForEach(ip =>
    {
      listener.Prefixes.Add("http://" + ip.ToString() + "/");
      Console.WriteLine("Listening on IP " + "http://"
      + ip.ToString() + "/");
    });
    return listener;
  }
  public static int maxSimultaneousConnections = 20;
  private static Semaphore sem = new Semaphore(maxSimultaneousConnections,
  maxSimultaneousConnections);

  /// <summary> 
  /// Begin listening to connections on separate worker thread.
  /// </summary>
  private static void Start(HttpListener listener)
  {
    listener.Start();
    Task.Run(() => RunServer(listener));
  }

  /// <summary>
  /// Start awating for connections up to "maxSimultaneousConnections" value.
  /// Runs on a separate thread.
  /// </summary>
  private static void RunServer(HttpListener listener)
  {
    while (true)
    {
      sem.WaitOne();
      StartConnectionListener(listener);
    }
  }

  /// <summary> 
  /// Await connections.
  /// </summary>
  private static async void StartConnectionListener(HttpListener listener)
  {
    HttpListenerContext context = await listener.GetContextAsync();
    sem.Release();
    HttpListenerRequest request = context.Request;
    string path = request.RawUrl.Split("?")[0];
    string verb = request.HttpMethod;
    string parms;
    if(request.RawUrl?.Split("?").Length == 1)
      parms = null;
    else
      parms = request.RawUrl?.Split("?")[1];
    Dictionary<string, string> kvParams = GetKeyValues(parms);
    var responsePckt = router.Route(verb, path, kvParams);
    Respond(context.Response, responsePckt);
    Log(context.Request);
  }

    private static void Respond(HttpListenerResponse response, ResponsePacket resp)
  {
    response.ContentType = resp.ContentType;
    response.ContentLength64 = resp.Data.Length;
    response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
    response.StatusCode = (int)HttpStatusCode.OK;
    response.OutputStream.Close();
  }

  private static Dictionary<string,string> GetKeyValues(string? paramString)
  {
    if (paramString is null)
      return null;
    Dictionary<string, string> keyValues = new ();
    var paramList = paramString.Split("=");
    for (int i = 0; i < paramList.Length - 1; i++)
    {
      keyValues.Add(paramList[i], paramList[i + 1]);
    }
    return keyValues;
  }

  public static string GetWebsitePath()
  {
    string websitePath = Assembly.GetExecutingAssembly().Location;
    websitePath = string.Join("\\",websitePath.Split("\\").SkipLast(4)) + "\\Website";
    return websitePath;
  }

  /// <summary>
  /// Log requests
  /// </summary>
  public static void Log(HttpListenerRequest request)
  {
    Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + "/"
    + request.Url?.AbsoluteUri.Split('/')[1]);
  }
  /// <summary>
  /// Starts the web server
  /// </summary>
  public static void Start()
  {
    router.WebsitePath = GetWebsitePath();
    List<IPAddress> localHostIPs = GetLocalHostIPs();
    HttpListener listener = InitializeListener(localHostIPs);
    Start(listener);
  }


}
