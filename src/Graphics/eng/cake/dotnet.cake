// Contains .NET 6-related Cake targets

var ext = IsRunningOnWindows() ? ".exe" : "";
var dotnetPath = $"./bin/dotnet/dotnet{ext}";

Task("dotnet")
    .Description("Provisions .NET 6 into bin/dotnet based on eng/Versions.props")
    .Does(() =>
    {
        var binlog = $"artifacts/dotnet-{configuration}.binlog";
        var settings = new DotNetCoreBuildSettings
        {
            MSBuildSettings = new DotNetCoreMSBuildSettings()
                .EnableBinaryLogger(binlog)
                .SetConfiguration(configuration),
        };
        DotNetCoreBuild("./build/DotNet/DotNet.csproj", settings);
    });

Task("dotnet-pack")
    .Description("Build and create .NET 6 NuGet packages")
    .Does(()=>
    {

        var settings = new DotNetCoreToolSettings
        {
            DiagnosticOutput = true,
            ArgumentCustomization = args => args.Append($"./eng/package.ps1 -configuration \"{configuration}\"")
        };

        DotNetCoreTool("pwsh", settings);

    });