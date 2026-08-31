function Read-ReplicationWindowsManifestXml {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Content,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Description
    )

    if ([Text.Encoding]::UTF8.GetByteCount($Content) -gt 512KB) {
        throw "$Description is oversized."
    }

    $settings = [Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null
    $settings.MaxCharactersInDocument = 512KB
    $stringReader = [IO.StringReader]::new($Content)
    $reader = $null
    try {
        $reader = [Xml.XmlReader]::Create($stringReader, $settings)
        $document = [Xml.XmlDocument]::new()
        $document.XmlResolver = $null
        $document.Load($reader)
        return $document
    } catch {
        throw "$Description is not a safe, well-formed XML document: $($_.Exception.Message)"
    } finally {
        if ($null -ne $reader) { $reader.Dispose() }
        $stringReader.Dispose()
    }
}

function Get-ReplicationJsonObjectPropertyMap {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [Text.Json.JsonElement]$Element,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    if ($Element.ValueKind -ne [Text.Json.JsonValueKind]::Object) {
        throw "$Description must be a JSON object."
    }
    $properties =
        [Collections.Generic.Dictionary[string, Text.Json.JsonElement]]::new(
            [StringComparer]::Ordinal)
    foreach ($property in $Element.EnumerateObject()) {
        if (-not $properties.TryAdd($property.Name, $property.Value)) {
            throw "$Description contains duplicate property '$($property.Name)'."
        }
    }
    return ,$properties
}

function Get-ReplicationJsonStringProperty {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, Text.Json.JsonElement]]
        $Properties,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $element = $Properties[$Name]
    if ($element.ValueKind -ne [Text.Json.JsonValueKind]::String) {
        throw "$Description property '$Name' must be a JSON string."
    }
    return $element.GetString()
}

function Get-ReplicationJsonInt32Property {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [Collections.Generic.Dictionary[string, Text.Json.JsonElement]]
        $Properties,

        [Parameter(Mandatory = $true)]
        [string]$Name,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $element = $Properties[$Name]
    $value = 0
    if ($element.ValueKind -ne [Text.Json.JsonValueKind]::Number -or
        -not $element.TryGetInt32([ref]$value)) {
        throw "$Description property '$Name' must be a JSON integer."
    }
    return $value
}

function ConvertFrom-ReplicationWindowsWinUiRegistrationPolicyJson {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Content
    )

    $options = [Text.Json.JsonDocumentOptions]::new()
    $options.AllowTrailingCommas = $false
    $options.CommentHandling = [Text.Json.JsonCommentHandling]::Disallow
    $options.MaxDepth = 8
    try {
        $document = [Text.Json.JsonDocument]::Parse($Content, $options)
    } catch {
        throw "Trusted WinUI registration policy is not strict JSON: $($_.Exception.Message)"
    }
    try {
        $root = Get-ReplicationJsonObjectPropertyMap `
            -Element $document.RootElement `
            -Description 'Trusted WinUI registration policy'
        $expectedRootProperties = @(
            'generatorPackageId',
            'generatorTaskRelativePath',
            'generatorTaskSha256',
            'generatorVersion',
            'normalizedRecordsFormat',
            'profiles',
            'registrationRelativePath',
            'registrationSourceSha256',
            'schemaVersion',
            'webView2MetadataRelativePath',
            'webView2MetadataSha256',
            'webView2PackageId',
            'webView2Version',
            'windowsAppSdkNuspecRelativePath',
            'windowsAppSdkPackageId',
            'windowsAppSdkVersion',
            'win2dMetadataRelativePath',
            'win2dMetadataSha256',
            'win2dPackageId',
            'win2dVersion',
            'winUiPackageId',
            'winUiVersion'
        ) | Sort-Object
        $actualRootProperties = @($root.Keys | Sort-Object)
        if (($actualRootProperties -join "`n") -cne
            ($expectedRootProperties -join "`n")) {
            throw 'Trusted WinUI registration policy has unexpected properties.'
        }
        [void](Get-ReplicationJsonInt32Property `
                -Properties $root `
                -Name 'schemaVersion' `
                -Description 'Trusted WinUI registration policy')
        foreach ($name in @(
            'generatorPackageId',
            'generatorTaskRelativePath',
            'generatorTaskSha256',
            'generatorVersion',
            'normalizedRecordsFormat',
            'registrationRelativePath',
            'registrationSourceSha256',
            'webView2MetadataRelativePath',
            'webView2MetadataSha256',
            'webView2PackageId',
            'webView2Version',
            'windowsAppSdkNuspecRelativePath',
            'windowsAppSdkPackageId',
            'windowsAppSdkVersion',
            'win2dMetadataRelativePath',
            'win2dMetadataSha256',
            'win2dPackageId',
            'win2dVersion',
            'winUiPackageId',
            'winUiVersion'
        )) {
            [void](Get-ReplicationJsonStringProperty `
                    -Properties $root `
                    -Name $name `
                    -Description 'Trusted WinUI registration policy')
        }
        $profilesElement = $root['profiles']
        if ($profilesElement.ValueKind -ne [Text.Json.JsonValueKind]::Array) {
            throw 'Trusted WinUI registration policy profiles must be a JSON array.'
        }
        $expectedProfileProperties = @(
            'extensionCount',
            'generatedManifestFixtureProvenance',
            'generatedManifestFixtureRelativePath',
            'generatedManifestFixtureSha256',
            'identityName',
            'normalizedRecordsSha256',
            'recordCount'
        ) | Sort-Object
        foreach ($profileElement in $profilesElement.EnumerateArray()) {
            $profile = Get-ReplicationJsonObjectPropertyMap `
                -Element $profileElement `
                -Description 'Trusted WinUI registration package profile'
            $actualProfileProperties = @($profile.Keys | Sort-Object)
            if (($actualProfileProperties -join "`n") -cne
                ($expectedProfileProperties -join "`n")) {
                throw (
                    'Trusted WinUI registration package profile has ' +
                    'unexpected properties.')
            }
            foreach ($name in @(
                'generatedManifestFixtureProvenance',
                'generatedManifestFixtureRelativePath',
                'generatedManifestFixtureSha256',
                'identityName',
                'normalizedRecordsSha256'
            )) {
                [void](Get-ReplicationJsonStringProperty `
                        -Properties $profile `
                        -Name $name `
                        -Description 'Trusted WinUI registration package profile')
            }
            foreach ($name in @('extensionCount', 'recordCount')) {
                [void](Get-ReplicationJsonInt32Property `
                        -Properties $profile `
                        -Name $name `
                        -Description 'Trusted WinUI registration package profile')
            }
        }
    } finally {
        $document.Dispose()
    }

    return $Content | ConvertFrom-Json -Depth 8
}

function Get-ReplicationWindowsWinUiRegistrationPolicy {
    [CmdletBinding()]
    param()

    $policyPath = Join-Path $PSScriptRoot (
        'ReplicationWindowsWinUiRegistrations.json')
    $policyItem = Get-Item -LiteralPath $policyPath -Force -ErrorAction Stop
    if ($policyItem.PSIsContainer -or
        $policyItem.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $policyItem.Length -le 0 -or $policyItem.Length -gt 16KB) {
        throw 'Trusted WinUI registration policy must be a bounded regular file.'
    }
    $policy = ConvertFrom-ReplicationWindowsWinUiRegistrationPolicyJson `
        -Content (Get-Content -LiteralPath $policyPath -Raw)
    $ordinal = [StringComparer]::Ordinal
    $versionPattern = '^[1-9][0-9]*(?:\.[0-9]+){2,3}$'
    if ([int]$policy.schemaVersion -ne 1 -or
        -not $ordinal.Equals(
            [string]$policy.windowsAppSdkPackageId,
            'Microsoft.WindowsAppSDK') -or
        [string]$policy.windowsAppSdkVersion -cnotmatch $versionPattern -or
        -not $ordinal.Equals(
            [string]$policy.windowsAppSdkNuspecRelativePath,
            ("microsoft.windowsappsdk/$($policy.windowsAppSdkVersion)/" +
                'microsoft.windowsappsdk.nuspec')) -or
        -not $ordinal.Equals(
            [string]$policy.winUiPackageId,
            'Microsoft.WindowsAppSDK.WinUI') -or
        [string]$policy.winUiVersion -cnotmatch $versionPattern -or
        -not $ordinal.Equals(
            [string]$policy.registrationRelativePath,
            ("microsoft.windowsappsdk.winui/$($policy.winUiVersion)/" +
                'build/native/LiftedWinRTClassRegistrations.xml')) -or
        [string]$policy.registrationSourceSha256 -cnotmatch
            '^[0-9a-f]{64}$' -or
        -not $ordinal.Equals(
            [string]$policy.win2dPackageId,
            'Microsoft.Graphics.Win2D') -or
        [string]$policy.win2dVersion -cnotmatch $versionPattern -or
        -not $ordinal.Equals(
            [string]$policy.win2dMetadataRelativePath,
            ("microsoft.graphics.win2d/$($policy.win2dVersion)/" +
                'lib/uap10.0/Microsoft.Graphics.Canvas.winmd')) -or
        [string]$policy.win2dMetadataSha256 -cnotmatch
            '^[0-9a-f]{64}$' -or
        -not $ordinal.Equals(
            [string]$policy.webView2PackageId,
            'Microsoft.Web.WebView2') -or
        [string]$policy.webView2Version -cnotmatch $versionPattern -or
        -not $ordinal.Equals(
            [string]$policy.webView2MetadataRelativePath,
            ("microsoft.web.webview2/$($policy.webView2Version)/" +
                'lib/Microsoft.Web.WebView2.Core.winmd')) -or
        [string]$policy.webView2MetadataSha256 -cnotmatch
            '^[0-9a-f]{64}$' -or
        -not $ordinal.Equals(
            [string]$policy.generatorPackageId,
            'Microsoft.Windows.SDK.BuildTools.MSIX') -or
        [string]$policy.generatorVersion -cnotmatch $versionPattern -or
        -not $ordinal.Equals(
            [string]$policy.generatorTaskRelativePath,
            ("microsoft.windows.sdk.buildtools.msix/" +
                "$($policy.generatorVersion)/tools/net6.0/" +
                'Microsoft.Windows.SDK.BuildTools.MSIX.dll')) -or
        [string]$policy.generatorTaskSha256 -cnotmatch
            '^[0-9a-f]{64}$' -or
        -not $ordinal.Equals(
            [string]$policy.normalizedRecordsFormat,
            'ordinal-sorted UTF-8 lines: Path|ActivatableClassId|ThreadingModel')) {
        throw 'Trusted WinUI registration policy is malformed.'
    }

    $profiles = @($policy.profiles)
    $expectedFixturePaths =
        [Collections.Generic.Dictionary[string, string]]::new($ordinal)
    $expectedFixturePaths.Add(
        'com.microsoft.maui.sandbox',
        '.github/scripts/tests/fixtures/WindowsGeneratedSandboxAppxManifest.xml')
    $expectedFixturePaths.Add(
        'com.microsoft.maui.controls.devicetests',
        '.github/scripts/tests/fixtures/WindowsGeneratedDeviceTestsAppxManifest.xml')
    $profileNames = [Collections.Generic.HashSet[string]]::new($ordinal)
    $expectedProfileProperties = @(
        'extensionCount',
        'generatedManifestFixtureProvenance',
        'generatedManifestFixtureRelativePath',
        'generatedManifestFixtureSha256',
        'identityName',
        'normalizedRecordsSha256',
        'recordCount'
    ) | Sort-Object
    if ($profiles.Count -ne $expectedFixturePaths.Count) {
        throw 'Trusted WinUI registration policy must contain every package profile.'
    }
    foreach ($profile in $profiles) {
        $profileProperties = @(
            $profile.PSObject.Properties.Name | Sort-Object)
        $identityName = [string]$profile.identityName
        if (($profileProperties -join "`n") -cne
                ($expectedProfileProperties -join "`n") -or
            -not $expectedFixturePaths.ContainsKey($identityName) -or
            -not $profileNames.Add($identityName) -or
            [int]$profile.extensionCount -le 0 -or
            [int]$profile.extensionCount -gt 32 -or
            [int]$profile.recordCount -le 0 -or
            [int]$profile.recordCount -gt 32768 -or
            [string]$profile.normalizedRecordsSha256 -cnotmatch
                '^[0-9a-f]{64}$' -or
            -not $ordinal.Equals(
                [string]$profile.generatedManifestFixtureRelativePath,
                [string]$expectedFixturePaths[$identityName]) -or
            [string]$profile.generatedManifestFixtureSha256 -cnotmatch
                '^[0-9a-f]{64}$' -or
            [string]::IsNullOrWhiteSpace(
                [string]$profile.generatedManifestFixtureProvenance) -or
            ([string]$profile.generatedManifestFixtureProvenance).Length -gt
                1000) {
            throw 'Trusted WinUI registration package profile is malformed.'
        }
    }
    return $policy
}

function Get-ReplicationWindowsInProcessRegistrationRecords {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Xml.XmlDocument]$Document,
        [Parameter(Mandatory = $true)]
        [ValidateSet('Expected', 'Manifest')][string]$Shape,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $ordinal = [StringComparer]::Ordinal
    $foundationNamespace =
        'http://schemas.microsoft.com/appx/manifest/foundation/windows10'
    $root = $Document.DocumentElement
    if ($Shape -eq 'Expected') {
        if (-not $ordinal.Equals($root.LocalName, 'Data') -or
            -not $ordinal.Equals($root.NamespaceURI, $foundationNamespace)) {
            throw "$Description must contain a foundation-namespace Data root."
        }
        $extensions = @($root.ChildNodes | Where-Object {
                $_.NodeType -eq [Xml.XmlNodeType]::Element
            })
    } else {
        $extensionContainers = @($Document.SelectNodes(
                "/*[local-name()='Package']/*[local-name()='Extensions']"))
        $allExtensionContainers = @($Document.SelectNodes(
                "//*[local-name()='Extensions']"))
        $applicationExtensions = @($Document.SelectNodes(
                "/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']/*[local-name()='Extensions' or local-name()='Extension']"))
        if ($applicationExtensions.Count -ne 0) {
            throw "$Description must not declare application extensions."
        }
        if ($extensionContainers.Count -ne 1 -or
            $allExtensionContainers.Count -ne 1) {
            throw "$Description must contain exactly one package Extensions container."
        }
        $extensionContainer = $extensionContainers[0]
        $containerAttributes = @(
            $extensionContainer.Attributes | Where-Object {
                -not $ordinal.Equals(
                    $_.NamespaceURI,
                    'http://www.w3.org/2000/xmlns/')
            })
        if (-not $ordinal.Equals(
                $extensionContainer.NamespaceURI,
                $foundationNamespace) -or
            $containerAttributes.Count -ne 0) {
            throw "$Description contains an untrusted Extensions container."
        }
        $extensions = @($extensionContainer.ChildNodes | Where-Object {
                $_.NodeType -eq [Xml.XmlNodeType]::Element
            })
        $allExtensions = @($Document.SelectNodes("//*[local-name()='Extension']"))
        if ($allExtensions.Count -ne $extensions.Count) {
            throw "$Description contains an extension outside the package allowlist."
        }
    }

    if ($extensions.Count -le 0 -or $extensions.Count -gt 32) {
        throw "$Description contains an invalid number of package extensions."
    }
    $records = [Collections.Generic.List[string]]::new()
    foreach ($extension in $extensions) {
        $extensionAttributes = @($extension.Attributes | Where-Object {
                -not $ordinal.Equals(
                    $_.NamespaceURI,
                    'http://www.w3.org/2000/xmlns/')
            })
        if (-not $ordinal.Equals($extension.LocalName, 'Extension') -or
            -not $ordinal.Equals(
                $extension.NamespaceURI,
                $foundationNamespace) -or
            -not $ordinal.Equals(
                [string]$extension.GetAttribute('Category'),
                'windows.activatableClass.inProcessServer') -or
            $extensionAttributes.Count -ne 1) {
            throw "$Description contains an untrusted package extension."
        }
        $extensionChildren = @($extension.ChildNodes | Where-Object {
                $_.NodeType -eq [Xml.XmlNodeType]::Element
            })
        if ($extensionChildren.Count -ne 1 -or
            -not $ordinal.Equals(
                $extensionChildren[0].LocalName,
                'InProcessServer') -or
            -not $ordinal.Equals(
                $extensionChildren[0].NamespaceURI,
                $foundationNamespace) -or
            @($extensionChildren[0].Attributes | Where-Object {
                    -not $ordinal.Equals(
                        $_.NamespaceURI,
                        'http://www.w3.org/2000/xmlns/')
                }).Count -ne 0) {
            throw "$Description contains a malformed in-process server extension."
        }
        $serverChildren = @($extensionChildren[0].ChildNodes | Where-Object {
                $_.NodeType -eq [Xml.XmlNodeType]::Element
            })
        $paths = @($serverChildren | Where-Object {
                    $ordinal.Equals($_.LocalName, 'Path')
            })
        $classes = @($serverChildren | Where-Object {
                    $ordinal.Equals($_.LocalName, 'ActivatableClass')
            })
        if ($paths.Count -ne 1 -or $classes.Count -le 0 -or
            $classes.Count -gt 1024 -or
            $serverChildren.Count -ne 1 + $classes.Count -or
            -not $ordinal.Equals(
                $paths[0].NamespaceURI,
                $foundationNamespace) -or
            @($paths[0].Attributes | Where-Object {
                    -not $ordinal.Equals(
                        $_.NamespaceURI,
                        'http://www.w3.org/2000/xmlns/')
                }).Count -ne 0 -or
            $paths[0].ChildNodes.Count -ne 1 -or
            $paths[0].FirstChild.NodeType -ne [Xml.XmlNodeType]::Text) {
            throw "$Description contains malformed in-process server content."
        }
        $serverPath = [string]$paths[0].InnerText
        if ($serverPath -cnotmatch '^[A-Za-z0-9._-]{1,200}\.dll$' -or
            -not $ordinal.Equals(
                [IO.Path]::GetFileName($serverPath),
                $serverPath)) {
            throw "$Description contains an unsafe in-process server path."
        }
        foreach ($class in $classes) {
            $classId = [string]$class.GetAttribute('ActivatableClassId')
            $threadingModel = [string]$class.GetAttribute('ThreadingModel')
            $classAttributes = @($class.Attributes | Where-Object {
                    -not $ordinal.Equals(
                        $_.NamespaceURI,
                        'http://www.w3.org/2000/xmlns/')
                })
            if (-not $ordinal.Equals(
                    $class.NamespaceURI,
                    $foundationNamespace) -or
                $classAttributes.Count -ne 2 -or
                $class.HasChildNodes -or
                $classId -cnotmatch '^[A-Za-z_][A-Za-z0-9_.]{0,500}$' -or
                -not (
                    $ordinal.Equals($threadingModel, 'both') -or
                    $ordinal.Equals($threadingModel, 'sta') -or
                    $ordinal.Equals($threadingModel, 'mta'))) {
                throw "$Description contains an unsafe activatable class registration."
            }
            $records.Add("$serverPath|$classId|$threadingModel")
        }
    }
    $recordSet = [Collections.Generic.HashSet[string]]::new($ordinal)
    foreach ($record in $records) {
        if (-not $recordSet.Add($record)) {
            throw "$Description contains duplicate activatable class registrations."
        }
    }
    $ordered = [string[]]$recordSet
    [Array]::Sort($ordered, $ordinal)
    return $ordered
}

function Get-ReplicationWindowsRegistrationRecordsSha256 {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string[]]$Records
    )

    $bytes = [Text.UTF8Encoding]::new($false).GetBytes(
        [string]::Join("`n", $Records))
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        return ([BitConverter]::ToString(
                $sha256.ComputeHash($bytes))).Replace(
                    '-',
                    '').ToLowerInvariant()
    } finally {
        $sha256.Dispose()
    }
}

function Assert-ReplicationWindowsWinUiExtensions {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Xml.XmlDocument]$Document,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $policy = Get-ReplicationWindowsWinUiRegistrationPolicy
    $identityName = [string]$Document.SelectSingleNode(
        "/*[local-name()='Package']/*[local-name()='Identity']").GetAttribute(
            'Name')
    $profiles = @($policy.profiles | Where-Object {
            [StringComparer]::Ordinal.Equals(
                [string]$_.identityName,
                $identityName)
        })
    if ($profiles.Count -ne 1) {
        throw "$Description has no trusted Windows registration profile."
    }
    $profile = $profiles[0]
    $records = @(Get-ReplicationWindowsInProcessRegistrationRecords `
        -Document $Document `
        -Shape Manifest `
        -Description $Description)
    $extensions = @($Document.SelectNodes(
            "/*[local-name()='Package']/*[local-name()='Extensions']/" +
            "*[local-name()='Extension']"))
    $recordsSha256 =
        Get-ReplicationWindowsRegistrationRecordsSha256 -Records $records
    if ($extensions.Count -ne [int]$profile.extensionCount -or
        $records.Count -ne [int]$profile.recordCount -or
        -not [StringComparer]::Ordinal.Equals(
            $recordsSha256,
            [string]$profile.normalizedRecordsSha256)) {
        throw (
            "$Description does not match its trusted Windows registration " +
            "profile: extensions $($extensions.Count)/$($profile.extensionCount), " +
            "records $($records.Count)/$($profile.recordCount), " +
            "SHA-256 $recordsSha256/$($profile.normalizedRecordsSha256).")
    }
    return $true
}

function Assert-ReplicationWindowsAppContainerManifestDocument {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [Xml.XmlDocument]$Document,

        [Parameter(Mandatory = $true)]
        [ValidateNotNullOrEmpty()]
        [string]$Description,

        [string]$ExpectedPublisher = 'CN=DotNetMauiReplication',

        [switch]$AllowTrustedWinUiExtensions
    )

    $foundationNamespace =
        'http://schemas.microsoft.com/appx/manifest/foundation/windows10'
    $uap10Namespace =
        'http://schemas.microsoft.com/appx/manifest/uap/windows10/10'
    if (
        $Document.DocumentElement.LocalName -cne 'Package' -or
        $Document.DocumentElement.NamespaceURI -cne $foundationNamespace
    ) {
        throw "$Description must be a Windows 10 package manifest."
    }

    $identityNodes = @($Document.SelectNodes(
            "/*[local-name()='Package']/*[local-name()='Identity']"))
    $applicationNodes = @($Document.SelectNodes(
            "/*[local-name()='Package']/*[local-name()='Applications']/*[local-name()='Application']"))
    if ($identityNodes.Count -ne 1 -or $applicationNodes.Count -ne 1) {
        throw "$Description must contain exactly one package identity and one application."
    }

    $identity = $identityNodes[0]
    $application = $applicationNodes[0]
    if ([string]$identity.GetAttribute('Publisher') -cne $ExpectedPublisher) {
        throw "$Description must use the dedicated replication signing publisher."
    }
    if ([string]$application.GetAttribute('Id') -cne 'App') {
        throw "$Description must expose exactly the trusted App application identity."
    }
    $entryPoint = [string]$application.GetAttribute('EntryPoint')
    if ($entryPoint -cne 'Windows.PartialTrustApplication') {
        $reportedEntryPoint =
            (($entryPoint -replace '[^\x20-\x7E]', '?') -replace
                '##(?=\[|vso\[)', '## ')
        if ($reportedEntryPoint.Length -gt 1000) {
            $reportedEntryPoint = $reportedEntryPoint.Substring(0, 1000)
        }
        throw (
            "$Description must use the partial-trust Windows application entry point. " +
            "Actual EntryPoint='$reportedEntryPoint'.")
    }
    $actualTrustLevel =
        [string]$application.GetAttribute('TrustLevel', $uap10Namespace)
    $actualRuntimeBehavior =
        [string]$application.GetAttribute('RuntimeBehavior', $uap10Namespace)
    if (
        $actualTrustLevel -cne 'appContainer' -or
        $actualRuntimeBehavior -cne 'packagedClassicApp'
    ) {
        $attributeSummary = @(
            $application.Attributes |
                ForEach-Object {
                    $text = "$($_.Name){$($_.NamespaceURI)}=$($_.Value)"
                    $text -replace '[^\x20-\x7E]', '?'
                }
        ) -join '; '
        $attributeSummary = $attributeSummary -replace '##(?=\[|vso\[)', '## '
        if ($attributeSummary.Length -gt 1000) {
            $attributeSummary = $attributeSummary.Substring(0, 1000)
        }
        $reportedTrustLevel =
            ($actualTrustLevel -replace '[^\x20-\x7E]', '?') -replace
                '##(?=\[|vso\[)', '## '
        $reportedRuntimeBehavior =
            ($actualRuntimeBehavior -replace '[^\x20-\x7E]', '?') -replace
                '##(?=\[|vso\[)', '## '
        throw (
            "$Description must require appContainer trust and packagedClassicApp " +
            "runtime behavior. Actual TrustLevel='$reportedTrustLevel', " +
            "RuntimeBehavior='$reportedRuntimeBehavior'; Application attributes: " +
            $attributeSummary)
    }

    $capabilities = @($Document.SelectNodes(
            "//*[local-name()='Capabilities']/* | //*[local-name()='Capability'] | //*[local-name()='DeviceCapability']"))
    if ($capabilities.Count -ne 0) {
        $names = @(
            $capabilities |
                ForEach-Object {
                    $name = [string]$_.GetAttribute('Name')
                    if ($name) { $name } else { $_.LocalName }
                } |
                Sort-Object -CaseSensitive -Unique
        )
        throw "$Description must not declare package capabilities: $($names -join ', ')."
    }

    $extensions = @($Document.SelectNodes(
            "//*[local-name()='Extensions' or local-name()='Extension']"))
    if ($AllowTrustedWinUiExtensions) {
        $null = Assert-ReplicationWindowsWinUiExtensions `
            -Document $Document `
            -Description $Description
    } elseif ($extensions.Count -ne 0) {
        throw "$Description must not declare application or package extensions."
    }

    foreach ($forbidden in @(
        'runFullTrust',
        'internetClient',
        'internetClientServer',
        'privateNetworkClientServer',
        'location'
    )) {
        if ($Document.OuterXml -match "(?i)\b$([regex]::Escape($forbidden))\b") {
            throw "$Description contains forbidden capability '$forbidden'."
        }
    }

    return [pscustomobject]@{
        Name = [string]$identity.GetAttribute('Name')
        Publisher = [string]$identity.GetAttribute('Publisher')
        ApplicationId = [string]$application.GetAttribute('Id')
        EntryPoint = $entryPoint
        TrustLevel = [string]$application.GetAttribute('TrustLevel', $uap10Namespace)
        RuntimeBehavior = [string]$application.GetAttribute(
            'RuntimeBehavior',
            $uap10Namespace)
    }
}

function Assert-ReplicationWindowsAppContainerManifest {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [string]$ExpectedPublisher = 'CN=DotNetMauiReplication'
    )

    $fullPath = [IO.Path]::GetFullPath($Path)
    $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Length -le 0 -or
        $item.Length -gt 128KB) {
        throw "Windows replication manifest must be a bounded regular file: $fullPath"
    }
    $document = Read-ReplicationWindowsManifestXml `
        -Content ([IO.File]::ReadAllText($fullPath)) `
        -Description "Windows replication manifest '$fullPath'"
    return Assert-ReplicationWindowsAppContainerManifestDocument `
        -Document $document `
        -Description "Windows replication manifest '$fullPath'" `
        -ExpectedPublisher $ExpectedPublisher
}

function Get-ReplicationWindowsMsixManifest {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$PackagePath)

    $fullPath = [IO.Path]::GetFullPath($PackagePath)
    $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
    if ($item.PSIsContainer -or
        $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
        $item.Length -le 0 -or
        $item.Length -gt 2GB -or
        $item.Extension -cne '.msix') {
        throw "Windows replication package must be a bounded regular .msix file: $fullPath"
    }

    $archive = $null
    $stream = $null
    $reader = $null
    try {
        $archive = [IO.Compression.ZipFile]::OpenRead($fullPath)
        $entries = @($archive.Entries | Where-Object {
                $_.FullName -ceq 'AppxManifest.xml'
            })
        if ($entries.Count -ne 1 -or $entries[0].Length -le 0 -or
            $entries[0].Length -gt 512KB) {
            throw 'The MSIX must contain exactly one bounded root AppxManifest.xml.'
        }
        $stream = $entries[0].Open()
        $reader = [IO.StreamReader]::new(
            $stream,
            [Text.UTF8Encoding]::new($false, $true),
            $true,
            4096,
            $true)
        $content = $reader.ReadToEnd()
    } finally {
        if ($null -ne $reader) { $reader.Dispose() }
        if ($null -ne $stream) { $stream.Dispose() }
        if ($null -ne $archive) { $archive.Dispose() }
    }

    $document = Read-ReplicationWindowsManifestXml `
        -Content $content `
        -Description "MSIX manifest in '$fullPath'"
    $identity = Assert-ReplicationWindowsAppContainerManifestDocument `
        -Document $document `
        -Description "MSIX manifest in '$fullPath'" `
        -AllowTrustedWinUiExtensions
    if ([string]::IsNullOrWhiteSpace($identity.Name) -or
        $identity.Name -notmatch '^[A-Za-z0-9.-]{3,50}$') {
        throw "MSIX manifest in '$fullPath' has an invalid package identity."
    }
    return $identity
}

function New-ReplicationWindowsSigningCertificate {
    [CmdletBinding()]
    param()

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'Windows replication package signing requires a Windows host.'
    }

    $subject = 'CN=DotNetMauiReplication'
    $rsa = [Security.Cryptography.RSA]::Create(2048)
    $request = [Security.Cryptography.X509Certificates.CertificateRequest]::new(
        $subject,
        $rsa,
        [Security.Cryptography.HashAlgorithmName]::SHA256,
        [Security.Cryptography.RSASignaturePadding]::Pkcs1)
    $oids = [Security.Cryptography.OidCollection]::new()
    [void]$oids.Add([Security.Cryptography.Oid]::new(
            '1.3.6.1.5.5.7.3.3',
            'Code Signing'))
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509EnhancedKeyUsageExtension]::new(
            $oids,
            $false))
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509BasicConstraintsExtension]::new(
            $false,
            $false,
            0,
            $false))
    $request.CertificateExtensions.Add(
        [Security.Cryptography.X509Certificates.X509KeyUsageExtension]::new(
            [Security.Cryptography.X509Certificates.X509KeyUsageFlags]::DigitalSignature,
            $false))
    $created = $request.CreateSelfSigned(
        [DateTimeOffset]::UtcNow.AddDays(-1),
        [DateTimeOffset]::UtcNow.AddDays(2))
    try {
        $certificate = [Security.Cryptography.X509Certificates.X509Certificate2]::new(
            $created.Export(
                [Security.Cryptography.X509Certificates.X509ContentType]::Pfx),
            '',
            [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::UserKeySet -bor
                [Security.Cryptography.X509Certificates.X509KeyStorageFlags]::PersistKeySet)
    } finally {
        $created.Dispose()
        $rsa.Dispose()
    }

    $userStore = [Security.Cryptography.X509Certificates.X509Store]::new(
        'My',
        [Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser)
    $machineStore = [Security.Cryptography.X509Certificates.X509Store]::new(
        'TrustedPeople',
        [Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
    $userAdded = $false
    try {
        $userStore.Open(
            [Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $userStore.Add($certificate)
        $userAdded = $true
        $machineStore.Open(
            [Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
        $machineStore.Add($certificate)
    } catch {
        if ($userAdded) {
            try { $userStore.Remove($certificate) } catch {}
        }
        $certificate.Dispose()
        throw ("Windows replication could not install its ephemeral package-signing " +
            "certificate in LocalMachine\TrustedPeople: $($_.Exception.Message)")
    } finally {
        $userStore.Close()
        $machineStore.Close()
    }

    return [pscustomobject]@{
        Thumbprint = $certificate.Thumbprint
        Certificate = $certificate
    }
}

function Remove-ReplicationWindowsSigningCertificate {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)]$SigningCertificate)

    $thumbprint = [string]$SigningCertificate.Thumbprint
    foreach ($storeDefinition in @(
        @('My', [Security.Cryptography.X509Certificates.StoreLocation]::CurrentUser),
        @('TrustedPeople', [Security.Cryptography.X509Certificates.StoreLocation]::LocalMachine)
    )) {
        $store = [Security.Cryptography.X509Certificates.X509Store]::new(
            [string]$storeDefinition[0],
            $storeDefinition[1])
        try {
            $store.Open(
                [Security.Cryptography.X509Certificates.OpenFlags]::ReadWrite)
            foreach ($certificate in @($store.Certificates.Find(
                        [Security.Cryptography.X509Certificates.X509FindType]::FindByThumbprint,
                        $thumbprint,
                        $false))) {
                $store.Remove($certificate)
                $certificate.Dispose()
            }
        } finally {
            $store.Close()
        }
    }
    if ($SigningCertificate.PSObject.Properties['Certificate']) {
        $SigningCertificate.Certificate.Dispose()
    }
}

function Invoke-ReplicationWindowsAppxBridge {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('Query', 'Install', 'Remove')]
        [string]$Operation,
        [string]$PackageName = '',
        [string]$PackagePath = '',
        [ValidateRange(1, 600)][int]$TimeoutSeconds = 300
    )

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'The Windows AppX bridge requires a Windows host.'
    }
    $powershell = Join-Path $env:SystemRoot (
        'System32\WindowsPowerShell\v1.0\powershell.exe')
    $bridge = Join-Path $PSScriptRoot 'Invoke-ReplicationWindowsAppx.ps1'
    foreach ($path in @($powershell, $bridge)) {
        $item = Get-Item -LiteralPath $path -Force -ErrorAction Stop
        if ($item.PSIsContainer -or
            $item.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Windows AppX bridge input must be a regular file: $path"
        }
    }
    $arguments = @(
        '-NoLogo',
        '-NoProfile',
        '-NonInteractive',
        '-ExecutionPolicy', 'Bypass',
        '-File', $bridge,
        '-Operation', $Operation
    )
    if (-not [string]::IsNullOrWhiteSpace($PackageName)) {
        if ($PackageName -cnotmatch '^[A-Za-z0-9.-]{3,50}$') {
            throw 'Windows AppX bridge package name is invalid.'
        }
        $arguments += @('-PackageName', $PackageName)
    }
    if (-not [string]::IsNullOrWhiteSpace($PackagePath)) {
        $fullPackagePath = [IO.Path]::GetFullPath($PackagePath)
        $arguments += @('-PackagePath', $fullPackagePath)
    }

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $powershell
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.CreateNoWindow = $true
    foreach ($argument in $arguments) {
        [void]$startInfo.ArgumentList.Add($argument)
    }
    $startInfo.Environment.Clear()
    foreach ($name in @(
        'SystemRoot',
        'WINDIR',
        'SystemDrive',
        'PATH',
        'PATHEXT',
        'TEMP',
        'TMP',
        'USERPROFILE',
        'HOMEDRIVE',
        'HOMEPATH',
        'LOCALAPPDATA',
        'APPDATA',
        'PROGRAMDATA',
        'PROGRAMFILES',
        'PROGRAMFILES(X86)'
    )) {
        $value = [Environment]::GetEnvironmentVariable($name)
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $startInfo.Environment[$name] = $value
        }
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        if (-not $process.Start()) {
            throw 'Windows PowerShell AppX bridge did not start.'
        }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            $process.Kill($true)
            [void]$process.WaitForExit(10000)
            throw "Windows PowerShell AppX bridge timed out after $TimeoutSeconds seconds."
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult().Trim()
        $stderr = $stderrTask.GetAwaiter().GetResult().Trim()
        if ($process.ExitCode -ne 0) {
            if ($stderr.Length -gt 1000) {
                $stderr = $stderr.Substring($stderr.Length - 1000)
            }
            throw "Windows PowerShell AppX bridge failed with exit $($process.ExitCode): $stderr"
        }
        if ($stdout.Length -gt 1MB) {
            throw 'Windows PowerShell AppX bridge produced oversized output.'
        }
        return $stdout
    } finally {
        $process.Dispose()
    }
}

function Get-ReplicationWindowsInstalledPackage {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$PackageName)

    $json = Invoke-ReplicationWindowsAppxBridge `
        -Operation Query `
        -PackageName $PackageName
    if ([string]::IsNullOrWhiteSpace($json) -or $json -ceq 'null') {
        return $null
    }
    $package = $json | ConvertFrom-Json -Depth 6
    $expected = @(
        'installLocation',
        'manifestXml',
        'name',
        'packageFamilyName',
        'packageFullName',
        'publisher'
    )
    $actual = @($package.PSObject.Properties.Name | Sort-Object)
    if (($actual -join ',') -cne (($expected | Sort-Object) -join ',')) {
        throw 'Windows PowerShell AppX bridge returned an unexpected package schema.'
    }
    return $package
}

function Assert-ReplicationInstalledWindowsAppContainerPackage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]$Package,
        [string]$ExpectedPublisher = 'CN=DotNetMauiReplication'
    )

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'Installed Windows package validation requires a Windows host.'
    }
    $manifest = Read-ReplicationWindowsManifestXml `
        -Content ([string]$Package.manifestXml) `
        -Description "Installed package '$($Package.packageFullName)'"
    $identity = Assert-ReplicationWindowsAppContainerManifestDocument `
        -Document $manifest `
        -Description "Installed package '$($Package.packageFullName)'" `
        -ExpectedPublisher $ExpectedPublisher `
        -AllowTrustedWinUiExtensions
    if ([string]$Package.publisher -cne $ExpectedPublisher -or
        [string]$Package.name -cne [string]$identity.Name) {
        throw 'Installed Windows replication package identity does not match its audited manifest.'
    }
    return $identity
}

function Remove-ReplicationWindowsAppContainerPackage {
    [CmdletBinding()]
    param([Parameter(Mandatory = $true)][string]$PackageName)

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'Windows replication package cleanup requires a Windows host.'
    }
    $null = Invoke-ReplicationWindowsAppxBridge `
        -Operation Remove `
        -PackageName $PackageName
}

function Install-ReplicationWindowsAppContainerPackage {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$PackagePath
    )

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'Windows replication package installation requires a Windows host.'
    }
    $fullPath = [IO.Path]::GetFullPath($PackagePath)
    $identity = Get-ReplicationWindowsMsixManifest -PackagePath $fullPath
    Remove-ReplicationWindowsAppContainerPackage -PackageName $identity.Name

    $packageDirectory = Split-Path -Parent $fullPath
    $dependencies = @(
        Get-ChildItem -LiteralPath $packageDirectory -Filter '*.msix' -File -Recurse |
            Where-Object {
                $_.FullName -cne $fullPath -and
                $_.FullName -match '(?i)[\\/]Dependencies[\\/]x64[\\/]'
            } |
            Sort-Object FullName -Unique
    )
    foreach ($dependency in $dependencies) {
        if ($dependency.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw "Windows replication dependency must be a regular file: $($dependency.FullName)"
        }
        $null = Invoke-ReplicationWindowsAppxBridge `
            -Operation Install `
            -PackagePath $dependency.FullName
    }
    $null = Invoke-ReplicationWindowsAppxBridge `
        -Operation Install `
        -PackagePath $fullPath

    $package = Get-ReplicationWindowsInstalledPackage -PackageName $identity.Name
    if ($null -eq $package) {
        throw "Windows replication package '$($identity.Name)' was not installed."
    }
    $null = Assert-ReplicationInstalledWindowsAppContainerPackage -Package $package
    $localStatePath = Join-Path $env:LOCALAPPDATA (
        "Packages\$($package.packageFamilyName)\LocalState")
    New-Item -ItemType Directory -Path $localStatePath -Force | Out-Null

    return [pscustomobject]@{
        Name = [string]$package.name
        PackageFullName = [string]$package.packageFullName
        PackageFamilyName = [string]$package.packageFamilyName
        InstallLocation = [string]$package.installLocation
        LocalStatePath = [IO.Path]::GetFullPath($localStatePath)
        PackagePath = $fullPath
        PackageSha256 = (Get-FileHash -LiteralPath $fullPath -Algorithm SHA256).Hash.ToLowerInvariant()
    }
}

function Initialize-ReplicationWindowsNativeMethods {
    if ('ReplicationWindowsNativeMethods' -as [type]) {
        return
    }

    Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

[ComImport, Guid("2e941141-7f97-4756-ba1d-9decde894a3d"),
 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IReplicationApplicationActivationManager
{
    int ActivateApplication(
        [In, MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
        [In, MarshalAs(UnmanagedType.LPWStr)] string arguments,
        [In] uint options,
        [Out] out uint processId);
}

[ComImport, Guid("45BA127D-10A8-46EA-8AB7-56EA9078943C")]
public class ReplicationApplicationActivationManager
{
}

public static class ReplicationWindowsNativeMethods
{
    const uint TOKEN_QUERY = 0x0008;
    const int TokenIsAppContainer = 29;
    const int ERROR_INSUFFICIENT_BUFFER = 122;

    [DllImport("kernel32.dll", SetLastError = true)]
    static extern bool CloseHandle(IntPtr handle);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out IntPtr tokenHandle);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern bool GetTokenInformation(
        IntPtr tokenHandle,
        int tokenInformationClass,
        out int tokenInformation,
        int tokenInformationLength,
        out int returnLength);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern int GetPackageFullName(
        IntPtr processHandle,
        ref int packageFullNameLength,
        StringBuilder packageFullName);

    public static uint Activate(string appUserModelId, string arguments)
    {
        var manager = (IReplicationApplicationActivationManager)
            new ReplicationApplicationActivationManager();
        int result = manager.ActivateApplication(
            appUserModelId,
            arguments ?? string.Empty,
            0,
            out uint processId);
        if (result != 0)
        {
            throw new Win32Exception(
                result,
                string.Format(
                    "ActivateApplication failed (HRESULT=0x{0:X8}).",
                    result));
        }
        return processId;
    }

    public static bool IsAppContainer(IntPtr processHandle)
    {
        if (!OpenProcessToken(processHandle, TOKEN_QUERY, out IntPtr token))
        {
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }
        try
        {
            if (!GetTokenInformation(
                    token,
                    TokenIsAppContainer,
                    out int isAppContainer,
                    sizeof(int),
                    out int returnLength))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
            return returnLength == sizeof(int) && isAppContainer != 0;
        }
        finally
        {
            CloseHandle(token);
        }
    }

    public static string GetProcessPackageFullName(IntPtr processHandle)
    {
        int length = 0;
        int result = GetPackageFullName(processHandle, ref length, null);
        if (result != ERROR_INSUFFICIENT_BUFFER || length <= 1)
        {
            throw new Win32Exception(result, "Process has no package identity.");
        }
        var value = new StringBuilder(length);
        result = GetPackageFullName(processHandle, ref length, value);
        if (result != 0)
        {
            throw new Win32Exception(result);
        }
        return value.ToString();
    }
}
'@ -ErrorAction Stop | Out-Null
}

function Assert-ReplicationWindowsAppContainerProcess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][Diagnostics.Process]$Process,
        [Parameter(Mandatory = $true)][string]$ExpectedPackageFullName
    )

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'Windows AppContainer process validation requires a Windows host.'
    }
    Initialize-ReplicationWindowsNativeMethods
    $Process.Refresh()
    if ($Process.HasExited) {
        throw "Windows replication process $($Process.Id) exited before AppContainer validation."
    }
    if (-not [ReplicationWindowsNativeMethods]::IsAppContainer($Process.Handle)) {
        throw "Windows replication process $($Process.Id) is not running in an AppContainer."
    }
    $actualPackage = [ReplicationWindowsNativeMethods]::GetProcessPackageFullName(
        $Process.Handle)
    if ($actualPackage -cne $ExpectedPackageFullName) {
        throw ("Windows replication process $($Process.Id) belongs to package " +
            "'$actualPackage', not '$ExpectedPackageFullName'.")
    }
    return $true
}

function Start-ReplicationWindowsAppContainerProcess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$PackageName,
        [AllowEmptyString()][string]$AppArguments = '',
        [switch]$RequireWindow,
        [ValidateRange(1, 120)][int]$WindowTimeoutSeconds = 30
    )

    if (-not [OperatingSystem]::IsWindows()) {
        throw 'Windows AppContainer launch requires a Windows host.'
    }
    $package = Get-ReplicationWindowsInstalledPackage -PackageName $PackageName
    if ($null -eq $package) {
        throw "Windows replication package '$PackageName' is not installed."
    }
    $null = Assert-ReplicationInstalledWindowsAppContainerPackage -Package $package
    Initialize-ReplicationWindowsNativeMethods
    $aumid = "$($package.packageFamilyName)!App"
    $processId = [ReplicationWindowsNativeMethods]::Activate($aumid, $AppArguments)
    if ($processId -le 0) {
        throw "Windows package activation returned invalid process ID '$processId'."
    }
    $process = Get-Process -Id $processId -ErrorAction Stop
    $null = Assert-ReplicationWindowsAppContainerProcess `
        -Process $process `
        -ExpectedPackageFullName $package.packageFullName

    if ($RequireWindow) {
        $deadline = [DateTimeOffset]::UtcNow.AddSeconds($WindowTimeoutSeconds)
        while ([DateTimeOffset]::UtcNow -lt $deadline) {
            $process.Refresh()
            if ($process.HasExited) {
                throw "Windows AppContainer process $processId exited before creating a window."
            }
            if ($process.MainWindowHandle -ne [IntPtr]::Zero) {
                break
            }
            Start-Sleep -Milliseconds 250
        }
        if ($process.MainWindowHandle -eq [IntPtr]::Zero) {
            throw "Windows AppContainer process $processId did not create a top-level window."
        }
    }

    return [pscustomobject]@{
        Process = $process
        ProcessId = [int]$processId
        MainWindowHandle = $process.MainWindowHandle.ToInt64()
        PackageName = [string]$package.name
        PackageFullName = [string]$package.packageFullName
        PackageFamilyName = [string]$package.packageFamilyName
        AppUserModelId = $aumid
    }
}

function ConvertTo-ReplicationWindowsAppArguments {
    [CmdletBinding()]
    param([AllowEmptyCollection()][string[]]$Arguments = @())

    return (@(
        foreach ($argument in $Arguments) {
            if ($null -eq $argument -or
                $argument.IndexOf([char]0) -ge 0 -or
                $argument -match '[\r\n"]') {
                throw 'Windows packaged-app arguments contain an invalid value.'
            }
            '"' + $argument + '"'
        }
    ) -join ' ')
}
