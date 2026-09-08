#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Merge-DevicePerformanceCategories.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-categories-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -cne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    $trusted = Join-Path $testRoot "trusted.cs"
    $target = Join-Path $testRoot "target.cs"
    $trustedText = @'
namespace Microsoft.Maui.DeviceTests
{
    public static class TestCategory
    {
        public const string Button = "Button";
        public const string PerformanceOne = nameof(PerformanceOne);
        public const string PerformanceTwo = nameof(PerformanceTwo);
    }
}
'@
    $targetText = @'
namespace Microsoft.Maui.DeviceTests
{
    public static class TestCategory
    {
        public const string ListView = "ListView";
        public const string Custom = "Custom";
        public const string PerformanceOne = "PerformanceOne";
    }
}
'@
    [IO.File]::WriteAllText($trusted, $trustedText)
    [IO.File]::WriteAllText($target, $targetText, [Text.UTF8Encoding]::new($true))
    & $script -TrustedCategoryPath $trusted -TargetCategoryPath $target
    $merged = Get-Content $target -Raw
    Assert-Equal $true ($merged.Contains('public const string ListView = "ListView";')) "Older revision category preservation"
    Assert-Equal $true ($merged.Contains('public const string Custom = "Custom";')) "Revision-specific category preservation"
    Assert-Equal $false ($merged.Contains('public const string Button')) "Unrelated trusted categories must not replace the workload"
    Assert-Equal 1 ([regex]::Matches($merged, 'public const string PerformanceOne').Count) "Existing category must not be duplicated"
    Assert-Equal 1 ([regex]::Matches($merged, 'public const string PerformanceTwo').Count) "Missing trusted category addition"
    Assert-Equal 0xEF ([IO.File]::ReadAllBytes($target)[0]) "UTF-8 BOM preservation"

    $firstHash = (Get-FileHash $target).Hash
    & $script -TrustedCategoryPath $trusted -TargetCategoryPath $target
    Assert-Equal $firstHash (Get-FileHash $target).Hash "Idempotent overlay"

    [IO.File]::WriteAllText($target, $targetText.Replace('"PerformanceOne"', '"WrongCategory"'))
    $conflictRejected = $false
    try {
        & $script -TrustedCategoryPath $trusted -TargetCategoryPath $target
    }
    catch {
        $conflictRejected = $_.Exception.Message -like "*conflicting performance category*"
    }
    Assert-Equal $true $conflictRejected "Conflicting workload categories must fail closed"

    [IO.File]::WriteAllText($target, $targetText.Replace("class TestCategory", "class DifferentCategory"))
    $missingClassRejected = $false
    try {
        & $script -TrustedCategoryPath $trusted -TargetCategoryPath $target
    }
    catch {
        $missingClassRejected = $_.Exception.Message -like "*exactly one TestCategory class*"
    }
    Assert-Equal $true $missingClassRejected "Unexpected category container must fail clearly"

    $pipeline = Join-Path $PSScriptRoot "..\pipelines\common\device-performance-build-job.yml"
    $pipelineSource = Get-Content $pipeline -Raw
    Assert-Equal 2 ([regex]::Matches($pipelineSource, 'Merge-DevicePerformanceCategories\.ps1').Count) "CI must snapshot and invoke the trusted merger"
    Assert-Equal $true ($pipelineSource.Contains('device-performance-harness/src/*')) "CI overlay must exclude the saved full category file"

    Write-Host "All device performance category merge tests passed."
}
finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
