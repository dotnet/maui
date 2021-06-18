https://gist.github.com/grendello/0e0829dd159bac26d9a5558735a99a2e

https://github.com/dotnet/diagnostics

dotnet run --project C:\Projects\diagnostics\src\Tools\dotnet-dsrouter\dotnet-dsrouter.csproj -- client-server -tcps 127.0.0.1:9001 -ipcc /tmp/maui-app --verbose debug

dotnet-trace collect --diagnostic-port /tmp/maui-app --format speedscope -o /tmp/hellomaui-app-trace

..\..\bin\dotnet\dotnet.exe build -f net6.0-android -t:run -p:AndroidLinkResources=true -p:AndroidEnableProfiler=true
