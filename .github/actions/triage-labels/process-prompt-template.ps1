param(
    [Parameter(Mandatory=$true)]
    [string]$Template,
    [Parameter(Mandatory=$true)]
    [string]$Output
)

# Check if template file exists
if (-not (Test-Path -Path $Template)) {
    Write-Error "The specified template file '$Template' does not exist. Please check the path and try again."
    exit 1
}

# Read the template file
$lines = Get-Content $Template
$outputContent = @()

Write-Host "Processing template: $Template"
Write-Host "Output will be written to: $Output"

foreach ($line in $lines) {
    # Check for EXEC: command prefix
    if ($line -match "^EXEC:\s*(.+)$") {

        # Extract the command part
        $command = $matches[1]
        Write-Host "Executing command: $command"
        
        try {
            # Execute the command
            $result = Invoke-Expression $command
            Write-Host "Command output:"
            Write-Host $result

            # Append the result to output content
            $outputContent += $result
        } catch {
            Write-Error "ERROR executing command '$command': $_"
            exit 1
        }
    } else {
        # Keep original line
        $outputContent += $line
    }
}

# Ensure output directory exists
$outputDir = Split-Path -Parent $Output
if ($outputDir -and -not (Test-Path -Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

# Save the processed content to the output file
Set-Content -Path $Output -Value $outputContent -ErrorAction Stop

# Log the created prompt for debugging
Write-Host "Created prompt from template:"
Get-Content $Output
