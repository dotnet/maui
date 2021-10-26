$PACKAGEVERSION = "6.0.101-preview.10.9991"

& dotnet new -u "Microsoft.Maui.Templates"
# & dotnet new -u "../../artifacts/Microsoft.Maui.Templates.$PACKAGEVERSION.nupkg"

if (Test-Path "../../artifacts/Microsoft.Maui.Templates.*.nupkg") {
  Remove-Item -Force "../../artifacts/Microsoft.Maui.Templates.*.nupkg"
}

& dotnet build -t:Rebuild src/Microsoft.Maui.Templates.csproj -p:PackageVersion="$PACKAGEVERSION"
& dotnet pack src/Microsoft.Maui.Templates.csproj -p:PackageVersion="$PACKAGEVERSION"
& dotnet new -u Microsoft.Maui.Templates

if (Test-Path ~/.templateengine/) {
    Remove-Item -Path ~/.templateengine/ -Recurse -Force
}

& dotnet new -i "../../artifacts/Microsoft.Maui.Templates.$PACKAGEVERSION.nupkg"

if (Test-Path ./TestMaui/) {
  Remove-Item -Force -Recurse ./TestMaui/
}

& dotnet new maui -n TestMaui
# & code ./TestMaui/