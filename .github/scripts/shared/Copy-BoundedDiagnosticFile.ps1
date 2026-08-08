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
