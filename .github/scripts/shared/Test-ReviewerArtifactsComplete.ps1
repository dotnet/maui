function Test-ReviewerArtifactsComplete {
    param([Parameter(Mandatory)][string]$PRAgentDir)

    foreach ($relative in @(
        'pre-flight/content.md', 'expert-pr-eval/content.md', 'report/content.md',
        'inline-findings.json', 'winner.json', 'pr-finalize/content.md'
    )) {
        $path = Join-Path $PRAgentDir $relative
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { return $false }
        if ((Get-Item -LiteralPath $path -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
            return $false
        }
        $content = Get-Content -Raw -LiteralPath $path -ErrorAction Stop
        if ([string]::IsNullOrWhiteSpace($content)) { return $false }
        if ($relative -eq 'expert-pr-eval/content.md' -and
            $content -match '(?im)^#{1,3}\s+Code Review:\s*SKIPPED\s*$') {
            return $false
        }
    }
    return $true
}
