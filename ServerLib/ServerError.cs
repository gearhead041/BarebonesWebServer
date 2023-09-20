namespace ServerBrains;

public enum ServerError
{
  OK,
  ExpiredSession,
  NotAuthorized,
  FileNotFound,
  PageNotFound,
  ServerError,
  UnknownType
}