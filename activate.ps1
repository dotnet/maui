#
# This file must be used by invoking ". .\activate.ps1" from the command line.
# You cannot run it directly.
# To exit from the environment this creates, execute the 'deactivate' function.
#

if ($MyInvocation.InvocationName -ne '.') {
    Write-Host -f Red "This script must be dot sourced. Run it by invoking '. .\activate.ps1'."
    return
}

function deactivate ([switch]$init) {
    # reset old environment variables
    if (Test-Path variable:_OLD_PATH) {
        $env:PATH = $_OLD_PATH
        Remove-Item variable:_OLD_PATH
    }

    if (Test-Path function:_old_prompt) {
        Set-Item Function:prompt -Value $function:_old_prompt -ErrorAction Ignore
        Remove-Item function:_old_prompt
    }

    Remove-Item env:DOTNET_ROOT -ErrorAction Ignore
    if (-not $init) {
        # Remove the deactivate function
        Remove-Item function:deactivate
    }
}

# Cleanup the environment
deactivate -init

$_OLD_PATH = $env:PATH
# Tell dotnet where to find itself
$env:DOTNET_ROOT = Join-Path $PSScriptRoot '.dotnet'
# Put dotnet first on PATH
$env:PATH = "$env:DOTNET_ROOT$([IO.Path]::PathSeparator)$env:PATH"

# Set the shell prompt
if (-not $env:DISABLE_CUSTOM_PROMPT) {
    $function:_old_prompt = $function:prompt
    function maui_prompt {
        # Add a prefix to the current prompt, but don't discard it.
        Write-Host -f Green "(maui) " -NoNewLine
        & $function:_old_prompt
    }

    Set-Item Function:prompt -Value $function:maui_prompt -ErrorAction Ignore
}

Write-Host -f Magenta "Enabled the .NET environment for MAUI. Execute 'deactivate' to exit."

$_exeName = 'dotnet'
if ($PSVersionTable.PSVersion.Major -lt 6 -or $IsWindows) {
    $_exeName = 'dotnet.exe'
}

$_dotnet = Join-Path $env:DOTNET_ROOT $_exeName
if (-not (Test-Path $_dotnet)) {
    Write-Host -f Yellow ".NET has not been installed yet. Run '.\build.ps1 -Target dotnet' to install it."
}
else {
    Write-Host "dotnet = $_dotnet - $(& $_dotnet --version)"
}
Remove-Item variable:_dotnet, variable:_exeName
