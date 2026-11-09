Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-MauiInflightLabelCondition {
    param ([string] $SandboxPrefix)
    $condition = "'`$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current' or `$([System.Text.RegularExpressions.Regex]::IsMatch('`$(BUILD_SOURCEBRANCH)', '^refs/heads/inflight/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*`$'))"
    if ($SandboxPrefix) {
        $condition += " or `$([System.Text.RegularExpressions.Regex]::IsMatch('`$(BUILD_SOURCEBRANCH)', '^refs/heads/$([Regex]::Escape($SandboxPrefix))/inflight/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*`$'))"
    }
    return $condition
}

function Get-MauiBranchLabelCondition {
    param (
        [Parameter(Mandatory)][ValidateSet('ci.inflight', 'servicing')][string] $Label,
        [string] $SandboxPrefix
    )
    $branch = if ($Label -ceq 'servicing') { 'release' } else { 'inflight' }
    $condition = "`$([System.Text.RegularExpressions.Regex]::IsMatch('`$(BUILD_SOURCEBRANCH)', '\Arefs/heads/$branch/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*\z'))"
    if ($Label -ceq 'ci.inflight') {
        $condition = "'`$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current' or $condition"
    }
    if ($SandboxPrefix) {
        $condition += " or `$([System.Text.RegularExpressions.Regex]::IsMatch('`$(BUILD_SOURCEBRANCH)', '\Arefs/heads/$([Regex]::Escape($SandboxPrefix))/$branch/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*\z'))"
    }
    return $condition
}

function Get-MauiStabilizationDefault {
    param ([string] $SandboxPrefix)
    $branch = if ($SandboxPrefix) { "(?:release|$([Regex]::Escape($SandboxPrefix))/release)" } else { 'release' }
    "`$([System.Text.RegularExpressions.Regex]::IsMatch('`$(PreReleaseVersionLabel):`$(BUILD_SOURCEBRANCH)', '\Aservicing:refs/heads/$branch/[1-9][0-9]*\.0\.[1-9][0-9]*xx-sr[1-9][0-9]*\z').ToString().ToLowerInvariant())"
}

function Get-MauiReleaseVersion {
    [CmdletBinding()]
    param ([Parameter(Mandatory)][string] $Content)

    $document = [System.Xml.XmlDocument]::new()
    $document.PreserveWhitespace = $true
    $document.XmlResolver = $null
    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $reader = [System.Xml.XmlReader]::Create([System.IO.StringReader]::new($Content), $settings)
    try { $document.Load($reader) } finally { $reader.Dispose() }
    $values = @{}
    $versionGroup = $null
    foreach ($name in @('MajorVersion', 'MinorVersion', 'PatchVersion', 'SdkBandVersion',
            'PreReleaseVersionIteration', 'StabilizePackageVersion')) {
        $nodes = @($document.SelectNodes("/Project/PropertyGroup/$name"))
        if ($nodes.Count -ne 1 -or $nodes[0].SelectNodes('*').Count -ne 0) {
            throw "Versions.props must contain exactly one literal '$name'."
        }
        if ($null -eq $versionGroup) { $versionGroup = $nodes[0].ParentNode }
        if ($nodes[0].ParentNode -ne $versionGroup -or $versionGroup.Attributes.Count -ne 0) {
            throw 'Release version properties must share one unconditional PropertyGroup.'
        }
        if ($name -ne 'StabilizePackageVersion' -and $nodes[0].Attributes.Count -ne 0) {
            throw "Conditional '$name' is not supported for automatic release configuration."
        }
        $values[$name] = $nodes[0].InnerText.Trim()
    }
    foreach ($name in @('MajorVersion', 'MinorVersion', 'PatchVersion')) {
        if ($values[$name] -cnotmatch '^(0|[1-9][0-9]*)$' -or [long] $values[$name] -gt [int]::MaxValue) {
            throw "Versions.props has an invalid '$name'."
        }
    }
    $labels = @($document.SelectNodes('/Project/PropertyGroup/PreReleaseVersionLabel'))
    $defaultLabels = @($labels | Where-Object { !$_.HasAttribute('Condition') })
    $branchDerivedLabels = $labels.Count -eq 3
    $labelSandboxPrefix = $null
    if ($defaultLabels.Count -ne 1 -or $labels.Count -gt 3 -or
        $defaultLabels[0].Attributes.Count -ne 0 -or
        $defaultLabels[0].InnerText -cnotin @('ci.main', 'servicing')) {
        throw "Only existing ci.main or servicing package branding can be configured automatically."
    }
    if ($branchDerivedLabels) {
        if ($labels[0] -ne $defaultLabels[0] -or $labels[0].InnerText -cne 'ci.main' -or
            $labels[1].InnerText -cne 'ci.inflight' -or $labels[2].InnerText -cne 'servicing') {
            throw 'The central branch-label policy must contain ci.main, ci.inflight, and servicing in that order.'
        }
        $properties = @($versionGroup.SelectNodes('*'))
        $labelIndex = [Array]::IndexOf($properties, $labels[0])
        if ($labelIndex -lt 0 -or $labelIndex + 2 -ge $properties.Count -or
            $properties[$labelIndex + 1] -ne $labels[1] -or $properties[$labelIndex + 2] -ne $labels[2] -or
            $labelIndex + 2 -ge [Array]::IndexOf($properties, $document.SelectSingleNode('/Project/PropertyGroup/IsServicingBuild'))) {
            throw 'Central branch labels must be consecutive properties before IsServicingBuild.'
        }
        if ($labels[1].GetAttribute('Condition') -cmatch 'refs/heads/(?<sandbox>release-rollover-trial/[a-z0-9][a-z0-9-]{0,40})/inflight/') {
            $labelSandboxPrefix = $Matches.sandbox
        }
    }
    foreach ($label in $labels) {
        if ($label.ParentNode -ne $versionGroup -or $label.SelectNodes('*').Count -ne 0 -or
            ($label.HasAttribute('Condition') -and
                ($label.Attributes.Count -ne 1 -or
                    ($label.InnerText -cne 'ci.inflight' -and (!$branchDerivedLabels -or $label.InnerText -cne 'servicing'))))) {
            throw "Versions.props has an unsupported prerelease-label override."
        }
        if ($label.HasAttribute('Condition')) {
            $condition = $label.GetAttribute('Condition')
            if ($branchDerivedLabels) {
                if ($condition -cne (Get-MauiBranchLabelCondition -Label $label.InnerText -SandboxPrefix $labelSandboxPrefix)) {
                    throw 'Unknown central branch-label condition requires review; it will not be replaced automatically.'
                }
                continue
            }
            $sandbox = if ($condition -cmatch 'refs/heads/(?<sandbox>release-rollover-trial/[a-z0-9][a-z0-9-]{0,40})/inflight/') {
                $Matches.sandbox
            } else { $null }
            if ($condition -cne "'`$(BUILD_SOURCEBRANCH)' == 'refs/heads/inflight/current'" -and
                $condition -cne (Get-MauiInflightLabelCondition -SandboxPrefix $sandbox)) {
                throw 'Unknown CI-label condition requires review; it will not be replaced automatically.'
            }
        }
    }
    $stable = $document.SelectSingleNode('/Project/PropertyGroup/StabilizePackageVersion')
    $automaticStabilization = $branchDerivedLabels -and
        $values.StabilizePackageVersion -ceq (Get-MauiStabilizationDefault -SandboxPrefix $labelSandboxPrefix)
    if ($stable.Attributes.Count -ne 1 -or
        $stable.GetAttribute('Condition') -cne "'`$(StabilizePackageVersion)' == ''" -or
        (!$automaticStabilization -and $values.StabilizePackageVersion -cnotin @('true', 'false')) -or
        $values.PreReleaseVersionIteration -cne '') {
        throw "Automatic servicing configuration requires the standard stabilization default and an empty prerelease iteration."
    }
    if ($automaticStabilization -and
        [Array]::IndexOf($properties, $stable) -ge [Array]::IndexOf($properties,
            $document.SelectSingleNode('/Project/PropertyGroup/DotNetFinalVersionKind'))) {
        throw 'Automatic stabilization must precede the final-version condition.'
    }
    foreach ($required in @(
            @{ name = 'IsServicingBuild'; condition = "'`$(PreReleaseVersionLabel)' == 'servicing'"; value = 'true' },
            @{ name = 'DotNetFinalVersionKind'; condition = "'`$(StabilizePackageVersion)' == 'true'"; value = 'release' }
        )) {
        $nodes = @($document.SelectNodes("/Project/PropertyGroup/$($required.name)"))
        if ($nodes.Count -ne 1 -or $nodes[0].ParentNode -ne $versionGroup -or
            $nodes[0].Attributes.Count -ne 1 -or
            $nodes[0].GetAttribute('Condition').Trim() -cne $required.condition -or
            $nodes[0].InnerText.Trim() -cne $required.value) {
            throw "Versions.props has unsupported '$($required.name)' wiring; release configuration requires review."
        }
    }
    [pscustomobject]@{
        major = [int] $values.MajorVersion
        minor = [int] $values.MinorVersion
        patch = [int] $values.PatchVersion
        sdkBand = $values.SdkBandVersion
        label = $defaultLabels[0].InnerText
        stable = $values.StabilizePackageVersion -ceq 'true'
        labelCount = $labels.Count
        branchDerivedLabels = $branchDerivedLabels
        labelSandboxPrefix = $labelSandboxPrefix
        automaticStabilization = $automaticStabilization
    }
}

function Get-MauiReleaseVersionContent {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $Content,
        [Parameter(Mandatory)][string] $ReleaseBand,
        [Parameter(Mandatory)][ValidateRange(1, 214748362)][int] $Sr,
        [Parameter(Mandatory)][ValidateSet('candidate', 'servicing', 'fixes', 'inflight')][string] $Role,
        [int] $SourceSr = $Sr,
        [string] $SandboxPrefix
    )

    if ($ReleaseBand -cnotmatch '^(?<major>[1-9][0-9]*)\.0\.[1-9][0-9]*xx$') {
        throw "Invalid release band '$ReleaseBand'."
    }
    $major = [int] $Matches.major
    if ($SandboxPrefix -and $SandboxPrefix -cnotmatch '^release-rollover-trial/[a-z0-9][a-z0-9-]{0,40}$') {
        throw 'Invalid sandbox prefix.'
    }
    if ($SourceSr -lt 1 -or $SourceSr -gt $Sr -or $SourceSr -lt ($Sr - 1) -or
        ($Role -cin @('candidate', 'servicing') -and $SourceSr -ne $Sr)) {
        throw 'Candidate and servicing preparation preserve the verified cycle; CI lanes may advance exactly one verified cycle.'
    }
    $version = Get-MauiReleaseVersion -Content $Content
    if ($version.branchDerivedLabels -and $version.labelSandboxPrefix -and
        $version.labelSandboxPrefix -cne $SandboxPrefix) {
        throw 'A scoped central label policy requires its exact authorized sandbox scope.'
    }
    if ($version.major -ne $major -or $version.minor -ne 0 -or
        $version.sdkBand -cne $ReleaseBand.Replace('xx', '00') -or
        $version.patch -lt ([long] $SourceSr * 10) -or $version.patch -ge (([long] $SourceSr + 1) * 10)) {
        throw "Version metadata does not match $ReleaseBand SR$SourceSr. A branch suffix is not package-version evidence."
    }
    $newline = if ($Content.Contains("`r`n")) { "`r`n" } else { "`n" }
    $labelPattern = '(?m)^(?<indent>[ \t]*)<PreReleaseVersionLabel(?: [^<>\r\n]*)?>[^<\r\n]*</PreReleaseVersionLabel>[ \t]*(?:\r?\n|$)'
    $labelMatches = [Regex]::Matches($Content, $labelPattern)
    if ($labelMatches.Count -ne $version.labelCount) {
        throw 'Unsupported prerelease-label layout; no source content was replaced.'
    }
    if (!$version.branchDerivedLabels -or ($SandboxPrefix -and $version.labelSandboxPrefix -cne $SandboxPrefix)) {
        $indent = $labelMatches[0].Groups['indent'].Value
        $replacement = "${indent}<PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>$newline"
        if ($Role -cne 'servicing' -or $version.branchDerivedLabels) {
            $condition = if ($version.branchDerivedLabels) {
                Get-MauiBranchLabelCondition -Label ci.inflight -SandboxPrefix $SandboxPrefix
            } else { Get-MauiInflightLabelCondition -SandboxPrefix $SandboxPrefix }
            $condition = $condition.Replace('&', '&amp;').Replace('"', '&quot;').Replace('<', '&lt;').Replace('>', '&gt;')
            $replacement = "${indent}<PreReleaseVersionLabel>ci.main</PreReleaseVersionLabel>$newline" +
                "${indent}<PreReleaseVersionLabel Condition=`"$condition`">ci.inflight</PreReleaseVersionLabel>$newline"
            if ($version.branchDerivedLabels) {
                $condition = Get-MauiBranchLabelCondition -Label servicing -SandboxPrefix $SandboxPrefix
                $condition = $condition.Replace('&', '&amp;').Replace('"', '&quot;').Replace('<', '&lt;').Replace('>', '&gt;')
                $replacement += "${indent}<PreReleaseVersionLabel Condition=`"$condition`">servicing</PreReleaseVersionLabel>$newline"
            }
        }
        # Match only XML-validated leaf properties, preserving all unrelated bytes.
        for ($i = $labelMatches.Count - 1; $i -ge 0; $i--) {
            $match = $labelMatches[$i]
            $text = if ($i -eq 0) { $replacement } else { '' }
            $Content = $Content.Remove($match.Index, $match.Length).Insert($match.Index, $text)
        }
    }
    foreach ($change in @(
            @{ name = 'PatchVersion'; value = if ($SourceSr -eq $Sr) { $version.patch } else { $Sr * 10 } },
            @{ name = 'StabilizePackageVersion'; value = if ($version.automaticStabilization) {
                    Get-MauiStabilizationDefault -SandboxPrefix $SandboxPrefix
                } elseif ($Role -ceq 'servicing') { 'true' } else { 'false' } }
        )) {
        $pattern = "(?<open><$($change.name)(?: [^<>]*)?>)[^<]*(?<close></$($change.name)>)"
        $propertyMatches = [Regex]::Matches($Content, $pattern)
        if ($propertyMatches.Count -ne 1) { throw "Unsupported '$($change.name)' layout." }
        $match = $propertyMatches[0]
        $Content = $Content.Remove($match.Index, $match.Length).Insert(
            $match.Index, $match.Groups['open'].Value + $change.value + $match.Groups['close'].Value)
    }
    $null = Get-MauiReleaseVersion -Content $Content
    return $Content
}

function Get-MauiReleasedVersionContent {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $Content,
        [Parameter(Mandatory)][string] $Tag
    )

    if ($Tag -cnotmatch '^[1-9][0-9]*\.0\.(0|[1-9][0-9]*)$') {
        throw 'Bug-template housekeeping requires a valid release version.'
    }
    $newVersion = [System.Management.Automation.SemanticVersion]::new($Tag)
    $Content = [Regex]::Replace($Content,
        '(?m)^(        - [1-9][0-9]*\.0\.(?:0|[1-9][0-9]*)) \(candidate\)(\r?)$', '$1$2')
    foreach ($id in @('version-with-bug', 'version-that-worked')) {
        $sections = @([Regex]::Matches($Content, '(?ms)^  - type: [^\r\n]+.*?(?=^  - type: |\z)') |
            Where-Object { $_.Value -cmatch "(?m)^    id: $id[ \t]*`r?`$" })
        if ($sections.Count -ne 1) { throw "Bug template must contain exactly one '$id' field." }
        $section = $sections[0]
        $options = [Regex]::Matches($section.Value, '(?m)^      options:\r?\n(?<items>(?:^        -[^\r\n]*(?:\r?\n|$))+)')
        if ($options.Count -ne 1) { throw "Bug template has an unsupported '$id' options list." }
        $items = [Regex]::Matches($options[0].Groups['items'].Value, '(?m)^        -(?<value>[^\r\n]*)(?:\r?\n|$)')
        $versions = @(foreach ($item in $items) {
            $value = $item.Groups['value'].Value.Trim()
            $parsed = $null
            if ([System.Management.Automation.SemanticVersion]::TryParse(($value -split ' ')[0], [ref] $parsed)) {
                [pscustomobject]@{ item = $item; version = $parsed }
            }
        })
        if (@($versions | Where-Object { $_.version -eq $newVersion }).Count) { continue }
        if ($versions.Count -eq 0) { throw "Bug template '$id' contains no version insertion anchors." }
        $newline = if ($section.Value.Contains("`r`n")) { "`r`n" } else { "`n" }
        $following = @($versions | Where-Object {
            if ($id -ceq 'version-with-bug') { $_.version -lt $newVersion } else { $_.version -gt $newVersion }
        })
        $offset = if ($following.Count -gt 0) {
            $following[0].item.Index
        } else {
            $last = $versions[-1].item
            $last.Index + $last.Length
        }
        $position = $section.Index + $options[0].Groups['items'].Index + $offset
        $separator = if ($position -gt 0 -and $Content[$position - 1] -cne [char] 10) { $newline } else { '' }
        $Content = $Content.Insert($position, "${separator}        - $Tag$newline")
    }
    return $Content
}

function Get-MauiVerifiedPublishedVersion {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][object] $Release,
        [Parameter(Mandatory)][string] $VersionContent,
        [Parameter(Mandatory)][string] $ReleaseBand,
        [Parameter(Mandatory)][int] $CurrentSr,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $TagCommit
    )

    foreach ($property in @('tag_name', 'draft', 'prerelease', 'published_at', 'html_url')) {
        if ($property -notin $Release.PSObject.Properties.Name) {
            throw "Published-release evidence is missing '$property'."
        }
    }
    if ($Release.draft -isnot [bool] -or $Release.prerelease -isnot [bool] -or
        $Release.draft -or $Release.prerelease -or
        [string]::IsNullOrWhiteSpace([string] $Release.published_at)) {
        throw 'A candidate merge, tag alone, draft, or prerelease is not verified stable publication.'
    }
    $publishedAt = [DateTimeOffset]::Parse($Release.published_at, [Globalization.CultureInfo]::InvariantCulture)
    if ($Release.tag_name -cnotmatch '^(?<major>[1-9][0-9]*)\.0\.(?<patch>[1-9][0-9]*)$') {
        throw 'Publication does not identify a stable servicing tag.'
    }
    $patch = [int] $Matches.patch
    $major = [int] $Matches.major
    $sr = [int] [Math]::Floor($patch / 10)
    if ($sr -lt 1 -or $sr -gt $CurrentSr -or !$ReleaseBand.StartsWith("$major.0.", [StringComparison]::Ordinal)) {
        throw 'Published version is outside the trusted active servicing train.'
    }
    $null = Get-MauiReleaseVersionContent -Content $VersionContent -ReleaseBand $ReleaseBand -Sr $sr -Role servicing
    $version = Get-MauiReleaseVersion -Content $VersionContent
    if ($version.patch -ne $patch -or !($version.stable -or $version.automaticStabilization) -or
        (!$version.branchDerivedLabels -and ($version.label -cne 'servicing' -or $version.labelCount -ne 1))) {
        throw 'The immutable published tag disagrees with its stable servicing version metadata.'
    }
    [pscustomobject]@{
        tag = $Release.tag_name
        commit = $TagCommit
        sr = $sr
        publishedAt = $publishedAt.ToUniversalTime().ToString('o')
        url = $Release.html_url
    }
}

function Invoke-MauiReleaseGit {
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][string[]] $Arguments,
        [hashtable] $Environment = @{},
        [AllowEmptyString()][string] $InputText = ''
    )

    $info = [System.Diagnostics.ProcessStartInfo]::new('git')
    $info.UseShellExecute = $false
    $info.RedirectStandardOutput = $true
    $info.RedirectStandardError = $true
    $info.RedirectStandardInput = $true
    foreach ($argument in (@('-C', $RepositoryPath) + $Arguments)) { $info.ArgumentList.Add($argument) }
    foreach ($entry in $Environment.GetEnumerator()) { $info.Environment[$entry.Key] = $entry.Value }
    $process = [System.Diagnostics.Process]::Start($info)
    try {
        $stdout = $process.StandardOutput.ReadToEndAsync()
        $stderr = $process.StandardError.ReadToEndAsync()
        $process.StandardInput.Write($InputText)
        $process.StandardInput.Close()
        $process.WaitForExit()
        if ($process.ExitCode -ne 0) {
            throw "Release Git operation '$($Arguments[0])' failed: $($stderr.GetAwaiter().GetResult().Trim())"
        }
        return $stdout.GetAwaiter().GetResult()
    }
    finally { $process.Dispose() }
}

function Get-MauiReleaseFile {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Commit,
        [Parameter(Mandatory)][ValidateSet('eng/Versions.props', '.github/ISSUE_TEMPLATE/bug-report.yml', 'eng/release-rollover.json')][string] $Path
    )
    $entry = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('ls-tree', $Commit, '--', $Path)
    if ($entry -cnotmatch "^100644 blob [0-9a-f]{40}`t$([Regex]::Escape($Path))`n$") {
        throw "'$Path' must be a regular tracked file at the pinned release commit."
    }
    Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('cat-file', 'blob', "${Commit}:$Path")
}

function New-MauiReleaseContentCommit {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Parent,
        [Parameter(Mandatory)][System.Collections.IDictionary] $Changes,
        [Parameter(Mandatory)][string] $Message
    )

    $changed = [ordered]@{}
    foreach ($path in $Changes.Keys) {
        $original = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Parent -Path $path
        if ($original -cne $Changes[$path]) { $changed[$path] = $Changes[$path] }
    }
    if ($changed.Count -eq 0) {
        return [pscustomobject]@{ sha = $Parent; parent = $Parent; changedPaths = @(); message = $Message }
    }
    $index = Join-Path ([System.IO.Path]::GetTempPath()) "maui-release-index-$([Guid]::NewGuid().ToString('N'))"
    $environment = @{ GIT_INDEX_FILE = $index }
    try {
        $null = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('read-tree', $Parent) -Environment $environment
        foreach ($path in $changed.Keys) {
            $blob = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('hash-object', '-w', '--stdin') -InputText $changed[$path]).Trim()
            $null = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('update-index', '--cacheinfo', '100644', $blob, $path) -Environment $environment
        }
        $tree = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('write-tree') -Environment $environment).Trim()
        $epoch = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('show', '-s', '--format=%ct', $Parent)).Trim()
        $environment.GIT_AUTHOR_NAME = $environment.GIT_COMMITTER_NAME = 'MAUI release automation'
        $environment.GIT_AUTHOR_EMAIL = $environment.GIT_COMMITTER_EMAIL = '41898282+github-actions[bot]@users.noreply.github.com'
        $environment.GIT_AUTHOR_DATE = $environment.GIT_COMMITTER_DATE = "$epoch +0000"
        $sha = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('-c', 'commit.gpgSign=false', 'commit-tree', $tree, '-p', $Parent) `
            -Environment $environment -InputText ($Message.TrimEnd() + "`n")).Trim()
        return [pscustomobject]@{ sha = $sha; parent = $Parent; changedPaths = @($changed.Keys); message = $Message }
    }
    finally {
        foreach ($path in @($index, "$index.lock")) {
            if ([System.IO.File]::Exists($path)) { [System.IO.File]::Delete($path) }
        }
    }
}

function New-MauiReleaseLaneIntegrationCommit {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Fixes,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Inflight
    )

    # Keep the normalized integration and pinned future payload in ancestry;
    # release-only servicing changes must never enter these candidate lanes.
    $tree = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
        'merge-tree', '--write-tree', $Fixes, $Inflight)).Trim()
    if ($tree -cnotmatch '^[0-9a-f]{40}$') {
        throw 'Next-lane integration requires a clean Git merge; no conflict resolution is invented.'
    }
    $epoch = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('show', '-s', '--format=%ct', $Fixes)).Trim()
    $environment = @{
        GIT_AUTHOR_NAME = 'MAUI release automation'
        GIT_COMMITTER_NAME = 'MAUI release automation'
        GIT_AUTHOR_EMAIL = '41898282+github-actions[bot]@users.noreply.github.com'
        GIT_COMMITTER_EMAIL = '41898282+github-actions[bot]@users.noreply.github.com'
        GIT_AUTHOR_DATE = "$epoch +0000"
        GIT_COMMITTER_DATE = "$epoch +0000"
    }
    $message = "Integrate normalized fixes with next release inflight`n"
    $sha = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
        '-c', 'commit.gpgSign=false', 'commit-tree', $tree, '-p', $Fixes, '-p', $Inflight
    ) -Environment $environment -InputText $message).Trim()
    [pscustomobject]@{ sha = $sha; parents = @($Fixes, $Inflight); message = $message.TrimEnd() }
}

function New-MauiReleaseNextLanePlan {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][string] $ReleaseBand,
        [Parameter(Mandatory)][ValidateRange(1, 214748360)][int] $Sr,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Merge,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Candidate,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Inflight,
        [string] $SandboxPrefix
    )

    $parents = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
        'show', '-s', '--format=%P', $Merge)).Trim().Split(' ')
    if ($parents.Count -ne 2 -or $parents[1] -cne $Candidate) {
        throw 'Next-lane preparation requires the exact two-parent candidate integration merge.'
    }
    $null = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
        'merge-base', '--is-ancestor', $Candidate, $Inflight)
    foreach ($commit in @($Candidate, $Merge)) {
        $content = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $commit -Path 'eng/Versions.props'
        $null = Get-MauiReleaseVersionContent -Content $content -ReleaseBand $ReleaseBand -Sr $Sr -Role candidate `
            -SandboxPrefix $SandboxPrefix
        $version = Get-MauiReleaseVersion -Content $content
        if ($version.label -cne 'ci.main' -or $version.stable) {
            throw 'Candidate integration must keep CI metadata; servicing belongs in a post-cut release PR.'
        }
    }
    $candidateVersion = Get-MauiReleaseVersion -Content (
        Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Candidate -Path 'eng/Versions.props')
    if ($version.patch -ne $candidateVersion.patch) {
        throw 'The integration changed the approved candidate package version.'
    }
    $candidateTemplate = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Candidate -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
    $tag = "$($candidateVersion.major).0.$($candidateVersion.patch)"
    if ((Get-MauiReleasedVersionContent -Content $candidateTemplate -Tag $tag) -cne $candidateTemplate) {
        throw 'The candidate must already list its upcoming version in both bug-template dropdowns.'
    }
    $inflightContent = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Inflight -Path 'eng/Versions.props'
    $null = Get-MauiReleaseVersionContent -Content $inflightContent -ReleaseBand $ReleaseBand -Sr ($Sr + 1) -Role inflight `
        -SandboxPrefix $SandboxPrefix
    $inflightVersion = Get-MauiReleaseVersion -Content $inflightContent
    if ($inflightVersion.stable -or $inflightVersion.label -cne 'ci.main' -or
        (!$inflightVersion.branchDerivedLabels -and $inflightVersion.labelCount -ne 2)) {
        throw 'The following inflight must have correct next-cycle CI metadata before preparing new lanes.'
    }
    $mergeContent = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Merge -Path 'eng/Versions.props'
    $servicingContent = Get-MauiReleaseVersionContent -Content $mergeContent -ReleaseBand $ReleaseBand `
        -Sr $Sr -Role servicing -SandboxPrefix $SandboxPrefix
    $servicing = New-MauiReleaseContentCommit -RepositoryPath $RepositoryPath -Parent $Merge `
        -Changes @{ 'eng/Versions.props' = $servicingContent } `
        -Message "Configure release $ReleaseBand SR$Sr for servicing"
    $normalizedContent = Get-MauiReleaseVersionContent -Content $mergeContent -ReleaseBand $ReleaseBand `
        -Sr ($Sr + 1) -SourceSr $Sr -Role fixes -SandboxPrefix $SandboxPrefix
    $normalizedVersion = Get-MauiReleaseVersion -Content $normalizedContent
    $mergeTemplate = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $Merge -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
    $normalizedTemplate = Get-MauiReleasedVersionContent -Content $mergeTemplate `
        -Tag "$($normalizedVersion.major).0.$($normalizedVersion.patch)"
    $normalized = New-MauiReleaseContentCommit -RepositoryPath $RepositoryPath -Parent $Merge `
        -Changes @{ 'eng/Versions.props' = $normalizedContent; '.github/ISSUE_TEMPLATE/bug-report.yml' = $normalizedTemplate } `
        -Message "Prepare normalized integration ancestry for $ReleaseBand SR$($Sr + 1)"
    $integration = New-MauiReleaseLaneIntegrationCommit -RepositoryPath $RepositoryPath `
        -Fixes $normalized.sha -Inflight $Inflight
    $nextContent = Get-MauiReleaseVersionContent -Content (
        Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $integration.sha -Path 'eng/Versions.props') `
        -ReleaseBand $ReleaseBand -Sr ($Sr + 1) -Role candidate -SandboxPrefix $SandboxPrefix
    $nextCandidate = New-MauiReleaseContentCommit -RepositoryPath $RepositoryPath -Parent $integration.sha `
        -Changes @{ 'eng/Versions.props' = $nextContent } -Message "Configure candidate $ReleaseBand SR$($Sr + 1)"
    $nextVersion = Get-MauiReleaseVersion -Content $nextContent
    $template = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $nextCandidate.sha -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
    $nextTemplate = Get-MauiReleasedVersionContent -Content $template -Tag "$($nextVersion.major).0.$($nextVersion.patch)"
    $metadata = New-MauiReleaseContentCommit -RepositoryPath $RepositoryPath -Parent $nextCandidate.sha `
        -Changes @{ '.github/ISSUE_TEMPLATE/bug-report.yml' = $nextTemplate } -Message "List upcoming MAUI $($nextVersion.major).0.$($nextVersion.patch) in bug template"
    $futureContent = Get-MauiReleaseVersionContent -Content $nextContent -ReleaseBand $ReleaseBand `
        -Sr ($Sr + 2) -SourceSr ($Sr + 1) -Role inflight -SandboxPrefix $SandboxPrefix
    $futureVersion = Get-MauiReleaseVersion -Content $futureContent
    $futureTemplate = Get-MauiReleasedVersionContent -Content $nextTemplate `
        -Tag "$($futureVersion.major).0.$($futureVersion.patch)"
    $nextInflight = New-MauiReleaseContentCommit -RepositoryPath $RepositoryPath -Parent $metadata.sha `
        -Changes @{ 'eng/Versions.props' = $futureContent; '.github/ISSUE_TEMPLATE/bug-report.yml' = $futureTemplate } `
        -Message "Configure inflight $ReleaseBand SR$($Sr + 2) and list its version"
    [pscustomobject][ordered]@{
        mergeSha = $Merge
        candidateSha = $Candidate
        inflightSha = $Inflight
        normalizedSha = $normalized.sha
        servicingSha = $servicing.sha
        nextCandidateSha = $metadata.sha
        nextInflightSha = $nextInflight.sha
    }
}

function New-MauiReleaseLaneReceipt {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][System.Collections.IDictionary] $Record
    )

    $blob = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('hash-object', '-w', '--stdin') `
        -InputText (($Record | ConvertTo-Json -Depth 6) + "`n")).Trim()
    $tree = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('mktree') `
        -InputText "100644 blob $blob`trelease-lanes.json`n").Trim()
    $epoch = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
        'show', '-s', '--format=%ct', $Record.plan.mergeSha)).Trim()
    $environment = @{
        GIT_AUTHOR_NAME = 'MAUI release automation'; GIT_COMMITTER_NAME = 'MAUI release automation'
        GIT_AUTHOR_EMAIL = '41898282+github-actions[bot]@users.noreply.github.com'
        GIT_COMMITTER_EMAIL = '41898282+github-actions[bot]@users.noreply.github.com'
        GIT_AUTHOR_DATE = "$epoch +0000"; GIT_COMMITTER_DATE = "$epoch +0000"
    }
    $arguments = @('-c', 'commit.gpgSign=false', 'commit-tree', $tree, '-p', $Record.plan.nextInflightSha)
    if ('servicingSha' -in $Record.plan.PSObject.Properties.Name) {
        $arguments += @('-p', $Record.plan.servicingSha)
    }
    (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments $arguments `
        -Environment $environment -InputText "Prepare next release lanes after $($Record.releaseBand) SR$($Record.sr)`n").Trim()
}

function Get-MauiReleaseLaneReceipt {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Commit
    )

    $tree = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('ls-tree', $Commit)
    if ($tree -cnotmatch '^100644 blob [0-9a-f]{40}\trelease-lanes\.json\n$') {
        throw 'Release-lane receipt must contain only its regular metadata file.'
    }
    $content = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @('show', "${Commit}:release-lanes.json")
    if ([Text.Encoding]::UTF8.GetByteCount($content) -gt 8192) { throw 'Release-lane receipt exceeds its size bound.' }
    $content | ConvertFrom-Json
}

function Get-MauiReleaseCompletionPlan {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory)][string] $RepositoryPath,
        [Parameter(Mandatory)][object] $LanePlan,
        [Parameter(Mandatory)][string] $ReleaseBand,
        [Parameter(Mandatory)][int] $Sr,
        [Parameter(Mandatory)][ValidatePattern('^[0-9a-f]{40}$')][string] $Fixes,
        [AllowNull()][string] $Release,
        [AllowNull()][string] $Staging
    )

    foreach ($sha in @($LanePlan.mergeSha, $LanePlan.normalizedSha)) {
        if ($sha -cnotmatch '^[0-9a-f]{40}$') { throw 'Release completion requires pinned full commit SHAs.' }
    }
    $parent = (Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
        'show', '-s', '--format=%P', $LanePlan.normalizedSha)).Trim()
    if ($parent -cne $LanePlan.mergeSha) {
        throw 'CI housekeeping must be the exact prepared child of the approved integration merge.'
    }
    $content = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $LanePlan.mergeSha -Path 'eng/Versions.props'
    $version = Get-MauiReleaseVersion -Content $content
    $null = Get-MauiReleaseVersionContent -Content $content -ReleaseBand $ReleaseBand -Sr $Sr -Role candidate `
        -SandboxPrefix $version.labelSandboxPrefix
    if ($version.stable -or $version.label -cne 'ci.main') {
        throw 'The initial release cut must retain the approved candidate CI metadata.'
    }
    if ($Release -and $Release -cne $LanePlan.mergeSha) {
        $null = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
            'merge-base', '--is-ancestor', $LanePlan.mergeSha, $Release)
    }
    if ($Staging -and $Staging -cne $LanePlan.mergeSha) {
        throw 'The create-once release staging ref must retain the exact approved candidate merge.'
    }
    $housekeeping = Get-MauiReleaseVersion -Content (
        Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $LanePlan.normalizedSha -Path 'eng/Versions.props')
    if ($housekeeping.patch -ne (($Sr + 1) * 10) -or $housekeeping.label -cne 'ci.main' -or
        $housekeeping.stable -or (!$housekeeping.branchDerivedLabels -and $housekeeping.labelCount -ne 2)) {
        throw 'Prepared housekeeping does not restore the next-cycle CI metadata.'
    }
    $template = Get-MauiReleaseFile -RepositoryPath $RepositoryPath -Commit $LanePlan.normalizedSha -Path '.github/ISSUE_TEMPLATE/bug-report.yml'
    if ((Get-MauiReleasedVersionContent -Content $template -Tag "$($housekeeping.major).0.$($housekeeping.patch)") -cne $template) {
        throw 'CI housekeeping must update the bug-template version in the same patch-bump commit.'
    }
    $restoreFixes = $Fixes -ceq $LanePlan.mergeSha
    if (!$restoreFixes) {
        $null = Invoke-MauiReleaseGit -RepositoryPath $RepositoryPath -Arguments @(
            'merge-base', '--is-ancestor', $LanePlan.normalizedSha, $Fixes)
    }
    [pscustomobject]@{
        releaseSha = $LanePlan.mergeSha
        stagingSha = $LanePlan.mergeSha
        normalizationSha = $LanePlan.normalizedSha
        createStaging = !$Staging
        createRelease = $false
        restoreFixes = $restoreFixes
    }
}

Export-ModuleMember -Function Get-MauiReleaseVersion, Get-MauiReleaseVersionContent,
    Get-MauiReleasedVersionContent, Get-MauiVerifiedPublishedVersion,
    Get-MauiReleaseFile, New-MauiReleaseContentCommit, New-MauiReleaseLaneIntegrationCommit,
    New-MauiReleaseNextLanePlan, New-MauiReleaseLaneReceipt, Get-MauiReleaseLaneReceipt,
    Get-MauiReleaseCompletionPlan
