function Get-MauiDotNetVersion {
    param (
        [string]$propsFilePath = "..\..\Directory.Build.props"
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

function Uninstall-MauiTemplates {
    $currentMauiVersion = Get-MauiDotNetVersion
    dotnet new uninstall Microsoft.Maui.Templates.$currentMauiVersion
}

function Empty-UserHomeTemplateEngineFolder {
    if (Test-Path ~/.templateengine/) {
        Remove-Item -Path ~/.templateengine/ -Recurse -Force
    }
}