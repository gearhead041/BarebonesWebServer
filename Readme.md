# BareBones

## Description
___ 
A lightweight server implemented from scratch in C# that can be used to host a website. The server is capable of handling GET and POST requests, and can serve static files and dynamic content. The server is also capable of handling multiple requests at once, and can be configured to use a custom port number.
___
## How to use
1. Clone the repository
2. Open the solution in Visual Studio Code
3. Navigate to the Server Console folder and type in command
   ```
   dotnet run
   ```
4. Terminal would show where webserver is listening
  ```
  Now listening on IP: http://youripaddress/
  ```
5. Logging also implemented in the console
  ```
  [2021-01-01 12:00:00.000] GET / HTTP/1.1 200 0.0000
  ```