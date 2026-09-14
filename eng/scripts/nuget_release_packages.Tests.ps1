#Requires -Modules Pester

Describe 'nuget_release_packages.ps1' {
  BeforeEach {
    $scriptPath = Join-Path $PSScriptRoot 'nuget_release_packages.ps1'
    $packagesPath = Join-Path $TestDrive ([guid]::NewGuid().ToString())
    New-Item -ItemType Directory -Path $packagesPath | Out-Null
  }

  Describe 'retained MAUI manifest identity validation' {
    BeforeAll {
      function Set-TestNuspec([string] $Path, [string] $Content, [string] $Name = 'manifest.nuspec') {
        $archive = [System.IO.Compression.ZipFile]::Open($Path, [System.IO.Compression.ZipArchiveMode]::Update)
        try {
          $entry = $archive.GetEntry($Name)
          if ($entry) {
            $entry.Delete()
          }
          $writer = [System.IO.StreamWriter]::new($archive.CreateEntry($Name).Open())
          try {
            $writer.Write($Content)
          }
          finally {
            $writer.Dispose()
          }
        }
        finally {
          $archive.Dispose()
        }
      }
    }

    BeforeEach {
      $identity = @{ id = 'Microsoft.NET.Sdk.Maui.Manifest-10.0.100'; version = '10.0.101'; normalizedVersion = '10.0.101'; fileName = 'Microsoft.NET.Sdk.Maui.Manifest-10.0.100.10.0.101.nupkg' }
      ConvertTo-Json -InputObject @($identity) | Set-Content (Join-Path $packagesPath 'expected-packages.json')
      $packagePath = Join-Path $packagesPath $identity.fileName
      Set-TestNuspec $packagePath "<package xmlns='http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd'><metadata><id>$($identity.id)</id><version>$($identity.version)</version></metadata></package>"
      Mock Invoke-WebRequest { throw 'Identity validation must not contact NuGet.org' }
    }

    It 'accepts the MAUI manifest variant <Suffix>' -ForEach @(
      @{ Suffix = '' }, @{ Suffix = '.Msi.arm64' }, @{ Suffix = '.Msi.x64' }, @{ Suffix = '.Msi.x86' }
    ) {
      $identity.id += $Suffix
      $identity.fileName = "$($identity.id).$($identity.version).nupkg"
      Set-TestNuspec $packagePath "<package><metadata><id>$($identity.id)</id><version>$($identity.version)</version></metadata></package>"
      if ($Suffix) {
        Move-Item $packagePath (Join-Path $packagesPath $identity.fileName)
      }
      ConvertTo-Json -InputObject @($identity) | Set-Content (Join-Path $packagesPath 'expected-packages.json')
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Not -Throw
    }

    It 'validates a manifest without modifying it or querying NuGet.org' {
      $hash = (Get-FileHash $packagePath).Hash
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Not -Throw
      (Get-FileHash $packagePath).Hash | Should -Be $hash
      Should -Invoke Invoke-WebRequest -Times 0
    }

    It 'rejects a workload pack even when it appears in the expected inventory' {
      $identity.id = 'Microsoft.Maui.Sdk'
      $identity.fileName = 'Microsoft.Maui.Sdk.10.0.101.nupkg'
      ConvertTo-Json -InputObject @($identity) | Set-Content (Join-Path $packagesPath 'expected-packages.json')
      Move-Item $packagePath (Join-Path $packagesPath $identity.fileName)
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw '*not an expected MAUI workload manifest*'
    }

    It 'rejects a renamed package whose nuspec does not match the inventory' {
      $identity.id += '.Msi.x64'
      $identity.fileName = "$($identity.id).10.0.101.nupkg"
      ConvertTo-Json -InputObject @($identity) | Set-Content (Join-Path $packagesPath 'expected-packages.json')
      Move-Item $packagePath (Join-Path $packagesPath $identity.fileName)
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw '*does not match its expected ID and version*'
    }

    It 'rejects a package version that differs from its nuspec' {
      $identity.version = '10.0.102'
      ConvertTo-Json -InputObject @($identity) | Set-Content (Join-Path $packagesPath 'expected-packages.json')
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw '*does not match its expected ID and version*'
    }

    It 'rejects a missing manifest instead of silently treating it as published' {
      Remove-Item $packagePath
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw
    }

    It 'rejects duplicate inventory entries' {
      ConvertTo-Json -InputObject @($identity, $identity) | Set-Content (Join-Path $packagesPath 'expected-packages.json')
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw '*duplicate file name*'
    }

    It 'rejects multiple root nuspecs' {
      Set-TestNuspec $packagePath '<package />' 'second.nuspec'
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw '*one root nuspec*'
    }

    It 'rejects malformed nuspec XML and prohibited DTDs' -ForEach @(
      @{ Xml = '<package>' }
      @{ Xml = '<!DOCTYPE package [<!ENTITY id "unexpected">]><package>&id;</package>' }
    ) {
      Set-TestNuspec $packagePath $Xml
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } | Should -Throw
    }

    It 'rejects ambiguous nuspec identity metadata <Name>' -ForEach @(
      @{ Name = 'id' }, @{ Name = 'version' }
    ) {
      Set-TestNuspec $packagePath "<package><metadata><id>$($identity.id)</id><version>$($identity.version)</version><$Name>unexpected</$Name></metadata></package>"
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath } |
        Should -Throw '*does not match its expected ID and version*'
    }

    It 'compares package contents against the approved recovery audit' {
      $auditPath = Join-Path $packagesPath 'recovery-audit.json'
      @{ manifests = @(@{ id = $identity.id; version = $identity.version; fileName = $identity.fileName; sha256 = (Get-FileHash $packagePath).Hash }) } |
        ConvertTo-Json -Depth 4 | Set-Content $auditPath
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath -RecoveryAuditPath $auditPath } | Should -Not -Throw

      Add-Content $packagePath 'changed bytes'
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath -RecoveryAuditPath $auditPath } |
        Should -Throw '*does not match the approved recovery audit*'
    }

    It 'rejects a changed manifest selection after approval' {
      $auditPath = Join-Path $packagesPath 'recovery-audit.json'
      '{"manifests":[]}' | Set-Content $auditPath
      { & $scriptPath -Action ValidateManifests -PackagesPath $packagesPath -RecoveryAuditPath $auditPath } |
        Should -Throw '*do not match the approved recovery audit*'
    }
  }

  It 'removes published packages and keeps unpublished packages' {
    @(
      [pscustomobject]@{ id = 'Existing.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Existing.Package.1.0.0.nupkg' }
      [pscustomobject]@{ id = 'New.Package'; version = '2.0.0'; normalizedVersion = '2.0.0'; fileName = 'New.Package.2.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'Existing.Package.1.0.0.nupkg') ''
    Set-Content (Join-Path $packagesPath 'New.Package.2.0.0.nupkg') ''

    Mock Invoke-WebRequest {
      [pscustomobject]@{ StatusCode = if ($Uri -like '*existing.package*') { 200 } else { 404 } }
    }

    & $scriptPath -Action FilterExisting -PackagesPath $packagesPath

    Test-Path (Join-Path $packagesPath 'Existing.Package.1.0.0.nupkg') | Should -BeFalse
    Test-Path (Join-Path $packagesPath 'New.Package.2.0.0.nupkg') | Should -BeTrue
  }

  It 'skips publishing when every package already exists' {
    @(
      [pscustomobject]@{ id = 'Existing.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Existing.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'Existing.Package.1.0.0.nupkg') ''
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 200 } }

    $output = & $scriptPath -Action FilterExisting -PackagesPath $packagesPath 6>&1

    $output -join "`n" | Should -Match 'NuGetPackagesToPublish]false'
  }

  It 'accepts an already-published package removed during preparation' {
    @(
      [pscustomobject]@{ id = 'Existing.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Existing.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 200 } }

    $output = & $scriptPath -Action FilterExisting -PackagesPath $packagesPath 6>&1

    $output -join "`n" | Should -Match 'NuGetPackagesToPublish]false'
  }

  It 'rejects an unpublished package missing from the artifact' {
    @(
      [pscustomobject]@{ id = 'Missing.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Missing.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 404 } }

    { & $scriptPath -Action FilterExisting -PackagesPath $packagesPath } |
      Should -Throw '*Unpublished package*was not found*'
  }

  It 'fails closed on an unexpected NuGet response' {
    @(
      [pscustomobject]@{ id = 'Unknown.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Unknown.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'Unknown.Package.1.0.0.nupkg') ''
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 503 } }

    { & $scriptPath -Action FilterExisting -PackagesPath $packagesPath -StatusQueryAttempts 1 -StatusQueryDelaySeconds 0 } |
      Should -Throw '*HTTP 503*'
  }

  It 'verifies published packages' {
    @(
      [pscustomobject]@{ id = 'Existing.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Existing.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 200 } }

    { & $scriptPath -Action Verify -PackagesPath $packagesPath -MaxAttempts 1 -DelaySeconds 0 } | Should -Not -Throw
  }

  It 'reports packages that are still missing' {
    @(
      [pscustomobject]@{ id = 'Missing.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Missing.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 404 } }

    { & $scriptPath -Action Verify -PackagesPath $packagesPath -MaxAttempts 1 -DelaySeconds 0 } |
      Should -Throw '*Missing.Package 1.0.0*'
  }

  It 'removes a previously attempted package before it is indexed' {
    @(
      [pscustomobject]@{ id = 'Attempted.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Attempted.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'Attempted.Package.1.0.0.nupkg') ''
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 404 } }

    & $scriptPath -Action FilterExisting -PackagesPath $packagesPath -SkipFilters 'Attempted.*'

    Test-Path (Join-Path $packagesPath 'Attempted.Package.1.0.0.nupkg') | Should -BeFalse
    Should -Invoke Invoke-WebRequest -Times 0
  }

  It 'retries a transient package status response' {
    @(
      [pscustomobject]@{ id = 'New.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'New.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'New.Package.1.0.0.nupkg') ''
    $script:requestCount = 0
    Mock Invoke-WebRequest {
      $script:requestCount++
      [pscustomobject]@{ StatusCode = if ($script:requestCount -eq 1) { 503 } else { 404 } }
    }

    { & $scriptPath -Action FilterExisting -PackagesPath $packagesPath -StatusQueryAttempts 2 -StatusQueryDelaySeconds 0 } |
      Should -Not -Throw
    Should -Invoke Invoke-WebRequest -Times 2 -Exactly
  }

  It 'retries a transient package status query failure' {
    @(
      [pscustomobject]@{ id = 'New.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'New.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'New.Package.1.0.0.nupkg') ''
    $script:requestCount = 0
    Mock Invoke-WebRequest {
      $script:requestCount++
      if ($script:requestCount -eq 1) {
        throw 'temporary network failure'
      }
      [pscustomobject]@{ StatusCode = 404 }
    }

    { & $scriptPath -Action FilterExisting -PackagesPath $packagesPath -StatusQueryAttempts 2 -StatusQueryDelaySeconds 0 } |
      Should -Not -Throw
    Should -Invoke Invoke-WebRequest -Times 2 -Exactly
  }

  It 'verifies a package that becomes available on a later attempt' {
    @(
      [pscustomobject]@{ id = 'Delayed.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Delayed.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    $script:requestCount = 0
    Mock Invoke-WebRequest {
      $script:requestCount++
      [pscustomobject]@{ StatusCode = if ($script:requestCount -eq 1) { 404 } else { 200 } }
    }

    { & $scriptPath -Action Verify -PackagesPath $packagesPath -MaxAttempts 2 -DelaySeconds 0 -StatusQueryAttempts 1 } |
      Should -Not -Throw
    Should -Invoke Invoke-WebRequest -Times 2 -Exactly
  }

  It 'uses the normalized package version in the NuGet.org URL' {
    @(
      [pscustomobject]@{ id = 'Metadata.Package'; version = '1.0.0+build.1'; normalizedVersion = '1.0.0'; fileName = 'Metadata.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Mock Invoke-WebRequest { [pscustomobject]@{ StatusCode = 200 } }

    & $scriptPath -Action Verify -PackagesPath $packagesPath

    Should -Invoke Invoke-WebRequest -Times 1 -Exactly -ParameterFilter {
      $Uri -like '*/metadata.package/1.0.0/metadata.package.1.0.0.nupkg' -and $Uri -notlike '*build.1*'
    }
  }

  It 'rejects package file names containing a directory' {
    @(
      [pscustomobject]@{ id = 'Unsafe.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = '../Unsafe.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')

    { & $scriptPath -Action Verify -PackagesPath $packagesPath } | Should -Throw '*must not contain a directory*'
  }

  It 'rejects Windows package paths on any host platform' {
    @(
      [pscustomobject]@{ id = 'Unsafe.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = '..\Unsafe.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')

    { & $scriptPath -Action Verify -PackagesPath $packagesPath } | Should -Throw '*must not contain a directory*'
  }

  It 'rejects a package without a normalized version' {
    @(
      [pscustomobject]@{ id = 'Invalid.Package'; version = '1.0.0'; fileName = 'Invalid.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')

    { & $scriptPath -Action Verify -PackagesPath $packagesPath } | Should -Throw "*without 'normalizedVersion'*"
  }

  It 'rejects package files absent from the manifest' {
    @(
      [pscustomobject]@{ id = 'Expected.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Expected.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'Unexpected.Package.1.0.0.nupkg') ''

    { & $scriptPath -Action Verify -PackagesPath $packagesPath } | Should -Throw '*absent from the manifest*'
  }

  It 'rejects a previously attempted filter that matches no package' {
    @(
      [pscustomobject]@{ id = 'Expected.Package'; version = '1.0.0'; normalizedVersion = '1.0.0'; fileName = 'Expected.Package.1.0.0.nupkg' }
    ) | ConvertTo-Json | Set-Content (Join-Path $packagesPath 'expected-packages.json')
    Set-Content (Join-Path $packagesPath 'Expected.Package.1.0.0.nupkg') ''

    { & $scriptPath -Action FilterExisting -PackagesPath $packagesPath -SkipFilters 'Typo.*' } |
      Should -Throw '*matched no expected packages*'
  }
}
