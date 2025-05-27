param(
    [Parameter(Mandatory=$true)][string]$Inputs,
    [Parameter(Mandatory=$true)][string]$Output
)

$ErrorActionPreference = 'Stop'

Write-Host "Processing input files: $Inputs"
Write-Host "Output will be written to: $Output"

# Accept both comma and newline separated input
$allFiles = @()
foreach ($item in $Inputs) {
    $allFiles += $item -split '[,\n\r]+' |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ }
}

# Read all JSON files and merge root properties
$merged = @{}
foreach ($file in $allFiles) {
    if (Test-Path $file) {
        Write-Host "Processing file: $file..."
        $json = Get-Content $file | ConvertFrom-Json
        foreach ($prop in $json.PSObject.Properties) {
            $name = $prop.Name
            $value = $prop.Value
            if ($merged.ContainsKey($name)) {
                $merged[$name] += $value
            } else {
                $merged[$name] = $value
            }
        }
    }
}

# Save to the output file
New-Item -Path (Split-Path -Path $Output) -ItemType Directory -Force | Out-Null
$merged |
    ConvertTo-Json |
    Set-Content $Output
