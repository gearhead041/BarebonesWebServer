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
  private static HttpListener? listener;
  public static Router router = new Router();
  public static Func<ServerError, string?> onError;

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
    string? path = request.RawUrl?.Split("?")[0];
    string verb = request.HttpMethod;
    string? parms;

    if (request.RawUrl?.Split("?").Length == 1)
      parms = null;
    else
      parms = request.RawUrl?.Split("?")[1];

    Dictionary<string, string> kvParams = GetKeyValues(parms);
    string data = new StreamReader(context.Request.InputStream,
    context.Request.ContentEncoding).ReadToEnd();
    kvParams = GetKeyValues(data, kvParams);
    Log(kvParams);
    var responsePckt = router.Route(verb, path, kvParams);
    if (responsePckt.Error != ServerError.OK)
    {
      responsePckt.Redirect = onError(responsePckt.Error);
    }
    Respond(context.Request, context.Response, responsePckt);
    Log(context.Request);
  }


  private static void Respond(HttpListenerRequest request, HttpListenerResponse response, ResponsePacket resp)
  {
    if (string.IsNullOrEmpty(resp.Redirect))
    {
      response.ContentType = resp.ContentType;
      response.ContentLength64 = resp.Data.Length;
      response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
      response.ContentEncoding = resp.Encoding;
      response.StatusCode = (int)HttpStatusCode.OK;
    }
    else
    {
      response.StatusCode = (int)HttpStatusCode.Redirect;
      response.Redirect("http://" + request.UserHostAddress + resp.Redirect);
    }
    response.OutputStream.Close();
  }

  private static Dictionary<string, string> GetKeyValues(string? paramString)
  {
    if (paramString is null)
      return null;
    Dictionary<string, string> keyValues = new();
    var paramList = paramString.Split("=");
    for (int i = 0; i < paramList.Length - 1; i++)
    {
      keyValues.Add(paramList[i], paramList[i + 1]);
    }
    return keyValues;
  }

  private static Dictionary<string, string> GetKeyValues(string data, Dictionary<string, string> kv = null)
  {
    if (kv == null)
    {
      kv = new Dictionary<string, string>();
    }
    if (data.Length > 0)
    {
      string[] dataList = data.Split('&');
      for (int i = 0; i < dataList.Length; i++)
      {
        var items = dataList[i].Split('=');
        kv[items[0]] = items[1];
      }
    }
    return kv;
  }

  public static string GetWebsitePath()
  {
    string websitePath = Assembly.GetExecutingAssembly().Location;
    websitePath = string.Join("\\", websitePath.Split("\\").SkipLast(4)) + "\\Website";
    return websitePath;
  }

  /// <summary>
  /// Log requests
  /// </summary>
  public static void Log(HttpListenerRequest request)
  {
    Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /"
    + request.Url?.LocalPath);
  }

  private static void Log(Dictionary<string, string> kv)
  {
    foreach (var item in kv)
      Console.WriteLine(item.Key + " : " + item.Value);
  }

  public static string? ErrorHandler(ServerError error)
  {
    string? ret = null;
    switch (error)
    {
      case ServerError.ExpiredSession:
        ret = "/Error/expiredSession.html";
        break;
      case ServerError.FileNotFound:
        ret = "/Error/fileNotFound.html";
        break;
      case ServerError.NotAuthorized:
        ret = "/Error/notAuthorized.html";
        break;
      case ServerError.PageNotFound:
        ret = "/Error/pageNotFound.html";
        break;
      case ServerError.ServerError:
        ret = "/Error/serverError.html";
        break;
      case ServerError.UnknownType:
        ret = "/Error/unknownType.html";
        break;
    }

    return ret;
  }

  /// <summary>
  /// Starts the web server
  /// </summary>
  public static void Start(string websitePath)
  {
    router.WebsitePath = websitePath;
    List<IPAddress> localHostIPs = GetLocalHostIPs();
    HttpListener listener = InitializeListener(localHostIPs);
    Start(listener);
  }


}
