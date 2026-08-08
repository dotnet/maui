function Copy-BoundedDiagnosticFile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Source,

        [Parameter(Mandatory = $true)]
        [string]$Destination,

        [Parameter(Mandatory = $false)]
        [ValidateRange(256, [long]::MaxValue)]
        [long]$MaxBytes = 16MB
    )

    $sourceItem = Get-Item -LiteralPath $Source -ErrorAction Stop
    if ($sourceItem.PSIsContainer) {
        throw "Diagnostic source must be a file: '$Source'."
    }
    if ($sourceItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Diagnostic source must not be a reparse point: '$Source'."
    }

    $sourcePath = [System.IO.Path]::GetFullPath($sourceItem.FullName)
    $destinationPath = [System.IO.Path]::GetFullPath($Destination)
    if ($sourcePath -eq $destinationPath) {
        throw "Diagnostic source and destination must differ."
    }

    $destinationDirectory = Split-Path -Parent $destinationPath
    if (-not [string]::IsNullOrWhiteSpace($destinationDirectory) -and
        -not (Test-Path -LiteralPath $destinationDirectory)) {
        New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
    }

    if ($sourceItem.Length -le $MaxBytes) {
        Copy-Item -LiteralPath $sourcePath -Destination $destinationPath -Force
        return [pscustomobject]@{
            SourceBytes = [long]$sourceItem.Length
            CopiedBytes = [long]$sourceItem.Length
            Truncated   = $false
        }
    }

    $encoding = [System.Text.UTF8Encoding]::new($false)
    $prefix = "--- Diagnostic log truncated from $($sourceItem.Length) bytes; preserving the final content within a $MaxBytes-byte artifact limit. ---`n"
    $prefixBytes = $encoding.GetBytes($prefix)
    $tailBytes = $MaxBytes - $prefixBytes.Length
    if ($tailBytes -le 0) {
        throw "MaxBytes is too small for the truncation notice."
    }

    $inputStream = [System.IO.File]::Open(
        $sourcePath,
        [System.IO.FileMode]::Open,
        [System.IO.FileAccess]::Read,
        [System.IO.FileShare]::ReadWrite)
    $outputStream = [System.IO.File]::Open(
        $destinationPath,
        [System.IO.FileMode]::Create,
        [System.IO.FileAccess]::Write,
        [System.IO.FileShare]::None)

    try {
        $outputStream.Write($prefixBytes, 0, $prefixBytes.Length)
        [void]$inputStream.Seek(-1 * $tailBytes, [System.IO.SeekOrigin]::End)

        $bufferSize = [int][Math]::Min([long]81920, $tailBytes)
        $buffer = [byte[]]::new($bufferSize)
        $remaining = [long]$tailBytes
        while ($remaining -gt 0) {
            $toRead = [int][Math]::Min([long]$buffer.Length, $remaining)
            $read = $inputStream.Read($buffer, 0, $toRead)
            if ($read -le 0) {
                break
            }

            $outputStream.Write($buffer, 0, $read)
            $remaining -= $read
        }
    } finally {
        $outputStream.Dispose()
        $inputStream.Dispose()
    }

    $copiedBytes = (Get-Item -LiteralPath $destinationPath -ErrorAction Stop).Length
    return [pscustomobject]@{
        SourceBytes = [long]$sourceItem.Length
        CopiedBytes = [long]$copiedBytes
        Truncated   = $true
    }
}

function Copy-BoundedDiagnosticFileSet {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.IO.FileInfo[]]$Files,

        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory,

        [Parameter(Mandatory = $false)]
        [ValidateRange(256, [long]::MaxValue)]
        [long]$MaxTotalBytes = 96MB,

        [Parameter(Mandatory = $false)]
        [ValidateRange(256, [long]::MaxValue)]
        [long]$MaxTextFileBytes = 16MB,

        [Parameter(Mandatory = $false)]
        [ValidateRange(256, [long]::MaxValue)]
        [long]$MaxBinaryFileBytes = 16MB,

        [Parameter(Mandatory = $false)]
        [string[]]$TextFileNames = @('appium.log', 'android-device.log', 'test-output.log')
    )

    New-Item -ItemType Directory -Path $DestinationDirectory -Force | Out-Null
    $destinationRoot = [System.IO.Path]::GetFullPath($DestinationDirectory)

    $textNames = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    foreach ($name in $TextFileNames) {
        if (-not [string]::IsNullOrWhiteSpace($name)) {
            [void]$textNames.Add($name)
        }
    }

    $safeFiles = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    $manifestLines = [System.Collections.Generic.List[string]]::new()
    $unsafeCount = 0
    foreach ($file in @($Files)) {
        if ($null -eq $file) {
            continue
        }

        try {
            $item = Get-Item -LiteralPath $file.FullName -ErrorAction Stop
            if ($item.PSIsContainer -or
                ($item.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
                $unsafeCount++
                $safeName = $item.Name -replace '[\r\n]', ' '
                [void]$manifestLines.Add("UNSAFE`t$safeName")
                continue
            }

            [void]$safeFiles.Add($item)
        } catch {
            $unsafeCount++
            $safeName = $file.Name -replace '[\r\n]', ' '
            [void]$manifestLines.Add("UNREADABLE`t$safeName")
        }
    }

    $textFiles = @($safeFiles |
        Where-Object { $textNames.Contains($_.Name) } |
        Sort-Object Name, FullName)
    $orderedBinaryFiles = @($safeFiles |
        Where-Object { -not $textNames.Contains($_.Name) } |
        Sort-Object LastWriteTimeUtc, FullName)

    # Preserve evidence from both ends of a long failure cascade: the oldest
    # files normally show the initiating failure, while the newest show the
    # terminal state. Exact duplicates are represented by one payload plus a
    # manifest entry naming the retained file.
    $binaryFiles = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    $left = 0
    $right = $orderedBinaryFiles.Count - 1
    while ($left -le $right) {
        [void]$binaryFiles.Add($orderedBinaryFiles[$left])
        $left++
        if ($left -le $right) {
            [void]$binaryFiles.Add($orderedBinaryFiles[$right])
            $right--
        }
    }

    $copiedFiles = 0
    $copiedBytes = 0L
    $truncatedTextFiles = 0
    $duplicateFiles = 0
    $budgetFiles = 0
    $oversizedFiles = 0
    $failedFiles = 0
    $retainedByHash = [System.Collections.Generic.Dictionary[string, string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)

    foreach ($file in $textFiles) {
        $remainingBytes = $MaxTotalBytes - $copiedBytes
        if ($remainingBytes -lt 256) {
            $budgetFiles++
            $safeName = $file.Name -replace '[\r\n]', ' '
            [void]$manifestLines.Add("BUDGET`t$safeName`t$($file.Length)")
            continue
        }

        $fileLimit = [long][Math]::Min($MaxTextFileBytes, $remainingBytes)
        $destination = Join-Path $destinationRoot $file.Name
        try {
            $copyResult = Copy-BoundedDiagnosticFile `
                -Source $file.FullName `
                -Destination $destination `
                -MaxBytes $fileLimit
            $copiedFiles++
            $copiedBytes += [long]$copyResult.CopiedBytes
            if ($copyResult.Truncated) {
                $truncatedTextFiles++
            }
        } catch {
            $failedFiles++
            $safeName = $file.Name -replace '[\r\n]', ' '
            [void]$manifestLines.Add("FAILED`t$safeName")
        }
    }

    foreach ($file in $binaryFiles) {
        $safeName = $file.Name -replace '[\r\n]', ' '
        if ($file.Length -gt $MaxBinaryFileBytes) {
            $oversizedFiles++
            [void]$manifestLines.Add("OVERSIZED`t$safeName`t$($file.Length)")
            continue
        }

        try {
            $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256 -ErrorAction Stop).Hash
        } catch {
            $failedFiles++
            [void]$manifestLines.Add("FAILED-HASH`t$safeName")
            continue
        }

        if ($retainedByHash.ContainsKey($hash)) {
            $duplicateFiles++
            [void]$manifestLines.Add("DUPLICATE`t$safeName`t$($retainedByHash[$hash])")
            continue
        }

        if (($copiedBytes + $file.Length) -gt $MaxTotalBytes) {
            $budgetFiles++
            [void]$manifestLines.Add("BUDGET`t$safeName`t$($file.Length)")
            continue
        }

        $destinationName = $file.Name
        $destination = Join-Path $destinationRoot $destinationName
        $collision = 1
        while (Test-Path -LiteralPath $destination) {
            $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
            $extension = [System.IO.Path]::GetExtension($file.Name)
            $destinationName = "$baseName-$collision$extension"
            $destination = Join-Path $destinationRoot $destinationName
            $collision++
        }

        try {
            Copy-Item -LiteralPath $file.FullName -Destination $destination -Force -ErrorAction Stop
            $copiedFiles++
            $copiedBytes += [long]$file.Length
            $retainedByHash[$hash] = $destinationName
        } catch {
            $failedFiles++
            [void]$manifestLines.Add("FAILED-COPY`t$safeName")
        }
    }

    $manifestPath = $null
    if ($manifestLines.Count -gt 0) {
        $manifestPath = Join-Path $destinationRoot 'diagnostic-capture-manifest.txt'
        $header = @(
            '# Bounded UI-test diagnostic capture'
            "MaxPayloadBytes`t$MaxTotalBytes"
            "CopiedPayloadBytes`t$copiedBytes"
            "Reason`tSource`tRetained-or-bytes"
        )
        @($header + $manifestLines) |
            Set-Content -LiteralPath $manifestPath -Encoding UTF8
    }

    return [pscustomobject]@{
        SourceFiles        = @($Files).Count
        SourceBytes        = [long](($safeFiles | Measure-Object Length -Sum).Sum)
        CopiedFiles        = $copiedFiles
        CopiedBytes        = [long]$copiedBytes
        TruncatedTextFiles = $truncatedTextFiles
        DuplicateFiles     = $duplicateFiles
        BudgetFiles        = $budgetFiles
        OversizedFiles     = $oversizedFiles
        UnsafeFiles        = $unsafeCount
        FailedFiles        = $failedFiles
        ManifestPath       = $manifestPath
    }
}
