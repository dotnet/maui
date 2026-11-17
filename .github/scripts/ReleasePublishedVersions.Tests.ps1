BeforeAll {
    $publicationPath = Join-Path $PSScriptRoot '../../eng/scripts/Sync-ReleasePublishedVersions.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [Management.Automation.Language.Parser]::ParseFile(
        $publicationPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors.Count) { throw ($parseErrors | Out-String) }
    $fetch = $ast.Find({
        param($node)
        $node -is [Management.Automation.Language.FunctionDefinitionAst] -and
        $node.Name -eq 'Invoke-PublicationFetch'
    }, $true)
    if (!$fetch) { throw 'Publication fetch helper was not found.' }
    Invoke-Expression $fetch.Extent.Text
    function Invoke-PublicationFixtureGit {
        param([string]$Repository, [string[]]$Arguments)
        $output = @(& git -C $Repository @Arguments 2>&1)
        if ($LASTEXITCODE -ne 0) { throw "Publication fixture Git failed: $($output -join "`n")" }
        ($output -join "`n").Trim()
    }
}

Describe 'Published-version ancestry preparation' {
    It 'verifies a valid servicing tag more than 32 commits behind a shallow checkout' {
        $source = Join-Path $TestDrive 'source'
        $remote = Join-Path $TestDrive 'remote.git'
        $checkout = Join-Path $TestDrive 'checkout'
        $null = New-Item -ItemType Directory $source
        $null = Invoke-PublicationFixtureGit $source @('init', '--quiet', '-b', 'main')
        $null = Invoke-PublicationFixtureGit $source @('config', 'user.name', 'Publication fixture')
        $null = Invoke-PublicationFixtureGit $source @('config', 'user.email', 'fixture@example.invalid')
        'Initial published payload' | Set-Content (Join-Path $source 'payload.txt')
        $null = Invoke-PublicationFixtureGit $source @('add', '.')
        $null = Invoke-PublicationFixtureGit $source @('commit', '--quiet', '-m', 'Stable servicing publication')
        $tag = Invoke-PublicationFixtureGit $source @('rev-parse', 'HEAD')
        for ($i = 1; $i -le 40; $i++) {
            "Later servicing payload $i" | Set-Content (Join-Path $source 'payload.txt')
            $null = Invoke-PublicationFixtureGit $source @('commit', '--quiet', '-am', "Servicing follow-up $i")
        }
        $tip = Invoke-PublicationFixtureGit $source @('rev-parse', 'HEAD')
        $null = Invoke-PublicationFixtureGit $TestDrive @('clone', '--quiet', '--bare', $source, $remote)
        $remoteUrl = ([Uri]::new($remote)).AbsoluteUri
        $null = Invoke-PublicationFixtureGit $TestDrive @('clone', '--quiet', '--depth=1', $remoteUrl, $checkout)
        (Invoke-PublicationFixtureGit $checkout @('rev-parse', '--is-shallow-repository')) | Should -Be 'true'
        $savedActions = $env:GITHUB_ACTIONS
        $state = @{ repository = $remoteUrl }
        Push-Location $checkout
        try {
            $env:GITHUB_ACTIONS = 'true'
            Invoke-PublicationFetch -Commit $tip
            Invoke-PublicationFetch -Commit $tag
            & git merge-base --is-ancestor $tag $tip
            $LASTEXITCODE | Should -Be 0
            (& git rev-parse --is-shallow-repository) | Should -Be 'false'
            $before = & git rev-list --count HEAD
            Invoke-PublicationFetch -Commit $tip
            (& git rev-parse --is-shallow-repository) | Should -Be 'false'
            (& git rev-list --count HEAD) | Should -Be $before
        } finally {
            Pop-Location
            $env:GITHUB_ACTIONS = $savedActions
        }
    }
}
