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
        $fileContents = Get-Content $file

        # Remove empty lines
        $fileContents = $fileContents | Where-Object { $_ -ne '' }

        # Remove the wrapping lines if they contain ```
        if ($fileContents[0] -match '^\s*```') {
            $fileContents = $fileContents[1..($fileContents.Length - 1)]
        }
        if ($fileContents[-1] -match '^\s*```') {
            $fileContents = $fileContents[0..($fileContents.Length - 2)]
        }
        
        # Convert from JSON
        $json = $fileContents | ConvertFrom-Json
        foreach ($prop in $json.PSObject.Properties) {
            $name = $prop.Name
            $value = $prop.Value
            
            # Merge properties by name
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
