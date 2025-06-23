[CmdletBinding(PositionalBinding=$false)]
Param(
  [switch][Alias('h')]$help,
  [switch][Alias('t')]$test,
  [ValidateSet("Debug","Release")][string[]][Alias('c')]$configuration = @("Debug"),
  [string][Alias('v')]$verbosity = "minimal",
  [switch]$vs,
  [ValidateSet("windows","linux","osx")][string]$os,
  [switch]$testnobuild,
  [ValidateSet("x86","x64","arm","arm64")][string[]][Alias('a')]$arch = @([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant()),

  # Run tests with code coverage
  [Parameter(ParameterSetName='CommandLine')]
  [switch] $testCoverage,

  [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

function Get-Help() {
  Write-Host "Common settings:"
  Write-Host "  -arch (-a)                     Target platform: x86, x64, arm or arm64."
  Write-Host "                                 [Default: Your machine's architecture.]"
  Write-Host "  -binaryLog (-bl)               Output binary log."
  Write-Host "  -configuration (-c)            Build configuration: Debug or Release."
  Write-Host "                                 [Default: Debug]"
  Write-Host "  -help (-h)                     Print help and exit."
  Write-Host "  -os                            Target operating system: windows, linux or osx."
  Write-Host "                                 [Default: Your machine's OS.]"
  Write-Host "  -verbosity (-v)                MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]."
  Write-Host "                                 [Default: Minimal]"
  Write-Host "  -vs                            Open the solution with Visual Studio using the locally acquired SDK."
  Write-Host ""

  Write-Host "Actions (defaults to -restore -build):"
  Write-Host "  -build (-b)             Build all source projects."
  Write-Host "                          This assumes -restore has been run already."
  Write-Host "  -clean                  Clean the solution."
  Write-Host "  -pack                   Package build outputs into NuGet packages."
  Write-Host "  -publish                Publish artifacts (e.g. symbols)."
  Write-Host "                          This assumes -build has been run already."
  Write-Host "  -rebuild                Rebuild all source projects."
  Write-Host "  -restore                Restore dependencies."
  Write-Host "  -sign                   Sign build outputs."
  Write-Host "  -test (-t)              Incrementally builds and runs tests."
  Write-Host "                          Use in conjunction with -testnobuild to only run tests."
  Write-Host "  -testCoverage           Run unit tests and capture code coverage information."
  Write-Host ""

  Write-Host "Libraries settings:"
  Write-Host "  -testnobuild            Skip building tests when invoking -test."
  Write-Host ""

  Write-Host "Command-line arguments not listed above are passed through to MSBuild."
  Write-Host "The above arguments can be shortened as much as to be unambiguous."
  Write-Host "(Example: -con for configuration, -t for test, etc.)."
  Write-Host ""
}

if ($help) {
  Get-Help
  exit 0
}

if ($vs) {
  $solution = Split-Path $PSScriptRoot -Parent | Join-Path -ChildPath "xcsync.sln"

  . $PSScriptRoot\common\tools.ps1

  # This tells .NET Core to use the bootstrapped runtime
  $env:DOTNET_ROOT=InitializeDotNetCli -install:$true -createSdkLocationFile:$true

  # This tells MSBuild to load the SDK from the directory of the bootstrapped SDK
  $env:DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR=$env:DOTNET_ROOT

  # Put our local dotnet.exe on PATH first so Visual Studio knows which one to use
  $env:PATH=($env:DOTNET_ROOT + ";" + $env:PATH);

  # Launch Visual Studio with the locally defined environment variables
  ."$solution"

  exit 0
}

# Check if an action is passed in
$actions = "b","build","r","restore","rebuild","sign","testnobuild","publish","clean"
$actionPassedIn = @(Compare-Object -ReferenceObject @($PSBoundParameters.Keys) -DifferenceObject $actions -ExcludeDifferent -IncludeEqual).Length -ne 0
if ($null -ne $properties -and $actionPassedIn -ne $true) {
  $actionPassedIn = @(Compare-Object -ReferenceObject $properties -DifferenceObject $actions.ForEach({ "-" + $_ }) -ExcludeDifferent -IncludeEqual).Length -ne 0
}

if (!$actionPassedIn) {
  $arguments = "-restore -build"
}

foreach ($argument in $PSBoundParameters.Keys)
{
  switch($argument)
  {
    "testCoverage"           { <# this argument is handled in this script only #> }
    "os"                     { $arguments += " /p:TargetOS=$($PSBoundParameters[$argument])" }
    "properties"             { $arguments += " " + $properties }
    "verbosity"              { $arguments += " -$argument " + $($PSBoundParameters[$argument]) }
    "configuration"          { $configuration = (Get-Culture).TextInfo.ToTitleCase($($PSBoundParameters[$argument])); $arguments += " -configuration $configuration" }
    "arch"                   { $arguments += " /p:TargetArchitecture=$($PSBoundParameters[$argument])" }
    default                  { $arguments += " /p:$argument=$($PSBoundParameters[$argument])" }
  }
}

if ($env:TreatWarningsAsErrors -eq 'false') {
  $arguments += " -warnAsError 0"
}

Write-Host "& `"$PSScriptRoot/common/build.ps1`" $arguments"
Invoke-Expression "& `"$PSScriptRoot/common/build.ps1`" $arguments"


# Perform code coverage as the last operation, this enables the following scenarios:
#   .\build.cmd -restore -build -c Release -testCoverage
if ($testCoverage) {
  try {
    # Install required toolset
    . $PSScriptRoot/common/tools.ps1
    InitializeDotNetCli -install $true | Out-Null

    Push-Location $PSScriptRoot/../

    $testResultPath = "./artifacts/TestResults/$configuration";

    # Run tests and collect code coverage
    ./.dotnet/dotnet dotnet-coverage collect --settings ./eng/CodeCoverage.config --output $testResultPath/local.cobertura.xml "build.cmd -test -configuration $configuration"

    # Generate the code coverage report and open it in the browser
    ./.dotnet/dotnet reportgenerator -reports:$testResultPath/*.cobertura.xml -targetdir:$testResultPath/CoverageResultsHtml -reporttypes:HtmlInline_AzurePipelines
    Start-Process $testResultPath/CoverageResultsHtml/index.html
  }
  catch {
    Write-Host $_.Exception.Message -Foreground "Red"
    Write-Host $_.ScriptStackTrace -Foreground "DarkGray"
    exit $global:LASTEXITCODE;
  }
  finally {
    Pop-Location
  }
}

exit 0