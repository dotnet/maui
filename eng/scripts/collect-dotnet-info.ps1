################################################################################
# Usage:
#
# Current directory:
#
#     . { iwr https://aka.ms/collect-maui-info.ps1 } | iex; cdnmi
#
# Specific sln/folder:
#
#     . { iwr https://aka.ms/collect-maui-info.ps1 } | iex; cdnmi </path/to/sln>
#
# For an added shortcut, if you append ` | scb ` to the command, the output will
# be copied to the clipboard!
#
################################################################################

New-Module -Name Collect-DotNet-Maui-Info -ScriptBlock {
    
    function Write-Both {
        [CmdletBinding(PositionalBinding=$false)]
        param (
            [Parameter(Position=0)]
            [AllowNull()]
            [AllowEmptyString()]
            [AllowEmptyCollection()]
            [string[]]$Messages,
            [Parameter(Mandatory=$false)]
            [AllowNull()]
            [System.ConsoleColor]$ForegroundColor
        )
    
        if ($ForegroundColor) {
            $previousColor = $Host.UI.RawUI.ForegroundColor
            $Host.UI.RawUI.ForegroundColor = $ForegroundColor
            Write-Output $Messages
            $Host.UI.RawUI.ForegroundColor = $previousColor
        } else {
            Write-Output $Messages
        }
    }

    function Collect-DotNet-Maui-Info {
        [CmdletBinding()]
        param (
            [Parameter(ValueFromPipeline=$true)]
            [string]
            $Solution
        )
        
        if (!$Solution) {
            $Solution = Get-Location
        }
        
        $WorkingDir = $Solution
        if (Test-Path $Solution -PathType Leaf) {
            $WorkingDir = Split-Path $Solution
        }
        
        Write-Host ""
        Write-Host "===================================" -ForegroundColor Magenta
        Write-Host "= .NET MAUI Information Collector =" -ForegroundColor Magenta
        Write-Host "===================================" -ForegroundColor Magenta
        Write-Host ""
        Write-Host "This script is a quick way to collect your .NET MAUI and VS install information" -ForegroundColor DarkBlue
        Write-Host "So that you can copy-paste it into a GitHub issue without having to work hard." -ForegroundColor DarkBlue
        Write-Host ""
        Write-Host "Your information:"
        Write-Host " * Working directory: '$WorkingDir'"
        Write-Host ""
        
        Write-Host "Copy the information below this line and paste into GitHub:" -ForegroundColor Yellow
        Write-Host "--------------------------------------------------------------------------------" -ForegroundColor Yellow
        Write-Both ""
        
        Push-Location $WorkingDir
        
        try {
            # Detect and print the short .NET SDK info
            Write-Both "**.NET SDK Information:**"
            Write-Both ""
            Write-Both "Version: ``$(dotnet --version)``"
            Write-Both "Workloads:"
            Write-Both "``````"
            $dotnetWorkloads = $(dotnet workload list)
            foreach ($line in $dotnetWorkloads) {
                if ($line.Length -gt 0 -and -not $line.Contains("dotnet workload search")) {
                    Write-Both $line -ForegroundColor DarkBlue
                }
            }
            Write-Both "``````"
        
            # Print the full .NET info in a collapsed section
            Write-Both "<details>"
            Write-Both "  <summary>Click Here For Full Information</summary>"
            Write-Both ""
            Write-Both "``````"
            $dotnetInfo = $(dotnet --info)
            foreach ($line in $dotnetInfo) {
                Write-Both $line -ForegroundColor DarkBlue
            }
            Write-Both "``````"
            Write-Both ""
            Write-Both "</details>"
            Write-Both ""
        
            # Read VS information
            if (Test-Path 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe') {
                Write-Both "**Visual Studio Information:**"
                Write-Both ""
                Write-Both "``````"
                [xml]$vsInfo = $(& 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe' -prerelease -format xml)
                foreach ($instanceItem in $vsinfo.instances) {
                    $instance = $instanceItem.instance
            
                    Write-Both "Name                                    Version"
                    Write-Both "-----------------------------------------------------------------------------------"
                    Write-Both "$($instance.displayName.PadRight(38))  $($instance.catalog.productDisplayVersion)"
                }
                Write-Both "``````"
            }
        } finally {
            Pop-Location
        }
        
        
        Write-Host "--------------------------------------------------------------------------------" -ForegroundColor Yellow
        Write-Host "Stop copying just above this line." -ForegroundColor Yellow
        Write-Host ""
    }

    Set-Alias cdnmi -Value Collect-DotNet-Maui-Info

    Export-ModuleMember -Function Collect-DotNet-Maui-Info -Alias cdnmi
}
