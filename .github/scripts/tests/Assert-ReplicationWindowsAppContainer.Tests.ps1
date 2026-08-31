#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-ReplicationWindowsAppContainer.ps1')
    $script:BridgePath = Join-Path $PSScriptRoot (
        '../shared/Invoke-ReplicationWindowsAppx.ps1')
    $script:SandboxManifest = Join-Path $PSScriptRoot (
        '../../../src/Controls/samples/Controls.Sample.Sandbox/Platforms/Windows/ReplicationAppContainerManifest.xml')
    $script:DeviceManifest = Join-Path $PSScriptRoot (
        '../../../src/Controls/tests/DeviceTests/Platforms/Windows/ReplicationAppContainerManifest.xml')
    $script:OverrideTargets = Join-Path $PSScriptRoot (
        '../shared/ReplicationWindowsAppContainerManifest.targets')
    $script:RegistrationPolicy = Join-Path $PSScriptRoot (
        '../shared/ReplicationWindowsWinUiRegistrations.json')
    $script:GeneratedManifestFixtures = @(
        Join-Path $PSScriptRoot (
            'fixtures/WindowsGeneratedSandboxAppxManifest.xml')
        Join-Path $PSScriptRoot (
            'fixtures/WindowsGeneratedDeviceTestsAppxManifest.xml')
    )
    $script:GeneratorProbeProject = Join-Path $PSScriptRoot (
        'fixtures/GenerateWindowsRegistrationManifest.proj')
    $script:GitAttributes = Join-Path $PSScriptRoot '../../../.gitattributes'
    $packagesRoot = if ([string]::IsNullOrWhiteSpace($env:NUGET_PACKAGES)) {
        Join-Path ([Environment]::GetFolderPath(
            [Environment+SpecialFolder]::UserProfile)) '.nuget/packages'
    } else {
        $env:NUGET_PACKAGES
    }
    $script:RealRegistrationPath = Join-Path $packagesRoot (
        'microsoft.windowsappsdk.winui/1.8.260528001/build/native/' +
        'LiftedWinRTClassRegistrations.xml')
    $script:RealWindowsAppSdkNuspecPath = Join-Path $packagesRoot (
        'microsoft.windowsappsdk/1.8.260529003/' +
        'microsoft.windowsappsdk.nuspec')
    $script:RealWin2DMetadataPath = Join-Path $packagesRoot (
        'microsoft.graphics.win2d/1.4.0/lib/uap10.0/' +
        'Microsoft.Graphics.Canvas.winmd')
    $script:RealWebView2MetadataPath = Join-Path $packagesRoot (
        'microsoft.web.webview2/1.0.3179.45/lib/' +
        'Microsoft.Web.WebView2.Core.winmd')
    $script:RealGeneratorTaskPath = Join-Path $packagesRoot (
        'microsoft.windows.sdk.buildtools.msix/1.7.20250829.1/' +
        'tools/net6.0/Microsoft.Windows.SDK.BuildTools.MSIX.dll')
}

Describe 'Windows AppX compatibility bridge' {
    It 'keeps AppX cmdlets inside a fixed Windows PowerShell 5.1 script' {
        $bridge = Get-Content -LiteralPath $script:BridgePath -Raw
        $parent = Get-Content -LiteralPath (Join-Path $PSScriptRoot (
            '../shared/Assert-ReplicationWindowsAppContainer.ps1')) -Raw
        $sandbox = Get-Content -LiteralPath (Join-Path $PSScriptRoot (
            '../BuildAndRunSandbox.ps1')) -Raw

        $bridge | Should -Match '^#!/usr/bin/env powershell\.exe'
        $bridge | Should -Match (
            "\[ValidateSet\('Query', 'Install', 'Remove'\)\]")
        $bridge | Should -Match 'Get-AppxPackage'
        $bridge | Should -Match 'Get-AppxPackageManifest'
        $bridge | Should -Match 'Add-AppxPackage'
        $bridge | Should -Match 'Remove-AppxPackage'
        $parent | Should -Not -Match (
            '(?m)^\s*(?:Get|Add|Remove)-AppxPackage')
        $sandbox | Should -Not -Match (
            '(?m)^\s*(?:Get|Add|Remove)-AppxPackage')
        $parent | Should -Match (
            'System32\\WindowsPowerShell\\v1\.0\\powershell\.exe')
        $parent | Should -Match '\$startInfo\.Environment\.Clear\(\)'
    }
}

Describe 'Windows replication AppContainer manifests' {
    It 'accepts only the dedicated capability-free manifests' {
        foreach ($path in @($script:SandboxManifest, $script:DeviceManifest)) {
            $result = Assert-ReplicationWindowsAppContainerManifest -Path $path
            $result.Publisher | Should -BeExactly 'CN=DotNetMauiReplication'
            $result.EntryPoint |
                Should -BeExactly 'Windows.PartialTrustApplication'
            $result.TrustLevel | Should -BeExactly 'appContainer'
            $result.RuntimeBehavior | Should -BeExactly 'packagedClassicApp'
        }
    }

    It 'rejects every network, full-trust, and device capability' {
        $source = Get-Content -LiteralPath $script:SandboxManifest -Raw
        foreach ($capability in @(
            'runFullTrust',
            'internetClient',
            'internetClientServer',
            'privateNetworkClientServer',
            'location'
        )) {
            $mutated = $source.Replace(
                '</Package>',
                "<Capabilities><Capability Name=`"$capability`" /></Capabilities></Package>")
            $document = Read-ReplicationWindowsManifestXml `
                -Content $mutated `
                -Description 'mutated manifest'
            {
                Assert-ReplicationWindowsAppContainerManifestDocument `
                    -Document $document `
                    -Description 'mutated manifest'
            } | Should -Throw '*must not declare package capabilities*'
        }
    }

    It 'rejects full-trust runtime behavior and URI or service extensions' {
        $source = Get-Content -LiteralPath $script:SandboxManifest -Raw
        $fullTrust = $source.Replace(
            'uap10:TrustLevel="appContainer"',
            'uap10:TrustLevel="mediumIL"')
        {
            $document = Read-ReplicationWindowsManifestXml `
                -Content $fullTrust `
                -Description 'full trust manifest'
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'full trust manifest'
        } | Should -Throw (
            "*Actual TrustLevel='mediumIL', " +
            "RuntimeBehavior='packagedClassicApp'; Application attributes:*" +
            'TrustLevel{http://schemas.microsoft.com/appx/manifest/uap/windows10/10}=mediumIL*')

        $extension = $source.Replace(
            '</Application>',
            '<Extensions><uap:Extension Category="windows.protocol" /></Extensions></Application>')
        {
            $document = Read-ReplicationWindowsManifestXml `
                -Content $extension `
                -Description 'extension manifest'
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'extension manifest'
        } | Should -Throw '*must not declare application or package extensions*'
    }

    It 'admits only the exact attested registration profile for each package' {
        $policyItem = Get-Item -LiteralPath $script:RegistrationPolicy -Force
        $policyItem.PSIsContainer | Should -BeFalse
        ($policyItem.Attributes -band [IO.FileAttributes]::ReparsePoint) |
            Should -BeFalse
        $policyItem.Length | Should -BeLessOrEqual 16KB
        $policy = Get-ReplicationWindowsWinUiRegistrationPolicy
        $policy.profiles.Count | Should -BeExactly 2
        $attributes = Get-Content -LiteralPath $script:GitAttributes -Raw
        foreach ($name in @(
            'WindowsGeneratedSandboxAppxManifest.xml',
            'WindowsGeneratedDeviceTestsAppxManifest.xml'
        )) {
            $attributes | Should -Match (
                [regex]::Escape(
                    ".github/scripts/tests/fixtures/$name text eol=lf"))
        }

        $previousPackagesRoot = $env:NUGET_PACKAGES
        try {
            $env:NUGET_PACKAGES = Join-Path $TestDrive 'intentionally-absent'
            foreach ($fixturePath in $script:GeneratedManifestFixtures) {
                $fixtureItem = Get-Item -LiteralPath $fixturePath -Force
                $fixtureItem.PSIsContainer | Should -BeFalse
                ($fixtureItem.Attributes -band
                    [IO.FileAttributes]::ReparsePoint) | Should -BeFalse
                $fixtureItem.Length | Should -BeLessOrEqual 512KB
                $document = Read-ReplicationWindowsManifestXml `
                    -Content ([IO.File]::ReadAllText($fixturePath)) `
                    -Description "generated fixture '$fixturePath'"
                $identityName = [string]$document.SelectSingleNode(
                    "/*[local-name()='Package']/*[local-name()='Identity']"
                ).GetAttribute('Name')
                $profile = @($policy.profiles | Where-Object {
                        [StringComparer]::Ordinal.Equals(
                            [string]$_.identityName,
                            $identityName)
                    })
                $profile.Count | Should -BeExactly 1
                $profile[0].generatedManifestFixtureProvenance |
                    Should -Match 'WinAppSdkGenerateAppxManifest'
                (Get-FileHash -LiteralPath $fixturePath `
                        -Algorithm SHA256).Hash.ToLowerInvariant() |
                    Should -BeExactly (
                        $profile[0].generatedManifestFixtureSha256)
                {
                    Assert-ReplicationWindowsAppContainerManifestDocument `
                        -Document $document `
                        -Description "generated fixture '$fixturePath'" `
                        -AllowTrustedWinUiExtensions
                } | Should -Not -Throw
            }
        } finally {
            if ($null -eq $previousPackagesRoot) {
                Remove-Item Env:NUGET_PACKAGES -ErrorAction SilentlyContinue
            } else {
                $env:NUGET_PACKAGES = $previousPackagesRoot
            }
        }

        $document = Read-ReplicationWindowsManifestXml `
            -Content ([IO.File]::ReadAllText(
                    $script:GeneratedManifestFixtures[1])) `
            -Description 'DeviceTests generated fixture'
        $identityNode = $document.SelectSingleNode(
            "/*[local-name()='Package']/*[local-name()='Identity']")
        $originalIdentityName = $identityNode.GetAttribute('Name')
        $identityNode.SetAttribute('Name', 'com.microsoft.maui.untrusted')
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'untrusted package profile' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*has no trusted Windows registration profile*'
        $identityNode.SetAttribute('Name', $originalIdentityName)

        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'source fixture'
        } | Should -Throw '*must not declare application or package extensions*'

        $pathNode = $document.SelectSingleNode(
            "//*[local-name()='InProcessServer']/*[local-name()='Path']")
        $originalPath = $pathNode.InnerText
        $pathNode.InnerText = 'evil.dll'
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'malicious path' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*does not match its trusted Windows registration profile*'
        $pathNode.InnerText = $originalPath

        $classNode = $document.SelectSingleNode(
            "//*[local-name()='ActivatableClass']")
        $originalClass = $classNode.GetAttribute('ActivatableClassId')
        $classNode.SetAttribute('ActivatableClassId', 'Microsoft.UI.Xaml.Evil')
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'malicious class' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*does not match its trusted Windows registration profile*'
        $classNode.SetAttribute('ActivatableClassId', $originalClass)

        $extensionNode = $document.SelectSingleNode(
            "/*[local-name()='Package']/*[local-name()='Extensions']/*[local-name()='Extension']")
        $extensionNode.SetAttribute('Category', 'windows.protocol')
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'malicious category' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*untrusted package extension*'
    }

    It 'records every rejected MSIX profile as diagnostic-only evidence' {
        $document = Read-ReplicationWindowsManifestXml `
            -Content ([IO.File]::ReadAllText(
                    $script:GeneratedManifestFixtures[0])) `
            -Description 'observed Sandbox manifest'
        $extensions = @($document.SelectNodes(
            "/*[local-name()='Package']/*[local-name()='Extensions']/" +
            "*[local-name()='Extension']"))
        for ($index = $extensions.Count - 1; $index -ge 2; $index--) {
            [void]$extensions[$index].ParentNode.RemoveChild(
                $extensions[$index])
        }
        $remainingExtensions = @($document.SelectNodes(
            "/*[local-name()='Package']/*[local-name()='Extensions']/" +
            "*[local-name()='Extension']"))
        $keepCounts = @(68, 1)
        for ($index = 0; $index -lt $remainingExtensions.Count; $index++) {
            $classes = @($remainingExtensions[$index].SelectNodes(
                "*[local-name()='InProcessServer']/" +
                "*[local-name()='ActivatableClass']"))
            for ($classIndex = $classes.Count - 1;
                $classIndex -ge $keepCounts[$index];
                $classIndex--) {
                [void]$classes[$classIndex].ParentNode.RemoveChild(
                    $classes[$classIndex])
            }
        }
        $content = $document.OuterXml
        $packagePath = Join-Path $TestDrive 'observed.msix'
        $archive = [IO.Compression.ZipFile]::Open(
            $packagePath,
            [IO.Compression.ZipArchiveMode]::Create)
        try {
            $entry = $archive.CreateEntry('AppxManifest.xml')
            $stream = $entry.Open()
            $writer = [IO.StreamWriter]::new(
                $stream,
                [Text.UTF8Encoding]::new($false))
            try {
                $writer.Write($content)
            } finally {
                $writer.Dispose()
                $stream.Dispose()
            }
        } finally {
            $archive.Dispose()
        }
        $observationRoot = Join-Path $TestDrive 'artifact-root'
        $observationDirectory = Join-Path $observationRoot (
            'sandbox/windows-manifest-observation-attempt-1')
        New-Item -ItemType Directory -Path $observationRoot | Out-Null

        foreach ($sequence in 1..2) {
            {
                Get-ReplicationWindowsMsixManifest `
                    -PackagePath $packagePath `
                    -ManifestObservationRoot $observationRoot `
                    -ManifestObservationDirectory $observationDirectory
            } | Should -Throw '*extensions 2/15, records 69/855*'
        }

        $observations = @(Get-ChildItem -LiteralPath $observationDirectory `
            -Directory | Sort-Object Name)
        $observations.Count | Should -BeExactly 2
        for ($index = 0; $index -lt $observations.Count; $index++) {
            $profilePath = Join-Path $observations[$index].FullName (
                'profile.json')
            $profile = Get-Content -LiteralPath $profilePath -Raw |
                ConvertFrom-Json
            $profile.kind |
                Should -BeExactly 'windows-registration-profile-observation'
            $profile.authorization | Should -BeExactly 'diagnostic-only'
            $profile.canAuthorizeCurrentRun | Should -BeFalse
            $profile.sequence | Should -BeExactly ($index + 1)
            $profile.identityName |
                Should -BeExactly 'com.microsoft.maui.sandbox'
            $profile.actualExtensionCount | Should -BeExactly 2
            $profile.actualRecordCount | Should -BeExactly 69
            $profile.expectedExtensionCount | Should -BeExactly 15
            $profile.expectedRecordCount | Should -BeExactly 855
            $profile.sourceMsixSha256 |
                Should -BeExactly (
                    Get-FileHash -LiteralPath $packagePath -Algorithm SHA256
                ).Hash.ToLowerInvariant()
            $expectedFiles = @(
                'AppxManifest.xml',
                'normalized-records.txt',
                'profile.json'
            )
            $files = @(Get-ChildItem -LiteralPath $observations[$index].FullName `
                -File)
            @($files.Name | Sort-Object) |
                Should -Be @($expectedFiles | Sort-Object)
            foreach ($name in $expectedFiles) {
                $item = Get-Item -LiteralPath (
                    Join-Path $observations[$index].FullName $name)
                $item.PSIsContainer | Should -BeFalse
                ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) |
                    Should -BeFalse
                $maximumLength = switch ($name) {
                    'AppxManifest.xml' { 512KB }
                    'normalized-records.txt' { 2MB }
                    'profile.json' { 16KB }
                }
                $item.Length | Should -BeGreaterThan 0
                $item.Length | Should -BeLessOrEqual $maximumLength
            }
            $files.Extension | Should -Not -Contain '.msix'
            $records = @(Get-Content -LiteralPath (
                    Join-Path $observations[$index].FullName (
                        'normalized-records.txt')))
            $records.Count | Should -BeExactly 69
            (Get-ReplicationWindowsRegistrationRecordsSha256 `
                    -Records $records) |
                Should -BeExactly $profile.actualNormalizedRecordsSha256
        }

        {
            Get-ReplicationWindowsMsixManifest `
                -PackagePath $packagePath `
                -ManifestObservationRoot $observationRoot `
                -ManifestObservationDirectory (Join-Path $TestDrive 'outside')
        } | Should -Throw '*must be inside its trusted root*'
    }

    It 'rejects malformed package extension containers' {
        $content = [IO.File]::ReadAllText(
            $script:GeneratedManifestFixtures[1])

        $document = Read-ReplicationWindowsManifestXml `
            -Content $content `
            -Description 'wrong namespace fixture'
        $container = $document.SelectSingleNode(
            "/*[local-name()='Package']/*[local-name()='Extensions']")
        $replacement = $document.CreateElement('Extensions', 'urn:untrusted')
        foreach ($child in @($container.ChildNodes)) {
            [void]$replacement.AppendChild($child.CloneNode($true))
        }
        [void]$container.ParentNode.ReplaceChild($replacement, $container)
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'wrong namespace fixture' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*untrusted Extensions container*'

        $document = Read-ReplicationWindowsManifestXml `
            -Content $content `
            -Description 'attributed container fixture'
        $container = $document.SelectSingleNode(
            "/*[local-name()='Package']/*[local-name()='Extensions']")
        $container.SetAttribute('Executable', 'untrusted.exe')
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'attributed container fixture' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*untrusted Extensions container*'

        $document = Read-ReplicationWindowsManifestXml `
            -Content $content `
            -Description 'nested container fixture'
        $nested = $document.CreateElement(
            'Extensions',
            'http://schemas.microsoft.com/appx/manifest/foundation/windows10')
        [void]$document.SelectSingleNode(
            "/*[local-name()='Package']/*[local-name()='Properties']"
        ).AppendChild($nested)
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $document `
                -Description 'nested container fixture' `
                -AllowTrustedWinUiExtensions
        } | Should -Throw '*exactly one package Extensions container*'
    }

    It 'rejects coercive and duplicate registration policy JSON' {
        $content = [IO.File]::ReadAllText($script:RegistrationPolicy)
        $mutations = @(
            $content.Replace(
                '"schemaVersion": 1,',
                '"schemaVersion": "1",'),
            $content.Replace(
                '"extensionCount": 15,',
                '"extensionCount": "15",'),
            $content.Replace(
                '"recordCount": 855,',
                '"recordCount": true,'),
            $content.Replace(
                '"schemaVersion": 1,',
                '"schemaVersion": 1, "schemaVersion": 1,'),
            $content.Replace(
                '"identityName": "com.microsoft.maui.sandbox",',
                ('"identityName": "com.microsoft.maui.sandbox", ' +
                    '"identityName": "com.microsoft.maui.sandbox",'))
        )
        foreach ($mutation in $mutations) {
            {
                ConvertFrom-ReplicationWindowsWinUiRegistrationPolicyJson `
                    -Content $mutation
            } | Should -Throw
        }

        $structuralMutations = @(
            $content.Replace(
                '"schemaVersion": 1,',
                '"schemaVersion": 1, "unexpectedRoot": true,'),
            $content.Replace(
                ('  "normalizedRecordsFormat": "ordinal-sorted UTF-8 lines: ' +
                    'Path|ActivatableClassId|ThreadingModel",' + "`n"),
                ''),
            $content.Replace(
                '"identityName": "com.microsoft.maui.sandbox",',
                ('"identityName": "com.microsoft.maui.sandbox", ' +
                    '"unexpectedProfile": true,')),
            $content.Replace(
                '      "recordCount": 855,' + "`n",
                '')
        )
        foreach ($mutation in $structuralMutations) {
            {
                ConvertFrom-ReplicationWindowsWinUiRegistrationPolicyJson `
                    -Content $mutation
            } | Should -Throw '*unexpected properties*'
        }
    }

    It 'rejects structural defects before semantic defects through the full policy loader' {
        $content = [IO.File]::ReadAllText($script:RegistrationPolicy)
        $script:MixedInvalidPolicy = $content.Replace(
            '"schemaVersion": 1,',
            '"schemaVersion": 1, "unexpectedRoot": true,').Replace(
            '"windowsAppSdkPackageId": "Microsoft.WindowsAppSDK",',
            '"windowsAppSdkPackageId": "Untrusted.Package",')
        Mock Get-Content {
            return $script:MixedInvalidPolicy
        }
        {
            Get-ReplicationWindowsWinUiRegistrationPolicy
        } | Should -Throw '*policy has unexpected properties*'

        $script:MixedInvalidPolicy = $content.Replace(
            '"identityName": "com.microsoft.maui.sandbox",',
            ('"identityName": "com.microsoft.maui.untrusted", ' +
                '"unexpectedProfile": true,'))
        {
            Get-ReplicationWindowsWinUiRegistrationPolicy
        } | Should -Throw '*profile has unexpected properties*'
    }

    It 'uses the larger synchronized bound required by generated manifests' {
        $source = Get-Content -LiteralPath $script:SandboxManifest -Raw
        $largeManifest = $source.Replace(
            '</Package>',
            "<!-- $('x' * 200KB) --></Package>")
        {
            Read-ReplicationWindowsManifestXml `
                -Content $largeManifest `
                -Description 'bounded large manifest'
        } | Should -Not -Throw
        $bridge = Get-Content -LiteralPath $script:BridgePath -Raw
        $bridge | Should -Match 'GetByteCount\(\$manifestXml\) -gt 512KB'
        $parent = Get-Content -LiteralPath (Join-Path $PSScriptRoot (
            '../shared/Assert-ReplicationWindowsAppContainer.ps1')) -Raw
        $parent | Should -Match '\$entries\[0\]\.Length -gt 512KB'
    }

    It 'matches snapshots to exact restored package and generator provenance' {
        if (-not (
            (Test-Path -LiteralPath $script:RealRegistrationPath -PathType Leaf) -and
            (Test-Path -LiteralPath $script:RealWindowsAppSdkNuspecPath -PathType Leaf) -and
            (Test-Path -LiteralPath $script:RealWin2DMetadataPath -PathType Leaf) -and
            (Test-Path -LiteralPath $script:RealWebView2MetadataPath -PathType Leaf) -and
            (Test-Path -LiteralPath $script:RealGeneratorTaskPath -PathType Leaf)
        )) {
            Set-ItResult -Skipped -Because 'the Windows-only packages are not restored'
            return
        }
        $policy = Get-ReplicationWindowsWinUiRegistrationPolicy
        (Get-FileHash -LiteralPath $script:RealRegistrationPath `
                -Algorithm SHA256).Hash.ToLowerInvariant() |
            Should -BeExactly $policy.registrationSourceSha256
        $registrationDocument = Read-ReplicationWindowsManifestXml `
            -Content ([IO.File]::ReadAllText($script:RealRegistrationPath)) `
            -Description 'restored WinUI registrations'
        $records = @(Get-ReplicationWindowsInProcessRegistrationRecords `
            -Document $registrationDocument `
            -Shape Expected `
            -Description 'restored WinUI registrations')
        $sandboxProfile = @($policy.profiles | Where-Object {
                $_.identityName -ceq 'com.microsoft.maui.sandbox'
            })[0]
        $records.Count | Should -BeExactly $sandboxProfile.recordCount
        (Get-ReplicationWindowsRegistrationRecordsSha256 -Records $records) |
            Should -BeExactly $sandboxProfile.normalizedRecordsSha256
        @($registrationDocument.DocumentElement.ChildNodes | Where-Object {
                $_.NodeType -eq [Xml.XmlNodeType]::Element
            }).Count | Should -BeExactly $sandboxProfile.extensionCount

        $nuspec = Read-ReplicationWindowsManifestXml `
            -Content ([IO.File]::ReadAllText(
                    $script:RealWindowsAppSdkNuspecPath)) `
            -Description 'restored Windows App SDK package metadata'
        [string]$nuspec.package.metadata.id |
            Should -BeExactly $policy.windowsAppSdkPackageId
        [string]$nuspec.package.metadata.version |
            Should -BeExactly $policy.windowsAppSdkVersion
        $dependency = @($nuspec.package.metadata.dependencies.dependency |
            Where-Object id -CEQ $policy.winUiPackageId)
        $dependency.Count | Should -BeExactly 1
        [string]$dependency[0].version |
            Should -BeExactly "[$($policy.winUiVersion)]"
        (Get-FileHash -LiteralPath $script:RealWin2DMetadataPath `
                -Algorithm SHA256).Hash.ToLowerInvariant() |
            Should -BeExactly $policy.win2dMetadataSha256
        (Get-FileHash -LiteralPath $script:RealWebView2MetadataPath `
                -Algorithm SHA256).Hash.ToLowerInvariant() |
            Should -BeExactly $policy.webView2MetadataSha256
        (Get-FileHash -LiteralPath $script:RealGeneratorTaskPath `
                -Algorithm SHA256).Hash.ToLowerInvariant() |
            Should -BeExactly $policy.generatorTaskSha256

        $probeSource = [IO.File]::ReadAllText(
            $script:DeviceManifest).Replace('$placeholder$', 'replication')
        $probeDocument = Read-ReplicationWindowsManifestXml `
            -Content $probeSource `
            -Description 'generator provenance input'
        $probeIdentity = $probeDocument.SelectSingleNode(
            "/*[local-name()='Package']/*[local-name()='Identity']")
        $probeIdentity.SetAttribute(
            'Name',
            'com.microsoft.maui.controls.devicetests')
        $probeIdentity.SetAttribute('Version', '0.0.0.1')
        $extensions = $probeDocument.CreateElement(
            'Extensions',
            'http://schemas.microsoft.com/appx/manifest/foundation/windows10')
        foreach ($extension in @(
            $registrationDocument.DocumentElement.ChildNodes |
                Where-Object {
                    $_.NodeType -eq [Xml.XmlNodeType]::Element
                }
        )) {
            [void]$extensions.AppendChild(
                $probeDocument.ImportNode($extension, $true))
        }
        [void]$probeDocument.DocumentElement.AppendChild($extensions)
        $inputPath = Join-Path $TestDrive 'generator-input.xml'
        $outputPath = Join-Path $TestDrive 'generator-output.xml'
        $probeDocument.Save($inputPath)

        $generatorOutput = @(& dotnet msbuild $script:GeneratorProbeProject `
            -t:Generate `
            "-p:GeneratorTaskAssembly=$script:RealGeneratorTaskPath" `
            "-p:InputManifest=$inputPath" `
            "-p:OutputManifest=$outputPath" `
            "-p:Win2DMetadata=$script:RealWin2DMetadataPath" `
            "-p:WebView2Metadata=$script:RealWebView2MetadataPath" `
            -nologo `
            -v:minimal 2>&1)
        $generatorExitCode = $LASTEXITCODE
        $generatorExitCode | Should -Be 0 -Because (
            $generatorOutput -join "`n")
        $generatedDocument = Read-ReplicationWindowsManifestXml `
            -Content ([IO.File]::ReadAllText($outputPath)) `
            -Description 'independently regenerated DeviceTests manifest'
        {
            Assert-ReplicationWindowsAppContainerManifestDocument `
                -Document $generatedDocument `
                -Description 'independently regenerated DeviceTests manifest' `
                -AllowTrustedWinUiExtensions
        } | Should -Not -Throw
    }

    It 'rejects generated or full-trust application entry points' {
        $source = Get-Content -LiteralPath $script:SandboxManifest -Raw
        foreach ($entryPoint in @(
            '$targetentrypoint$',
            'windows.partialTrustApplication',
            'Windows.FullTrustApplication'
        )) {
            $mutated = $source.Replace(
                'EntryPoint="Windows.PartialTrustApplication"',
                "EntryPoint=`"$entryPoint`"")
            {
                $document = Read-ReplicationWindowsManifestXml `
                    -Content $mutated `
                    -Description 'mutated manifest'
                Assert-ReplicationWindowsAppContainerManifestDocument `
                    -Document $document `
                    -Description 'mutated manifest'
            } | Should -Throw '*partial-trust Windows application entry point*'
        }
    }

    It 'quotes packaged-app arguments without introducing a shell' {
        ConvertTo-ReplicationWindowsAppArguments -Arguments @(
            'C:\Path With Spaces\result.xml',
            'Issue37540'
        ) | Should -BeExactly '"C:\Path With Spaces\result.xml" "Issue37540"'

        {
            ConvertTo-ReplicationWindowsAppArguments -Arguments @("value`nnext")
        } | Should -Throw '*invalid value*'
    }

    It 'replaces the manifest item only for the exact top-level project' {
        $content = Get-Content -LiteralPath $script:OverrideTargets -Raw
        $content | Should -Match (
            "MSBuildProjectFullPath.*MauiReplicationAppContainerProject")
        $content | Should -Match '<AppxManifest Remove="@\(AppxManifest\)"'
        $content | Should -Match (
            '<AppxManifest Include="\$\(MauiReplicationAppContainerManifest\)"')
        $content | Should -Match (
            'BeforeTargets="MauiGeneratePackageAppxManifest"')
        $content | Should -Match (
            'DependsOnTargets="MauiGeneratePackageAppxManifest"')
        $content | Should -Match '<XmlPeek'
        $content | Should -Match (
            'generated replication manifest does not preserve the partial-trust AppContainer')
    }
}
