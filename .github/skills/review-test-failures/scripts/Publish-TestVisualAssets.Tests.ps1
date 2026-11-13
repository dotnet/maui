#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Publish-TestVisualAssets.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Get-SafeAssetSlug',
            'Invoke-GhApiJson',
            'Test-PngFile',
            'Test-AzDoAttachmentUrl',
            'Get-SnapshotRoot',
            'Get-ValidatedSnapshotPathHint',
            'Get-SnapshotCandidatePaths',
            'Select-BaselineCandidate',
            'Get-VisualEvidenceDedupKey',
            'Invoke-DownloadFile',
            'Get-AssetBranchRef',
            'Get-ValidatedAssetBranchState',
            'Initialize-AssetBranch',
            'Publish-GitAssets',
            'Remove-VisualDownloadDirectory'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Visual asset input validation' {
    It 'accepts only the exact public AzDO attachment URL for the expected result' {
        Test-AzDoAttachmentUrl `
            -Url 'https://dev.azure.com/dnceng-public/public/_apis/test/Runs/12/Results/34/Attachments/56' `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeTrue

        Test-AzDoAttachmentUrl `
            -Url 'https://evil.example/dnceng-public/public/_apis/test/Runs/12/Results/34/Attachments/56' `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeFalse

        Test-AzDoAttachmentUrl `
            -Url 'https://dev.azure.com/dnceng-public/public/_apis/test/Runs/12/Results/99/Attachments/56' `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeFalse
    }

    It 'rejects the expected attachment path when the URL is adorned with userinfo, a port, a query, or a fragment' {
        # The allowlist gate must reject anything beyond the bare https://dev.azure.com/<path> form
        # even when the path itself matches, so a crafted attachment URL cannot smuggle credentials,
        # redirect to a non-default port, or tack on a query/fragment that changes what is fetched.
        $base = 'dev.azure.com/dnceng-public/public/_apis/test/Runs/12/Results/34/Attachments/56'
        foreach ($adorned in @(
            "https://attacker@$base",
            "https://$($base -replace 'dev\.azure\.com','dev.azure.com:8443')",
            "https://$base?download=true",
            "https://$base#frag")) {
            Test-AzDoAttachmentUrl `
                -Url $adorned `
                -RunId 12 `
                -ResultId 34 `
                -AttachmentId 56 | Should -BeFalse -Because "adorned URL '$adorned' must not pass the allowlist"
        }

        # An explicit default port (:443) is still the canonical endpoint and must remain accepted.
        Test-AzDoAttachmentUrl `
            -Url "https://dev.azure.com:443/dnceng-public/public/_apis/test/Runs/12/Results/34/Attachments/56" `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeTrue
    }

    It 'validates the PNG signature and size bound without decoding untrusted image data' {
        $valid = Join-Path $TestDrive 'valid.png'
        [System.IO.File]::WriteAllBytes(
            $valid,
            [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAF/gL+XyO8WQAAAABJRU5ErkJggg=='))
        Test-PngFile -Path $valid -MaximumBytes 1024 | Should -BeTrue

        $invalid = Join-Path $TestDrive 'invalid.png'
        [System.IO.File]::WriteAllText($invalid, '<html>not an image</html>')
        Test-PngFile -Path $invalid -MaximumBytes 1024 | Should -BeFalse
        Test-PngFile -Path $valid -MaximumBytes 8 | Should -BeFalse

        $oversizedDimensions = Join-Path $TestDrive 'oversized-dimensions.png'
        $bytes = [System.IO.File]::ReadAllBytes($valid)
        $bytes[16] = 0x00
        $bytes[17] = 0x01
        $bytes[18] = 0x00
        $bytes[19] = 0x01
        [System.IO.File]::WriteAllBytes($oversizedDimensions, $bytes)
        Test-PngFile -Path $oversizedDimensions -MaximumBytes 1024 | Should -BeFalse
    }

    It 'normalizes untrusted names into bounded asset slugs' {
        Get-SafeAssetSlug -Value '../My <Snapshot> Name?!' | Should -Be 'my-snapshot-name'
        (Get-SafeAssetSlug -Value ('A' * 200)).Length | Should -BeLessOrEqual 72
    }
}

Describe 'Snapshot baseline candidates' {
    It 'puts the runtime environment and trusted path hint before fallback directories' {
        $root = Join-Path $TestDrive 'repo'
        $snapshotRoot = Join-Path $root 'src/Controls/tests/TestCases.iOS.Tests/snapshots'
        New-Item -ItemType Directory -Force -Path (Join-Path $snapshotRoot 'ios') | Out-Null
        New-Item -ItemType Directory -Force -Path (Join-Path $snapshotRoot 'ios-26') | Out-Null

        $paths = @(Get-SnapshotCandidatePaths `
            -Platform 'ios' `
            -SnapshotFileName 'Sample.png' `
            -EnvironmentName 'ios-26' `
            -BaselinePathHint 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/Sample.png' `
            -RepositoryRoot $root)

        $paths[0] | Should -Be 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/Sample.png'
        $paths | Should -Contain 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios/Sample.png'
        $paths.Count | Should -Be 2
    }

    It 'rejects unsafe filenames and path hints' {
        @(Get-SnapshotCandidatePaths `
            -Platform 'ios' `
            -SnapshotFileName '../payload.png' `
            -EnvironmentName 'ios' `
            -BaselinePathHint '../../payload.png' `
            -RepositoryRoot $TestDrive).Count | Should -Be 0
    }

    It 'does not substitute another environment when a preferred path was unavailable' {
        $selection = Select-BaselineCandidate `
            -PreferredPath 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/Sample.png' `
            -CandidateFiles @(
                [pscustomobject]@{
                    repositoryPath = 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios/Sample.png'
                    localPath = '/tmp/ios/Sample.png'
                }
            )

        $selection.localPath | Should -BeNullOrEmpty
        $selection.repositoryPath | Should -BeNullOrEmpty
        $selection.status | Should -Match 'preferred runtime environment'
    }

    It 'uses a sole candidate only when no preferred environment is known' {
        $selection = Select-BaselineCandidate `
            -PreferredPath $null `
            -CandidateFiles @(
                [pscustomobject]@{
                    repositoryPath = 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios/Sample.png'
                    localPath = '/tmp/ios/Sample.png'
                }
            )

        $selection.repositoryPath | Should -Be 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios/Sample.png'
    }

    It 'validates a missing-baseline repository path hint for exact attribution' {
        Get-ValidatedSnapshotPathHint `
            -Platform 'android' `
            -SnapshotFileName 'Sample.png' `
            -PathHint 'src/Controls/tests/TestCases.Android.Tests/snapshots/android-notch-36/Sample.png' |
            Should -Be 'src/Controls/tests/TestCases.Android.Tests/snapshots/android-notch-36/Sample.png'

        Get-ValidatedSnapshotPathHint `
            -Platform 'android' `
            -SnapshotFileName 'Sample.png' `
            -PathHint '../other-environment/Sample.png' |
            Should -BeNullOrEmpty
    }
}

Describe 'Visual evidence deduplication' {
    It 'keeps same snapshot failures distinct across runtime environments' {
        $ios26 = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $ios16 = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-iphonex'
            buildId = 100
            runId = 201
            resultId = 301
        }

        Get-VisualEvidenceDedupKey -Evidence $ios26 |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $ios16)
    }

    It 'collapses retry attempts of the same snapshot in the same environment to one key' {
        $firstAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $retryAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
            buildId = 101
            runId = 205
            resultId = 999
        }

        Get-VisualEvidenceDedupKey -Evidence $firstAttempt |
            Should -Be (Get-VisualEvidenceDedupKey -Evidence $retryAttempt)
    }

    It 'keeps same snapshot failures distinct across platforms' {
        $ios = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
        }
        $android = [pscustomobject]@{
            platform = 'android'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
        }

        Get-VisualEvidenceDedupKey -Evidence $ios |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $android)
    }

    It 'keeps distinct legs separate when the environment is unresolved (multi-hint build)' {
        # The gatherer sets environmentName to null when a build exposes multiple environment
        # hints for one platform. Two distinct iOS legs failing the same snapshot must not
        # collapse onto "ios|name.png|" as if one were a retry of the other.
        $legA = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            buildId = 100
            runId = 200
            resultId = 300
        }
        $legB = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            buildId = 100
            runId = 201
            resultId = 301
        }

        Get-VisualEvidenceDedupKey -Evidence $legA |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $legB)
    }

    It 'collapses same-leg retry attempts when the environment is unresolved but the run name is stable' {
        # A retry re-runs the same pipeline job/leg and reuses its test-run name, so even though the
        # environment could not be resolved (multi-hint build) the two attempts share a stable leg
        # identity and must collapse to one key -- otherwise each retry consumes a MaxComparisons slot
        # and crowds out genuinely distinct failures.
        $firstAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-26)'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $retryAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-26)'
            buildId = 101
            runId = 205
            resultId = 999
        }

        Get-VisualEvidenceDedupKey -Evidence $firstAttempt |
            Should -Be (Get-VisualEvidenceDedupKey -Evidence $retryAttempt)
    }

    It 'keeps distinct legs separate when the environment is unresolved but the run names differ' {
        # Distinct legs (e.g. two iOS device queues) run under different pipeline jobs, so their
        # test-run names differ even when the environment cannot be resolved. They must stay separate.
        $legA = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-26)'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $legB = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-iphonex)'
            buildId = 100
            runId = 200
            resultId = 300
        }

        Get-VisualEvidenceDedupKey -Evidence $legA |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $legB)
    }
}

Describe 'Download budget enforcement' {
    It 'throws without attempting a network call when the publish deadline has already passed' {
        # F2: the aggregate publish budget must gate each download. A deadline in the past has to
        # fail fast -- before any HttpClient work -- so a stalled host cannot hold the Helix job
        # open past its ceiling. The guard sits ahead of the try/catch, so it surfaces the budget
        # message rather than a swallowed connection error.
        $destination = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
        {
            Invoke-DownloadFile `
                -Url 'https://dev.azure.com/should-not-be-contacted.png' `
                -Path $destination `
                -MaximumBytes 1048576 `
                -Deadline ((Get-Date).AddSeconds(-1))
        } | Should -Throw '*Publish budget exhausted*'

        Test-Path -LiteralPath $destination | Should -BeFalse
    }
}

Describe 'Git publication budget enforcement' {
    It 'throws before starting gh when the publish deadline has passed' {
        {
            Invoke-GhApiJson `
                -Method 'GET' `
                -Endpoint 'repos/dotnet/maui' `
                -Deadline ((Get-Date).AddSeconds(-1))
        } | Should -Throw '*Publish budget exhausted*'
    }

    It 'initializes a missing branch from an asset-only root instead of the default branch tree' {
        $script:refReadCount = 0
        Mock Get-AssetBranchRef {
            $script:refReadCount++
            if ($script:refReadCount -eq 1) {
                return $null
            }
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'root-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/blobs') {
                return [pscustomobject]@{ sha = 'marker-blob' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/trees') {
                return [pscustomobject]@{ sha = 'root-tree' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/commits') {
                return [pscustomobject]@{ sha = 'root-commit' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/refs') {
                return $null
            }
            if ($Method -eq 'GET' -and $Endpoint -like '*/git/commits/root-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'root-tree' } }
            }
            if ($Method -eq 'GET' -and $Endpoint -like '*/git/trees/root-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{
                            path = '.review-tests-assets'
                            mode = '100644'
                            type = 'blob'
                        }
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        $state = Initialize-AssetBranch `
            -Repository 'dotnet/maui' `
            -Branch 'review-tests-assets-v2'

        $state.ref.object.sha | Should -Be 'root-commit'
        Should -Invoke Invoke-GhApiJson -Times 1 -Exactly -ParameterFilter {
            $Method -eq 'POST' -and
            $Endpoint -eq 'repos/dotnet/maui/git/blobs' -and
            $Body.content -eq 'Generated by /review tests. Do not edit.' -and
            $Body.encoding -eq 'utf-8'
        }
        Should -Invoke Invoke-GhApiJson -Times 1 -Exactly -ParameterFilter {
            $Method -eq 'POST' -and
            $Endpoint -eq 'repos/dotnet/maui/git/trees' -and
            -not $Body.ContainsKey('base_tree') -and
            @($Body.tree).Count -eq 1 -and
            $Body.tree[0].path -eq '.review-tests-assets' -and
            $Body.tree[0].mode -eq '100644' -and
            $Body.tree[0].type -eq 'blob' -and
            $Body.tree[0].sha -eq 'marker-blob'
        }
        Should -Invoke Invoke-GhApiJson -Times 1 -Exactly -ParameterFilter {
            $Method -eq 'POST' -and
            $Endpoint -eq 'repos/dotnet/maui/git/commits' -and
            $Body.tree -eq 'root-tree' -and
            @($Body.parents).Count -eq 0
        }
        Should -Invoke Invoke-GhApiJson -Times 1 -Exactly -ParameterFilter {
            $Method -eq 'POST' -and
            $Endpoint -eq 'repos/dotnet/maui/git/refs' -and
            $Body.ref -eq 'refs/heads/review-tests-assets-v2' -and
            $Body.sha -eq 'root-commit'
        }
        Should -Invoke Invoke-GhApiJson -Times 0 -Exactly -ParameterFilter {
            $Method -eq 'GET' -and $Endpoint -eq 'repos/dotnet/maui'
        }
    }

    It 'uses an existing asset-only branch without creating Git objects' {
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'existing-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Method -eq 'GET' -and $Endpoint -like '*/git/commits/existing-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'asset-tree' } }
            }
            if ($Method -eq 'GET' -and $Endpoint -like '*/git/trees/asset-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{
                            path = '.review-tests-assets'
                            mode = '100644'
                            type = 'blob'
                        },
                        [pscustomobject]@{ path = 'pr-123'; type = 'tree' }
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        $state = Initialize-AssetBranch `
            -Repository 'dotnet/maui' `
            -Branch 'review-tests-assets-v2'

        $state.ref.object.sha | Should -Be 'existing-commit'
        Should -Invoke Invoke-GhApiJson -Times 0 -Exactly -ParameterFilter {
            $Method -eq 'POST'
        }
    }

    It 'rejects an existing branch that contains repository files' {
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'unsafe-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Endpoint -like '*/git/commits/unsafe-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'unsafe-tree' } }
            }
            if ($Endpoint -like '*/git/trees/unsafe-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{ path = '.github'; type = 'tree' }
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        {
            Initialize-AssetBranch `
                -Repository 'dotnet/maui' `
                -Branch 'review-tests-assets-v2'
        } | Should -Throw "*unexpected top-level entry(ies): .github*"
    }

    It 'tolerates arbitrary top-level files (a blob cannot carry a workflow)' {
        # A top-level blob (README.md, LICENSE, .gitattributes, or the .review-tests-assets marker)
        # can never contain a workflow, so it must not trip the guard -- otherwise a maintainer
        # adding a README to the long-lived asset branch would lock out screenshot publishing
        # repo-wide with no in-code recovery path.
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'blobs-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Endpoint -like '*/git/commits/blobs-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'blobs-tree' } }
            }
            if ($Endpoint -like '*/git/trees/blobs-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{ path = '.review-tests-assets'; type = 'blob'; mode = '100644' }
                        [pscustomobject]@{ path = 'README.md'; type = 'blob'; mode = '100644' }
                        [pscustomobject]@{ path = 'LICENSE'; type = 'blob'; mode = '100644' }
                        [pscustomobject]@{ path = 'pr-42'; type = 'tree' }
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        $state = Initialize-AssetBranch `
            -Repository 'dotnet/maui' `
            -Branch 'review-tests-assets-v2'
        $state.ref.object.sha | Should -Be 'blobs-commit'
    }

    It 'accepts an empty root tree (JSON [] maps to zero entries, not one <invalid>)' {
        # Refactor-safety pin: pwsh maps a JSON empty tree ([]) to @() -> zero elements, which must
        # pass validation. The distinction from a $null tree property (which @(...) turns into a
        # single $null element the predicate would flag) is subtle and lives on a dense multi-line
        # predicate, so lock the empty-tree pass-through against a future refactor of that line.
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'empty-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Endpoint -like '*/git/commits/empty-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'empty-tree' } }
            }
            if ($Endpoint -like '*/git/trees/empty-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @()
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        $state = Initialize-AssetBranch `
            -Repository 'dotnet/maui' `
            -Branch 'review-tests-assets-v2'
        $state.ref.object.sha | Should -Be 'empty-commit'
    }

    It 'fails closed on a non-blob, non-pr-N entry (submodule gitlink cannot slip through)' {
        # A top-level submodule gitlink is a tree entry of type 'commit', not 'tree'/'blob'. It is
        # neither a plain file nor a pr-<number> directory, so the allowlist must reject it rather
        # than let an unexpected top-level path back in under a different entry type.
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'gitlink-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Endpoint -like '*/git/commits/gitlink-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'gitlink-tree' } }
            }
            if ($Endpoint -like '*/git/trees/gitlink-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{ path = '.github'; type = 'commit' }
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        {
            Initialize-AssetBranch `
                -Repository 'dotnet/maui' `
                -Branch 'review-tests-assets-v2'
        } | Should -Throw "*unexpected top-level entry(ies): .github*"
    }

    It 'fails closed on a malformed tree containing a null entry' {
        # A $null element in the tree array (malformed API response) is neither a blob nor a pr-N
        # directory, so it must be rejected (surfaced as <invalid>) rather than silently passing.
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'malformed-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Endpoint -like '*/git/commits/malformed-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'malformed-tree' } }
            }
            if ($Endpoint -like '*/git/trees/malformed-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{ path = 'pr-7'; type = 'tree' }
                        $null
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        {
            Initialize-AssetBranch `
                -Repository 'dotnet/maui' `
                -Branch 'review-tests-assets-v2'
        } | Should -Throw "*Asset branch 'review-tests-assets-v2' must contain only pr-<number> directories*"
    }

    It 'fails closed with an actionable message when the root tree is truncated' {
        # A truncated trees API response means the branch grew too many top-level entries; the guard
        # must fail closed (can't validate what it can't see) AND tell oncall how to recover
        # (prune pr-<n> dirs / rotate the branch) rather than emitting a dead-end error.
        Mock Get-AssetBranchRef {
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'truncated-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Endpoint -like '*/git/commits/truncated-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'truncated-tree' } }
            }
            if ($Endpoint -like '*/git/trees/truncated-tree') {
                return [pscustomobject]@{
                    truncated = $true
                    tree = @()
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        {
            Initialize-AssetBranch `
                -Repository 'dotnet/maui' `
                -Branch 'review-tests-assets-v2'
        } | Should -Throw "*truncated*prun*"
    }

    It 'accepts a concurrently-created branch only after validating its tree' {
        $script:refReadCount = 0
        Mock Get-AssetBranchRef {
            $script:refReadCount++
            if ($script:refReadCount -eq 1) {
                return $null
            }
            return [pscustomobject]@{
                object = [pscustomobject]@{ sha = 'winner-commit' }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/blobs') {
                return [pscustomobject]@{ sha = 'marker-blob' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/trees') {
                return [pscustomobject]@{ sha = 'loser-tree' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/commits') {
                return [pscustomobject]@{ sha = 'loser-commit' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/refs') {
                throw 'HTTP 422 Reference already exists'
            }
            if ($Method -eq 'GET' -and $Endpoint -like '*/git/commits/winner-commit') {
                return [pscustomobject]@{ tree = [pscustomobject]@{ sha = 'winner-tree' } }
            }
            if ($Method -eq 'GET' -and $Endpoint -like '*/git/trees/winner-tree') {
                return [pscustomobject]@{
                    truncated = $false
                    tree = @(
                        [pscustomobject]@{
                            path = '.review-tests-assets'
                            mode = '100644'
                            type = 'blob'
                        }
                    )
                }
            }
            throw "Unexpected API call: $Method $Endpoint"
        }

        $state = Initialize-AssetBranch `
            -Repository 'dotnet/maui' `
            -Branch 'review-tests-assets-v2'

        $state.ref.object.sha | Should -Be 'winner-commit'
        Should -Invoke Invoke-GhApiJson -Times 1 -Exactly -ParameterFilter {
            $Method -eq 'GET' -and $Endpoint -like '*/git/trees/winner-tree'
        }
    }

    It 'passes the shared deadline to every publication API call' {
        $assetPath = Join-Path $TestDrive 'asset.png'
        [System.IO.File]::WriteAllBytes($assetPath, [byte[]](1, 2, 3))
        $deadline = (Get-Date).AddMinutes(5)

        Mock Initialize-AssetBranch {
            return [pscustomobject]@{
                ref = [pscustomobject]@{
                    object = [pscustomobject]@{ sha = 'parent-sha' }
                }
                commit = [pscustomobject]@{
                    tree = [pscustomobject]@{ sha = 'parent-tree' }
                }
            }
        }
        Mock Invoke-GhApiJson {
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/blobs') {
                return [pscustomobject]@{ sha = 'blob-sha' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/trees') {
                return [pscustomobject]@{ sha = 'new-tree' }
            }
            if ($Method -eq 'POST' -and $Endpoint -like '*/git/commits') {
                return [pscustomobject]@{ sha = 'new-commit' }
            }
            return $null
        }

        $result = Publish-GitAssets `
            -Repository 'dotnet/maui' `
            -Branch 'review-tests-assets-v2' `
            -Assets @([pscustomobject]@{ localPath = $assetPath; assetPath = 'pr-123/asset.png' }) `
            -CommitMessage 'test' `
            -Deadline $deadline

        $result | Should -Be 'new-commit'
        Should -Invoke Invoke-GhApiJson -Times 4 -Exactly -ParameterFilter {
            $Deadline -eq $deadline
        }
    }
}

Describe 'Visual download cleanup' {
    It 'removes the per-run download directory recursively' {
        $directory = Join-Path $TestDrive 'downloads'
        New-Item -ItemType Directory -Path $directory | Out-Null
        Set-Content -LiteralPath (Join-Path $directory 'asset.png') -Value 'data'

        Remove-VisualDownloadDirectory -Path $directory

        Test-Path -LiteralPath $directory | Should -BeFalse
    }
}
