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
                Total        = 0
                Passed       = 0
                Failed       = 0
                Skipped      = 0
                Results      = @()
                TrxPaths     = @()
                ArtifactName = $artName
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

    return $byCategory
}
