# run-integration-tests.ps1
param(
    [string]$TestCategory = "Build"
)

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 1: Build and Pack MAUI" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
dotnet cake --target=dotnet-pack --configuration=Release

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 2: Move Packages to Artifacts Root" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path artifacts | Out-Null
Copy-Item artifacts\packages\Release\Shipping\*.nupkg artifacts\ -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 3: Install .NET Without Workloads" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
.\.dotnet\dotnet.exe build .\src\DotNet\DotNet.csproj `
  -p:InstallWorkloadPacks=false `
  -c Release `
  -bl:.\artifacts\log\install-dotnet.binlog

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 4: Install Workloads from Packages" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
.\.dotnet\dotnet.exe build .\src\DotNet\DotNet.csproj `
  -t:Install `
  -c Release `
  -bl:.\artifacts\log\install-dotnet-workload.binlog

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 5: Extract MAUI Version" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
$nupkg = Get-ChildItem artifacts\Microsoft.Maui.Controls.*.nupkg | Select-Object -First 1
$version = [regex]::Match($nupkg.Name, '(\d+\.\d+\.\d+-[a-z]+\.[a-z]+\.\d+\.\d+)').Groups[1].Value
$env:MAUI_PACKAGE_VERSION = $version
Write-Host "MAUI_PACKAGE_VERSION: $version"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 6: Restore dotnet Tools" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
.\.dotnet\dotnet.exe tool restore

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 7: Run Integration Tests" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Running tests for category: $TestCategory"

New-Item -ItemType Directory -Force -Path .\artifacts\TestResults | Out-Null

.\.dotnet\dotnet.exe test `
  .\src\TestUtils\src\Microsoft.Maui.IntegrationTests\Microsoft.Maui.IntegrationTests.csproj `
  -c Release `
  --filter "Category=$TestCategory" `
  --logger "trx;LogFileName=integration-tests-$TestCategory.trx" `
  --results-directory .\artifacts\TestResults `
  --logger "console;verbosity=normal"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Tests Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Results: .\artifacts\TestResults\integration-tests-$TestCategory.trx"
