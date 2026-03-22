param(
    [Parameter(Mandatory=$false)][string]$InputFiles,
    [Parameter(Mandatory=$false)][string]$InputDir,
    [Parameter(Mandatory=$true)][string]$Output
)

$ErrorActionPreference = 'Stop'

Write-Host "Input files: $InputFiles"
Write-Host "Input directory: $InputDir"
Write-Host "Output file: $Output"

$allFiles = @()

# Process individual files from InputFiles parameter
# Accept both comma and newline separated input
if ($InputFiles) {
    $allFiles += $InputFiles -split '[,\n\r]+' |
        ForEach-Object { $_.Trim() } |
        Where-Object { $_ }

    if ($allFiles.Count -gt 0) {
        Write-Host "Merging files from InputFiles parameter:"
        foreach ($file in $allFiles) {
            Write-Host "    $file"
        }
    }
}

# Process all JSON files from InputDir parameter if there are no files specified in InputFiles
if ($allFiles.Count -eq 0 -and $InputDir -and (Test-Path $InputDir)) {
    $jsonFiles = Get-ChildItem -Path $InputDir -Filter "*.json" -File
    
    if ($jsonFiles.Count -gt 0) {
        Write-Host "Merging files from InputDir parameter:"
    }

    foreach ($file in $jsonFiles) {
        $allFiles += $file.FullName

        Write-Host "    $file"
    }
}

# If no files were specified, exit with an error
if ($allFiles.Count -eq 0) {
    Write-Error "No input files specified. Please provide either InputFiles or InputDir with JSON files."
    exit 1
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
