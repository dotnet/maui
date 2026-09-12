#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Prepare-DevicePerformancePayload.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-payload-test-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual)
    {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try
{
    $linkValidationRoot = New-Item -ItemType Directory -Path (Join-Path $testRoot "CaseSensitiveApp")
    foreach ($hostIsWindows in @($true, $false))
    {
        $linkTests = [PowerShell]::Create()
        try
        {
            # Exercise both host policies without changing this process's automatic variables.
            $null = $linkTests.AddScript({
                param($scriptPath, $root, [bool]$hostIsWindows)
                $ErrorActionPreference = "Stop"
                Set-Variable -Name IsWindows -Value $hostIsWindows -Force

                $ast = [Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$null, [ref]$null)
                $definitions = @($ast.FindAll({
                    param($node)
                    $node -is [Management.Automation.Language.FunctionDefinitionAst] -and
                        $node.Name -in @("Test-IsLink", "Assert-SafeDirectoryLinks")
                }, $true))
                if ($definitions.Count -ne 2)
                {
                    throw "Expected both directory-link validation functions."
                }
                foreach ($definition in $definitions)
                {
                    . ([ScriptBlock]::Create($definition.Extent.Text))
                }

                function Get-ChildItem {
                    param([string]$LiteralPath, [switch]$Force, [switch]$Recurse)
                    $link
                }

                $caseVariantRoot = Join-Path (Split-Path $root -Parent) "casesensitiveapp"
                $cases = @(
                    @{ Name = "root"; Target = $root; Rejected = $false },
                    @{ Name = "descendant"; Target = (Join-Path $root "Resources/content.txt"); Rejected = $false },
                    @{ Name = "parent"; Target = (Split-Path $root -Parent); Rejected = $true },
                    @{ Name = "prefix sibling"; Target = (Join-Path "$root-sibling" "content.txt"); Rejected = $true },
                    @{ Name = "case-variant root"; Target = $caseVariantRoot; Rejected = -not $hostIsWindows },
                    @{ Name = "case-variant descendant"; Target = (Join-Path $caseVariantRoot "content.txt"); Rejected = -not $hostIsWindows }
                )
                foreach ($case in $cases)
                {
                    $link = [PSCustomObject]@{
                        Attributes = [IO.FileAttributes]::ReparsePoint
                        LinkType = "SymbolicLink"
                        FullName = (Join-Path $root "link")
                        Target = $case.Target
                    }
                    $link | Add-Member -MemberType ScriptMethod -Name ResolveLinkTarget -Value {
                        param([bool]$returnFinalTarget)
                        [IO.FileInfo]::new($this.Target)
                    }

                    $rejected = $false
                    try {
                        Assert-SafeDirectoryLinks ([IO.DirectoryInfo]::new($root))
                    }
                    catch {
                        if ($_.Exception.Message -notlike "*outside its artifact tree*")
                        {
                            throw
                        }
                        $rejected = $true
                    }
                    if ($rejected -ne $case.Rejected)
                    {
                        throw "Link containment case '$($case.Name)' with Windows=$hostIsWindows expected rejected=$($case.Rejected), actual=$rejected."
                    }
                }
                $cases.Count
            }).AddArgument($script).AddArgument($linkValidationRoot.FullName).AddArgument($hostIsWindows)
            $caseCounts = @($linkTests.Invoke())
            if ($linkTests.Streams.Error.Count -gt 0)
            {
                throw $linkTests.Streams.Error[0].Exception
            }
            Assert-Equal 1 $caseCounts.Count "Link containment cases should complete for Windows=$hostIsWindows"
            Assert-Equal 6 $caseCounts[0] "Link containment case count for Windows=$hostIsWindows"
        }
        finally
        {
            $linkTests.Dispose()
        }
    }

    $baseArtifacts = Join-Path $testRoot "base-artifacts/Controls.DeviceTests/Release/net10.0-android"
    $headArtifacts = Join-Path $testRoot "head-artifacts/Controls.DeviceTests/Release/net10.0-android"
    New-Item -ItemType Directory -Force -Path $baseArtifacts, $headArtifacts | Out-Null
    "base" | Set-Content (Join-Path $baseArtifacts "com.microsoft.maui.controls.devicetests-Signed.apk")
    "head" | Set-Content (Join-Path $headArtifacts "com.microsoft.maui.controls.devicetests-Signed.apk")
    @{
        schemaVersion = 1
        repository = "dotnet/maui"
        pullRequestNumber = 42
        variant = "base"
        platform = "android"
        commitSha = "abc123"
        harnessSha = "harness123"
        runtimeVariant = "mono"
        sdkVersion = "10.0.100"
    } | ConvertTo-Json | Set-Content (Join-Path $baseArtifacts "device-performance-build-metadata.json")
    @{
        schemaVersion = 1
        repository = "dotnet/maui"
        pullRequestNumber = 42
        variant = "head"
        platform = "android"
        commitSha = "def456"
        harnessSha = "harness123"
        runtimeVariant = "mono"
        sdkVersion = "10.0.101"
    } | ConvertTo-Json | Set-Content (Join-Path $headArtifacts "device-performance-build-metadata.json")

    $archive = Join-Path $testRoot "payload.zip"
    $metadataPath = Join-Path $testRoot "payload.json"
    & $script `
        -BaseArtifacts (Join-Path $testRoot "base-artifacts") `
        -HeadArtifacts (Join-Path $testRoot "head-artifacts") `
        -Platform android `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-keepitemsinview-update `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -PullRequestAuthor perf-author `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -OutputArchive $archive `
        -MetadataOut $metadataPath

    Assert-Equal 0 $LASTEXITCODE "Payload preparation should succeed"
    Assert-Equal $true (Test-Path $archive) "Payload archive should exist"

    $metadata = Get-Content $metadataPath -Raw | ConvertFrom-Json
    Assert-Equal "payload/base/com.microsoft.maui.controls.devicetests-Signed.apk" $metadata.baseAppRelativePath "Base relative path"
    Assert-Equal "payload/head/com.microsoft.maui.controls.devicetests-Signed.apk" $metadata.headAppRelativePath "Head relative path"
    Assert-Equal 2 $metadata.schemaVersion "Payload schema version"
    Assert-Equal 42 $metadata.pullRequestNumber "Payload PR number"
    Assert-Equal "perf-author" $metadata.pullRequestAuthor "Payload report author"
    Assert-Equal "harness123" $metadata.harnessSha "Payload harness SHA"
    Assert-Equal "collectionview-keepitemsinview-update" $metadata.expectedScenario "Expected scenario"
    Assert-Equal "10.0.100" $metadata.baseSdkVersion "Base SDK version"
    Assert-Equal "10.0.101" $metadata.headSdkVersion "Head SDK version"

    foreach ($applePlatform in @("ios", "maccatalyst"))
    {
        $appleRoots = @{
            base = Join-Path $testRoot "$applePlatform-base"
            head = Join-Path $testRoot "$applePlatform-head"
        }
        $otherPlatform = if ($applePlatform -eq "ios") { "maccatalyst" } else { "ios" }
        $appRelativePath = "Controls.DeviceTests/Release/net10.0-$applePlatform/Controls Tests.app"

        foreach ($variant in @("base", "head"))
        {
            $root = $appleRoots[$variant]
            $app = Join-Path $root $appRelativePath
            $framework = Join-Path $app "Frameworks/Dependency.framework"
            New-Item -ItemType Directory -Force -Path $framework | Out-Null
            "$variant executable" | Set-Content (Join-Path $app "Microsoft.Maui.Controls.DeviceTests")
            "$variant dependency" | Set-Content (Join-Path $framework "Dependency")
            if ($IsMacOS)
            {
                "$variant hidden resource" | Set-Content (Join-Path $app ".hidden-resource")
            }
            foreach ($excludedApp in @(
                "Controls.DeviceTests/Debug/net10.0-$applePlatform/Controls Tests.app",
                "Controls.DeviceTests/Release/net10.0-$otherPlatform/Controls Tests.app",
                "Core.DeviceTests/Release/net10.0-$applePlatform/Core Tests.app"
            ))
            {
                New-Item -ItemType Directory -Force -Path (Join-Path $root $excludedApp) | Out-Null
            }

            @{
                schemaVersion = 1
                repository = "dotnet/maui"
                pullRequestNumber = 42
                variant = $variant
                platform = $applePlatform
                commitSha = if ($variant -eq "base") { "abc123" } else { "def456" }
                harnessSha = "harness123"
                runtimeVariant = "mono"
                sdkVersion = "10.0.100"
            } | ConvertTo-Json | Set-Content (Join-Path $root "device-performance-build-metadata.json")
        }

        $appleArchive = Join-Path $testRoot "$applePlatform-payload.zip"
        $appleMetadataPath = Join-Path $testRoot "$applePlatform-payload.json"
        $appleArguments = @{
            BaseArtifacts = $appleRoots.base
            HeadArtifacts = $appleRoots.head
            Platform = $applePlatform
            BaseCommitSha = "abc123"
            HeadCommitSha = "def456"
            ExpectedScenario = "collectionview-grouped-scrollto-makevisible"
            Repository = "dotnet/maui"
            PullRequestNumber = 42
            HarnessSha = "harness123"
            AzdoBuildId = "100"
            AzdoBuildUrl = "https://build/100"
            OutputArchive = $appleArchive
            MetadataOut = $appleMetadataPath
        }
        & $script @appleArguments

        Assert-Equal 0 $LASTEXITCODE "$applePlatform payload preparation should succeed"
        Assert-Equal $true (Test-Path $appleArchive) "$applePlatform payload archive should exist"
        $appleMetadata = Get-Content $appleMetadataPath -Raw | ConvertFrom-Json
        Assert-Equal $applePlatform $appleMetadata.platform "$applePlatform payload platform"
        Assert-Equal "payload/base/Controls Tests.app" $appleMetadata.baseAppRelativePath "$applePlatform base relative path"
        Assert-Equal "payload/head/Controls Tests.app" $appleMetadata.headAppRelativePath "$applePlatform head relative path"

        $appleExtract = Join-Path $testRoot "$applePlatform-extract"
        Expand-Archive $appleArchive -DestinationPath $appleExtract
        foreach ($variant in @("base", "head"))
        {
            $originalApp = Join-Path $appleRoots[$variant] $appRelativePath
            $extractedApp = Join-Path $appleExtract $appleMetadata.("${variant}AppRelativePath")
            $originalFiles = @(Get-ChildItem $originalApp -File -Recurse -Force)
            $extractedFiles = @(Get-ChildItem $extractedApp -File -Recurse -Force)
            Assert-Equal $originalFiles.Count $extractedFiles.Count "$applePlatform $variant app file count"
            foreach ($file in $originalFiles)
            {
                $relativePath = [IO.Path]::GetRelativePath($originalApp, $file.FullName)
                $extractedFile = Join-Path $extractedApp $relativePath
                Assert-Equal (Get-FileHash $file.FullName).Hash (Get-FileHash $extractedFile).Hash "$applePlatform $variant $relativePath content"
            }
        }

        New-Item -ItemType Directory -Path (Join-Path $appleRoots.base "Controls.DeviceTests/Release/net10.0-$applePlatform/Duplicate.app") | Out-Null
        $duplicateAppRejected = $false
        try {
            & $script @appleArguments
        }
        catch {
            $duplicateAppRejected = $_.Exception.Message -like "Expected exactly one base Controls.DeviceTests $applePlatform app, found 2:*"
        }
        Assert-Equal $true $duplicateAppRejected "$applePlatform payload preparation must reject ambiguous apps"
    }

    $windowsBaseRoot = Join-Path $testRoot "windows-base/Controls.DeviceTests/Release/net10.0-windows/win-x64"
    $windowsHeadRoot = Join-Path $testRoot "windows-head/Controls.DeviceTests/Release/net10.0-windows/win-x64"
    $windowsBase = Join-Path $windowsBaseRoot "publish"
    $windowsHead = Join-Path $windowsHeadRoot "publish"
    New-Item -ItemType Directory -Force -Path $windowsBase, $windowsHead | Out-Null
    "base build output" | Set-Content (Join-Path $windowsBaseRoot "Microsoft.Maui.Controls.DeviceTests.exe")
    "head build output" | Set-Content (Join-Path $windowsHeadRoot "Microsoft.Maui.Controls.DeviceTests.exe")
    "base" | Set-Content (Join-Path $windowsBase "Microsoft.Maui.Controls.DeviceTests.exe")
    "base dependency" | Set-Content (Join-Path $windowsBase "Microsoft.Maui.Controls.DeviceTests.dll")
    "head" | Set-Content (Join-Path $windowsHead "Microsoft.Maui.Controls.DeviceTests.exe")
    "head dependency" | Set-Content (Join-Path $windowsHead "Microsoft.Maui.Controls.DeviceTests.dll")
    @{
        schemaVersion = 1
        repository = "dotnet/maui"
        pullRequestNumber = 42
        variant = "base"
        platform = "windows"
        commitSha = "abc123"
        harnessSha = "harness123"
        runtimeVariant = "coreclr"
        sdkVersion = "10.0.100"
    } | ConvertTo-Json | Set-Content (Join-Path $windowsBase "device-performance-build-metadata.json")
    @{
        schemaVersion = 1
        repository = "dotnet/maui"
        pullRequestNumber = 42
        variant = "head"
        platform = "windows"
        commitSha = "def456"
        harnessSha = "harness123"
        runtimeVariant = "coreclr"
        sdkVersion = "10.0.101"
    } | ConvertTo-Json | Set-Content (Join-Path $windowsHead "device-performance-build-metadata.json")

    $windowsArchive = Join-Path $testRoot "windows-payload.zip"
    $windowsMetadataPath = Join-Path $testRoot "windows-payload.json"
    & $script `
        -BaseArtifacts (Join-Path $testRoot "windows-base") `
        -HeadArtifacts (Join-Path $testRoot "windows-head") `
        -Platform windows `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario carouselview-wheel-snap-windows `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -OutputArchive $windowsArchive `
        -MetadataOut $windowsMetadataPath

    Assert-Equal 0 $LASTEXITCODE "Windows payload preparation should succeed"
    $windowsMetadata = Get-Content $windowsMetadataPath -Raw | ConvertFrom-Json
    Assert-Equal "payload/base/publish/Microsoft.Maui.Controls.DeviceTests.exe" $windowsMetadata.baseAppRelativePath "Windows base relative path"
    Assert-Equal "payload/head/publish/Microsoft.Maui.Controls.DeviceTests.exe" $windowsMetadata.headAppRelativePath "Windows head relative path"
    $windowsExtract = Join-Path $testRoot "windows-extract"
    Expand-Archive $windowsArchive -DestinationPath $windowsExtract
    Assert-Equal $true (Test-Path (Join-Path $windowsExtract $windowsMetadata.baseAppRelativePath)) "Windows base exe archive content"
    Assert-Equal $true (Test-Path (Join-Path $windowsExtract "payload/base/publish/Microsoft.Maui.Controls.DeviceTests.dll")) "Windows base dependency archive content"

    $windowsHeadMetadataPath = Join-Path $windowsHead "device-performance-build-metadata.json"
    $windowsHeadMetadata = Get-Content $windowsHeadMetadataPath -Raw | ConvertFrom-Json
    $windowsHeadMetadata.runtimeVariant = "coreclr`n##vso[task.setvariable variable=Injected]true"
    $windowsHeadMetadata | ConvertTo-Json | Set-Content $windowsHeadMetadataPath
    $metadataInjectionRejected = $false
    try {
        & $script `
            -BaseArtifacts (Join-Path $testRoot "windows-base") `
            -HeadArtifacts (Join-Path $testRoot "windows-head") `
            -Platform windows `
            -BaseCommitSha abc123 `
            -HeadCommitSha def456 `
            -ExpectedScenario carouselview-wheel-snap-windows `
            -Repository dotnet/maui `
            -PullRequestNumber 42 `
            -HarnessSha harness123 `
            -AzdoBuildId 100 `
            -AzdoBuildUrl https://build/100 `
            -OutputArchive (Join-Path $testRoot "invalid-metadata-payload.zip") `
            -MetadataOut (Join-Path $testRoot "invalid-metadata-payload.json")
    }
    catch {
        $metadataInjectionRejected = $_.Exception.Message -like "*runtimeVariant*does not match expected*"
    }
    Assert-Equal $true $metadataInjectionRejected "Payload preparation must reject untrusted runtime metadata"
    $windowsHeadMetadata.runtimeVariant = "coreclr"
    $windowsHeadMetadata | ConvertTo-Json | Set-Content $windowsHeadMetadataPath

    $outsidePublish = Join-Path $testRoot "outside-publish"
    New-Item -ItemType Directory -Force -Path $outsidePublish | Out-Null
    "must not be packaged" | Set-Content (Join-Path $outsidePublish "leak.txt")
    $linkedDirectory = Join-Path $windowsBase "linked-content"
    if ($IsWindows) {
        New-Item -ItemType Junction -Path $linkedDirectory -Target $outsidePublish | Out-Null
    }
    else {
        New-Item -ItemType SymbolicLink -Path $linkedDirectory -Target $outsidePublish | Out-Null
    }

    $linkedPayloadRejected = $false
    try {
        & $script `
            -BaseArtifacts (Join-Path $testRoot "windows-base") `
            -HeadArtifacts (Join-Path $testRoot "windows-head") `
            -Platform windows `
            -BaseCommitSha abc123 `
            -HeadCommitSha def456 `
            -ExpectedScenario carouselview-wheel-snap-windows `
            -Repository dotnet/maui `
            -PullRequestNumber 42 `
            -HarnessSha harness123 `
            -AzdoBuildId 100 `
            -AzdoBuildUrl https://build/100 `
            -OutputArchive (Join-Path $testRoot "linked-payload.zip") `
            -MetadataOut (Join-Path $testRoot "linked-payload.json")
    }
    catch {
        $linkedPayloadRejected = $_.Exception.Message -like "*outside its artifact tree*"
    }
    Assert-Equal $true $linkedPayloadRejected "Payload preparation must reject linked content"

    Write-Host "All device performance payload tests passed."
}
finally
{
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
