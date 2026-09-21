#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $false)]
    [string]$ConfigurationPath = (Join-Path $PSScriptRoot "..\ui-evidence\devflow.json"),

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $false)]
    [string]$SourceRoot,

    [Parameter(Mandatory = $false)]
    [string]$WorkingDirectory,

    [Parameter(Mandatory = $false)]
    [string]$DotNetPath = "dotnet",

    [Parameter(Mandatory = $false)]
    [string]$SourceOutputDirectory
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

function Invoke-CheckedProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FileName,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [Parameter(Mandatory = $true)]
        [string]$CurrentDirectory
    )

    Push-Location $CurrentDirectory
    try {
        & $FileName @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "$FileName exited with code $LASTEXITCODE."
        }
    }
    finally {
        Pop-Location
    }
}

function New-DeterministicArchive {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory,

        [Parameter(Mandatory = $true)]
        [string]$DestinationPath
    )

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    Remove-Item -LiteralPath $DestinationPath -Force -ErrorAction SilentlyContinue
    $stream = [IO.File]::Open($DestinationPath, [IO.FileMode]::CreateNew)
    try {
        $archive = [IO.Compression.ZipArchive]::new(
            $stream,
            [IO.Compression.ZipArchiveMode]::Create,
            $false)
        try {
            $fixedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
            foreach ($file in @(Get-UiEvidenceFiles $SourceDirectory | Sort-Object FullName)) {
                $relativePath = [IO.Path]::GetRelativePath($SourceDirectory, $file.FullName).Replace('\', '/')
                $entry = $archive.CreateEntry($relativePath, [IO.Compression.CompressionLevel]::Optimal)
                $entry.LastWriteTime = $fixedTimestamp
                $entryStream = $entry.Open()
                try {
                    $input = [IO.File]::OpenRead($file.FullName)
                    try {
                        $input.CopyTo($entryStream)
                    }
                    finally {
                        $input.Dispose()
                    }
                }
                finally {
                    $entryStream.Dispose()
                }
            }
        }
        finally {
            $archive.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }
}

function Remove-MauiPackageDependencies {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PackagePath,

        [Parameter(Mandatory = $true)]
        [string]$DestinationPath,

        [Parameter(Mandatory = $true)]
        [string]$TemporaryRoot
    )

    $extractRoot = Join-Path $TemporaryRoot ([IO.Path]::GetFileNameWithoutExtension($PackagePath))
    Remove-Item -LiteralPath $extractRoot -Recurse -Force -ErrorAction SilentlyContinue
    [IO.Compression.ZipFile]::ExtractToDirectory($PackagePath, $extractRoot)

    $nuspec = @(Get-ChildItem -LiteralPath $extractRoot -File -Filter "*.nuspec")
    if ($nuspec.Count -ne 1) {
        throw "Expected one nuspec in '$PackagePath', found $($nuspec.Count)."
    }

    [xml]$document = Get-Content -LiteralPath $nuspec[0].FullName -Raw -Encoding UTF8
    $dependencies = @($document.SelectNodes("//*[local-name()='dependency']"))
    foreach ($dependency in $dependencies) {
        if ([string]$dependency.id -in @("Microsoft.Maui.Controls", "Microsoft.Maui.Essentials")) {
            [void]$dependency.ParentNode.RemoveChild($dependency)
        }
    }
    $settings = [Xml.XmlWriterSettings]::new()
    $settings.Encoding = [Text.UTF8Encoding]::new($false)
    $settings.Indent = $true
    $writer = [Xml.XmlWriter]::Create($nuspec[0].FullName, $settings)
    try {
        $document.Save($writer)
    }
    finally {
        $writer.Dispose()
    }

    $coreProperties = @(
        Get-ChildItem `
            -LiteralPath (Join-Path $extractRoot "package\services\metadata\core-properties") `
            -File `
            -Filter "*.psmdcp"
    )
    if ($coreProperties.Count -ne 1) {
        throw "Expected one NuGet core-properties document in '$PackagePath'."
    }
    $fixedCorePropertiesPath = Join-Path $coreProperties[0].DirectoryName "core-properties.psmdcp"
    if ($coreProperties[0].FullName -ne $fixedCorePropertiesPath) {
        Move-Item -LiteralPath $coreProperties[0].FullName -Destination $fixedCorePropertiesPath -Force
    }

    $relationshipsPath = Join-Path $extractRoot "_rels\.rels"
    [xml]$relationships = Get-Content -LiteralPath $relationshipsPath -Raw -Encoding UTF8
    $relationshipNodes = @($relationships.SelectNodes("//*[local-name()='Relationship']"))
    foreach ($relationship in $relationshipNodes) {
        switch ([string]$relationship.Type) {
            "http://schemas.microsoft.com/packaging/2010/07/manifest" {
                $relationship.Id = "manifest"
            }
            "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" {
                $relationship.Id = "core-properties"
                $relationship.Target = "/package/services/metadata/core-properties/core-properties.psmdcp"
            }
        }
    }
    $relationshipsWriter = [Xml.XmlWriter]::Create($relationshipsPath, $settings)
    try {
        $relationships.Save($relationshipsWriter)
    }
    finally {
        $relationshipsWriter.Dispose()
    }

    New-DeterministicArchive $extractRoot $DestinationPath
}

function Apply-UiEvidenceCompatibilityPatch {
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourceDirectory
    )

    $walkerPath = Join-Path $SourceDirectory "src\DevFlow\Microsoft.Maui.DevFlow.Agent\VisualTreeWalker.cs"
    $walker = Get-Content -LiteralPath $walkerPath -Raw -Encoding UTF8
    $walker = [Regex]::Replace(
        $walker,
        '(?m)^using Microsoft\.Maui\.DevFlow\.Agent\.Windows;\r?\n',
        '')
    $walker = [Regex]::Replace(
        $walker,
        '(?ms)^#if WINDOWS\r?\n    private readonly NativeWindowProbe _nativeProbe = new\(\);\r?\n    private readonly object _nativeObjectsLock = new\(\);\r?\n    private Dictionary<string, object> _nativeObjects = new\(StringComparer\.OrdinalIgnoreCase\);\r?\n#endif\r?\n',
        '')

    $nativeStart = $walker.IndexOf(
        "    public override bool SupportsNativeElements => true;",
        [StringComparison]::Ordinal)
    $nativeEnd = $walker.IndexOf(
        "    private static IntPtr GetWindowHandle",
        [StringComparison]::Ordinal)
    if ($nativeStart -lt 0 -or $nativeEnd -le $nativeStart) {
        throw "Unable to locate the pinned Windows native UI Automation block."
    }

    $nativeStubs = @"
    public override bool SupportsNativeElements => false;

    public override IReadOnlyList<IntPtr> GetKnownNativeWindowHandles(Application app, int? windowIndex = null)
        => Array.Empty<IntPtr>();

    public override List<ElementInfo> WalkNativeTree(IReadOnlyList<IntPtr> knownWindowHandles, int maxDepth = 0)
        => [];

    public override object? GetNativeElementById(string id) => null;

    public override ElementInfo? GetNativeElementInfoById(string id) => null;

    public override string TryNativeElementTap(string elementId)
        => "Native Windows UI Automation is disabled in the UI evidence adapter";

    public override string TryNativeElementSetValue(string elementId, string value)
        => "Native Windows UI Automation is disabled in the UI evidence adapter";

    public override string TryNativeElementFocus(string elementId)
        => "Native Windows UI Automation is disabled in the UI evidence adapter";

    public override string TryNativeElementScroll(string elementId, double deltaX, double deltaY)
        => "Native Windows UI Automation is disabled in the UI evidence adapter";

"@
    $walker = $walker.Substring(0, $nativeStart) + $nativeStubs + $walker.Substring($nativeEnd)
    Set-Content -LiteralPath $walkerPath -Value $walker -Encoding UTF8

    $layoutWalkerPath = Join-Path $SourceDirectory "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\LayoutDiagnostics\VisualTreeWalker.LayoutDiagnostics.cs"
    $layoutWalker = Get-Content -LiteralPath $layoutWalkerPath -Raw -Encoding UTF8
    if (-not $layoutWalker.Contains("using Microsoft.Maui.Graphics;", [StringComparison]::Ordinal)) {
        $layoutWalker = $layoutWalker.Replace(
            "using Microsoft.Maui.Controls.Shapes;",
            "using Microsoft.Maui.Controls.Shapes;`r`nusing Microsoft.Maui.Graphics;",
            [StringComparison]::Ordinal)
        Set-Content -LiteralPath $layoutWalkerPath -Value $layoutWalker -Encoding UTF8
    }

    $serviceExtensionsPath = Join-Path $SourceDirectory "src\DevFlow\Microsoft.Maui.DevFlow.Agent\AgentServiceExtensions.cs"
    $serviceExtensions = Get-Content -LiteralPath $serviceExtensionsPath -Raw -Encoding UTF8
    foreach ($usingDirective in @(
        "using Microsoft.Maui.ApplicationModel;",
        "using Microsoft.Maui.Devices;",
        "using Microsoft.Maui.Storage;"
    )) {
        if (-not $serviceExtensions.Contains($usingDirective, [StringComparison]::Ordinal)) {
            $serviceExtensions = "$usingDirective`r`n$serviceExtensions"
        }
    }
    Set-Content -LiteralPath $serviceExtensionsPath -Value $serviceExtensions -Encoding UTF8

    $sourceReplacements = @{
        "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\FileStoragePathResolver.cs" = @{
            "requestedPath.IndexOf('\0')" = 'requestedPath.IndexOf("\0", StringComparison.Ordinal)'
        }
        "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\SourceMapping\XamlSourceMap.cs" = @{
            "remainder.IndexOf(';')" = "remainder.IndexOf(`";`", StringComparison.Ordinal)"
        }
        "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\AgentHttpServer.cs" = @{
            "fullPath.IndexOf('?')" = "fullPath.IndexOf(`"?`", StringComparison.Ordinal)"
            "lines[i].IndexOf(':')" = "lines[i].IndexOf(`":`", StringComparison.Ordinal)"
        }
        "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\DevFlowAgentService.cs" = @{
            "version.IndexOf('.')" = "version.IndexOf(`".`", StringComparison.Ordinal)"
            "gestureType.Name.Contains(`"TapGesture`")" = "gestureType.Name.Contains(`"TapGesture`", StringComparison.Ordinal)"
            "gestureType.BaseType.Name.Contains(`"TapGesture`")" = "gestureType.BaseType.Name.Contains(`"TapGesture`", StringComparison.Ordinal)"
        }
        "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\VisualTreeWalker.cs" = @{
            'marker.GetType().Name.Replace("Marker", "")' = 'marker.GetType().Name.Replace("Marker", "", StringComparison.Ordinal)'
            'gr.GetType().Name.Replace("GestureRecognizer", "")' = 'gr.GetType().Name.Replace("GestureRecognizer", "", StringComparison.Ordinal)'
        }
    }
    foreach ($fileEntry in $sourceReplacements.GetEnumerator()) {
        $path = Join-Path $SourceDirectory $fileEntry.Key
        $content = Get-Content -LiteralPath $path -Raw -Encoding UTF8
        foreach ($replacement in $fileEntry.Value.GetEnumerator()) {
            if (-not $content.Contains([string]$replacement.Key, [StringComparison]::Ordinal)) {
                throw "Unable to locate pinned compatibility expression '$($replacement.Key)'."
            }
            $content = $content.Replace(
                [string]$replacement.Key,
                [string]$replacement.Value,
                [StringComparison]::Ordinal)
        }
        Set-Content -LiteralPath $path -Value $content -Encoding UTF8
    }

    $agentProjectPath = Join-Path $SourceDirectory "src\DevFlow\Microsoft.Maui.DevFlow.Agent\Microsoft.Maui.DevFlow.Agent.csproj"
    [xml]$agentProject = Get-Content -LiteralPath $agentProjectPath -Raw -Encoding UTF8
    $frameworkReference = $agentProject.SelectSingleNode(
        "/Project/ItemGroup/FrameworkReference[@Include='Microsoft.WindowsDesktop.App']")
    if ($null -eq $frameworkReference) {
        throw "Unable to locate the pinned WindowsDesktop framework reference."
    }
    [void]$frameworkReference.ParentNode.RemoveChild($frameworkReference)
    $targetFrameworkNodes = @($agentProject.SelectNodes("/Project/PropertyGroup/TargetFrameworks"))
    foreach ($targetFrameworkNode in $targetFrameworkNodes) {
        [void]$targetFrameworkNode.ParentNode.RemoveChild($targetFrameworkNode)
    }
    $primaryPropertyGroup = $agentProject.SelectSingleNode("/Project/PropertyGroup[1]")
    $targetFrameworks = $agentProject.CreateElement("TargetFrameworks")
    $targetFrameworks.InnerText = "net10.0-android;net10.0-windows10.0.19041.0"
    [void]$primaryPropertyGroup.PrependChild($targetFrameworks)
    $compileGroup = $agentProject.CreateElement("ItemGroup")
    $compileRemove = $agentProject.CreateElement("Compile")
    $compileRemove.SetAttribute("Remove", "Windows\NativeWindowProbe.cs")
    [void]$compileGroup.AppendChild($compileRemove)
    [void]$agentProject.DocumentElement.AppendChild($compileGroup)
    $agentControlsReference = $agentProject.SelectSingleNode(
        "/Project/ItemGroup/PackageReference[@Include='Microsoft.Maui.Controls']")
    if ($null -eq $agentControlsReference) {
        throw "Unable to locate the Agent MAUI Controls package reference."
    }
    $agentControlsReference.SetAttribute("Condition", "'`$(MauiUiEvidenceMauiRoot)' == ''")
    $agentSourceReferences = $agentProject.CreateElement("ItemGroup")
    $agentSourceReferences.SetAttribute("Condition", "'`$(MauiUiEvidenceMauiRoot)' != ''")
    $agentControlsProject = $agentProject.CreateElement("ProjectReference")
    $agentControlsProject.SetAttribute(
        "Include",
        "`$(MauiUiEvidenceMauiRoot)\src\Controls\src\Core\Controls.Core.csproj")
    [void]$agentSourceReferences.AppendChild($agentControlsProject)
    [void]$agentProject.DocumentElement.AppendChild($agentSourceReferences)
    $xmlSettings = [Xml.XmlWriterSettings]::new()
    $xmlSettings.Encoding = [Text.UTF8Encoding]::new($false)
    $xmlSettings.Indent = $true
    $projectWriter = [Xml.XmlWriter]::Create($agentProjectPath, $xmlSettings)
    try {
        $agentProject.Save($projectWriter)
    }
    finally {
        $projectWriter.Dispose()
    }

    $coreProjectPath = Join-Path $SourceDirectory "src\DevFlow\Microsoft.Maui.DevFlow.Agent.Core\Microsoft.Maui.DevFlow.Agent.Core.csproj"
    [xml]$coreProject = Get-Content -LiteralPath $coreProjectPath -Raw -Encoding UTF8
    foreach ($packageId in @("Microsoft.Maui.Controls", "Microsoft.Maui.Essentials")) {
        $packageReference = $coreProject.SelectSingleNode(
            "/Project/ItemGroup/PackageReference[@Include='$packageId']")
        if ($null -eq $packageReference) {
            throw "Unable to locate Agent.Core package reference '$packageId'."
        }
        $packageReference.SetAttribute("Condition", "'`$(MauiUiEvidenceMauiRoot)' == ''")
    }
    $coreSourceReferences = $coreProject.CreateElement("ItemGroup")
    $coreSourceReferences.SetAttribute("Condition", "'`$(MauiUiEvidenceMauiRoot)' != ''")
    foreach ($projectPath in @(
        "`$(MauiUiEvidenceMauiRoot)\src\Controls\src\Core\Controls.Core.csproj",
        "`$(MauiUiEvidenceMauiRoot)\src\Essentials\src\Essentials.csproj"
    )) {
        $projectReference = $coreProject.CreateElement("ProjectReference")
        $projectReference.SetAttribute("Include", $projectPath)
        [void]$coreSourceReferences.AppendChild($projectReference)
    }
    [void]$coreProject.DocumentElement.AppendChild($coreSourceReferences)
    $coreWriter = [Xml.XmlWriter]::Create($coreProjectPath, $xmlSettings)
    try {
        $coreProject.Save($coreWriter)
    }
    finally {
        $coreWriter.Dispose()
    }

    $globalJsonPath = Join-Path $SourceDirectory "global.json"
    $globalJson = Get-Content -LiteralPath $globalJsonPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $globalJson.sdk.version = "10.0.100"
    $globalJson.sdk.rollForward = "latestPatch"
    $globalJson |
        ConvertTo-Json -Depth 16 |
        Set-Content -LiteralPath $globalJsonPath -Encoding UTF8

    $versionsPath = Join-Path $SourceDirectory "eng\Versions.props"
    [xml]$versions = Get-Content -LiteralPath $versionsPath -Raw -Encoding UTF8
    foreach ($node in @($versions.SelectNodes(
        "/Project/PropertyGroup/*[starts-with(local-name(), 'MicrosoftExtensions')]"))) {
        $node.InnerText = "10.0.0"
    }
    foreach ($version in @{
        SystemTextJsonVersion = "10.0.0"
        FizzlerVersion = "1.3.0"
        SkiaSharpVersion = "3.116.1"
    }.GetEnumerator()) {
        $node = $versions.SelectSingleNode("/Project/PropertyGroup/$($version.Key)")
        if ($null -eq $node) {
            throw "Unable to locate DevFlow dependency property '$($version.Key)'."
        }
        $node.InnerText = $version.Value
    }
    $versionsWriter = [Xml.XmlWriter]::Create($versionsPath, $xmlSettings)
    try {
        $versions.Save($versionsWriter)
    }
    finally {
        $versionsWriter.Dispose()
    }

}

$configuration = Read-UiEvidenceJson $ConfigurationPath
if ($configuration.schemaVersion -ne 1) {
    throw "Unsupported DevFlow provisioning configuration version '$($configuration.schemaVersion)'."
}
Assert-UiEvidenceSha ([string]$configuration.commit) "DevFlow commit"
if (Test-Path -LiteralPath $DotNetPath -PathType Leaf) {
    $DotNetPath = (Resolve-Path -LiteralPath $DotNetPath).Path
}

$ownsWorkingDirectory = [string]::IsNullOrWhiteSpace($WorkingDirectory)
if ($ownsWorkingDirectory) {
    $WorkingDirectory = Join-Path ([IO.Path]::GetTempPath()) ("maui-ui-evidence-devflow-" + [Guid]::NewGuid().ToString("N"))
}
$WorkingDirectory = [IO.Path]::GetFullPath($WorkingDirectory)
$sourceDirectory = Join-Path $WorkingDirectory "source"
$providedSourceRoot = if ($SourceRoot) { [IO.Path]::GetFullPath($SourceRoot) } else { $null }
$rawFeed = Join-Path $WorkingDirectory "raw-feed"
$packageWork = Join-Path $WorkingDirectory "package-work"
$OutputDirectory = [IO.Path]::GetFullPath($OutputDirectory)

try {
    New-Item -ItemType Directory -Force -Path $WorkingDirectory, $rawFeed, $packageWork, $OutputDirectory | Out-Null
    Remove-Item -LiteralPath (Join-Path $OutputDirectory "*.nupkg") -Force -ErrorAction SilentlyContinue

    if ($providedSourceRoot) {
        $actualCommit = (& git -C $providedSourceRoot rev-parse HEAD).Trim()
        if ($LASTEXITCODE -ne 0 -or $actualCommit -ne [string]$configuration.commit) {
            throw "DevFlow source root is not at the configured commit '$($configuration.commit)'."
        }
        Invoke-CheckedProcess git @(
            "clone",
            "--quiet",
            "--no-checkout",
            $providedSourceRoot,
            $sourceDirectory
        ) $WorkingDirectory
        Invoke-CheckedProcess git @(
            "-C",
            $sourceDirectory,
            "sparse-checkout",
            "init",
            "--cone"
        ) $WorkingDirectory
        Invoke-CheckedProcess git @(
            "-C",
            $sourceDirectory,
            "sparse-checkout",
            "set",
            "src/DevFlow",
            "eng"
        ) $WorkingDirectory
        Invoke-CheckedProcess git @(
            "-C",
            $sourceDirectory,
            "checkout",
            "--quiet",
            "--detach",
            [string]$configuration.commit
        ) $WorkingDirectory
    }
    else {
        Invoke-CheckedProcess git @("init", "--quiet", $sourceDirectory) $WorkingDirectory
        Invoke-CheckedProcess git @("-C", $sourceDirectory, "remote", "add", "origin", [string]$configuration.repository) $WorkingDirectory
        Invoke-CheckedProcess git @("-C", $sourceDirectory, "fetch", "--quiet", "--depth", "1", "origin", [string]$configuration.commit) $WorkingDirectory
        Invoke-CheckedProcess git @("-C", $sourceDirectory, "sparse-checkout", "init", "--cone") $WorkingDirectory
        Invoke-CheckedProcess git @("-C", $sourceDirectory, "sparse-checkout", "set", "src/DevFlow", "eng") $WorkingDirectory
        Invoke-CheckedProcess git @("-C", $sourceDirectory, "checkout", "--quiet", "--detach", "FETCH_HEAD") $WorkingDirectory
    }

    Invoke-CheckedProcess git @(
        "-C",
        $sourceDirectory,
        "remote",
        "set-url",
        "origin",
        [string]$configuration.repository
    ) $WorkingDirectory
    Apply-UiEvidenceCompatibilityPatch $sourceDirectory

    $version = "$($configuration.packageVersionPrefix).$(([string]$configuration.commit).Substring(0, 8))"
    $projects = @(
        @{ Path = "src\DevFlow\Microsoft.Maui.DevFlow.Driver\Microsoft.Maui.DevFlow.Driver.csproj"; Extra = @("-p:PackDriverNuGet=true") }
    )

    foreach ($project in $projects) {
        $arguments = @(
            "pack",
            (Join-Path $sourceDirectory $project.Path),
            "-c", "Release",
            "-o", $rawFeed,
            "-p:Version=$version",
            "-p:Deterministic=true",
            "-p:ContinuousIntegrationBuild=true",
            "-p:PathMap=$sourceDirectory=/src",
            "-p:PublishWindowsPdb=false",
            "--nologo"
        ) + @($project.Extra)
        Invoke-CheckedProcess $DotNetPath $arguments $sourceDirectory
    }

    $rawPackages = @(Get-ChildItem -LiteralPath $rawFeed -File -Filter "*.nupkg" |
        Where-Object { $_.Name -notlike "*.symbols.nupkg" } |
        Sort-Object Name)
    foreach ($expectedPackage in @($configuration.packages)) {
            $packagePattern = "^$([Regex]::Escape([string]$expectedPackage))\.\d"
            if (@($rawPackages | Where-Object {
                $_.Name -match $packagePattern
            }).Count -ne 1) {
                throw "Expected exactly one packed '$expectedPackage' package."
            }
    }

    foreach ($package in $rawPackages) {
        $destination = Join-Path $OutputDirectory $package.Name
        Remove-MauiPackageDependencies $package.FullName $destination $packageWork
    }

    $packageEntries = @(
        Get-ChildItem -LiteralPath $OutputDirectory -File -Filter "*.nupkg" |
            Sort-Object Name |
            ForEach-Object {
                [PSCustomObject][ordered]@{
                    fileName = $_.Name
                    sizeBytes = [long]$_.Length
                    sha256 = Get-UiEvidenceSha256 $_.FullName
                }
            }
    )
    $manifest = [PSCustomObject][ordered]@{
        schemaVersion = 1
        repository = [string]$configuration.repository
        commit = [string]$configuration.commit
        packageVersion = $version
        mauiDependenciesRemoved = $true
        windowsNativeUiAutomationDisabled = $true
        targetFrameworks = @("net10.0-android", "net10.0-windows10.0.19041.0")
        dependencyBaseline = "dotnet-maui"
        packages = $packageEntries
    }
    Write-UiEvidenceJson $manifest (Join-Path $OutputDirectory "devflow-manifest.json")

    if ($SourceOutputDirectory) {
        $sourceOutput = New-UiEvidenceOutputDirectory $SourceOutputDirectory @($sourceDirectory)
        Get-ChildItem -LiteralPath $sourceDirectory -Force |
            Where-Object { $_.Name -notin @(".git", "artifacts", ".packages") } |
            Copy-Item -Destination $sourceOutput -Recurse -Force
        $sourceFiles = @(
            Get-UiEvidenceFiles $sourceOutput |
                Sort-Object FullName |
                ForEach-Object {
                    [PSCustomObject][ordered]@{
                        relativePath = Get-UiEvidenceRelativePath $sourceOutput $_.FullName
                        sizeBytes = [long]$_.Length
                        sha256 = Get-UiEvidenceSha256 $_.FullName
                    }
                }
        )
        Write-UiEvidenceJson ([PSCustomObject][ordered]@{
            schemaVersion = 1
            repository = [string]$configuration.repository
            commit = [string]$configuration.commit
            compatibility = [PSCustomObject][ordered]@{
                dependencyBaseline = "dotnet-maui"
                windowsNativeUiAutomationDisabled = $true
                sourceMauiProjectReferencesEnabled = $true
            }
            files = $sourceFiles
        }) (Join-Path $sourceOutput "devflow-source-manifest.json")
    }
    Write-Host "Provisioned DevFlow $version from $($configuration.commit) into $OutputDirectory."
}
finally {
    if ($ownsWorkingDirectory) {
        Remove-Item -LiteralPath $WorkingDirectory -Recurse -Force -ErrorAction SilentlyContinue
    }
}
