using System.Net;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace ServerBrains;

class Router
{
  public string WebsitePath { get; set; }
  private Dictionary<string, ExtensionInfo> extFolderMap;
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
    string text = File.ReadAllText(fullPath);
    ResponsePacket ret = new ResponsePacket()
    {
      Data = Encoding.UTF8.GetBytes(text),
      ContentType = extInfo.ContentType,
      Encoding = Encoding.UTF8
    };
    return ret;
  }

  private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
  {
    ResponsePacket ret = new ResponsePacket();
    if (fullPath == WebsitePath)
    {
      ret = Route("GET", "/index.html", null);
    }
    else
    {
      if (string.IsNullOrEmpty(ext))
      {
        fullPath = fullPath + "index.html";
      }
      fullPath = WebsitePath + "\\Pages" + "\\"+ fullPath.Split("/").TakeLast(1).First();
      ret = FileLoader(fullPath, ext, extInfo);
    }

    return ret;
  }

  public ResponsePacket Route(string verb, string path, Dictionary<string, string>? kvParams)
  {
  
    string ext = path.Split('.').TakeLast(1).First();
    if (ext == "/")
      ext = string.Empty;
    ExtensionInfo extInfo;
    ResponsePacket response = null;

    if (extFolderMap.TryGetValue(ext, out extInfo))
    {
      string fullPath = Path.Join(WebsitePath, path);
      response = extInfo.Loader(fullPath, ext, extInfo);
    }

    return response;
  }

  
}

public class ResponsePacket
{
  public string Redirect { get; set; }
  public byte[] Data { get; set; }
  public string ContentType { get; set; }
  public Encoding Encoding { get; set; }
}

internal class ExtensionInfo
{
  public string ContentType { get; set; }
  public Func< string, string, ExtensionInfo, ResponsePacket> Loader { get; set; }
}

