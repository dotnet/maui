param(
    [Parameter(Mandatory=$true)][string[]]$Inputs,
    [Parameter(Mandatory=$true)][string]$Output
)

$ErrorActionPreference = 'Stop'

# Accept both comma and newline separated input
$allFiles = @()
foreach ($item in $Inputs) {
    $allFiles += $item -split '[,`n`r]+' |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ }
}

# Read all JSON files and extract labels
$allLabels = @()
foreach ($file in $allFiles) {
    if (Test-Path $file) {
        $json = Get-Content $file | ConvertFrom-Json
        if ($json.labels) {
            $allLabels += $json.labels
        }
    }
}

# Generate the final merged object
$merged = @{
    labels = $allLabels;
}

# Save to the output file
New-Item -Path (Split-Path -Path $Output) -ItemType Directory -Force | Out-Null
$merged |
    ConvertTo-Json |
    Set-Content $Output
