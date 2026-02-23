param (
    [Parameter(Mandatory=$false, HelpMessage="Specify the type of .NET project to create (e.g., maui, maui-blazor, mauilib, etc.).")]
    [string]$projectType = "maui", # Default to maui if no project type is specified
    [Parameter(Mandatory=$false, HelpMessage="Specify the version number to use for the template pack build (should have x.y.z format)")]
    [string]$templateVersion = "13.3.7",
    [Parameter(Mandatory=$false, HelpMessage="Specify the path to the template project to build")]
    [string]$templatesProjectPath = "src\Microsoft.Maui.Templates.csproj",
    [Parameter(Mandatory=$false, HelpMessage="Specify whether to start Visual Studio (Code) after creating the new project with the latest template changes")]
    [bool]$startVsAfterBuild = $true,
    [Parameter(Mandatory=$false, HelpMessage="Additional arguments to pass to the dotnet new command (e.g., '--sample-content')")]
    [string]$additionalProjectArgs = ""
)

# Source the utils script for some common functionalities
. .\eng\utils.ps1

# Clean up previous artifacts
Remove-Item -Path .\.tempTemplateOutput -Recurse -Force -ErrorAction SilentlyContinue

# Build the Microsoft.Maui.Templates.csproj project
dotnet build -t:Rebuild $templatesProjectPath -p:PackageVersion=$templateVersion

# Pack the Microsoft.Maui.Templates.csproj project
dotnet pack $templatesProjectPath -p:PackageVersion=$templateVersion -o .tempTemplateOutput

# Find the resulting nupkg artifact
$nupkgPath = Get-ChildItem -Path .tempTemplateOutput -Filter *.nupkg -Recurse | Select-Object -First 1

if ($nupkgPath -eq $null) {
    Write-Error "No templates nupkg file found. Ensure the build was successful."
    exit 1
}

# Uninstall previous (manual) install of .NET MAUI templates
Uninstall-MauiTemplates

# Clean users templates folder
Empty-UserHomeTemplateEngineFolder

# Install the template pack
dotnet new install $nupkgPath.FullName

# Create a new dotnet project using the specified project type
if ($additionalProjectArgs) {
    dotnet new $projectType $additionalProjectArgs -o ./.tempTemplateOutput/NewProject --force
} else {
    dotnet new $projectType -o ./.tempTemplateOutput/NewProject --force
}

if ($startVsAfterBuild -eq $false) {
    exit 0
}

# Start Visual Studio with the newly created project
$projectFilePath = Resolve-Path "./.tempTemplateOutput/NewProject/NewProject.csproj"
$projectFolderPath = Split-Path -Path $projectFilePath

if ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::Windows)) {
    Start-Process "devenv.exe" -ArgumentList $projectFilePath
} elseif ([System.Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([System.Runtime.InteropServices.OSPlatform]::OSX)) {
    # Check if VS Code Insiders is installed
    $vscodeInsidersPath = "/Applications/Visual Studio Code - Insiders.app"
    $vscodeStablePath = "/Applications/Visual Studio Code.app"
    
    if (Test-Path $vscodeInsidersPath) {
        Start-Process "code-insiders" -ArgumentList $projectFolderPath
    } elseif (Test-Path $vscodeStablePath) {
        Start-Process "code" -ArgumentList $projectFolderPath
    } else {
        Write-Error "Neither Visual Studio Code Insiders nor Visual Studio Code stable is installed. Cannot open VS Code, however a new project is created at $projectFolderPath."
    }
} else {
    Write-Error "Unsupported operating system."
}