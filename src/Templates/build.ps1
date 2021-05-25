$PACKAGEVERSION = "6.0.100-preview.4.9991"

& dotnet new -u "../../artifacts/Microsoft.Maui.Templates.$PACKAGEVERSION.nupkg"

Remove-Item -Force "../../artifacts/Microsoft.Maui.Templates.*.nupkg"

& dotnet build -t:Rebuild src/Microsoft.Maui.Templates.csproj -p:PackageVersion="$PACKAGEVERSION"
& dotnet pack src/Microsoft.Maui.Templates.csproj -p:PackageVersion="$PACKAGEVERSION"
& dotnet new -u Microsoft.Maui.Templates
& dotnet new -i "../../artifacts/Microsoft.Maui.Templates.$PACKAGEVERSION.nupkg"

Remove-Item -Force -Recurse ./TestMaui/
& dotnet new maui -n TestMaui
# & code ./TestMaui/