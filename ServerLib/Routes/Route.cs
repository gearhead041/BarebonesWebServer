namespace ServerBrains.Routes;

public class Route
{
  public required string Verb { get; set; }
  public required string Path { get; set; }
  public required Func<Dictionary<string,string>?, string> Action { get; set; }
}