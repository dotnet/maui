# script inspired by https://andrewlock.net/simplifying-the-cake-global-tool-bootstrapper-scripts-in-netcore3-with-local-tools/

[CmdletBinding()]
Param(
    [string]$Script = "build.cake",
    [string]$Target,
    [Parameter(Position=0,Mandatory=$false,ValueFromRemainingArguments=$true)]
    [string[]]$ScriptArgs
)

# Restore Cake tool
# & dotnet tool restore
$tools = Get-Content ".config/dotnet-tools.json" | ConvertFrom-Json
foreach ($tool in $tools.tools.PsObject.Properties) {
    & dotnet tool install $tool.Name --version $tool.Value.version
}

# Build Cake arguments
$cakeArguments = @("$Script");
if ($Target) { $cakeArguments += "--target=$Target" }
$cakeArguments += $ScriptArgs

& dotnet tool run dotnet-cake -- $cakeArguments
exit $LASTEXITCODE
