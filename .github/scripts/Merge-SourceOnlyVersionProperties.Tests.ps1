#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . "$PSScriptRoot/Merge-SourceOnlyVersionProperties.ps1"

    function Write-TestXml {
        param(
            [Parameter(Mandatory = $true)]
            [string]$Path,
            [Parameter(Mandatory = $true)]
            [string]$Content
        )

        [System.IO.File]::WriteAllText(
            $Path,
            $Content.TrimStart("`r", "`n") + "`n",
            [System.Text.UTF8Encoding]::new($false))
    }
}

Describe 'Merge-SourceOnlyVersionProperty' {
    BeforeEach {
        $script:TestRoot = Join-Path ([System.IO.Path]::GetTempPath()) "maui-version-merge-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:TestRoot | Out-Null
        $script:SourcePath = Join-Path $script:TestRoot 'source.props'
        $script:TargetPath = Join-Path $script:TestRoot 'target.props'
    }

    AfterEach {
        Remove-Item -LiteralPath $script:TestRoot -Recurse -Force
    }

    It 'adds source-only properties while preserving release values' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <MicrosoftNETSdkPackageVersion>11.0.100-rc.1</MicrosoftNETSdkPackageVersion>
    <MicrosoftWindowsCsWin32PackageVersion>0.3.298</MicrosoftWindowsCsWin32PackageVersion>
    <MicrosoftAspNetCoreIdentityEntityFrameworkCorePackageVersion>11.0.0-rc.1</MicrosoftAspNetCoreIdentityEntityFrameworkCorePackageVersion>
    <MicrosoftEntityFrameworkCoreSqlitePackageVersion>11.0.0-rc.1</MicrosoftEntityFrameworkCoreSqlitePackageVersion>
    <MicrosoftAspNetCorePackageVersion>10.0.2</MicrosoftAspNetCorePackageVersion>
  </PropertyGroup>
  <PropertyGroup>
    <CommunityToolkitMvvmPackageVersion>8.3.2</CommunityToolkitMvvmPackageVersion>
    <AvaloniaControlsMauiPackageVersion>11.0.0-preview.6</AvaloniaControlsMauiPackageVersion>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <MicrosoftNETSdkPackageVersion>11.0.100-preview.7</MicrosoftNETSdkPackageVersion>
    <MicrosoftAspNetCorePackageVersion>10.0.2</MicrosoftAspNetCorePackageVersion>
  </PropertyGroup>
  <PropertyGroup>
    <CommunityToolkitMvvmPackageVersion>8.3.2</CommunityToolkitMvvmPackageVersion>
  </PropertyGroup>
</Project>
'@

        $result = Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        $updated = Get-Content -LiteralPath $script:TargetPath -Raw

        $result.Changed | Should -BeTrue
        $result.AddedProperties | Should -Be @(
            'MicrosoftWindowsCsWin32PackageVersion'
            'MicrosoftAspNetCoreIdentityEntityFrameworkCorePackageVersion'
            'MicrosoftEntityFrameworkCoreSqlitePackageVersion'
            'AvaloniaControlsMauiPackageVersion'
        )
        $updated | Should -Match '<MicrosoftNETSdkPackageVersion>11\.0\.100-preview\.7</MicrosoftNETSdkPackageVersion>'
        $updated | Should -Match '<MicrosoftWindowsCsWin32PackageVersion>0\.3\.298</MicrosoftWindowsCsWin32PackageVersion>'
        $updated | Should -Match '<MicrosoftAspNetCoreIdentityEntityFrameworkCorePackageVersion>11\.0\.0-rc\.1</MicrosoftAspNetCoreIdentityEntityFrameworkCorePackageVersion>'
        $updated | Should -Match '<MicrosoftEntityFrameworkCoreSqlitePackageVersion>11\.0\.0-rc\.1</MicrosoftEntityFrameworkCoreSqlitePackageVersion>'
        $updated | Should -Match '<AvaloniaControlsMauiPackageVersion>11\.0\.0-preview\.6</AvaloniaControlsMauiPackageVersion>'
    }

    It 'places a source-only property after a retained category comment' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <SamsungTizenSdkPackageVersion>8.0.148</SamsungTizenSdkPackageVersion>
    <!-- wasdk -->
    <MicrosoftWindowsCsWin32PackageVersion>0.3.298</MicrosoftWindowsCsWin32PackageVersion>
    <MicrosoftWindowsAppSDKPackageVersion>2.3.1</MicrosoftWindowsAppSDKPackageVersion>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <SamsungTizenSdkPackageVersion>8.0.148</SamsungTizenSdkPackageVersion>
    <!-- wasdk -->
    <MicrosoftWindowsAppSDKPackageVersion>1.8.0</MicrosoftWindowsAppSDKPackageVersion>
  </PropertyGroup>
</Project>
'@

        [void](Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath)
        $updated = Get-Content -LiteralPath $script:TargetPath -Raw

        $updated | Should -Match '(?s)<!-- wasdk -->\r?\n    <MicrosoftWindowsCsWin32PackageVersion>.*?</MicrosoftWindowsCsWin32PackageVersion>\r?\n    <MicrosoftWindowsAppSDKPackageVersion>'
    }

    It 'is idempotent' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <Existing>source</Existing>
    <NewProperty>new</NewProperty>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <Existing>target</Existing>
  </PropertyGroup>
</Project>
'@

        $first = Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        $afterFirst = Get-Content -LiteralPath $script:TargetPath -Raw
        $second = Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        $afterSecond = Get-Content -LiteralPath $script:TargetPath -Raw

        $first.Changed | Should -BeTrue
        $second.Changed | Should -BeFalse
        $afterSecond | Should -BeExactly $afterFirst
    }

    It 'does not overwrite a same-name target property' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <PinnedVersion>source</PinnedVersion>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <PinnedVersion>release</PinnedVersion>
  </PropertyGroup>
</Project>
'@

        $result = Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath

        $result.Changed | Should -BeFalse
        (Get-Content -LiteralPath $script:TargetPath -Raw) | Should -Match '<PinnedVersion>release</PinnedVersion>'
    }

    It 'fails closed when a source-only property is declared more than once' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <DuplicateVersion Condition="'$(A)' == 'true'">one</DuplicateVersion>
    <DuplicateVersion Condition="'$(A)' != 'true'">two</DuplicateVersion>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <Existing>value</Existing>
  </PropertyGroup>
</Project>
'@

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw "*DuplicateVersion*declared more than once*"
    }

    It 'fails closed for a multi-line source-only property' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <ComplexVersion>
      value
    </ComplexVersion>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <Existing>value</Existing>
  </PropertyGroup>
</Project>
'@

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw "*ComplexVersion*single-line element*"
    }

    It 'fails closed when a placement anchor is multi-line in the source' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <Existing>
      source
    </Existing>
    <NewProperty>new</NewProperty>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <Existing>release</Existing>
  </PropertyGroup>
</Project>
'@

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw "*previous placement anchor 'existing'*not a single-line element*"
    }

    It 'fails closed for malformed XML' {
        Write-TestXml -Path $script:SourcePath -Content '<Project><PropertyGroup><Broken></PropertyGroup></Project>'
        Write-TestXml -Path $script:TargetPath -Content '<Project><PropertyGroup /></Project>'

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw '*not valid safe XML*'
    }

    It 'rejects XML with a document type declaration' {
        Write-TestXml -Path $script:SourcePath -Content @'
<!DOCTYPE Project [<!ENTITY value "unsafe">]>
<Project>
  <PropertyGroup>
    <UnsafeVersion>&value;</UnsafeVersion>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content '<Project><PropertyGroup /></Project>'

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw '*not valid safe XML*'
    }

    It 'fails closed when the target lacks the matching property group' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <First>one</First>
  </PropertyGroup>
  <PropertyGroup>
    <Second>two</Second>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <First>one</First>
  </PropertyGroup>
</Project>
'@

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw '*no PropertyGroup matching source group 1*'
    }

    It 'fails closed when the corresponding target group shares no property' {
        Write-TestXml -Path $script:SourcePath -Content @'
<Project>
  <PropertyGroup>
    <Shared>one</Shared>
    <SourceOnly>two</SourceOnly>
  </PropertyGroup>
</Project>
'@
        Write-TestXml -Path $script:TargetPath -Content @'
<Project>
  <PropertyGroup>
    <Different>zzz</Different>
  </PropertyGroup>
</Project>
'@

        {
            Merge-SourceOnlyVersionProperty -SourcePath $script:SourcePath -TargetPath $script:TargetPath
        } | Should -Throw '*shares no property with the corresponding source group*'
    }
}
