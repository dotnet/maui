#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'WindowsTestPackage.ps1'
    $ast = [Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$null, [ref]$null)
    foreach ($definition in $ast.FindAll({ $args[0] -is [Management.Automation.Language.FunctionDefinitionAst] }, $true)) {
        Invoke-Expression $definition.Extent.Text
    }
    function Get-AppxPackage { [CmdletBinding()]param([string]$Name) }
    function Add-AppxPackage { [CmdletBinding()]param([string]$Path, [string[]]$DependencyPath) }
    function Remove-AppxPackage { [CmdletBinding()]param([string]$Package) }

    function New-PackageState {
        param([string] $Path)
        $owner = [guid]::NewGuid().ToString('N')
        $name = "maui.tests.$owner"
        $state = @{
            OwnerId=$owner; IdentityName=$name; Publisher="CN=MauiTests-$owner"; CertificateThumbprint=('A' * 40)
            InstallRequested=$true; PackageFullName="${name}_1.0.0.0_x64__publisher"
            PackageFamilyName="${name}_publisher"; ApplicationId='App'; AppUserModelId="${name}_publisher!App"
            InstallLocation=$TestDrive; TimeoutSeconds=10; ProcessId=0; ProcessStartUtcTicks=0; ProcessExited=$false
        }
        Write-WindowsTestPackageState $Path $state
        return $state
    }
    function New-PackageArchive {
        param([string] $Directory, [string] $Name, [string] $Publisher = 'CN=Tests')
        New-Item -ItemType Directory -Force -Path $Directory | Out-Null
        $archive = Join-Path $Directory 'test.msix'
        $zip = [IO.Compression.ZipFile]::Open($archive, [IO.Compression.ZipArchiveMode]::Create)
        try {
            $entry = $zip.CreateEntry('AppxManifest.xml')
            $writer = [IO.StreamWriter]::new($entry.Open())
            try { $writer.Write("<Package><Identity Name='$Name' Publisher='$Publisher'/><Applications><Application Id='App'/></Applications></Package>") }
            finally { $writer.Dispose() }
        }
        finally { $zip.Dispose() }
        return $archive
    }
}

Describe 'Actual certificate ownership verification' {
    BeforeAll {
        function Remove-Item {
            [CmdletBinding()]
            param([string]$LiteralPath, [switch]$DeleteKey)
            throw 'Certificate removal must be mocked.'
        }
    }
    BeforeEach {
        $owner = [guid]::NewGuid().ToString('N')
        $publisher = "CN=MauiTests-$owner"
        $thumbprint = 'A' * 40
        Mock Test-Path { $true }
        Mock Get-Item {
            [pscustomobject]@{ Subject=$publisher; Thumbprint=$thumbprint; FriendlyName="MAUI test $owner" }
        }
        Mock Remove-Item {}
    }
    It 'rejects a syntactically valid foreign thumbprint without deleting a certificate or private key' {
        Mock Get-Item { [pscustomobject]@{ Subject='CN=Foreign user certificate'; Thumbprint=$thumbprint; FriendlyName='user key' } }
        { Remove-WindowsTestCertificate -Thumbprint $thumbprint -Publisher $publisher -OwnerId $owner } |
            Should -Throw '*does not belong to the recorded test session*'
        Should -Invoke Remove-Item -Times 0 -Exactly
    }
    It 'verifies both stores before removing either entry' {
        Mock Get-Item {
            if ($LiteralPath -like '*TrustedPeople*') {
                return [pscustomobject]@{ Subject='CN=Foreign'; Thumbprint=$thumbprint; FriendlyName='' }
            }
            return [pscustomobject]@{ Subject=$publisher; Thumbprint=$thumbprint; FriendlyName="MAUI test $owner" }
        }
        { Remove-WindowsTestCertificate -Thumbprint $thumbprint -Publisher $publisher -OwnerId $owner } |
            Should -Throw '*does not belong to the recorded test session*'
        Should -Invoke Remove-Item -Times 0 -Exactly
    }
    It 'removes only the verified session certificate, including its owned private key' {
        Remove-WindowsTestCertificate -Thumbprint $thumbprint -Publisher $publisher -OwnerId $owner
        Should -Invoke Remove-Item -Times 1 -Exactly -ParameterFilter {
            $LiteralPath -eq "Cert:\CurrentUser\My\$thumbprint" -and $DeleteKey
        }
        Should -Invoke Remove-Item -Times 1 -Exactly -ParameterFilter {
            $LiteralPath -eq "Cert:\CurrentUser\TrustedPeople\$thumbprint" -and !$DeleteKey
        }
    }
}

Describe 'Owned MSIX installation and activation cleanup' {
    BeforeEach {
        $path = Join-Path $TestDrive "$([guid]::NewGuid().ToString('N')).json"
        $state = New-PackageState $path
        $registered = [pscustomobject]@{
            Name=$state.IdentityName; Publisher=$state.Publisher
            PackageFullName=$state.PackageFullName; PackageFamilyName=$state.PackageFamilyName; InstallLocation=$TestDrive
        }
        Mock Get-AppxPackage { $registered }
        Mock Add-AppxPackage {}
        Mock Remove-AppxPackage {}
        Mock Remove-WindowsTestCertificate {}
        Mock Stop-Process {}
    }
    It 'loads Desktop-only Appx commands through Windows PowerShell when called from pwsh' {
        Mock Import-Module {}
        Import-WindowsTestAppxModule -Edition Core
        Should -Invoke Import-Module -Times 1 -ParameterFilter {
            $Name -eq 'Appx' -and $UseWindowsPowerShell -and $Global
        }
    }
    It 'does not request a compatibility session from Windows PowerShell Desktop' {
        Mock Import-Module {}
        Import-WindowsTestAppxModule -Edition Desktop
        Should -Invoke Import-Module -Times 1 -ParameterFilter {
            $Name -eq 'Appx' -and !$UseWindowsPowerShell -and $Global
        }
    }
    It 'installs the exact owned MSIX and explicit x64 SDK dependencies' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $null = New-PackageArchive $directory $state.IdentityName $state.Publisher
        $dependencies = Join-Path $directory 'Dependencies/x64'
        New-Item -ItemType Directory -Path $dependencies -Force | Out-Null
        'dependency-fixture' | Set-Content (Join-Path $dependencies 'runtime.msix')
        $result = Install-WindowsTestPackage -Path $path -Directory $directory
        $result.AppUserModelId | Should -Be "$($state.PackageFamilyName)!App"
        Should -Invoke Add-AppxPackage -Times 1 -ParameterFilter {
            $Path -eq (Join-Path $directory 'test.msix') -and $DependencyPath.Count -eq 1
        }
    }
    It 'propagates Add-AppxPackage failures' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $null = New-PackageArchive $directory $state.IdentityName $state.Publisher
        Mock Add-AppxPackage { throw 'installation failed' }
        { Install-WindowsTestPackage -Path $path -Directory $directory } | Should -Throw '*installation failed*'
        (Get-Content $path -Raw | ConvertFrom-Json).InstallRequested | Should -BeTrue
    }
    It 'rejects a package artifact belonging to another session' {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $null = New-PackageArchive $directory 'someone.elses.package'
        { Install-WindowsTestPackage -Path $path -Directory $directory } | Should -Throw '*does not match*'
        Should -Invoke Add-AppxPackage -Times 0
    }
    It 'refuses to replace an already installed package before this session requested installation' {
        $state.InstallRequested = $false
        Write-WindowsTestPackageState $path $state
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $null = New-PackageArchive $directory $state.IdentityName $state.Publisher
        { Install-WindowsTestPackage -Path $path -Directory $directory } | Should -Throw '*already installed*'
        Should -Invoke Add-AppxPackage -Times 0
    }
    It 'uninstalls only its exact main package and never shared SDK dependencies' {
        Remove-WindowsTestPackage $path
        Should -Invoke Get-AppxPackage -Times 1 -ParameterFilter { $Name -eq $state.IdentityName }
        Should -Invoke Remove-AppxPackage -Times 1 -ParameterFilter { $Package -eq $state.PackageFullName }
        Should -Invoke Remove-WindowsTestCertificate -Times 1 -ParameterFilter { $Thumbprint -eq $state.CertificateThumbprint }
    }
    It 'does not uninstall a replaced registration with another package identity' {
        $registered.PackageFullName = 'another-package'
        { Remove-WindowsTestPackage $path } | Should -Throw '*unowned or replaced*'
        Should -Invoke Remove-AppxPackage -Times 0
    }
    It 'leaves a same-name package from another signing publisher untouched' {
        $registered.Publisher = 'CN=another-owner'
        Remove-WindowsTestPackage $path
        Should -Invoke Remove-AppxPackage -Times 0
    }
    It 'keeps private signing material non-exportable and all trust scoped to the current user' {
        $content = Get-Content $scriptPath -Raw
        $content | Should -Match 'CertStoreLocation Cert:\\CurrentUser\\My'
        $content | Should -Match 'KeyExportPolicy NonExportable'
        $content | Should -Match "'TrustedPeople', 'CurrentUser'"
        $content | Should -Not -Match 'LocalMachine|\.pfx|Export-PfxCertificate|ConvertTo-SecureString'
        $content | Should -Match '\$Certificate.RawData'
    }
    It 'rejects forged ownership before querying or touching the machine' {
        $state.Publisher = 'CN=not-this-job'
        Write-WindowsTestPackageState $path $state
        { Remove-WindowsTestPackage $path } | Should -Throw '*unowned*'
        Should -Invoke Get-AppxPackage -Times 0
        Should -Invoke Stop-Process -Times 0
    }
    It 'terminates only the recorded activated PID after checking start time and package identity' {
        $time = [DateTime]::UtcNow
        $state.ProcessId = 12345
        $state.ProcessStartUtcTicks = $time.Ticks
        Write-WindowsTestPackageState $path $state
        $process = [pscustomobject]@{ Id=12345; StartTime=$time; HasExited=$false }
        $process | Add-Member ScriptMethod WaitForExit { param($Milliseconds) return $true }
        Mock Get-Process { $process }
        Mock Get-WindowsTestProcessPackage { $state.PackageFullName }
        Remove-WindowsTestPackage $path
        Should -Invoke Stop-Process -Times 1 -ParameterFilter { $Id -eq 12345 -and $Force }
        Should -Invoke Get-WindowsTestProcessPackage -Times 1 -ParameterFilter { $ProcessId -eq 12345 }
    }
    It 'rejects a foreign process even when persisted package identity and start ticks match it' {
        $time = [DateTime]::UtcNow
        $state.ProcessId = 12345
        $state.ProcessStartUtcTicks = $time.Ticks
        $state.PackageFullName = 'foreign.application_1.0.0.0_x64__publisher'
        Write-WindowsTestPackageState $path $state
        Mock Get-Process { [pscustomobject]@{ StartTime=$time; HasExited=$false } }
        Mock Get-WindowsTestProcessPackage { $state.PackageFullName }
        { Remove-WindowsTestPackage $path } | Should -Throw '*process from another package*'
        Should -Invoke Stop-Process -Times 0 -Exactly
    }

    It 'never terminates a reused PID' {
        $state.ProcessId = 12345
        $state.ProcessStartUtcTicks = [DateTime]::UtcNow.AddMinutes(-10).Ticks
        Write-WindowsTestPackageState $path $state
        Mock Get-Process { [pscustomobject]@{ Id=12345; StartTime=[DateTime]::UtcNow; HasExited=$false } }
        { Remove-WindowsTestPackage $path } | Should -Throw '*reused PID*'
        Should -Invoke Stop-Process -Times 0
    }
    It 'never terminates a PID from another installed package' {
        $time = [DateTime]::UtcNow
        $state.ProcessId = 12345; $state.ProcessStartUtcTicks = $time.Ticks
        Write-WindowsTestPackageState $path $state
        Mock Get-Process { [pscustomobject]@{ Id=12345; StartTime=$time; HasExited=$false } }
        Mock Get-WindowsTestProcessPackage { 'another-package' }
        { Remove-WindowsTestPackage $path } | Should -Throw '*another package*'
        Should -Invoke Stop-Process -Times 0
    }
    It 'does not touch a PID recorded as already exited by the launcher' {
        $state.ProcessId = 12345; $state.ProcessExited = $true
        Write-WindowsTestPackageState $path $state
        Remove-WindowsTestPackage $path
        Should -Invoke Stop-Process -Times 0
    }
    It 'preserves certificate ownership state when user trust setup fails' {
        $project = Join-Path $TestDrive "$([guid]::NewGuid().ToString('N'))/Test.csproj"
        $manifestDirectory = Join-Path (Split-Path $project -Parent) 'Platforms/Windows'
        New-Item -ItemType Directory -Path $manifestDirectory -Force | Out-Null
        '<Package><Identity Publisher="CN=Tests"/></Package>' | Set-Content (Join-Path $manifestDirectory 'Package.appxmanifest')
        '<Project><PropertyGroup><ApplicationId>com.microsoft.maui.tests</ApplicationId></PropertyGroup></Project>' | Set-Content $project
        $newPath = Join-Path $TestDrive "$([guid]::NewGuid().ToString('N')).json"
        Mock New-WindowsTestCertificate { [pscustomobject]@{ Thumbprint=('B' * 40) } }
        Mock Trust-WindowsTestCertificate { throw 'trust permission denied' }
        { Initialize-WindowsTestPackage -Path $newPath -Project $project -TimeoutSeconds 10 } | Should -Throw '*trust permission*'
        (Get-Content $newPath -Raw | ConvertFrom-Json).CertificateThumbprint | Should -Be ('B' * 40)
    }
}
