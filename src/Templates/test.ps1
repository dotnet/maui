param (
    [Parameter(Mandatory=$false, HelpMessage="Specify the type of .NET project to create (e.g., maui, maui-blazor, mauilib, etc.).")]
    [string]$projectType = "maui", # Default to maui if no project type is specified
    [Parameter(Mandatory=$false, HelpMessage="Specify the version number to use for the template pack build (default 13.3.7, should have x.y.z format)")]
    [string]$templateVersion = "13.3.7" # Default to maui if no project type is specified
)

function Get-MauiDotNetVersion {
    param (
        [string]$propsFilePath
    )

    # Load the XML content
    [xml]$xmlContent = Get-Content -Path $propsFilePath

    # Read the values of _MauiDotNetVersionMajor and _MauiDotNetVersionMinor nodes
    $versionMajor = $xmlContent.Project.PropertyGroup._MauiDotNetVersionMajor.InnerText
    # $versionMinor = $xmlContent.Project.PropertyGroup._MauiDotNetVersionMinor.InnerText

    # Concatenate the values with "net" prefix
    $version = "net$versionMajor"

    # Return the concatenated version
    return $version
}

# Clean up previous artifacts
Remove-Item -Path .\tempTemplateOutput -Recurse -Force -ErrorAction SilentlyContinue

# Define the path to the Microsoft.Maui.Templates.csproj
$templatesProjectPath = "src\Microsoft.Maui.Templates.csproj"

# Build the Microsoft.Maui.Templates.csproj project
dotnet build $templatesProjectPath -p:PackageVersion=$templateVersion

# Pack the Microsoft.Maui.Templates.csproj project
dotnet pack $templatesProjectPath -p:PackageVersion=$templateVersion -o .tempTemplateOutput

# Find the resulting nupkg artifact
$nupkgPath = Get-ChildItem -Path .tempTemplateOutput -Filter *.nupkg -Recurse | Select-Object -First 1

if ($nupkgPath -eq $null) {
    Write-Error "No nupkg file found. Ensure the build was successful."
    exit 1
}

$currentMauiVersion = Get-MauiDotNetVersion -propsFilePath "..\..\Directory.Build.props"
dotnet new uninstall Microsoft.Maui.Templates.$currentMauiVersion

# Install the template pack
dotnet new install $nupkgPath.FullName

# Create a new dotnet project using the specified project type
dotnet new $projectType -o .tempTemplateOutput\NewProject --force

# Start Visual Studio with the newly created project
$solutionPath = Resolve-Path ".\.tempTemplateOutput\NewProject\NewProject.csproj"

if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    Start-Process "devenv.exe" -ArgumentList $solutionPath
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
    Start-Process "code" -ArgumentList $solutionPath
} else {
    Write-Error "Unsupported operating system."
}