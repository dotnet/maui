param(
    [Parameter(Mandatory=$true)]
    [string]$Template,
    [Parameter(Mandatory=$true)]
    [string]$Output,
    [string]$LabelPrefix,
    [string]$Label
)

Write-Host "Processing template: $Template"
Write-Host "Output will be written to: $Output"

# Check if template file exists
if (-not (Test-Path -Path $Template)) {
    Write-Error "The specified template file '$Template' does not exist. Please check the path and try again."
    exit 1
}

# Ensure output directory exists
$outputDir = Split-Path -Parent $Output
if ($outputDir -and -not (Test-Path -Path $outputDir)) {
    New-Item -Path $outputDir -ItemType Directory -Force | Out-Null
}

# Change to the output directory for processing
$originalLocation = Get-Location
try {
    Set-Location -Path $outputDir
    Write-Host "Changed working directory to: $outputDir"
    Write-Host ""

    # Read the template file
    $lines = Get-Content $Template
    $outputContent = @()

    foreach ($line in $lines) {
        # Replace the placeholders with actual values
        if ($LabelPrefix) {
            $line = $line.Replace('{{LABEL_PREFIX}}', $LabelPrefix)
        }
        if ($Label) {
            $line = $line.Replace('{{LABEL}}', $Label)
        }

        # Check for EXEC: command prefix
        if ($line -match "^EXEC:\s*(.+)$") {

            # Extract the command part
            $command = $matches[1]
            Write-Host "Executing command:"
            Write-Host "    $command"

            try {
                # Execute the command
                $result = Invoke-Expression $command
                Write-Host "Command output:"
                foreach ($resultLine in $result) {
                    Write-Host "    $resultLine"
                }

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

    # Save the processed content to the output file
    $outputFilename = Split-Path -Leaf $Output
    Set-Content -Path $outputFilename -Value $outputContent -ErrorAction Stop

    # Log the created prompt for debugging
    Write-Host ""
    Write-Host "Created prompt from template:"
    Write-Host ""
    Get-Content $outputFilename

} finally {
    # Return to original location
    Set-Location -Path $originalLocation
}
