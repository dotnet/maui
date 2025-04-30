# filepath: /Users/ruimarinho/dotnet/maui/eng/scripts/push_nuget_org.ps1
param (
    [Parameter(Mandatory)]
    [String]
    $ApiKey,

    [Parameter(Mandatory)]
    [String]
    $NuGetSearchPath,

    [String]
    $NuGetIncludeFilters,

    [String]
    $NuGetExcludeFilters,

    [String]
    $FeedUrl,

    [String]
    $NuGetOrgApiKey2,

    [String]
    $NuGetOrgApiKey3
)

# Set strict error handling
$ErrorActionPreference = "Stop"

# Start transcript logging for better diagnostics
$logFile = "$PSScriptRoot/nuget_push_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
Start-Transcript -Path $logFile -Append

# Write a starting message with timestamp
Write-Host "Starting NuGet package push operation at $(Get-Date)"

# Error Handling Function
function Write-ErrorLog {
    param (
        [string]$Message,
        [System.Management.Automation.ErrorRecord]$ErrorRecord
    )
    Write-Host "ERROR: $Message" -ForegroundColor Red
    if ($ErrorRecord) {
        Write-Host "Exception: $($ErrorRecord.Exception.Message)" -ForegroundColor Red
        Write-Host "Position: $($ErrorRecord.InvocationInfo.PositionMessage)" -ForegroundColor Red
    }
}

function CheckQuotaExceeded {
  param (
    [String] $pushOutput
  )
  return ($pushOutput -like "*Quota exceeded*" -or $pushOutput -like "*Rate limit is exceeded*")
}

function TryPushNuGetWithRetry {
  param (
    [System.IO.FileInfo] $nupkg,
    [String] $feedUrl,
    [String] $apiKey,
    [Int32] $maxAttempts,
    [Int32] $retryDelaySeconds = 5
  )

  $nupkgFile = $nupkg.FullName
  $packageName = [System.IO.Path]::GetFileNameWithoutExtension($nupkgFile)
  
  Write-Host "Starting push process for package: $packageName"
  
  for ($i = 1; $i -le $maxAttempts; $i++) {
    try {
      Write-Host "Pushing $nupkgFile to $feedUrl (attempt $i of $maxAttempts)"
      
      # Add a timeout to the dotnet nuget push command
      $output = & dotnet nuget push --source $feedUrl --api-key $apiKey --skip-duplicate --timeout 300 $nupkgFile
      $exitCode = $LASTEXITCODE
      
      if ($exitCode -eq 0) {
        return @{
          Success = $true
          Message = "`tSuccessfully pushed $packageName to $feedUrl. Output:`n`t$output"
        }
      } else {
        throw "Process failed with exit code '$exitCode'. Output:`n`t$output"
      }
    } catch {
      $errorMsg = $_.Exception.Message
      $quotaExceeded = $(CheckQuotaExceeded -pushOutput $errorMsg)
      
      if ($i -eq $maxAttempts -or $quotaExceeded -eq $true) {
        return @{
          Success = $false
          Message = "`tFailed to push $packageName. Error: $errorMsg"
          QuotaExceeded = $quotaExceeded
        }
      }
      
      # Add progressive backoff between retries
      $delay = $retryDelaySeconds * $i
      Write-Host "Retry attempt $i failed. Waiting $delay seconds before next attempt..."
      Start-Sleep -Seconds $delay
    }
  }
  
  # This should never be reached if maxAttempts is at least 1
  return @{
    Success = $false
    Message = "`tAll $maxAttempts attempts to push $packageName failed."
    QuotaExceeded = $false
  }
}

# If no feed was provided, then push to NuGet.org
$nugetOrgFeedUrl = "https://api.nuget.org/v3/index.json"
if (!$FeedUrl) {
  $FeedUrl = $nugetOrgFeedUrl
}

# Get the Filter from the list of included packages
if ([string]::IsNullOrEmpty($NuGetIncludeFilters) -or "$NuGetIncludeFilters" -eq "skip") {
  $includeFilters = @()
} else {
  $includeFilters = "$NuGetIncludeFilters" -split ";" | Where-Object { $_ } | ForEach-Object { $_.Trim() }
}
if ([string]::IsNullOrEmpty($NuGetExcludeFilters) -or "$NuGetExcludeFilters" -eq "skip") {
  $excludeFilters = @()
} else {
  $excludeFilters = "$NuGetExcludeFilters" -split ";" | Where-Object { $_ } | ForEach-Object { $_.Trim() }
}

$nupkgs = (Get-ChildItem -Path "$NuGetSearchPath" -Filter *.nupkg -Recurse -Include $includeFilters -Exclude $excludeFilters)
Write-Output "Publishing the following packages:"
foreach ($nupkg in $nupkgs) {
  Write-Output $nupkg.FullName
}

$nugetApiKeys = [System.Collections.Generic.List[string]]::new()
$initialKeys = @($ApiKey, $NuGetOrgApiKey2, $NuGetOrgApiKey3) | Where-Object { -not [string]::IsNullOrEmpty($_) }
foreach ($key in $initialKeys) {
    $nugetApiKeys.Add($key)
}
$nugetApiKeysExceedingQuota = [System.Collections.Generic.List[string]]::new()
$nugetApiKeyIndex = 1

# Define constants
$MAX_PUSH_ATTEMPTS = 3
$RETRY_DELAY = 5

# Create a summary report
$pushSummary = @{
    Successful = @()
    Failed = @()
    Skipped = @()
}

Write-Host "===== Starting package push process ====="
Write-Host "Total packages to process: $($nupkgs.Count)"

foreach ($nupkg in $nupkgs) {
    $packageName = [System.IO.Path]::GetFileNameWithoutExtension($nupkg.FullName)
    
    try {
        if ($env:PUSH_PACKAGES -ne "True") {
            Write-Host "`tDRY RUN: Not pushing $packageName" -ForegroundColor Cyan
            $pushSummary.Skipped += $packageName
            continue
        }

        if ($FeedUrl -eq $nugetOrgFeedUrl) {
            if ($nugetApiKeys.Count -eq 0) {
                throw "Unable to push to NuGet.org, all API keys have exceeded quota or none were provided."
            }
            
            $packagePushed = $false
            
            foreach ($key in $nugetApiKeys) {
                if (!$key) {
                    continue
                }
                
                $result = TryPushNuGetWithRetry -nupkg $nupkg -feedUrl $FeedUrl -apiKey $key -maxAttempts $MAX_PUSH_ATTEMPTS -retryDelaySeconds $RETRY_DELAY
                Write-Host $result.Message
                
                if ($result.Success) {
                    $pushSummary.Successful += $packageName
                    $packagePushed = $true
                    break
                }
                
                if ($result.QuotaExceeded) {
                    Write-Host "`tQuota exceeded for key $nugetApiKeyIndex, removing from available keys." -ForegroundColor Yellow
                    $nugetApiKeysExceedingQuota.Add($key)
                    $nugetApiKeyIndex++
                } else {
                    # If failure was not due to quota, stop trying with other keys
                    break
                }
            }
            
            if (-not $packagePushed) {
                $pushSummary.Failed += $packageName
                Write-Host "Failed to push package $packageName after trying all available API keys" -ForegroundColor Red
            }
            
            # Remove any keys that exceeded quota
            foreach ($keyToRemove in $nugetApiKeysExceedingQuota) {
                $nugetApiKeys.Remove($keyToRemove)
            }
            $nugetApiKeysExceedingQuota.Clear()
        } else {
            # Pushing to a feed other than NuGet.org
            $result = TryPushNuGetWithRetry -nupkg $nupkg -feedUrl $FeedUrl -apiKey $ApiKey -maxAttempts $MAX_PUSH_ATTEMPTS -retryDelaySeconds $RETRY_DELAY
            Write-Host $result.Message
            
            if ($result.Success) {
                $pushSummary.Successful += $packageName
            } else {
                $pushSummary.Failed += $packageName
            }
        }
    }
    catch {
        Write-ErrorLog -Message "Error processing package $packageName" -ErrorRecord $_
        $pushSummary.Failed += $packageName
    }
}

# Print a summary report
Write-Host "`n===== NuGet Push Summary =====" -ForegroundColor Cyan
Write-Host "Successful: $($pushSummary.Successful.Count) packages" -ForegroundColor Green
if ($pushSummary.Successful.Count -gt 0) {
    foreach ($pkg in $pushSummary.Successful) {
        Write-Host "  - $pkg" -ForegroundColor Green
    }
}

Write-Host "Failed: $($pushSummary.Failed.Count) packages" -ForegroundColor $(if ($pushSummary.Failed.Count -gt 0) { "Red" } else { "Green" })
if ($pushSummary.Failed.Count -gt 0) {
    foreach ($pkg in $pushSummary.Failed) {
        Write-Host "  - $pkg" -ForegroundColor Red
    }
}

Write-Host "Skipped: $($pushSummary.Skipped.Count) packages" -ForegroundColor Cyan
if ($pushSummary.Skipped.Count -gt 0 -and $pushSummary.Skipped.Count -lt 10) {
    foreach ($pkg in $pushSummary.Skipped) {
        Write-Host "  - $pkg" -ForegroundColor Cyan
    }
}

# Display API key usage information if pushing to NuGet.org
if ($FeedUrl -eq $nugetOrgFeedUrl) {
    Write-Host "`nAPI Key usage:" -ForegroundColor Cyan
    Write-Host "  - Available keys remaining: $($nugetApiKeys.Count)" -ForegroundColor $(if ($nugetApiKeys.Count -gt 0) { "Green" } else { "Red" })
    Write-Host "  - Keys with exceeded quota: $($nugetApiKeysExceedingQuota.Count)" -ForegroundColor $(if ($nugetApiKeysExceedingQuota.Count -gt 0) { "Yellow" } else { "Green" })
}

# End transcript logging
Write-Host "`nScript completed at $(Get-Date)" -ForegroundColor Cyan
Write-Host "Log file: $logFile" -ForegroundColor Cyan
Stop-Transcript

# Return an exit code based on whether there were any failures
if ($pushSummary.Failed.Count -gt 0) {
    exit 1
} else {
    exit 0
}
