function Dogfood-VS
{
    param(
        [string]$vs = "${env:ProgramFiles}\Microsoft Visual Studio\2022\Preview\Common7\IDE\devenv.exe",
        [string]$dotnet = ".\bin\dotnet\",
        [string]$sln = "",
        [bool] $debug = $false
    )
    # Get full path
    if ($sln -ne "")
    {
        $sln=(Get-Item $sln).FullName
    }
    $oldPATH=$env:PATH
    $oldMSBuildDebugEngine=$env:MSBuildDebugEngine
    $oldDOTNET_MULTILEVEL_LOOKUP=$env:DOTNET_MULTILEVEL_LOOKUP
    try {
        # Put our local dotnet.exe on PATH first so Visual Studio knows which one to use
        $env:PATH = ($dotnet + ";" + $env:PATH)
        # Get "full" .binlog in Project System Tools
        if ($debug -eq $true) {
            $env:MSBuildDebugEngine='1'
        }
        $env:DOTNET_MULTILEVEL_LOOKUP='0'
        & "$vs" "$sln"
    } finally {
        $env:PATH=$oldPATH
        $env:MSBuildDebugEngine=$oldMSBuildDebugEngine
        $env:DOTNET_MULTILEVEL_LOOKUP=$oldDOTNET_MULTILEVEL_LOOKUP
    }
}