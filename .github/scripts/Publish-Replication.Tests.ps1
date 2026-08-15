#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    function Get-ScriptFunctionText {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$Name
        )

        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($Path, [ref]$tokens, [ref]$errors)
        if ($errors) {
            throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
        }

        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $Name
        }, $true)
        if (-not $function) {
            throw "Function '$Name' was not found in $Path"
        }
        return $function.Extent.Text
    }

    $evidenceScript = Join-Path $PSScriptRoot 'shared/Publish-ReplicationEvidence.ps1'
    $prScript = Join-Path $PSScriptRoot 'shared/Publish-ReplicationPR.ps1'
    $script:EvidenceSource = Get-Content -LiteralPath $evidenceScript -Raw
    $script:PrSource = Get-Content -LiteralPath $prScript -Raw

    foreach ($name in @(
        'Test-ReplicationBlobPrefix',
        'Get-ReplicationEvidenceContentType',
        'Test-ReplicationPublicBaseUrl',
        'ConvertTo-ReplicationUrlPath',
        'Get-ReplicationPublicBlobUrl'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $evidenceScript -Name $name)
    }
    foreach ($name in @(
        'ConvertTo-ReplicationSingleLine',
        'ConvertTo-ReplicationInlineCode',
        'Get-ReplicationPullRequestMarker',
        'New-ReplicationBranchName',
        'New-ReplicationPullRequestBody'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $prScript -Name $name)
    }
}

Describe 'Trusted replication evidence publishing' {
    It 'rejects traversal-like and malformed blob prefixes' {
        Test-ReplicationBlobPrefix -Value 'maui-copilot/issue-37440/android/123' | Should -BeTrue
        Test-ReplicationBlobPrefix -Value '../secrets' | Should -BeFalse
        Test-ReplicationBlobPrefix -Value 'valid//empty' | Should -BeFalse
        Test-ReplicationBlobPrefix -Value '/absolute' | Should -BeFalse
    }

    It 'maps only the public evidence allowlist content types' {
        Get-ReplicationEvidenceContentType -FileName 'repro.mp4' | Should -BeExactly 'video/mp4'
        Get-ReplicationEvidenceContentType -FileName 'preview.gif' | Should -BeExactly 'image/gif'
        Get-ReplicationEvidenceContentType -FileName 'thumbnail.png' | Should -BeExactly 'image/png'
        { Get-ReplicationEvidenceContentType -FileName 'device.log' } | Should -Throw
    }

    It 'constructs encoded anonymous blob URLs' {
        Get-ReplicationPublicBlobUrl `
            -BaseUrl 'https://evidence.blob.core.windows.net/public' `
            -Prefix 'issue-37440/android/build 12' `
            -FileName 'repro.mp4' |
            Should -BeExactly 'https://evidence.blob.core.windows.net/public/issue-37440/android/build%2012/repro.mp4'
    }

    It 'accepts only the configured HTTPS Azure Blob container endpoint' {
        Test-ReplicationPublicBaseUrl `
            -Value 'https://mauievidence.blob.core.windows.net/public' `
            -StorageAccount 'mauievidence' `
            -Container 'public' |
            Should -BeTrue
        Test-ReplicationPublicBaseUrl `
            -Value 'https://example.test/public' `
            -StorageAccount 'mauievidence' `
            -Container 'public' |
            Should -BeFalse
    }

    It 'uses federated Azure login and immutable uploads' {
        $script:EvidenceSource | Should -Match '--auth-mode login'
        $script:EvidenceSource | Should -Match '--overwrite false'
        $script:EvidenceSource | Should -Not -Match 'sas-token'
        $script:EvidenceSource | Should -Match 'actualFailureMessage'
        $script:EvidenceSource | Should -Match '\.Contains\('
    }

    It 'rejects evidence publication when the targeted failure message does not match' {
        $candidatePath = Join-Path $TestDrive 'validated-candidate.json'
        [ordered]@{
            validationPassed = $true
            issueNumber = 12345
            platform = 'android'
            baseSha = 'abc123'
            testType = 'unit'
            testFilter = 'Issue12345'
            expectedFailureSignature = 'Issue12345'
            actualFailureMessage = 'Xunit failure: expected red but was blue'
        } |
            ConvertTo-Json |
            Set-Content -LiteralPath $candidatePath -Encoding utf8NoBOM

        $output = @(& pwsh -NoProfile -File $evidenceScript `
            -ValidatedCandidatePath $candidatePath `
            -EvidenceDirectory $TestDrive `
            -StorageAccount account123 `
            -Container public `
            -BlobPrefix issue-12345/android/test `
            -DryRun 2>&1)

        $LASTEXITCODE | Should -Not -Be 0
        $output -join [Environment]::NewLine |
            Should -Match 'targeted failure message'
    }
}

Describe 'Trusted replication pull request publishing' {
    It 'creates stable issue/platform markers and bounded branch names' {
        Get-ReplicationPullRequestMarker -IssueNumber 37440 -Platform 'Android' |
            Should -BeExactly '<!-- MAUI_COPILOT_REPLICATION issue=37440 platform=android -->'
        New-ReplicationBranchName -IssueNumber 37440 -Platform 'Mac Catalyst' -BuildId '12/34' |
            Should -BeExactly 'copilot/reproduce-37440-mac-catalyst-12-34'
    }

    It 'strips Azure logging directives and newlines from single-line values' {
        ConvertTo-ReplicationSingleLine -Value "Title`n##vso[task.setvariable variable=X]bad" |
            Should -BeExactly 'Title bad'
    }

    It 'generates a draft body with video evidence, guard semantics, and safety statement' {
        $candidate = [pscustomobject]@{
            issueNumber = 37440
            platform = 'android'
            baseSha = 'abc123'
            testType = 'device'
            testFilter = 'Issue37440'
            expectedFailureSignature = 'Expected: 1; Actual: 0'
            actualFailureMessage = 'Xunit failure. Expected: 1; Actual: 0'
            reproductionSteps = @('Launch the scenario', 'Tap the action button')
        }
        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        $body = New-ReplicationPullRequestBody `
            -Candidate $candidate `
            -Evidence $evidence `
            -IssueTitle 'Reported behavior' `
            -TargetOwner 'dotnet' `
            -TargetRepository 'maui' `
            -BuildUrl 'https://dev.azure.com/example/build/1'

        $body | Should -Match 'AI-generated \*\*reproduction evidence\*\*'
        $body | Should -Match 'MAUI_REPRODUCTION_ISSUE=37440'
        $body | Should -Match '\[!\[Reproduction preview\]\(https://example.test/preview.gif\)\]\(https://example.test/repro.mp4\)'
        $body | Should -Match 'No linked repository, archive, binary, script, package, or arbitrary external file was downloaded'
        $body | Should -Match 'MAUI_COPILOT_REPLICATION issue=37440 platform=android'
    }

    It 'rejects publication when the expected signature is absent from the failure message' {
        $candidate = [pscustomobject]@{
            issueNumber = 12345
            platform = 'android'
            baseSha = 'abc123'
            testType = 'unit'
            testFilter = 'Issue12345'
            expectedFailureSignature = 'Issue12345'
            actualFailureMessage = 'Xunit failure: expected red but was blue'
            reproductionSteps = @('Run the test')
        }
        $evidence = [pscustomobject]@{
            blobs = [pscustomobject]@{
                preview = 'https://example.test/preview.gif'
                video = 'https://example.test/repro.mp4'
                manifest = 'https://example.test/evidence.json'
            }
        }

        {
            New-ReplicationPullRequestBody `
                -Candidate $candidate `
                -Evidence $evidence `
                -IssueTitle 'Reported behavior' `
                -TargetOwner dotnet `
                -TargetRepository maui `
                -BuildUrl ''
        } | Should -Throw '*targeted failure message*'
    }

    It 'uses an add-only staged diff and creates a draft PR from the fork' {
        $script:PrSource | Should -Match 'git diff --cached --name-status --diff-filter=ACDMRTUXB'
        $script:PrSource | Should -Match '\s--draft'
        $script:PrSource | Should -Match 'GH_TOKEN is required'
        $script:PrSource | Should -Not -Match 'https://[^"\s]*\$env:GH_TOKEN'
    }
}
