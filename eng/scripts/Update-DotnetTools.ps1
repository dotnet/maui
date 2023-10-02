<#
.SYNOPSIS
    Updates the dotnet-tools.json file with the tools specified in the input JSON file
    and runs 'dotnet tool restore' for each tool.

.DESCRIPTION
    This script reads a JSON file containing tool information, updates the dotnet-tools.json
    file with the specified tools, and restores each tool using 'dotnet tool restore'.

.PARAMETER toolsJsonPath
    The path to the JSON file containing tool information.

.PARAMETER outputFolder
    The folder where the updated dotnet-tools.json file will be saved.

.EXAMPLE
    PS> .\Update-DotnetTools.ps1 -toolsJsonPath "path/to/tools.json" -outputFolder "path/to/output"
#>

param (
    [Parameter(Mandatory = $true, HelpMessage = "Path to the JSON file containing tool information.")]
    [string] $toolsJsonPath,

    [Parameter(Mandatory = $true, HelpMessage = "Folder where the updated dotnet-tools.json file will be saved.")]
    [string] $outputFolder
)

# Read the tools.json file and parse it as JSON
$toolsJson = Get-Content $toolsJsonPath | ConvertFrom-Json

# Create the .config folder if it doesn't exist
if (-not (Test-Path $outputFolder)) {
    New-Item -ItemType Directory -Path $outputFolder | Out-Null
}

# Iterate over the tools and update dotnet-tools.json one tool at a time
foreach ($tool in $toolsJson.tools.psobject.Properties) {
    $toolName = $tool.Name
    $toolInfo = $tool.Value

    # Add the current tool to the updated tools JSON
    $updatedToolsJson = @{
        version = $toolsJson.version
        isRoot = $toolsJson.isRoot
        tools = @{
            $toolName = $toolInfo
        }
    } | ConvertTo-Json -Depth 10

    # Path to the dotnet-tools.json file in the output folder
    $updatedJsonPath = Join-Path -Path $outputFolder -ChildPath "dotnet-tools.json"

    # Write the updated dotnet-tools.json to the output folder
    $updatedToolsJson | Set-Content $updatedJsonPath

    Write-Host "Updated dotnet-tools.json for tool: $toolName saved to: $updatedJsonPath"

    # Run dotnet tool restore for the current tool
    Write-Host "Restoring tool: $toolName"
    dotnet tool restore --tool-manifest $updatedJsonPath
}

Write-Host "All tools restored successfully."
