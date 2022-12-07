<#
.SYNOPSIS

Get asset information using darc

.DESCRIPTION

This will get asset and nuget

.PARAMETER identity

The identity of the asset to retrive

.PARAMETER version

The version of the asset to retrive

.PARAMETER token

A Build Asset Registry (BAR) password for interacting with Maestro++/BAR (e.g. obtaining build information needed for a drop) 

#>

param
(
    [string] $identity,
    [string] $version,
    [string] $token = ""
)

if($token -eq "")
{
    $token = $Env:BAR_PAT
}

if($token -eq "")
{
    Get-ChildItem env:
    Write-Host "No token to access maestro was provided"
    return
}

darc get-asset --name $identity --version $version --max-age 90 --password $token
