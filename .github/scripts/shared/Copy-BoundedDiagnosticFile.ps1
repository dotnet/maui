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

function Copy-BoundedRegularFileTree {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$DestinationDirectory,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$MaxFileCount = 2048,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [int]::MaxValue)]
        [int]$MaxDirectoryCount = 1024,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [long]::MaxValue)]
        [long]$MaxFileBytes = 16MB,

        [Parameter(Mandatory = $false)]
        [ValidateRange(1, [long]::MaxValue)]
        [long]$MaxTotalBytes = 128MB
    )

    $sourceItem = Get-Item -LiteralPath $SourceDirectory -Force -ErrorAction Stop
    if (-not $sourceItem.PSIsContainer) {
        throw "Bounded tree source must be a directory: '$SourceDirectory'."
    }
    if ($sourceItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
        throw "Bounded tree source must not be a reparse point: '$SourceDirectory'."
    }

    $sourceRoot = [System.IO.Path]::TrimEndingDirectorySeparator(
        [System.IO.Path]::GetFullPath($sourceItem.FullName))
    $destinationRoot = [System.IO.Path]::TrimEndingDirectorySeparator(
        [System.IO.Path]::GetFullPath($DestinationDirectory))
    if (Test-Path -LiteralPath $destinationRoot) {
        throw "Bounded tree destination must not already exist: '$destinationRoot'."
    }

    $destinationRelativeToSource = [System.IO.Path]::GetRelativePath(
        $sourceRoot,
        $destinationRoot)
    $parentPrefix = "..$([System.IO.Path]::DirectorySeparatorChar)"
    $alternateParentPrefix = "..$([System.IO.Path]::AltDirectorySeparatorChar)"
    $destinationIsInsideSource =
        $destinationRelativeToSource -eq '.' -or
        (-not [System.IO.Path]::IsPathRooted($destinationRelativeToSource) -and
            $destinationRelativeToSource -ne '..' -and
            -not $destinationRelativeToSource.StartsWith(
                $parentPrefix,
                [System.StringComparison]::Ordinal) -and
            -not $destinationRelativeToSource.StartsWith(
                $alternateParentPrefix,
                [System.StringComparison]::Ordinal))
    if ($destinationIsInsideSource) {
        throw 'Bounded tree destination must not be contained by the source directory.'
    }

    $pendingDirectories = [System.Collections.Generic.Stack[string]]::new()
    $pendingDirectories.Push($sourceRoot)
    $files = [System.Collections.Generic.List[object]]::new()
    $directoryCount = 0
    $totalBytes = 0L

    while ($pendingDirectories.Count -gt 0) {
        $directoryPath = $pendingDirectories.Pop()
        $directoryItem = Get-Item -LiteralPath $directoryPath -Force -ErrorAction Stop
        if (-not $directoryItem.PSIsContainer -or
            ($directoryItem.Attributes -band [System.IO.FileAttributes]::ReparsePoint)) {
            throw "Bounded tree contains an unsupported reparse point: '$directoryPath'."
        }

        $directoryCount++
        if ($directoryCount -gt $MaxDirectoryCount) {
            throw "Bounded tree exceeded the $MaxDirectoryCount-directory limit."
        }

        foreach ($entryPath in [System.IO.Directory]::EnumerateFileSystemEntries($directoryPath)) {
            $entry = Get-Item -LiteralPath $entryPath -Force -ErrorAction Stop
            if ($entry.Attributes -band [System.IO.FileAttributes]::ReparsePoint) {
                throw "Bounded tree contains an unsupported reparse point: '$entryPath'."
            }

            $entryFullPath = [System.IO.Path]::GetFullPath($entry.FullName)
            $relativePath = [System.IO.Path]::GetRelativePath($sourceRoot, $entryFullPath)
            if ($relativePath -eq '.' -or
                $relativePath -eq '..' -or
                [System.IO.Path]::IsPathRooted($relativePath) -or
                $relativePath.StartsWith($parentPrefix, [System.StringComparison]::Ordinal) -or
                $relativePath.StartsWith($alternateParentPrefix, [System.StringComparison]::Ordinal)) {
                throw "Bounded tree entry escaped the canonical source root: '$entryPath'."
            }

            if ($entry.PSIsContainer) {
                $pendingDirectories.Push($entryFullPath)
                continue
            }
            if ($entry -isnot [System.IO.FileInfo]) {
                throw "Bounded tree entry is not a regular file: '$entryPath'."
            }

            if ($files.Count -ge $MaxFileCount) {
                throw "Bounded tree exceeded the $MaxFileCount-file limit."
            }
            if ($entry.Length -gt $MaxFileBytes) {
                throw "Bounded tree file '$relativePath' exceeded the $MaxFileBytes-byte per-file limit."
            }
            if ($entry.Length -gt ($MaxTotalBytes - $totalBytes)) {
                throw "Bounded tree exceeded the $MaxTotalBytes-byte aggregate limit."
            }

            $totalBytes += [long]$entry.Length
            [void]$files.Add([pscustomobject]@{
                SourcePath   = $entryFullPath
                RelativePath = $relativePath
                Length       = [long]$entry.Length
            })
        }
    }

    try {
        New-Item -ItemType Directory -Path $destinationRoot -Force -ErrorAction Stop | Out-Null
        foreach ($file in @($files | Sort-Object RelativePath)) {
            $current = Get-Item -LiteralPath $file.SourcePath -Force -ErrorAction Stop
            if ($current.PSIsContainer -or
                ($current.Attributes -band [System.IO.FileAttributes]::ReparsePoint) -or
                $current.Length -ne $file.Length) {
                throw "Bounded tree source changed during import: '$($file.RelativePath)'."
            }

            $destination = [System.IO.Path]::GetFullPath(
                (Join-Path $destinationRoot $file.RelativePath))
            $destinationRelative = [System.IO.Path]::GetRelativePath(
                $destinationRoot,
                $destination)
            if ($destinationRelative -eq '.' -or
                $destinationRelative -eq '..' -or
                [System.IO.Path]::IsPathRooted($destinationRelative) -or
                $destinationRelative.StartsWith($parentPrefix, [System.StringComparison]::Ordinal) -or
                $destinationRelative.StartsWith(
                    $alternateParentPrefix,
                    [System.StringComparison]::Ordinal)) {
                throw "Bounded tree destination escaped its canonical root: '$destination'."
            }

            $destinationParent = Split-Path -Parent $destination
            if (-not (Test-Path -LiteralPath $destinationParent)) {
                New-Item -ItemType Directory -Path $destinationParent -Force -ErrorAction Stop |
                    Out-Null
            }
            Copy-Item `
                -LiteralPath $current.FullName `
                -Destination $destination `
                -Force `
                -ErrorAction Stop
        }
    } catch {
        if (Test-Path -LiteralPath $destinationRoot) {
            Remove-Item -LiteralPath $destinationRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
        throw
    }

    return [pscustomobject]@{
        CopiedFiles       = $files.Count
        CopiedDirectories = $directoryCount
        CopiedBytes       = $totalBytes
    }
}
