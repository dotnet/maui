BeforeAll {
    Import-Module (Join-Path $PSScriptRoot '../../eng/scripts/MauiReleaseConfiguration.psm1') -Force
    $tokens = $null
    $errors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile(
        (Join-Path $PSScriptRoot '../../eng/scripts/Sync-ReleasePublishing.ps1'), [ref]$tokens, [ref]$errors)
    if ($errors.Count) { throw ($errors | Out-String) }
    foreach ($function in @($ast.EndBlock.Statements | Where-Object { $_ -is [Management.Automation.Language.FunctionDefinitionAst] })) {
        Invoke-Expression $function.Extent.Text
    }
    $sweep = @($ast.EndBlock.Statements | Where-Object { $_ -is [Management.Automation.Language.TryStatementAst] })
    if ($sweep.Count -ne 1) { throw 'Expected one release publication sweep.' }
    $sweepBody = [scriptblock]::Create($sweep[0].Extent.Text)
}

Describe 'Completed release publication identities' {
    BeforeEach {
        $scope = 'production'
        $prefix = ''
        $SandboxPrefix = $null
        $PlanOnly = $false
        $RepositoryPath = $TestDrive
        $outputPath = $TestDrive
        $outputDirectory = $TestDrive
        $receiptPrefix = 'automation/release-lanes'
        $dispatchPrefix = 'automation/release-dispatch'
        $azureHeaders = $null
        $mirrorId = 'a8060d6f-4bda-43b2-b0a8-d68d713e502c'
        $roles = [pscustomobject]@{
            releaseBand = '11.0.1xx'; fixBranch = 'net11'; featureBranch = 'main'
        }
        $results = [Collections.Generic.List[object]]::new()
        $preparedResult = 'succeeded'
        $intentPackages = $false
        $automaticPolicy = $false
        $wrongCandidateBase = $false
        $signedSource = '2' * 40
        $publisherSource = 'c' * 40
        $plans = @{
            1 = [pscustomobject]@{ mergeSha = '1' * 40; candidateSha = '4' * 40; inflightSha = '5' * 40; servicingSha = '6' * 40 }
            2 = [pscustomobject]@{ mergeSha = '7' * 40; candidateSha = '8' * 40; inflightSha = '9' * 40; servicingSha = 'a' * 40 }
        }
        Mock Get-PublishingRefs {
            param($Filter)
            switch ($Filter) {
                'automation/release-lanes/11.0.1xx-sr*' {
                    [pscustomobject]@{ branch = 'automation/release-lanes/11.0.1xx-sr1'; sha = 'b' * 40 }
                    [pscustomobject]@{ branch = 'automation/release-lanes/11.0.1xx-sr2'; sha = 'd' * 40 }
                }
                'release/11.0.1xx-sr1' { [pscustomobject]@{ sha = '3' * 40 } }
                'release/11.0.1xx-sr2' { [pscustomobject]@{ sha = if ($automaticPolicy) { '7' * 40 } else { 'a' * 40 } } }
                'net11' { [pscustomobject]@{ sha = 'f' * 40 } }
                'automation/release-dispatch/11.0.1xx-sr1/prepare' { [pscustomobject]@{ sha = 'e' * 40 } }
            }
        }
        Mock Fetch-PublishingCommit {}
        Mock Invoke-PublishingGit { '' }
        Mock Get-MauiReleaseLaneReceipt {
            param($RepositoryPath, $Commit)
            $sr = if ($Commit -eq ('d' * 40)) { 2 } else { 1 }
            $record = [pscustomobject]@{
                schemaVersion = 1; scope = 'production'; releaseBand = '11.0.1xx'; sr = $sr
                fixBranch = 'net11'; featureBranch = 'main'; pullRequest = $sr; plan = $plans[$sr]
            }
            if ($Commit -eq ('e' * 40)) {
                $record | Add-Member dispatch ([pscustomobject]@{
                    definitionId = 1445; stage = 'prepare'; branch = 'main'; commit = $publisherSource
                    parameters = [pscustomobject]@{
                        ghOwner = 'dotnet'; ghRepo = 'maui'; commitHash = $signedSource
                        pushWorkloadSet = $true; pushNugetOrg = $true; pushPackages = $intentPackages
                    }
                })
            }
            $record
        }
        Mock New-MauiReleaseNextLanePlan { param($Sr) $plans[$Sr] }
        Mock New-MauiReleaseLaneReceipt { param($Record) if ($Record.sr -eq 1) { 'b' * 40 } else { 'd' * 40 } }
        Mock Get-MauiReleaseCompletionPlan { [pscustomobject]@{ restoreFixes = $false } }
        Mock Invoke-PublishingApi {
            param($Arguments)
            $sr = if (($Arguments -join ' ') -match 'sr2|pulls/2(?:$| )') { 2 } else { 1 }
            $branch = "release/11.0.1xx-sr$sr"
            $proof = [pscustomobject]@{
                number = $sr; merged = $true; merged_at = '2026-11-07T00:00:00Z'
                merged_by = [pscustomobject]@{ type = 'User' }
                merge_commit_sha = if ($sr -eq 1) { $signedSource } else { 'a' * 40 }
                head = [pscustomobject]@{
                    ref = "automation/servicing/11.0.1xx-sr$sr"; sha = $plans[$sr].servicingSha
                    repo = [pscustomobject]@{ full_name = 'dotnet/maui' }
                }
                base = [pscustomobject]@{ ref = $branch }
                body = "<!-- maui-release-servicing:production:11.0.1xx-sr$sr -->"
            }
            if ($automaticPolicy) {
                $proof.merge_commit_sha = $plans[$sr].mergeSha
                $proof.head.ref = "candidate/11.0.1xx-sr$sr"
                $proof.head.sha = $plans[$sr].candidateSha
                $proof.base.ref = if ($wrongCandidateBase) { 'main' } else { 'net11' }
                $proof.base | Add-Member repo ([pscustomobject]@{ full_name = 'dotnet/maui' })
                $proof.body = '<!-- maui-release-tracking:production:candidate -->'
            }
            $proof
        }
        Mock Get-MauiReleaseFile {
            param($RepositoryPath, $Commit)
            $patch = if ($Commit -in @(('a' * 40), ('7' * 40))) { 20 } else { 10 }
            $content = @"
<Project><PropertyGroup>
    <MajorVersion>11</MajorVersion>
    <MinorVersion>0</MinorVersion>
    <PatchVersion>$patch</PatchVersion>
    <SdkBandVersion>11.0.100</SdkBandVersion>
    <PreReleaseVersionIteration></PreReleaseVersionIteration>
    <PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>
    <StabilizePackageVersion Condition="'`$(StabilizePackageVersion)' == ''">true</StabilizePackageVersion>
    <IsServicingBuild Condition="'`$(PreReleaseVersionLabel)' == 'servicing'">true</IsServicingBuild>
    <DotNetFinalVersionKind Condition="'`$(StabilizePackageVersion)' == 'true'">release</DotNetFinalVersionKind>
</PropertyGroup></Project>
"@
            if ($automaticPolicy) {
                $expressions = & (Get-Module MauiReleaseConfiguration) {
                    Get-MauiBranchLabelCondition -Label ci.inflight
                    Get-MauiBranchLabelCondition -Label servicing
                    Get-MauiStabilizationDefault
                }
                $labels = "<PreReleaseVersionLabel>ci.main</PreReleaseVersionLabel>`n" +
                    "    <PreReleaseVersionLabel Condition=`"$($expressions[0])`">ci.inflight</PreReleaseVersionLabel>`n" +
                    "    <PreReleaseVersionLabel Condition=`"$($expressions[1])`">servicing</PreReleaseVersionLabel>"
                $content = $content.Replace('<PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>', $labels).Replace(
                    '">true</StabilizePackageVersion>', "`">$($expressions[2])</StabilizePackageVersion>")
            }
            $content
        }
        Mock Get-ReleaseAzureHeaders { @{ Accept = 'application/json' } }
        Mock Invoke-ReleaseAzure {
            param($Path)
            if ($Path -eq 'build/definitions/1095?api-version=7.1') {
                [pscustomobject]@{
                    repository = [pscustomobject]@{ id = $mirrorId; type = 'TfsGit' }
                    process = [pscustomobject]@{ yamlFilename = 'eng/pipelines/ci-official.yml' }
                }
            } else {
                [pscustomobject]@{ value = @([pscustomobject]@{
                    name = 'refs/heads/release/11.0.1xx-sr2'; objectId = if ($automaticPolicy) { '7' * 40 } else { 'a' * 40 }
                }) }
            }
        }
        Mock Get-MatchingReleaseBuild {
            param($Definition, $Branch, $Commit, $Parameters)
            if ($Definition -eq 1445) {
                if ($Commit -cne $publisherSource -or $Parameters.commitHash -cne $signedSource -or $Parameters.pushPackages) {
                    throw 'Publication lookup substituted an unverified identity.'
                }
                [pscustomobject]@{ status = 'completed'; result = $preparedResult }
            } elseif ($Branch -eq 'release/11.0.1xx-sr1') {
                if ($Commit -cne $signedSource) { throw 'Signed lookup substituted the later hotfix.' }
                [pscustomobject]@{ status = 'completed'; result = 'succeeded' }
            } else {
                [pscustomobject]@{ status = 'inProgress' }
            }
        }
        Mock Invoke-RestMethod { throw 'A regression fixture must never queue an actual release run.' }
    }

    It 'keeps a completed SR1 pinned and processes SR2 after the retained SR1 branch advances' {
        & $sweepBody
        $results.Count | Should -Be 2
        $results[0].sourceSha | Should -Be $signedSource
        $results[0].liveReleaseSha | Should -Be ('3' * 40)
        $results[0].status | Should -Be ready-for-publication-approval
        $results[1].status | Should -Be waiting-for-signed-build
        Should -Invoke Get-MatchingReleaseBuild -Times 1 -ParameterFilter { $Definition -eq 1445 -and $Commit -ceq $publisherSource }
        Should -Invoke Get-MatchingReleaseBuild -Times 1 -ParameterFilter { $Branch -ceq 'release/11.0.1xx-sr2' }
        Should -Invoke Invoke-RestMethod -Times 0
        Should -Invoke Invoke-PublishingGit -Times 1 -ParameterFilter { $Arguments[0] -ceq 'merge-base' -and $Arguments[2] -ceq $signedSource -and $Arguments[3] -ceq ('3' * 40) }
    }

    It 'does not claim completed historical preparation during an offline plan' {
        $PlanOnly = $true
        & $sweepBody
        $results.Count | Should -Be 2
        $results[0].status | Should -Be historical-release-awaiting-completed-dispatch-evidence
        $results[0].signedBuild | Should -BeNullOrEmpty
        Should -Invoke Get-ReleaseAzureHeaders -Times 0
        Should -Invoke Get-MatchingReleaseBuild -Times 0
    }

    It 'requires both successful builds rather than an intent or failed preparation' {
        $preparedResult = 'failed'
        { & $sweepBody } | Should -Throw '*no verified successful signing and preparation*'
        $results[0].status | Should -Be planned
        Should -Invoke Invoke-RestMethod -Times 0
    }

    It 'rejects a dispatch that requested customer publication' {
        $intentPackages = $true
        { & $sweepBody } | Should -Throw '*different release parameters*'
        Should -Invoke Get-MatchingReleaseBuild -Times 0
        Should -Invoke Invoke-RestMethod -Times 0
    }

    It 'uses the human-approved candidate cut directly when stabilization is branch-derived' {
        $automaticPolicy = $true
        $signedSource = '1' * 40
        foreach ($plan in $plans.Values) { $plan.servicingSha = $plan.mergeSha }
        & $sweepBody
        $results.Count | Should -Be 2
        $results[0].sourceSha | Should -Be $plans[1].mergeSha
        $results[0].status | Should -Be ready-for-publication-approval
        $results[1].sourceSha | Should -Be $plans[2].mergeSha
        $results[1].status | Should -Be waiting-for-signed-build
        $results[1].publicPublicationRequested | Should -BeFalse
        Should -Invoke Invoke-PublishingApi -Times 0 -ParameterFilter { $Arguments -contains 'state=closed' }
        Should -Invoke Get-MatchingReleaseBuild -Times 1 -ParameterFilter {
            $Branch -ceq 'release/11.0.1xx-sr2' -and $Commit -ceq ('7' * 40)
        }
        Should -Invoke Invoke-RestMethod -Times 0
    }

    It 'does not substitute automatic defaults for exact candidate approval' {
        $automaticPolicy = $true
        $wrongCandidateBase = $true
        foreach ($plan in $plans.Values) { $plan.servicingSha = $plan.mergeSha }
        { & $sweepBody } | Should -Throw '*exact verified human-approved candidate integration*'
        Should -Invoke Get-MatchingReleaseBuild -Times 0
        Should -Invoke Invoke-RestMethod -Times 0
    }
}
