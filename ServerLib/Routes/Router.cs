using System.Text;

namespace ServerBrains.Routes;

public class Router
{
  public string WebsitePath { get; set; }
  private Dictionary<string, ExtensionInfo> extFolderMap;

  public readonly static string GET = "get";
  public readonly static string POST = "post";
  public readonly static string PUT = "put";
  public readonly static string HEAD = "head";
  public readonly static string DELETE = "delete";
  public readonly static string TRACE = "trace";
  public readonly static string OPTIONS = "options";
  public readonly static string CONNECT = "connect";

  private readonly static List<Route> routes = new();
  public Router()
  {
    extFolderMap = new Dictionary<string, ExtensionInfo>()
    {
      {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
      {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
      {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
      {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
      {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
      {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
      {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
      {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
      {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
    };
  }

  /// <summary>
  /// Read in an image file and returns a responsePacket with the raw data.
  /// </summary>
  /// <param name="fullPath"></param>
  /// <param name="ext"></param>
  /// <param name="extInfo"></param>
  /// <returns></returns>
  private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extInfo)
  {
    FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    BinaryReader br = new BinaryReader(fileStream);
    ResponsePacket ret = new ResponsePacket()
    {
      Data = br.ReadBytes((int)fileStream.Length),
      ContentType = extInfo.ContentType
    };
    br.Close();
    fileStream.Close();
    return ret;
  }

  /// <summary>
  /// Read in what is basically a text file and return a ResponsePacket with the
  /// text UTF8 encoded.
  /// </summary>
  /// <param name="fullPath"></param>
  /// <param name="ext"></param>
  /// <param name="extInfo"></param>
  /// <returns></returns>
  private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
  {
    try
    {
      string text = File.ReadAllText(fullPath);
      ResponsePacket ret = new ResponsePacket()
      {
        Data = Encoding.UTF8.GetBytes(text),
        ContentType = extInfo.ContentType,
        Encoding = Encoding.UTF8
      };
      return ret;

    }
    catch (FileNotFoundException)
    {
      ResponsePacket ret = new ResponsePacket()
      {
        Error = ServerError.FileNotFound
      };
      return ret;
    }
    catch (DirectoryNotFoundException)
    {
      fullPath = WebsitePath + fullPath;
      string text = File.ReadAllText(fullPath);
      ResponsePacket ret = new ResponsePacket()
      {
        Data = Encoding.UTF8.GetBytes(text),
        ContentType = extInfo.ContentType,
        Encoding = Encoding.UTF8
      };
      return ret;
    }
  }

/// <summary>
/// Recognizes html files and loads them
/// </summary>
/// <param name="fullPath"></param>
/// <param name="ext"></param>
/// <param name="extInfo"></param>
/// <returns></returns>
  private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
  {
    ResponsePacket ret;
    if (fullPath == WebsitePath+"\\")
    {
      ret = Route(GET, "/index.html", null);
    }
    else
    {
      if (string.IsNullOrEmpty(ext))
      {
        fullPath += ".html";
      }
      //insert pages into path
      fullPath = WebsitePath + "\\Pages" + fullPath;
      ret = FileLoader(fullPath, ext, extInfo);
      if (ret.Error == ServerError.FileNotFound)
        ret.Error = ServerError.PageNotFound;
      return ret;
    }
    return ret;
  }

  public void AddRoute(Route route)
  {
    routes.Add(route);
  }

  public ResponsePacket Route(string verb, string? path, Dictionary<string, string>? kvParams)
  {
    string ext = string.Empty;
    if (path.Contains('.'))
    {
      ext = path.Split('.').TakeLast(1).First();
    }
    ExtensionInfo extInfo;
    ResponsePacket? response = null;
    verb = verb.ToLower();

    if (extFolderMap.TryGetValue(ext, out extInfo!))
    {
      string newPath = path.Replace("/", "\\");
      string fullPath;
      if (path == "/")
        fullPath = Path.Join(WebsitePath, newPath);
      else
        fullPath = Path.Combine(WebsitePath, newPath);
      Route? route = routes?.SingleOrDefault(r => verb == r.Verb.ToLower() && path == r.Path);
      if (route != null)
      {
        string redirect = route.Action(kvParams);
        if (string.IsNullOrEmpty(redirect))
        {
          response = extInfo.Loader(fullPath, ext, extInfo);
        }
        else
        {
          response = new ResponsePacket() { Redirect = redirect };
        }
      }
      else
      {
        response = extInfo?.Loader(fullPath, ext, extInfo);
      }
    }
    else
    {
      response = new ResponsePacket() { Error = ServerError.UnknownType };
    }
    return response;
  }


}

public class ResponsePacket
{
  public string? Redirect { get; set; }
  public byte[]? Data { get; set; }
  public string? ContentType { get; set; }
  public Encoding? Encoding { get; set; }
  public ServerError Error { get; set; }
}

internal class ExtensionInfo
{
  public string? ContentType { get; set; }
  public Func<string, string, ExtensionInfo, ResponsePacket>? Loader { get; set; }
}

