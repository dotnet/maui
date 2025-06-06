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
    [Int32] $maxAttempts
  )

  $nupkgFile = $nupkg.FullName
  for ($i = 1; $i -le $maxAttempts; $i++) {
    try {
      Write-Host "Pushing $nupkgFile to $feedUrl (attempt $i of $maxAttempts)"
      $output = & dotnet nuget push --source $feedUrl --api-key $apiKey --skip-duplicate $nupkgFile
      $exitCode = $LASTEXITCODE
      if ($exitCode -eq 0) {
        return "`tSuccessfully pushed $nupkgFile to $FeedUrl. Output:`n`t$output"
      } else {
        throw "Process failed with exit code '$exitCode'. Output:`n`t$output"
      }
    } catch {
      if ($i -eq $maxAttempts -or $(CheckQuotaExceeded -pushOutput $_.Exception.Message) -eq $true) {
        return "`t$($_.Exception.Message)"
      }
    }
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
  $includeFilters = "$NuGetIncludeFilters" -split ";" | Where-Object { $_ } | % { $_.Trim() }
}
if ([string]::IsNullOrEmpty($NuGetExcludeFilters) -or "$NuGetExcludeFilters" -eq "skip") {
  $excludeFilters = @()
} else {
  $excludeFilters = "$NuGetExcludeFilters" -split ";" | Where-Object { $_ } | % { $_.Trim() }
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

foreach ($nupkg in $nupkgs) {
  if ( $env:PUSH_PACKAGES -ne "true" ) {
    Write-Host "`tdry run, not pushing $($nupkg.FullName)"
    continue
  }

  if ($FeedUrl -eq $nugetOrgFeedUrl) {
    if ($nugetApiKeys.Count -eq 0) {
      throw "Unable to push to NuGet.org, all API keys have exceeded quota or none were provided."
    }
    foreach ($key in $nugetApiKeys) {
      if (!$key) {
        continue
      }
      $pushTryOutput = TryPushNuGetWithRetry -nupkg $nupkg -feedUrl $FeedUrl -apiKey $key -maxAttempts 3
      Write-Host $pushTryOutput
      if ($pushTryOutput -like "*Successfully pushed*") {
        break
      }
      if ($(CheckQuotaExceeded -pushOutput $pushTryOutput) -eq $true) {
        Write-Host "`tQuota exceeded, future push attempts will not try to use key $nugetApiKeyIndex."
        $nugetApiKeysExceedingQuota.Add($key)
        $nugetApiKeyIndex++
      } else {
        break
      }
    }
    foreach ($keyToRemove in $nugetApiKeysExceedingQuota) {
      $nugetApiKeys.Remove($keyToRemove)
    }
    $nugetApiKeysExceedingQuota.Clear()
  } else {
    $pushTryOutput = TryPushNuGetWithRetry -nupkg $nupkg -feedUrl $FeedUrl -apiKey $ApiKey -maxAttempts 3
    Write-Host $pushTryOutput
  }
}
