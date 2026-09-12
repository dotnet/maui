#!/usr/bin/env pwsh
#Requires -Modules Pester

# A certification level is a claim about work that happened hours earlier on
# another agent. These tests cover the document that says what it was earned
# on, and the recomputation that refuses to accept it when the artifacts in
# hand no longer match.

BeforeAll {
    . (Join-Path $PSScriptRoot '../shared/Assert-ReplicationCertificationBinding.ps1')

    $script:ScratchRoot = Join-Path $PSScriptRoot 'certification-binding-scratch'
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $script:ScratchRoot -Force | Out-Null

    $script:SourceVersion = ('a' * 40)
    $script:BaseSha = ('b' * 40)
    $script:HeadSha = ('c' * 40)
    $script:TreeHash = ('d' * 64)
    $script:PipelineSha = ('e' * 64)

    function script:New-TrustedScriptIdentities {
        return [ordered]@{
            'scripts/Replicate-Issue.ps1' = ('1' * 64)
            'scripts/shared/Invoke-ReplicationTestVerification.ps1' = ('2' * 64)
            'scripts/shared/Validate-ReplicationCandidate.ps1' = ('3' * 64)
            'scripts/templates/RunReplicationAppiumPlan.cs' = ('4' * 64)
            'skills/run-device-tests/scripts/Run-DeviceTests.ps1' = ('5' * 64)
        }

    }

    function script:New-SelectorContract {
        return [ordered]@{
            variant = 'device-category-only'
            raw = 'Category=Issue12345'
            project = 'Controls.DeviceTests'
            projectPath = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
            class = 'Microsoft.Maui.DeviceTests.Issue12345'
            method = 'ReproducesReportedFailure'
            platform = 'android'
            discoveredCount = 1
            executedCount = 1
            fixture = ''
        }
    }

    function script:New-BindingFixture {
        param([switch]$WithFix)

        $root = Join-Path $script:ScratchRoot ([guid]::NewGuid().ToString('N'))
        $artifacts = Join-Path $root 'artifacts'
        New-Item -ItemType Directory -Path (Join-Path $artifacts 'evidence') -Force | Out-Null
        New-Item -ItemType Directory -Path (Join-Path $artifacts 'verification') -Force | Out-Null

        Set-Content -LiteralPath (Join-Path $artifacts 'candidate.json') `
            -Value '{"schemaVersion":1,"status":"reproduced"}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $artifacts 'test.patch') `
            -Value "diff --git a/t b/t`n+new`n" -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $artifacts 'reproduction-result.json') `
            -Value '{"schemaVersion":1}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $artifacts 'evidence/evidence.json') `
            -Value '{"video":"repro.mp4"}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $artifacts 'verification/verification-result.json') `
            -Value '{"runCount":3}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $artifacts 'verification/negative-control-result.json') `
            -Value '{"runCount":3,"passCount":3}' -Encoding utf8NoBOM
        Set-Content -LiteralPath (Join-Path $artifacts 'verification/negative-control-console.log') `
            -Value 'control passed' -Encoding utf8NoBOM
        [IO.File]::WriteAllBytes((Join-Path $artifacts 'evidence/repro.mp4'), [byte[]](1..64))
        [IO.File]::WriteAllBytes((Join-Path $artifacts 'evidence/preview.gif'), [byte[]](1..32))
        if ($WithFix) {
            Set-Content -LiteralPath (Join-Path $artifacts 'fix.patch') `
                -Value "diff --git a/p b/p`n-old`n+new`n" -Encoding utf8NoBOM
            Set-Content -LiteralPath (Join-Path $artifacts 'fix-scope-baseline.json') `
                -Value '{"files":["p"]}' -Encoding utf8NoBOM
            Set-Content -LiteralPath (Join-Path $artifacts 'verification/fix-control-result.json') `
                -Value '{"runCount":3,"passCount":3}' -Encoding utf8NoBOM
            Set-Content -LiteralPath (Join-Path $artifacts 'verification/fix-control-console.log') `
                -Value 'fix passed' -Encoding utf8NoBOM
            Set-Content -LiteralPath (Join-Path $artifacts 'verification/restoration-result.json') `
                -Value '{"completedRunCount":3,"verificationPassed":true}' -Encoding utf8NoBOM
            Set-Content -LiteralPath (Join-Path $artifacts 'verification/restoration-console.log') `
                -Value 'restoration failed as expected' -Encoding utf8NoBOM
            foreach ($arm in @('baseline', 'fix')) {
                $directory = Join-Path $artifacts "regression/$arm"
                New-Item -ItemType Directory -Path $directory -Force | Out-Null
                $xmlPath = Join-Path $directory 'source-result-1.xml'
                Set-Content -LiteralPath $xmlPath -Encoding utf8NoBOM -Value (
                    '<assemblies><assembly total="2" passed="2" failed="0" skipped="0" errors="0">' +
                    '<collection><test name="Existing behavior" type="Microsoft.Maui.DeviceTests.LabelTests" ' +
                    'method="ExistingBehavior" result="Pass" />' +
                    '<test name="Second behavior" type="Microsoft.Maui.DeviceTests.LabelTests" ' +
                    'method="SecondBehavior" result="Pass" /></collection></assembly></assemblies>')
                [ordered]@{
                    schemaVersion = 1
                    completed = $true
                    runStartedUtc = '2026-01-01T00:00:00.0000000Z'
                    completedUtc = '2026-01-01T00:01:00.0000000Z'
                    project = 'Controls.DeviceTests'
                    platform = 'android'
                    testFilter = 'Category=Label'
                    includeClass = 'Microsoft.Maui.DeviceTests.LabelTests'
                    total = 2
                    passed = 2
                    failed = 0
                    skipped = 0
                    errors = 0
                    records = @(
                        [ordered]@{
                            type = 'Microsoft.Maui.DeviceTests.LabelTests'
                            method = 'ExistingBehavior'
                            displayName = 'Existing behavior'
                            outcome = 'Pass'
                            failureSignature = ''
                        },
                        [ordered]@{
                            type = 'Microsoft.Maui.DeviceTests.LabelTests'
                            method = 'SecondBehavior'
                            displayName = 'Second behavior'
                            outcome = 'Pass'
                            failureSignature = ''
                        }
                    )
                    resultFiles = @([ordered]@{
                        name = 'source-result-1.xml'
                        sha256 = (Get-FileHash $xmlPath -Algorithm SHA256).Hash.ToLowerInvariant()
                    })
                } | ConvertTo-Json -Depth 8 |
                    Set-Content -LiteralPath (Join-Path $directory 'strict-test-evidence.json') -Encoding utf8NoBOM
            }
            $fixPatchDigest = (Get-FileHash (Join-Path $artifacts 'fix.patch') -Algorithm SHA256).Hash.ToLowerInvariant()
            $baselineResult = Join-Path $artifacts 'regression/baseline/strict-test-evidence.json'
            $fixResult = Join-Path $artifacts 'regression/fix/strict-test-evidence.json'
            [ordered]@{
                schemaVersion = 1
                baselineSha = $script:BaseSha
                productPatchSha256 = $fixPatchDigest
                platform = 'android'
                project = 'Controls.DeviceTests'
                projectPath = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
                category = 'Label'
                testClass = 'Microsoft.Maui.DeviceTests.LabelTests'
                generatedTestPath = 'src/Controls/tests/DeviceTests/Elements/Label/Issue12345.Android.cs'
                baselineResult = 'regression/baseline/strict-test-evidence.json'
                fixResult = 'regression/fix/strict-test-evidence.json'
                baselineResultSha256 = (Get-FileHash $baselineResult -Algorithm SHA256).Hash.ToLowerInvariant()
                fixResultSha256 = (Get-FileHash $fixResult -Algorithm SHA256).Hash.ToLowerInvariant()
                comparison = 'pass'
            } | ConvertTo-Json -Depth 8 |
                Set-Content -LiteralPath (Join-Path $artifacts 'regression/regression-evidence.json') -Encoding utf8NoBOM
        }

        $bindingPath = Join-Path $root 'certification-binding.json'
        $binding = New-ReplicationCertificationBinding `
            -IssueNumber 12345 `
            -Platform 'android' `
            -ArtifactRoot $artifacts `
            -TrustedSourceVersion $script:SourceVersion `
            -TrustedTreeHash $script:TreeHash `
            -PipelineSha256 $script:PipelineSha `
            -ReplicationBaseSha $script:BaseSha `
            -ExecutionHeadSha $script:HeadSha `
            -TrustedScripts (script:New-TrustedScriptIdentities) `
            -Selector (Get-ReplicationBindingSelector `
                -Selector (script:New-SelectorContract) -TestType 'DeviceTest') `
            -ExpectedRegressionCategory 'Label' `
            -ExpectedRegressionClass 'Microsoft.Maui.DeviceTests.LabelTests' `
            -OutputPath $bindingPath

        return [pscustomobject]@{
            Root = $root
            Artifacts = $artifacts
            BindingPath = $bindingPath
            Binding = $binding
        }
    }

    function script:New-CatalystCompanionBindingFixture {
        $fixture = script:New-BindingFixture -WithFix
        $trustedFixture = Join-Path $fixture.Root 'trusted-fixture.cs'
        Copy-Item -LiteralPath (Join-Path $PSScriptRoot (
                '../fixtures/ReplicationGesturePlatformManagerRegression.iOS.cs')) `
            -Destination $trustedFixture

        foreach ($arm in @('baseline', 'fix')) {
            $primaryPath = Join-Path $fixture.Artifacts (
                "regression/$arm/strict-test-evidence.json")
            $primary = Get-Content -LiteralPath $primaryPath -Raw |
                ConvertFrom-Json -Depth 20
            $primary.platform = 'ios'
            $primary | ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $primaryPath -Encoding utf8NoBOM

            $directory = Join-Path $fixture.Artifacts "regression/catalyst-$arm"
            New-Item -ItemType Directory -Path $directory -Force | Out-Null
            $xmlPath = Join-Path $directory 'source-result-1.xml'
            Set-Content -LiteralPath $xmlPath -Encoding utf8NoBOM -Value (
                '<assemblies><assembly total="2" passed="2" failed="0" skipped="0" errors="0">' +
                '<collection><test name="Secondary to both" type="' +
                'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression" ' +
                'method="SecondaryToBothCreatesNativeTap" result="Pass" />' +
                '<test name="Secondary to primary" type="' +
                'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression" ' +
                'method="SecondaryToPrimaryCreatesNativeTap" result="Pass" />' +
                '</collection></assembly></assemblies>')
            [ordered]@{
                schemaVersion = 1
                completed = $true
                runStartedUtc = '2026-01-01T00:00:00.0000000Z'
                completedUtc = '2026-01-01T00:01:00.0000000Z'
                project = 'Controls'
                platform = 'catalyst'
                testFilter = 'Category=Gesture'
                includeClass =
                'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression'
                total = 2
                passed = 2
                failed = 0
                skipped = 0
                errors = 0
                records = @(
                    [ordered]@{
                        type = 'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression'
                        method = 'SecondaryToBothCreatesNativeTap'
                        displayName = 'Secondary to both'
                        outcome = 'Pass'
                        failureSignature = ''
                    },
                    [ordered]@{
                        type = 'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression'
                        method = 'SecondaryToPrimaryCreatesNativeTap'
                        displayName = 'Secondary to primary'
                        outcome = 'Pass'
                        failureSignature = ''
                    }
                )
                resultFiles = @([ordered]@{
                        name = 'source-result-1.xml'
                        sha256 = (Get-FileHash $xmlPath -Algorithm SHA256).Hash.ToLowerInvariant()
                    })
            } | ConvertTo-Json -Depth 8 |
                Set-Content -LiteralPath (Join-Path $directory (
                        'strict-test-evidence.json')) -Encoding utf8NoBOM
        }

        $comparisonPath = Join-Path $fixture.Artifacts (
            'regression/regression-evidence.json')
        $comparison = Get-Content -LiteralPath $comparisonPath -Raw |
            ConvertFrom-Json -Depth 20 -AsHashtable
        $comparison.schemaVersion = 2
        $comparison.platform = 'ios'
        $comparison.baselineResultSha256 = Get-ReplicationBindingFileDigest `
            -Path (Join-Path $fixture.Artifacts ([string]$comparison.baselineResult))
        $comparison.fixResultSha256 = Get-ReplicationBindingFileDigest `
            -Path (Join-Path $fixture.Artifacts ([string]$comparison.fixResult))
        $comparison.companion = [ordered]@{
            id = 'gesture-platform-manager-catalyst-v1'
            fixturePath =
            'scripts/fixtures/ReplicationGesturePlatformManagerRegression.iOS.cs'
            fixtureTargetPath =
            'src/Controls/tests/DeviceTests/ReplicationGesturePlatformManagerRegression.iOS.cs'
            fixtureSha256 =
            '9df820c9d684243f88dd4cc39c8090071299cba5e1731e8857e686aec66a0794'
            platform = 'catalyst'
            project = 'Controls'
            projectPath =
            'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
            category = 'Gesture'
            testClass =
            'Microsoft.Maui.DeviceTests.ReplicationGesturePlatformManagerRegression'
            methods = @(
                'SecondaryToBothCreatesNativeTap',
                'SecondaryToPrimaryCreatesNativeTap')
            baselineResult =
            'regression/catalyst-baseline/strict-test-evidence.json'
            fixResult = 'regression/catalyst-fix/strict-test-evidence.json'
            baselineResultSha256 = Get-ReplicationBindingFileDigest -Path (
                Join-Path $fixture.Artifacts (
                    'regression/catalyst-baseline/strict-test-evidence.json'))
            fixResultSha256 = Get-ReplicationBindingFileDigest -Path (
                Join-Path $fixture.Artifacts (
                    'regression/catalyst-fix/strict-test-evidence.json'))
            comparison = 'pass'
        }
        $comparison | ConvertTo-Json -Depth 20 |
            Set-Content -LiteralPath $comparisonPath -Encoding utf8NoBOM

        $binding = New-ReplicationCertificationBinding `
            -IssueNumber 12345 `
            -Platform 'ios' `
            -ArtifactRoot $fixture.Artifacts `
            -TrustedSourceVersion $script:SourceVersion `
            -TrustedTreeHash $script:TreeHash `
            -PipelineSha256 $script:PipelineSha `
            -ReplicationBaseSha $script:BaseSha `
            -ExecutionHeadSha $script:HeadSha `
            -TrustedScripts (script:New-TrustedScriptIdentities) `
            -Selector (Get-ReplicationBindingSelector `
                -Selector (script:New-SelectorContract) -TestType 'DeviceTest') `
            -ExpectedRegressionCategory 'Label' `
            -ExpectedRegressionClass 'Microsoft.Maui.DeviceTests.LabelTests' `
            -ExpectedFixPaths @(
            'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs') `
            -TrustedFixturePath $trustedFixture `
            -OutputPath $fixture.BindingPath
        $fixture.Binding = $binding
        $fixture | Add-Member -NotePropertyName TrustedFixture `
            -NotePropertyValue $trustedFixture
        return $fixture
    }

    function script:New-DowngradedGpmRegressionFixture {
        $fixture = script:New-CatalystCompanionBindingFixture
        $path = 'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs'
        $patchPath = Join-Path $fixture.Artifacts 'fix.patch'
        Set-Content -LiteralPath $patchPath -Encoding utf8NoBOM -Value (
            "diff --git a/$path b/$path`n--- a/$path`n+++ b/$path`n@@ -1 +1 @@`n-old`n+new")
        $comparisonPath = Join-Path $fixture.Artifacts 'regression/regression-evidence.json'
        $comparison = Get-Content -LiteralPath $comparisonPath -Raw |
            ConvertFrom-Json -AsHashtable
        $comparison.schemaVersion = 1
        [void]$comparison.Remove('companion')
        $comparison.productPatchSha256 = Get-ReplicationBindingFileDigest -Path $patchPath
        $comparison | ConvertTo-Json -Depth 20 |
            Set-Content -LiteralPath $comparisonPath -Encoding utf8NoBOM
        return $fixture
    }

    function script:Set-RegressionFixtureRecord {
        param(
            [Parameter(Mandatory = $true)][object]$Fixture,
            [Parameter(Mandatory = $true)][ValidateSet('baseline', 'fix')][string]$Arm,
            [Parameter(Mandatory = $true)][int]$Index,
            [Parameter(Mandatory = $true)][ValidateSet('Pass', 'Fail', 'Skip')][string]$Outcome,
            [string]$DisplayName = '',
            [string]$StackFrame = 'Microsoft.Maui.DeviceTests.LabelTests.ExistingBehavior()',
            [string]$StackLocation = '/agent/_work/1/s/LabelTests.cs:line 42'
        )

        $directory = Join-Path $Fixture.Artifacts "regression/$Arm"
        $xmlPath = Join-Path $directory 'source-result-1.xml'
        [xml]$xml = Get-Content -LiteralPath $xmlPath -Raw
        $tests = @($xml.SelectNodes('//test'))
        $test = $tests[$Index]
        $test.SetAttribute('result', $Outcome)
        if (-not [string]::IsNullOrWhiteSpace($DisplayName)) {
            $test.SetAttribute('name', $DisplayName)
        }

        $oldFailure = $test.SelectSingleNode('./failure')
        if ($oldFailure) { $null = $test.RemoveChild($oldFailure) }
        if ($Outcome -ceq 'Fail') {
            $failure = $xml.CreateElement('failure')
            $failure.SetAttribute('exception-type', 'Xunit.Sdk.EqualException')
            $message = $xml.CreateElement('message')
            $message.InnerText = 'Expected: Html'
            $stack = $xml.CreateElement('stack-trace')
            $stack.InnerText = "at $StackFrame in $StackLocation"
            $null = $failure.AppendChild($message)
            $null = $failure.AppendChild($stack)
            $null = $test.AppendChild($failure)
        }
        $assembly = $xml.SelectSingleNode('/assemblies/assembly')
        $assembly.SetAttribute('passed', [string]@($tests | Where-Object {
            $_.GetAttribute('result') -ceq 'Pass'
        }).Count)
        $assembly.SetAttribute('failed', [string]@($tests | Where-Object {
            $_.GetAttribute('result') -ceq 'Fail'
        }).Count)
        $assembly.SetAttribute('skipped', [string]@($tests | Where-Object {
            $_.GetAttribute('result') -ceq 'Skip'
        }).Count)
        $xml.Save($xmlPath)

        $strictPath = Join-Path $directory 'strict-test-evidence.json'
        $strict = Get-Content -LiteralPath $strictPath -Raw | ConvertFrom-Json
        $strict.records[$Index].displayName = $test.GetAttribute('name')
        $strict.records[$Index].outcome = $Outcome
        $strict.records[$Index].failureSignature = if ($Outcome -ceq 'Fail') {
            Get-ReplicationDeviceTestFailureSignature -Test $test
        } else { '' }
        $strict.passed = [int]$assembly.GetAttribute('passed')
        $strict.failed = [int]$assembly.GetAttribute('failed')
        $strict.skipped = [int]$assembly.GetAttribute('skipped')
        $strict.resultFiles[0].sha256 = (
            Get-FileHash -LiteralPath $xmlPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $strict | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $strictPath -Encoding utf8NoBOM

        $comparisonPath = Join-Path $Fixture.Artifacts 'regression/regression-evidence.json'
        $comparison = Get-Content -LiteralPath $comparisonPath -Raw | ConvertFrom-Json
        $digestProperty = if ($Arm -ceq 'baseline') {
            'baselineResultSha256'
        } else { 'fixResultSha256' }
        $comparison.$digestProperty = (
            Get-FileHash -LiteralPath $strictPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $comparison | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $comparisonPath -Encoding utf8NoBOM
    }

    function script:Set-RegressionFixtureRecords {
        param(
            [Parameter(Mandatory = $true)][object]$Fixture,
            [Parameter(Mandatory = $true)]
            [ValidateSet('baseline', 'fix')][string]$Arm,
            [Parameter(Mandatory = $true)][object[]]$Records
        )

        $directory = Join-Path $Fixture.Artifacts "regression/$Arm"
        $xmlPath = Join-Path $directory 'source-result-1.xml'
        $xml = [Xml.XmlDocument]::new()
        $assemblies = $xml.CreateElement('assemblies')
        $null = $xml.AppendChild($assemblies)
        $assembly = $xml.CreateElement('assembly')
        $null = $assemblies.AppendChild($assembly)
        $collection = $xml.CreateElement('collection')
        $null = $assembly.AppendChild($collection)
        foreach ($record in $Records) {
            $test = $xml.CreateElement('test')
            $test.SetAttribute(
                'type',
                'Microsoft.Maui.DeviceTests.LabelTests')
            $test.SetAttribute('method', [string]$record.Method)
            $test.SetAttribute('name', [string]$record.Name)
            $test.SetAttribute('result', [string]$record.Outcome)
            if ([string]$record.Outcome -ceq 'Fail') {
                $failure = $xml.CreateElement('failure')
                $failure.SetAttribute(
                    'exception-type',
                    'Xunit.Sdk.EqualException')
                $message = $xml.CreateElement('message')
                $message.InnerText = 'Expected: Html'
                $stack = $xml.CreateElement('stack-trace')
                $stack.InnerText = "at $([string]$record.Frame) in /agent/LabelTests.cs:line 42"
                $null = $failure.AppendChild($message)
                $null = $failure.AppendChild($stack)
                $null = $test.AppendChild($failure)
            }
            $null = $collection.AppendChild($test)
        }
        $passed = @($Records | Where-Object { $_.Outcome -ceq 'Pass' }).Count
        $failed = @($Records | Where-Object { $_.Outcome -ceq 'Fail' }).Count
        $skipped = @($Records | Where-Object { $_.Outcome -ceq 'Skip' }).Count
        $assembly.SetAttribute('total', [string]$Records.Count)
        $assembly.SetAttribute('passed', [string]$passed)
        $assembly.SetAttribute('failed', [string]$failed)
        $assembly.SetAttribute('skipped', [string]$skipped)
        $assembly.SetAttribute('errors', '0')
        $xml.Save($xmlPath)

        $parsed = Read-ReplicationDeviceTestResultXmlStrict `
            -Path $xmlPath `
            -NotBeforeUtc ([datetime]'2025-01-01T00:00:00Z') `
            -ExpectedClass 'Microsoft.Maui.DeviceTests.LabelTests'
        $strictPath = Join-Path $directory 'strict-test-evidence.json'
        $strict = Get-Content -LiteralPath $strictPath -Raw |
            ConvertFrom-Json
        $strict.total = $parsed.Total
        $strict.passed = $parsed.Passed
        $strict.failed = $parsed.Failed
        $strict.skipped = $parsed.Skipped
        $strict.errors = $parsed.Errors
        $strict.records = @($parsed.Records)
        $strict.resultFiles[0].sha256 = $parsed.Sha256
        $strict | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $strictPath -Encoding utf8NoBOM

        $comparisonPath = Join-Path $Fixture.Artifacts (
            'regression/regression-evidence.json')
        $comparison = Get-Content -LiteralPath $comparisonPath -Raw |
            ConvertFrom-Json
        $property = if ($Arm -ceq 'baseline') {
            'baselineResultSha256'
        } else {
            'fixResultSha256'
        }
        $comparison.$property = (
            Get-FileHash -LiteralPath $strictPath -Algorithm SHA256
        ).Hash.ToLowerInvariant()
        $comparison | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $comparisonPath -Encoding utf8NoBOM
    }

    function script:Invoke-BindingCheck {
        param(
            [Parameter(Mandatory = $true)][object]$Fixture,
            [hashtable]$Override = @{}
        )

        $parameters = @{
            Binding = $Fixture.BindingPath
            ArtifactRoot = $Fixture.Artifacts
            TrustedSourceVersion = $script:SourceVersion
            TrustedTreeHash = $script:TreeHash
            PipelineSha256 = $script:PipelineSha
            ReplicationBaseSha = $script:BaseSha
            TrustedScripts = (script:New-TrustedScriptIdentities)
            Selector = (Get-ReplicationBindingSelector `
                -Selector (script:New-SelectorContract) -TestType 'DeviceTest')
            IssueNumber = 12345
            Platform = 'android'
            ExpectedRegressionCategory = 'Label'
            ExpectedRegressionClass = 'Microsoft.Maui.DeviceTests.LabelTests'
        }
        foreach ($key in $Override.Keys) { $parameters[$key] = $Override[$key] }
        return Assert-ReplicationCertificationBinding @parameters
    }
}

AfterAll {
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'What a certification binding records' {
    It 'binds the pipeline revision, the base, the head, and the trusted tree' {
        $fixture = script:New-BindingFixture
        $fixture.Binding.schemaVersion | Should -Be 1
        $fixture.Binding.trustedSourceVersion | Should -Be $script:SourceVersion
        $fixture.Binding.replicationBaseSha | Should -Be $script:BaseSha
        $fixture.Binding.executionHeadSha | Should -Be $script:HeadSha
        $fixture.Binding.trustedTreeHash | Should -Be $script:TreeHash
        $fixture.Binding.pipelineSha256 | Should -Be $script:PipelineSha
        $fixture.Binding.digest | Should -Match '^[0-9a-f]{64}$'
    }

    It 'binds the SHA-256 of the test patch, and of the fix patch when there is one' {
        $withoutFix = script:New-BindingFixture
        $withoutFix.Binding.testPatchSha256 | Should -Match '^[0-9a-f]{64}$'
        # Absent, not omitted: "this run had no fix" must stay distinguishable
        # from "somebody removed the field".
        $withoutFix.Binding.fixPatchSha256 | Should -BeNullOrEmpty

        $withFix = script:New-BindingFixture -WithFix
        $withFix.Binding.fixPatchSha256 | Should -Match '^[0-9a-f]{64}$'
        $withFix.Binding.fixPatchSha256 | Should -Not -Be $withFix.Binding.testPatchSha256
        [string]$withFix.Binding.evidence['regression/regression-evidence.json'] |
            Should -Match '^[0-9a-f]{64}$'
    }

    It 'rejects a pass-to-fail sibling regression even when aggregate totals could offset' {
        $fixture = script:New-BindingFixture -WithFix
        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm fix -Index 0 -Outcome Fail

        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Throw '*incompatible outcome*Pass -> Fail*'
    }

    It 'allows an inherited failure only with the identical signature and allows it to become green' {
        $fixture = script:New-BindingFixture -WithFix
        $baselinePath = Join-Path $fixture.Artifacts 'regression/baseline/strict-test-evidence.json'
        $fixPath = Join-Path $fixture.Artifacts 'regression/fix/strict-test-evidence.json'
        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm baseline -Index 0 -Outcome Fail
        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm fix -Index 0 -Outcome Fail

        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } | Should -Not -Throw

        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm fix -Index 0 -Outcome Fail `
            -StackFrame 'Microsoft.Maui.DeviceTests.LabelTests.OtherAssertion()'
        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Throw '*incompatible outcome*Fail -> Fail*'

        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm fix -Index 0 -Outcome Pass
        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } | Should -Not -Throw
    }

    It 'treats volatile source roots and line numbers as the same failure location' {
        $fixture = script:New-BindingFixture -WithFix
        script:Set-RegressionFixtureRecord `
                -Fixture $fixture -Arm baseline -Index 0 -Outcome Fail `
                -StackLocation '/agent/_work/1/s/LabelTests.cs:line 42'
        script:Set-RegressionFixtureRecord `
                -Fixture $fixture -Arm fix -Index 0 -Outcome Fail `
                -StackLocation 'D:\agent\_work\9\s\LabelTests.cs:line 987'

        { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } | Should -Not -Throw
    }

    It 'rejects a newly skipped test and identity-set drift' {
        $fixture = script:New-BindingFixture -WithFix
        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm fix -Index 0 -Outcome Skip
        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Throw '*incompatible outcome*Pass -> Skip*'

        script:Set-RegressionFixtureRecord `
            -Fixture $fixture -Arm fix -Index 0 -Outcome Pass `
            -DisplayName 'different identity'
        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Throw '*same exact test identities*'
    }

    It 'discloses stable skips but requires a comparable passing sibling' {
        $fixture = script:New-BindingFixture -WithFix
        $baselinePath = Join-Path $fixture.Artifacts 'regression/baseline/strict-test-evidence.json'
        $fixPath = Join-Path $fixture.Artifacts 'regression/fix/strict-test-evidence.json'
        foreach ($arm in @('baseline', 'fix')) {
            script:Set-RegressionFixtureRecord `
                -Fixture $fixture -Arm $arm -Index 0 -Outcome Skip
        }

        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Not -Throw

        foreach ($arm in @('baseline', 'fix')) {
            script:Set-RegressionFixtureRecord `
                -Fixture $fixture -Arm $arm -Index 1 -Outcome Fail `
                -StackFrame 'Microsoft.Maui.DeviceTests.LabelTests.SecondBehavior()'
        }

        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Throw '*no comparable passing sibling execution*'
    }

        It 'accepts reordered all-pass duplicate identity groups without inventing case IDs' {
            $fixture = script:New-BindingFixture -WithFix
            $duplicateA = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'truncated(label: Label { ··· })'
                Outcome = 'Pass'; Frame = ''
            }
            $duplicateB = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'truncated(label: Label { ··· })'
                Outcome = 'Pass'; Frame = ''
            }
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm baseline -Records @($duplicateA, $duplicateB)
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm fix -Records @($duplicateB, $duplicateA)

            { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } | Should -Not -Throw
        }

        It 'rejects failed or skipped candidates in an all-pass duplicate identity group' {
            foreach ($outcome in @('Fail', 'Skip')) {
                $fixture = script:New-BindingFixture -WithFix
                $baseline = @(
                    [pscustomobject]@{
                        Method = 'MemberDataCase'; Name = 'collision'
                        Outcome = 'Pass'; Frame = ''
                    },
                    [pscustomobject]@{
                        Method = 'MemberDataCase'; Name = 'collision'
                        Outcome = 'Pass'; Frame = ''
                    })
                $fix = @(
                    [pscustomobject]@{
                        Method = 'MemberDataCase'; Name = 'collision'
                        Outcome = 'Pass'; Frame = ''
                    },
                    [pscustomobject]@{
                        Method = 'MemberDataCase'; Name = 'collision'
                        Outcome = $outcome
                        Frame = 'Microsoft.Maui.DeviceTests.LabelTests.OtherAssertion()'
                    })
                script:Set-RegressionFixtureRecords `
                    -Fixture $fixture -Arm baseline -Records $baseline
                script:Set-RegressionFixtureRecords `
                    -Fixture $fixture -Arm fix -Records $fix

                { Assert-ReplicationRegressionEvidence `
                        -ArtifactRoot $fixture.Artifacts `
                        -ExpectedBaselineSha $script:BaseSha `
                        -ExpectedPlatform 'android' } |
                    Should -Throw "*incompatible outcome*Pass -> $outcome*"
            }
        }

        It 'rejects a self-consistent duplicate-state tamper with every digest updated' {
            $fixture = script:New-BindingFixture -WithFix
            $pass = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Pass'; Frame = ''
            }
            $fail = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Fail'
                Frame = 'Microsoft.Maui.DeviceTests.LabelTests.SameAssertion()'
            }
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm baseline -Records @($pass, $pass)
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm fix -Records @($pass, $fail)

            $binding = Get-Content -LiteralPath $fixture.BindingPath -Raw |
                ConvertFrom-Json -Depth 20 -AsHashtable
            foreach ($path in @(
                'regression/baseline/strict-test-evidence.json',
                'regression/fix/strict-test-evidence.json',
                'regression/regression-evidence.json'
            )) {
                $binding.evidence[$path] = (
                    Get-FileHash -LiteralPath (
                        Join-Path $fixture.Artifacts $path
                    ) -Algorithm SHA256
                ).Hash.ToLowerInvariant()
            }
            $binding.digest = Get-ReplicationBindingDigest -Binding $binding
            $binding | ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM

            { script:Invoke-BindingCheck -Fixture $fixture } |
                Should -Throw '*incompatible outcome*Pass -> Fail*'
        }

        It 'rejects duplicate identity count drift' {
            $fixture = script:New-BindingFixture -WithFix
            $record = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Pass'; Frame = ''
            }
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm baseline -Records @($record, $record)
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm fix -Records @($record)

            { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } |
                Should -Throw '*same exact test identity multiset*'
        }

        It 'rejects method and display casing drift without merging identity groups' {
            foreach ($field in @('Method', 'Name')) {
                $fixture = script:New-BindingFixture -WithFix
                $first = [pscustomobject]@{
                    Method = 'MemberCase'; Name = 'Collision'; Outcome = 'Pass'; Frame = ''
                }
                $second = [pscustomobject]@{
                    Method = 'MemberCase'; Name = 'Collision'; Outcome = 'Pass'; Frame = ''
                }
                $second.$field = $second.$field.ToLowerInvariant()
                script:Set-RegressionFixtureRecords `
                    -Fixture $fixture -Arm baseline -Records @($first, $second)
                script:Set-RegressionFixtureRecords `
                    -Fixture $fixture -Arm fix -Records @($first, $first)

                { Assert-ReplicationRegressionEvidence `
                        -ArtifactRoot $fixture.Artifacts `
                        -ExpectedBaselineSha $script:BaseSha `
                        -ExpectedPlatform 'android' } |
                    Should -Throw '*same exact test identities*'
            }
        }

        It 'rejects JSON identity casing changes against unchanged retained XML' {
            foreach ($field in @('method', 'displayName')) {
                $fixture = script:New-BindingFixture -WithFix
                $first = [pscustomobject]@{
                    Method = 'MemberCase'; Name = 'Collision'; Outcome = 'Pass'; Frame = ''
                }
                $second = [pscustomobject]@{
                    Method = 'membercase'; Name = 'collision'; Outcome = 'Pass'; Frame = ''
                }
                script:Set-RegressionFixtureRecords `
                    -Fixture $fixture -Arm baseline -Records @($first, $second)
                $path = Join-Path $fixture.Artifacts 'regression/baseline/strict-test-evidence.json'
                $document = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -Depth 20
                $document.records[1].$field = $document.records[0].$field
                $document | ConvertTo-Json -Depth 20 |
                    Set-Content -LiteralPath $path -Encoding utf8NoBOM

                { Read-ReplicationRegressionRunEvidence `
                        -Path $path -ExpectedPlatform 'android' `
                        -ExpectedProject 'Controls.DeviceTests' -ExpectedCategory 'Label' `
                        -ExpectedClass 'Microsoft.Maui.DeviceTests.LabelTests' } |
                    Should -Throw '*records do not match retained XML*'
            }
        }

        It 'rejects offsetting mixed duplicate states under every possible pairing' {
            $fixture = script:New-BindingFixture -WithFix
            $pass = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Pass'; Frame = ''
            }
            $fail = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Fail'
                Frame = 'Microsoft.Maui.DeviceTests.LabelTests.SameAssertion()'
            }
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm baseline -Records @($pass, $fail)
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm fix -Records @($fail, $pass)

            { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } |
                Should -Throw '*incompatible outcome*Pass -> Fail*'
        }

        It 'accepts only all-pass candidates when duplicate baselines have different failure signatures' {
            $fixture = script:New-BindingFixture -WithFix
            $firstFailure = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'; Outcome = 'Fail'
                Frame = 'Microsoft.Maui.DeviceTests.LabelTests.FirstAssertion()'
            }
            $secondFailure = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'; Outcome = 'Fail'
                Frame = 'Microsoft.Maui.DeviceTests.LabelTests.SecondAssertion()'
            }
            $pass = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Pass'; Frame = ''
            }
            $executedSibling = [pscustomobject]@{
                Method = 'ExistingBehavior'; Name = 'ExistingBehavior'
                Outcome = 'Pass'; Frame = ''
            }
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm baseline `
                -Records @($firstFailure, $secondFailure, $executedSibling)
            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm fix `
                -Records @($firstFailure, $pass, $executedSibling)
            { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } |
                Should -Throw '*incompatible outcome*Fail -> Fail*'

            script:Set-RegressionFixtureRecords `
                -Fixture $fixture -Arm fix `
                -Records @($pass, $pass, $executedSibling)
            { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } | Should -Not -Throw
        }

        It 'does not count stable duplicate skips as executed sibling coverage' {
            $fixture = script:New-BindingFixture -WithFix
            $skip = [pscustomobject]@{
                Method = 'MemberDataCase'; Name = 'collision'
                Outcome = 'Skip'; Frame = ''
            }
            $failure = [pscustomobject]@{
                Method = 'ExistingFailure'; Name = 'ExistingFailure'
                Outcome = 'Fail'
                Frame = 'Microsoft.Maui.DeviceTests.LabelTests.ExistingFailure()'
            }
            foreach ($arm in @('baseline', 'fix')) {
                script:Set-RegressionFixtureRecords `
                    -Fixture $fixture -Arm $arm -Records @($skip, $skip, $failure)
            }

            { Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform 'android' } |
                Should -Throw '*no comparable passing sibling execution*'
        }

    It 'rejects malformed regression JSON with duplicate properties' {
        $fixture = script:New-BindingFixture -WithFix
        $comparisonPath = Join-Path $fixture.Artifacts 'regression/regression-evidence.json'
        $raw = Get-Content -LiteralPath $comparisonPath -Raw
        $raw = $raw -replace (
            '"schemaVersion"\s*:\s*1\s*,',
            '"schemaVersion": 1, "schemaVersion": 1,')
        Set-Content -LiteralPath $comparisonPath -Value $raw -Encoding utf8NoBOM

        { Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform 'android' } |
            Should -Throw '*duplicate JSON property*'
    }

    It 'binds the typed selector identity and the trusted counts' {
        $fixture = script:New-BindingFixture
        $fixture.Binding.selector.variant | Should -Be 'device-category-only'
        $fixture.Binding.selector.testType | Should -Be 'DeviceTest'
        $fixture.Binding.selector.testClassName | Should -Be 'Microsoft.Maui.DeviceTests.Issue12345'
        $fixture.Binding.selector.testMethodName | Should -Be 'ReproducesReportedFailure'
        $fixture.Binding.selector.testProject | Should -Be 'Controls.DeviceTests'
        $fixture.Binding.selector.platform | Should -Be 'android'
        $fixture.Binding.selector.discoveredCount | Should -Be '1'
        $fixture.Binding.selector.executedCount | Should -Be '1'
    }

    It 'binds the trusted verifier, runner, and validator identities' {
        $fixture = script:New-BindingFixture
        foreach ($name in @(
            'scripts/Replicate-Issue.ps1',
            'scripts/shared/Invoke-ReplicationTestVerification.ps1',
            'scripts/shared/Validate-ReplicationCandidate.ps1',
            'scripts/templates/RunReplicationAppiumPlan.cs',
            'skills/run-device-tests/scripts/Run-DeviceTests.ps1')) {
            [string]$fixture.Binding.trustedScripts[$name] | Should -Match '^[0-9a-f]{64}$'
        }
    }

    It 'binds every evidence file, present or absent' {
        $fixture = script:New-BindingFixture
        foreach ($name in (Get-ReplicationBindingEvidenceNames)) {
            $fixture.Binding.evidence.Contains($name) | Should -BeTrue -Because "$name must be named either way"
        }
        [string]$fixture.Binding.evidence['evidence/repro.mp4'] | Should -Match '^[0-9a-f]{64}$'
        $fixture.Binding.evidence['evidence/thumbnail.png'] | Should -BeNullOrEmpty
    }

    It 'is deterministic and independent of the serializer' {
        # Two bindings over identical inputs must agree byte for byte, or the
        # digest changes when nothing did.
        $first = script:New-BindingFixture
        $second = script:New-BindingFixture
        $first.Binding.digest | Should -Be $second.Binding.digest
    }

    It 'refuses inputs that are not immutable identities' {
        $fixture = script:New-BindingFixture
        $common = @{
            IssueNumber = 12345
            Platform = 'android'
            ArtifactRoot = $fixture.Artifacts
            TrustedSourceVersion = $script:SourceVersion
            TrustedTreeHash = $script:TreeHash
            PipelineSha256 = $script:PipelineSha
            ReplicationBaseSha = $script:BaseSha
            ExecutionHeadSha = $script:HeadSha
            TrustedScripts = (script:New-TrustedScriptIdentities)
        }
        { New-ReplicationCertificationBinding @common -TrustedSourceVersion 'HEAD' } |
            Should -Throw '*lowercase 40-character commit*'
        { New-ReplicationCertificationBinding @common -TrustedTreeHash 'not-a-hash' } |
            Should -Throw '*SHA-256 trusted tree hash*'
        { New-ReplicationCertificationBinding @common -TrustedScripts ([ordered]@{ 'x' = 'nope' }) } |
            Should -Throw '*SHA-256 hash for trusted script*'
        { New-ReplicationCertificationBinding @common -IssueNumber 0 } |
            Should -Throw '*positive issue number*'
    }
}

Describe 'Re-checking a certification binding' {
    It 'accepts a binding whose inputs are all still in place' {
        $fixture = script:New-BindingFixture -WithFix
        $result = script:Invoke-BindingCheck -Fixture $fixture
        $result.Digest | Should -Be $fixture.Binding.digest
    }

    It 'refuses an artifact mutated after validation' {
        # The specific window this exists for: the clean job validated, the
        # publisher downloaded, and something changed in between.
        $fixture = script:New-BindingFixture -WithFix
        Add-Content -LiteralPath (Join-Path $fixture.Artifacts 'test.patch') -Value '+extra'
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*evidence.test.patch*'
    }

    It 'refuses retained regression XML mutated after validation' {
        $fixture = script:New-BindingFixture -WithFix
        Add-Content -LiteralPath (
            Join-Path $fixture.Artifacts 'regression/fix/source-result-1.xml'
        ) -Value '<!-- changed -->'
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*source result digest does not match*'
    }

    It 'reparses retained XML when every attacker-controlled digest is updated' {
        $fixture = script:New-BindingFixture -WithFix
        $xmlPath = Join-Path $fixture.Artifacts 'regression/fix/source-result-1.xml'
        [xml]$xml = Get-Content -LiteralPath $xmlPath -Raw
        $test = @($xml.SelectNodes('//test'))[0]
        $test.SetAttribute('result', 'Fail')
        $failure = $xml.CreateElement('failure')
        $failure.SetAttribute('exception-type', 'Xunit.Sdk.EqualException')
        $message = $xml.CreateElement('message')
        $message.InnerText = 'Expected: Html'
        $stack = $xml.CreateElement('stack-trace')
        $stack.InnerText = (
            'at Microsoft.Maui.DeviceTests.LabelTests.ExistingBehavior() ' +
            'in /agent/LabelTests.cs:line 42')
        $null = $failure.AppendChild($message)
        $null = $failure.AppendChild($stack)
        $null = $test.AppendChild($failure)
        $assembly = $xml.SelectSingleNode('/assemblies/assembly')
        $assembly.SetAttribute('passed', '1')
        $assembly.SetAttribute('failed', '1')
        $xml.Save($xmlPath)

        $strictPath = Join-Path $fixture.Artifacts 'regression/fix/strict-test-evidence.json'
        $strict = Get-Content -LiteralPath $strictPath -Raw | ConvertFrom-Json
        $strict.passed = 1
        $strict.failed = 1
        $strict.resultFiles[0].sha256 = (
            Get-FileHash -LiteralPath $xmlPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $strict | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $strictPath -Encoding utf8NoBOM

        $comparisonPath = Join-Path $fixture.Artifacts 'regression/regression-evidence.json'
        $comparison = Get-Content -LiteralPath $comparisonPath -Raw | ConvertFrom-Json
        $comparison.fixResultSha256 = (
            Get-FileHash -LiteralPath $strictPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $comparison | ConvertTo-Json -Depth 8 |
            Set-Content -LiteralPath $comparisonPath -Encoding utf8NoBOM

        $binding = Get-Content -LiteralPath $fixture.BindingPath -Raw |
            ConvertFrom-Json -Depth 20 -AsHashtable
        $binding.evidence['regression/fix/strict-test-evidence.json'] = (
            Get-FileHash -LiteralPath $strictPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $binding.evidence['regression/regression-evidence.json'] = (
            Get-FileHash -LiteralPath $comparisonPath -Algorithm SHA256).Hash.ToLowerInvariant()
        $binding.digest = Get-ReplicationBindingDigest -Binding $binding
        $binding | ConvertTo-Json -Depth 20 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM

        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*records do not match retained XML*'
    }

    It 'refuses a mutated fix patch' {
        $fixture = script:New-BindingFixture -WithFix
        Set-Content -LiteralPath (Join-Path $fixture.Artifacts 'fix.patch') `
            -Value 'diff --git a/other b/other' -Encoding utf8NoBOM
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*fix.patch*'
    }

    It 'refuses mutated control-arm evidence' -TestCases @(
        @{ Name = 'negative control'; Path = 'verification/negative-control-result.json' }
        @{ Name = 'fix control'; Path = 'verification/fix-control-result.json' }
        @{ Name = 'restoration'; Path = 'verification/restoration-result.json' }
        @{ Name = 'fix scope'; Path = 'fix-scope-baseline.json' }
    ) {
        param($Name, $Path)

        $fixture = script:New-BindingFixture -WithFix
        Add-Content -LiteralPath (Join-Path $fixture.Artifacts $Path) -Value 'tampered'
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw "*evidence.$Path*" -Because "$Name determines whether the fix may publish"
    }

    It 'refuses a mutated media file' {
        $fixture = script:New-BindingFixture
        [IO.File]::WriteAllBytes((Join-Path $fixture.Artifacts 'evidence/repro.mp4'), [byte[]](9..99))
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*evidence/repro.mp4*'
    }

    It 'refuses an evidence file that was removed' {
        $fixture = script:New-BindingFixture
        Remove-Item -LiteralPath (Join-Path $fixture.Artifacts 'evidence/preview.gif') -Force
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*evidence/preview.gif*'
    }

    It 'refuses an evidence file that was added where there was none' {
        $fixture = script:New-BindingFixture
        [IO.File]::WriteAllBytes((Join-Path $fixture.Artifacts 'evidence/thumbnail.png'), [byte[]](1..16))
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*evidence/thumbnail.png*'
    }

    It 'refuses a different pipeline revision' {
        $fixture = script:New-BindingFixture
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ TrustedSourceVersion = ('f' * 40) } } |
            Should -Throw '*trustedSourceVersion*'
    }

    It 'refuses a different replication base commit' {
        $fixture = script:New-BindingFixture
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ ReplicationBaseSha = ('9' * 40) } } |
            Should -Throw '*replicationBaseSha*'
    }

    It 'refuses a different trusted tree hash' {
        $fixture = script:New-BindingFixture
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ TrustedTreeHash = ('0' * 64) } } |
            Should -Throw '*trustedTreeHash*'
    }

    It 'refuses a different pipeline definition' {
        $fixture = script:New-BindingFixture
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ PipelineSha256 = ('7' * 64) } } |
            Should -Throw '*pipelineSha256*'
    }

    It 'refuses a trusted runner that was swapped underneath the run' {
        $fixture = script:New-BindingFixture
        $swapped = script:New-TrustedScriptIdentities
        $swapped['scripts/templates/RunReplicationAppiumPlan.cs'] = ('8' * 64)
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ TrustedScripts = $swapped } } |
            Should -Throw '*RunReplicationAppiumPlan.cs*'
    }

    It 'refuses a trusted script set with a different membership' {
        $fixture = script:New-BindingFixture
        $extra = script:New-TrustedScriptIdentities
        $extra['scripts/shared/Something-New.ps1'] = ('5' * 64)
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ TrustedScripts = $extra } } |
            Should -Throw '*trustedScripts (set)*'
    }

    It 'refuses a selector that names a different test' {
        $fixture = script:New-BindingFixture
        $selector = script:New-SelectorContract
        $selector['method'] = 'SomeOtherTest'
        {
            script:Invoke-BindingCheck -Fixture $fixture -Override @{
                Selector = (Get-ReplicationBindingSelector -Selector $selector -TestType 'DeviceTest')
            }
        } | Should -Throw '*selector.testMethodName*'
    }

    It 'refuses a selector whose trusted counts disagree' {
        # A selection count is the difference between "one test failed" and
        # "the filter matched nothing and the run said so anyway".
        $fixture = script:New-BindingFixture
        $selector = script:New-SelectorContract
        $selector['executedCount'] = 0
        {
            script:Invoke-BindingCheck -Fixture $fixture -Override @{
                Selector = (Get-ReplicationBindingSelector -Selector $selector -TestType 'DeviceTest')
            }
        } | Should -Throw '*selector.executedCount*'
    }

    It 'refuses a selector whose variant changed' {
        $fixture = script:New-BindingFixture
        $selector = script:New-SelectorContract
        $selector['variant'] = 'ui-parameterized-fixture'
        {
            script:Invoke-BindingCheck -Fixture $fixture -Override @{
                Selector = (Get-ReplicationBindingSelector -Selector $selector -TestType 'DeviceTest')
            }
        } | Should -Throw '*selector.variant*'
    }

    It 'refuses a different issue or platform' {
        $fixture = script:New-BindingFixture
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ IssueNumber = 999 } } |
            Should -Throw '*issueNumber*'
        { script:Invoke-BindingCheck -Fixture $fixture -Override @{ Platform = 'ios' } } |
            Should -Throw '*platform*'
    }
}

Describe 'Reading a certification binding document' {
    It 'refuses a document whose digest was recomputed over edited contents' {
        $fixture = script:New-BindingFixture
        $document = Get-Content -LiteralPath $fixture.BindingPath -Raw | ConvertFrom-Json -Depth 12
        $document.executionHeadSha = ('7' * 40)
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM
        { Read-ReplicationCertificationBinding -Path $fixture.BindingPath } |
            Should -Throw '*digest does not cover its own contents*'
    }

    It 'refuses a document with a field nobody expects' {
        # A binding that quietly accepts new fields is a binding an author can
        # extend to mean whatever they want.
        $fixture = script:New-BindingFixture
        $document = Get-Content -LiteralPath $fixture.BindingPath -Raw | ConvertFrom-Json -Depth 12
        $document | Add-Member -NotePropertyName 'overrideEverything' -NotePropertyValue $true
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM
        { Read-ReplicationCertificationBinding -Path $fixture.BindingPath } |
            Should -Throw '*unexpected or missing fields*'
    }

    It 'refuses a document that dropped a field' {
        $fixture = script:New-BindingFixture
        $document = Get-Content -LiteralPath $fixture.BindingPath -Raw | ConvertFrom-Json -Depth 12
        $document.PSObject.Properties.Remove('executionHeadSha')
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM
        { Read-ReplicationCertificationBinding -Path $fixture.BindingPath } |
            Should -Throw '*unexpected or missing fields*'
    }

    It 'refuses a selector or evidence set with a different shape' {
        $fixture = script:New-BindingFixture
        $document = Get-Content -LiteralPath $fixture.BindingPath -Raw | ConvertFrom-Json -Depth 12
        $document.selector.PSObject.Properties.Remove('executedCount')
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM
        { Read-ReplicationCertificationBinding -Path $fixture.BindingPath } |
            Should -Throw '*selector has unexpected or missing fields*'

        $fixture = script:New-BindingFixture
        $document = Get-Content -LiteralPath $fixture.BindingPath -Raw | ConvertFrom-Json -Depth 12
        $document.evidence | Add-Member -NotePropertyName 'evidence/extra.png' -NotePropertyValue $null
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM
        { Read-ReplicationCertificationBinding -Path $fixture.BindingPath } |
            Should -Throw '*evidence has unexpected or missing entries*'
    }

    It 'refuses a malformed commit or hash' {
        $fixture = script:New-BindingFixture
        $document = Get-Content -LiteralPath $fixture.BindingPath -Raw | ConvertFrom-Json -Depth 12
        $document.replicationBaseSha = 'main'
        $document.digest = Get-ReplicationBindingDigest -Binding $document
        $document | ConvertTo-Json -Depth 12 |
            Set-Content -LiteralPath $fixture.BindingPath -Encoding utf8NoBOM
        { Read-ReplicationCertificationBinding -Path $fixture.BindingPath } |
            Should -Throw '*invalid commit for replicationBaseSha*'
    }

    It 'refuses a binding that is simply missing' {
        { Read-ReplicationCertificationBinding -Path (Join-Path $script:ScratchRoot 'absent.json') } |
            Should -Throw '*missing*'
    }

    It 'refuses a binding input replaced by a link' -Skip:([System.OperatingSystem]::IsWindows()) {
        $fixture = script:New-BindingFixture
        $decoy = Join-Path $fixture.Root 'decoy.patch'
        Set-Content -LiteralPath $decoy -Value 'diff' -Encoding utf8NoBOM
        $victim = Join-Path $fixture.Artifacts 'test.patch'
        Remove-Item -LiteralPath $victim -Force
        & ln -s $decoy $victim
        { script:Invoke-BindingCheck -Fixture $fixture } |
            Should -Throw '*not a link*'
    }
}

Describe 'Fixed Catalyst production companion binding' {
    It 'activates only for the singleton iOS GesturePlatformManager product path' {
        $path =
        'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs'
        $required = Get-ReplicationFixedCompanionRequirement `
            -Platform ios -FixPaths @($path)
        $required.Id | Should -BeExactly 'gesture-platform-manager-catalyst-v1'
        @($required.Methods).Count | Should -Be 2
        Get-ReplicationFixedCompanionRequirement `
            -Platform ios `
            -FixPaths @('src/Controls/src/Core/Label/Label.cs') |
            Should -BeNullOrEmpty
    }

    It 'rejects mixed scope containing the fixed GesturePlatformManager path' {
        {
            Get-ReplicationFixedCompanionRequirement `
                -Platform ios `
                -FixPaths @(
                'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs',
                'src/Controls/src/Core/Label/Label.cs')
        } | Should -Throw '*rejects mixed product-fix scope*'
    }

    It 'keeps Catalyst GPM fixes on their existing primary regression lane' {
        $path = 'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs'
        Get-ReplicationFixedCompanionRequirement -Platform catalyst -FixPaths @($path) |
            Should -BeNullOrEmpty
        Get-ReplicationFixedCompanionRequirement -Platform catalyst -FixPaths @(
            $path, 'src/Controls/src/Core/Label/Label.cs') | Should -BeNullOrEmpty
    }

    It 'rejects an iOS regression downgrade when validated fix paths are omitted' {
        $fixture = script:New-DowngradedGpmRegressionFixture
        {
            Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform ios `
                -ExpectedCategory Label `
                -ExpectedClass Microsoft.Maui.DeviceTests.LabelTests
        } | Should -Throw '*requires validated fix paths*'
    }

    It 'refuses to create an iOS fix binding without validated fix paths' {
        $fixture = script:New-DowngradedGpmRegressionFixture
        {
            New-ReplicationCertificationBinding `
                -IssueNumber 12345 -Platform ios -ArtifactRoot $fixture.Artifacts `
                -TrustedSourceVersion $script:SourceVersion `
                -TrustedTreeHash $script:TreeHash -PipelineSha256 $script:PipelineSha `
                -ReplicationBaseSha $script:BaseSha -ExecutionHeadSha $script:HeadSha `
                -TrustedScripts (script:New-TrustedScriptIdentities) `
                -Selector (Get-ReplicationBindingSelector `
                    -Selector (script:New-SelectorContract) -TestType DeviceTest) `
                -ExpectedRegressionCategory Label `
                -ExpectedRegressionClass Microsoft.Maui.DeviceTests.LabelTests
        } | Should -Throw '*requires validated fix paths*'
    }

    It 'accepts exact two-pass baseline and candidate Catalyst evidence' {
        $fixture = script:New-CatalystCompanionBindingFixture
        {
            Assert-ReplicationCertificationBinding `
                -Binding $fixture.BindingPath `
                -ArtifactRoot $fixture.Artifacts `
                -TrustedSourceVersion $script:SourceVersion `
                -ReplicationBaseSha $script:BaseSha `
                -IssueNumber 12345 -Platform ios `
                -ExpectedRegressionCategory Label `
                -ExpectedRegressionClass Microsoft.Maui.DeviceTests.LabelTests `
                -ExpectedFixPaths @(
                'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs') `
                -TrustedFixturePath $fixture.TrustedFixture
        } | Should -Not -Throw
    }

    It 'rejects a changed trusted Catalyst fixture' {
        $fixture = script:New-CatalystCompanionBindingFixture
        Add-Content -LiteralPath $fixture.TrustedFixture -Value '// tampered'
        {
            Assert-ReplicationCertificationBinding `
                -Binding $fixture.BindingPath `
                -ArtifactRoot $fixture.Artifacts `
                -TrustedSourceVersion $script:SourceVersion `
                -ReplicationBaseSha $script:BaseSha `
                -IssueNumber 12345 -Platform ios `
                -ExpectedRegressionCategory Label `
                -ExpectedRegressionClass Microsoft.Maui.DeviceTests.LabelTests `
                -ExpectedFixPaths @(
                'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs') `
                -TrustedFixturePath $fixture.TrustedFixture
        } | Should -Throw '*fixture digest*'
    }

    It 'rejects fixed Catalyst evidence when its raw XML is missing' {
        $fixture = script:New-CatalystCompanionBindingFixture
        Remove-Item -LiteralPath (Join-Path $fixture.Artifacts (
                'regression/catalyst-fix/source-result-1.xml')) -Force

        {
            Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform ios `
                -ExpectedCategory Label `
                -ExpectedClass Microsoft.Maui.DeviceTests.LabelTests `
                -ExpectedFixPaths @(
                'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs') `
                -TrustedFixturePath $fixture.TrustedFixture
        } | Should -Throw '*source result digest does not match*'
    }

    It 'rejects a skipped fixed Catalyst baseline' {
        $fixture = script:New-CatalystCompanionBindingFixture
        $strictPath = Join-Path $fixture.Artifacts (
            'regression/catalyst-baseline/strict-test-evidence.json')
        $strict = Get-Content -LiteralPath $strictPath -Raw |
            ConvertFrom-Json -Depth 20
        $strict.passed = 1
        $strict.skipped = 1
        $strict.records[0].outcome = 'Skip'
        $strict | ConvertTo-Json -Depth 20 |
            Set-Content -LiteralPath $strictPath -Encoding utf8NoBOM

        {
            Assert-ReplicationRegressionEvidence `
                -ArtifactRoot $fixture.Artifacts `
                -ExpectedBaselineSha $script:BaseSha `
                -ExpectedPlatform ios `
                -ExpectedCategory Label `
                -ExpectedClass Microsoft.Maui.DeviceTests.LabelTests `
                -ExpectedFixPaths @(
                'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs') `
                -TrustedFixturePath $fixture.TrustedFixture
        } | Should -Throw
    }

    It 'rejects missing duplicate and reordered fixed Catalyst methods' {
        foreach ($methods in @(
                @('SecondaryToBothCreatesNativeTap'),
                @('SecondaryToBothCreatesNativeTap', 'SecondaryToBothCreatesNativeTap'),
                @('SecondaryToPrimaryCreatesNativeTap', 'SecondaryToBothCreatesNativeTap')
            )) {
            $fixture = script:New-CatalystCompanionBindingFixture
            $comparisonPath = Join-Path $fixture.Artifacts (
                'regression/regression-evidence.json')
            $comparison = Get-Content -LiteralPath $comparisonPath -Raw |
                ConvertFrom-Json -Depth 20
            $comparison.companion.methods = $methods
            $comparison | ConvertTo-Json -Depth 20 |
                Set-Content -LiteralPath $comparisonPath -Encoding utf8NoBOM

            {
                Assert-ReplicationRegressionEvidence `
                    -ArtifactRoot $fixture.Artifacts `
                    -ExpectedBaselineSha $script:BaseSha `
                    -ExpectedPlatform ios `
                    -ExpectedCategory Label `
                    -ExpectedClass Microsoft.Maui.DeviceTests.LabelTests `
                    -ExpectedFixPaths @(
                    'src/Controls/src/Core/Platform/GestureManager/GesturePlatformManager.iOS.cs') `
                    -TrustedFixturePath $fixture.TrustedFixture
            } | Should -Throw '*missing, reordered, or unexpected methods*'
        }
    }
}
