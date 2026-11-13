function Get-AggregatedTrxFromDirectory {
    param([string]$RootDir)

    $byCategory = @{}
    if (-not (Test-Path $RootDir)) {
        return $byCategory
    }
    $trxFiles = @(Get-ChildItem -Path $RootDir -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)
    Write-Host "  Found $($trxFiles.Count) TRX file(s) under $RootDir" -ForegroundColor Gray

    foreach ($trx in $trxFiles) {
        $trxResult = Get-TrxResults -TrxPath $trx.FullName
        if (-not $trxResult) { continue }

        $relative = $trx.FullName.Substring($RootDir.Length).TrimStart('/','\')
        $artName = $relative.Split([System.IO.Path]::DirectorySeparatorChar)[0]
        $category = Get-CategoryFromArtifactName -ArtifactName $artName

        if (-not $byCategory.ContainsKey($category)) {
            $byCategory[$category] = @{
                Total                      = 0
                Passed                     = 0
                Failed                     = 0
                Skipped                    = 0
                Results                    = @()
                TrxPaths                   = @()
                ArtifactName               = $artName
                SetupFailure               = $false
                SetupFailureCount          = 0
                SetupFailureMessage        = ''
                SetupFailureStack          = ''
                SetupFailureSignatureCount = 0
                SetupFailureIsAppCrash     = $false
            }
        }
        $cur = $byCategory[$category]
        $cur.Total   += [int]$trxResult.Total
        $cur.Passed  += [int]$trxResult.Passed
        $cur.Failed  += [int]$trxResult.Failed
        $cur.Skipped += [int]$trxResult.Skipped
        $cur.Results = @($cur.Results) + @($trxResult.Results)
        $cur.TrxPaths = @($cur.TrxPaths) + @($trx.FullName)
        $byCategory[$category] = $cur
    }

    foreach ($category in @($byCategory.Keys)) {
        $cur = $byCategory[$category]
        $failedResults = @($cur.Results | Where-Object { $_.status -eq 'Failed' })
        if ($failedResults.Count -eq 0) {
            continue
        }

        $setupFailures = @($failedResults | Where-Object {
            $errorText = [string]($_.error)
            $stackText = [string]($_.stack)
            $errorText -match '^\s*OneTimeSetUp:' -or
                $errorText -match 'Timed out waiting for Go To Test button to appear' -or
                $errorText -match 'The app was expected to be running still, investigate as possible crash' -or
                $stackText -match '(_GalleryUITest\.FixtureSetup|\bFixtureSetup\b|UITestBase\.(OneTimeSetup|TestSetup|UITestBaseTearDown))'
        })

        if ($setupFailures.Count -ne $failedResults.Count) {
            continue
        }

        $signatures = @{}
        foreach ($failure in $setupFailures) {
            $errorText = ([string]($failure.error) -replace '\s+', ' ').Trim()
            $stackText = ([string]($failure.stack) -replace '\s+', ' ').Trim()
            $signatures["$errorText|$stackText"] = $true
        }

        # An app-crash signature ("...investigate as possible crash" raised in
        # UITestBaseTearDown) means the HostApp died mid-run; every subsequent
        # OneTimeSetUp then times out waiting for "Go To Test button". Unlike a
        # pure navigation/fixture setup failure (an emulator-side infra flake),
        # a crash can also be a regression introduced by the PR — so flag it
        # distinctly and let the renderer surface it cautiously.
        $appCrash = @($setupFailures | Where-Object {
            ([string]($_.error)) -match 'investigate as possible crash' -or
                ([string]($_.stack)) -match 'UITestBase\.UITestBaseTearDown'
        }).Count -gt 0

        # Prefer an app-crash entry as the representative sample so the rendered
        # message reflects the crash itself, not an incidental Go-To-Test
        # timeout from the cascade that followed it.
        $sample = $setupFailures | Where-Object {
            ([string]($_.error)) -match 'investigate as possible crash' -or
                ([string]($_.stack)) -match 'UITestBase\.UITestBaseTearDown'
        } | Select-Object -First 1
        if (-not $sample) { $sample = $setupFailures | Select-Object -First 1 }

        $cur.SetupFailure = $true
        $cur.SetupFailureCount = $setupFailures.Count
        $cur.SetupFailureMessage = ([string]($sample.error)).Trim()
        $cur.SetupFailureStack = ([string]($sample.stack)).Trim()
        $cur.SetupFailureSignatureCount = $signatures.Count
        $cur.SetupFailureIsAppCrash = $appCrash
        $byCategory[$category] = $cur
    }

    return $byCategory
}
