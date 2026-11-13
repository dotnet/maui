#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Shared helpers for parsing MAUI release version metadata.

.DESCRIPTION
    Pure functions that map between version numbers, milestone names, branch
    names, and tags for the dotnet/maui release scheme.

    Consumers:
      - Fix-MilestoneDrift.ps1                                   (.github/scripts/)
      - release-readiness skill (Get-ReleaseReadiness.ps1, etc.) (.github/skills/release-readiness/)

    Naming convention reminder:
      release/<major>.0.1xx              -> ".NET <major>.0 GA"
      release/<major>.0.1xx-sr<N>        -> ".NET <major> SR<N>"        (e.g., SR8)
                                            stable tags <major>.0.<N0>   (e.g., 10.0.80)
                                            sub-patches  <major>.0.<NSub> -> ".NET <major> SR<N>.<Sub>" (e.g., SR8.1 = 10.0.81)
      release/<major>.0.1xx-preview<N>   -> ".NET <major>.0-preview<N>"
      release/<major>.0.1xx-rc<N>        -> ".NET <major>.0-rc<N>"

.NOTES
    StrictMode and $ErrorActionPreference are set INSIDE the module so module
    functions get strict behavior without leaking those preferences out to the
    caller's session (Import-Module isolates these unlike dot-sourcing).
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Get-CurrentMajorVersion {
    <#
    .SYNOPSIS
        Returns the MajorVersion value from origin/main:eng/Versions.props.
    .PARAMETER Repo
        Path to a git checkout with origin/main fetched.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Repo
    )
    $versionXml = git -C $Repo --no-pager show origin/main:eng/Versions.props 2>&1
    if ($LASTEXITCODE -eq 0) {
        $joined = ($versionXml -join "`n")
        if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
            return [int]$Matches[1]
        }
    }
    throw "Could not read MajorVersion from origin/main:eng/Versions.props"
}

function Get-MainBranchForVersion {
    <#
    .SYNOPSIS
        Returns the long-lived development branch that currently owns work for
        a given .NET major version.
    .DESCRIPTION
        If main's MajorVersion matches, the version lives on main. Otherwise it
        lives on net{Major}.0. This correctly tracks main rolling forward between
        major versions (e.g., when main moves from .NET 10 to .NET 11, the previous
        major's work shifts from main to net10.0).
    .PARAMETER Major
        The .NET major version (e.g., 10, 11).
    .PARAMETER Repo
        Path to a git checkout with origin/main fetched.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$Major,
        [Parameter(Mandatory)][string]$Repo
    )
    $versionXml = git -C $Repo --no-pager show origin/main:eng/Versions.props 2>&1
    if ($LASTEXITCODE -eq 0) {
        $joined = ($versionXml -join "`n")
        if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
            $mainMajor = [int]$Matches[1]
            if ($mainMajor -eq $Major) { return "main" }
            Write-Verbose "origin/main has MajorVersion=$mainMajor, not $Major - version lives on net$Major.0"
            return "net$Major.0"
        }
    }
    Write-Warning "Could not read MajorVersion from origin/main:eng/Versions.props - falling back to net$Major.0"
    return "net$Major.0"
}

function Get-VersionFromGitRef {
    <#
    .SYNOPSIS
        Reads MajorVersion, PatchVersion, and (optionally) pre-release label
        and iteration from eng/Versions.props at any git ref.
    .DESCRIPTION
        Returns a hashtable: @{ Tag; PreLabel; PreIter }.
          Tag      = synthetic release tag (e.g., "10.0.71", "11.0.0")
          PreLabel = "preview" | "rc" | $null (anything else - ci.main, ci.inflight, servicing - is treated as stable)
          PreIter  = integer iteration ($null if not pre-release)

        Auto-fetches the ref if not present locally (useful for refs that were
        merged to inflight branches and not yet present in the local checkout).
    .PARAMETER GitRef
        The ref to read from, e.g. "origin/main" or "origin/release/10.0.1xx-sr8".
    .PARAMETER Repo
        Path to a git checkout.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$GitRef,
        [Parameter(Mandatory)][string]$Repo
    )
    $versionXml = git -C $Repo --no-pager show "${GitRef}:eng/Versions.props" 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Ref not in local history - fetch it.
        # Strip "origin/" prefix for the fetch refspec (git fetch origin <branch>, not origin/origin/<branch>)
        $fetchRef = $GitRef -replace '^origin/', ''
        Write-Verbose "  Fetching ref $fetchRef..."
        $null = git -C $Repo fetch origin $fetchRef --quiet 2>&1
        $versionXml = git -C $Repo --no-pager show "${GitRef}:eng/Versions.props" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Could not read Versions.props at $GitRef (even after fetch)"
            return $null
        }
    }
    $joined = ($versionXml -join "`n")
    if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
        $major = $Matches[1]
    } else {
        Write-Warning "Could not parse MajorVersion from Versions.props at $GitRef"
        return $null
    }
    if ($joined -match '<PatchVersion>(\d+)</PatchVersion>') {
        $patch = $Matches[1]
    } else {
        Write-Warning "Could not parse PatchVersion from Versions.props at $GitRef"
        return $null
    }

    # Detect pre-release label (preview, rc) and iteration.
    # Other labels like ci.main, ci.inflight, and servicing are stable builds.
    $preLabel = $null
    $preIter = $null
    if ($joined -match '<PreReleaseVersionLabel[^>]*>([^<]+)</PreReleaseVersionLabel>') {
        $rawLabel = $Matches[1]
        if ($rawLabel -match '^(preview|rc)$') {
            $preLabel = $rawLabel
            if ($joined -match '<PreReleaseVersionIteration>(\d+)</PreReleaseVersionIteration>') {
                $preIter = [int]$Matches[1]
            }
        }
    }

    return @{
        Tag       = "$major.0.$patch"
        PreLabel  = $preLabel
        PreIter   = $preIter
    }
}

function ConvertTo-Milestone {
    <#
    .SYNOPSIS
        Maps a version (tag + optional pre-release info) to a milestone name.
    .EXAMPLE
        ConvertTo-Milestone '10.0.50'              -> '.NET 10 SR5'
        ConvertTo-Milestone '10.0.41'              -> '.NET 10 SR4.1'
        ConvertTo-Milestone '10.0.0'               -> '.NET 10.0 GA'
        ConvertTo-Milestone '11.0.0' 'preview' 3   -> '.NET 11.0-preview3'
        ConvertTo-Milestone '11.0.0' 'rc' 1        -> '.NET 11.0-rc1'
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)][AllowEmptyString()][AllowNull()][string]$ReleaseTag,
        [Parameter(Position = 1)][AllowEmptyString()][AllowNull()][string]$PreLabel,
        [Parameter(Position = 2)][int]$PreIter
    )
    if ([string]::IsNullOrEmpty($ReleaseTag))    { return $null }
    if ($ReleaseTag -notmatch '^(\d+)\.0\.(\d+)$') { return $null }
    $major = [int]$Matches[1]; $patch = [int]$Matches[2]

    # Pre-release: preview/rc milestones
    if ($PreLabel -and $PreIter -gt 0) {
        return ".NET $major.0-$PreLabel$PreIter"
    }
    if ($PreLabel -and $PreIter -le 0) {
        Write-Warning "PreReleaseVersionLabel is '$PreLabel' but PreReleaseVersionIteration is missing or 0 - falling back to GA/SR mapping"
    }

    if ($patch -eq 0)  { return ".NET $major.0 GA" }
    if ($patch -lt 10) { return ".NET $major.0 SR1" }
    $sr = [math]::Floor($patch / 10)
    $sub = $patch % 10
    if ($sub -eq 0) { return ".NET $major SR$sr" }
    return ".NET $major SR$sr.$sub"
}

function ConvertBranchToMilestone {
    <#
    .SYNOPSIS
        Maps a release branch name to a milestone name.
    .EXAMPLE
        ConvertBranchToMilestone 'release/10.0.1xx'        -> '.NET 10.0 GA'
        ConvertBranchToMilestone 'release/10.0.1xx-sr5'    -> '.NET 10 SR5'
        ConvertBranchToMilestone 'release/11.0.1xx-preview3' -> '.NET 11.0-preview3'
        ConvertBranchToMilestone 'release/11.0.1xx-rc1'    -> '.NET 11.0-rc1'
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)][AllowEmptyString()][AllowNull()][string]$BranchName
    )
    if ([string]::IsNullOrEmpty($BranchName)) { return $null }
    if ($BranchName -match '^release/(\d+)\.0\.\d+xx$') {
        return ".NET $([int]$Matches[1]).0 GA"
    }
    if ($BranchName -match '^release/(\d+)\.0\.\d+xx-sr(\d+)$') {
        return ".NET $([int]$Matches[1]) SR$([int]$Matches[2])"
    }
    if ($BranchName -match '^release/(\d+)\.0\.\d+xx-(preview|rc)(\d+)$') {
        return ".NET $([int]$Matches[1]).0-$($Matches[2])$([int]$Matches[3])"
    }
    return $null
}

function Get-TagSortKey {
    <#
    .SYNOPSIS
        Returns a numeric sort key for chronological ordering of release tags.
    .DESCRIPTION
        preview1 (100) < preview7 (107) < rc1 (200) < rc2 (201) < GA/stable (500+patch)
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)][AllowEmptyString()][AllowNull()][string]$ReleaseTag
    )
    if ([string]::IsNullOrEmpty($ReleaseTag)) { return 0 }
    if ($ReleaseTag -match '-preview\.(\d+)') { return 100 + [int]$Matches[1] }
    if ($ReleaseTag -match '-rc\.(\d+)')      { return 200 + [int]$Matches[1] }
    if ($ReleaseTag -match '^(\d+)\.0\.(\d+)$') { return 500 + [int]$Matches[2] }
    return 0
}

function Find-PreviousTag {
    <#
    .SYNOPSIS
        Finds the immediately preceding release tag (chronologically) for the
        same major version.
    .DESCRIPTION
        Works for both stable tags (10.0.50 -> 10.0.41) and preview/RC tags
        (11.0.0-preview.3.x -> 11.0.0-preview.2.x). Cross-major tags are ignored.
    .PARAMETER ReleaseTag
        The tag to look up the predecessor for.
    .PARAMETER AllTags
        The candidate list of tags to search.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)][AllowEmptyString()][AllowNull()][string]$ReleaseTag,
        [Parameter(Position = 1, Mandatory)][string[]]$AllTags
    )
    if ([string]::IsNullOrEmpty($ReleaseTag)) { return $null }
    if ($ReleaseTag -notmatch '^(\d+)\.') { return $null }
    $major = [int]$Matches[1]
    $thisKey = Get-TagSortKey $ReleaseTag

    # Find all tags for this major version with a lower sort key
    $candidates = $AllTags | Where-Object {
        ($_ -match "^$major\.0\.") -and (Get-TagSortKey $_) -lt $thisKey
    } | Sort-Object { Get-TagSortKey $_ }
    return ($candidates | Select-Object -Last 1)
}

function Get-MilestoneSortKey {
    <#
    .SYNOPSIS
        Returns a numeric chronological sort key for a release milestone name.
    .DESCRIPTION
        Higher key = released later. Comparable across major versions.

        Phase ordering within a major:
          preview1..preview9  (100..109)
          rc1..rc9            (200..209)
          GA                  (300)
          SR1, SR1.1, SR1.2, ..., SR2, ... (400 + N*10 + sub)

        Returns $null for non-release milestones (Backlog, Planning, Future, etc.)
        so callers can detect "not comparable" and fall back to default behavior.
    .EXAMPLE
        Get-MilestoneSortKey '.NET 11.0-preview3'   -> 11103
        Get-MilestoneSortKey '.NET 10 SR6'          -> 10460
        Get-MilestoneSortKey '.NET 10 SR4.1'        -> 10441
        Get-MilestoneSortKey '.NET 10.0 GA'         -> 10300
        Get-MilestoneSortKey '.NET 11.0-rc1'        -> 11201
        Get-MilestoneSortKey 'Backlog'              -> $null
        Get-MilestoneSortKey '.NET 11 Planning'     -> $null
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)][AllowEmptyString()][AllowNull()][string]$Milestone
    )
    if ([string]::IsNullOrWhiteSpace($Milestone)) { return $null }

    # ".NET 11.0-preview3"
    if ($Milestone -match '^\.NET (\d+)\.0-preview(\d+)$') {
        return ([int]$Matches[1]) * 1000 + 100 + [int]$Matches[2]
    }
    # ".NET 11.0-rc1"
    if ($Milestone -match '^\.NET (\d+)\.0-rc(\d+)$') {
        return ([int]$Matches[1]) * 1000 + 200 + [int]$Matches[2]
    }
    # ".NET 11.0 GA" / ".NET 11 GA" (accept both — production uses both forms)
    if ($Milestone -match '^\.NET (\d+)(?:\.0)? GA$') {
        return ([int]$Matches[1]) * 1000 + 300
    }
    # ".NET 10 SR5.1" / ".NET 10.0 SR5.1" (sub-patch — check before bare SR)
    # Production milestone names use BOTH forms (e.g. ".NET 10 SR4.1" AND ".NET 10.0 SR2.1"),
    # so the `.0` between major and SR must be optional. Without this, Compare-MauiMilestone
    # returns $null for ".NET 10.0 SR*" milestones and the earliest-release-wins guard
    # silently fails open.
    if ($Milestone -match '^\.NET (\d+)(?:\.0)? SR(\d+)\.(\d+)$') {
        return ([int]$Matches[1]) * 1000 + 400 + ([int]$Matches[2] * 10) + [int]$Matches[3]
    }
    # ".NET 10 SR5" / ".NET 10.0 SR5"
    if ($Milestone -match '^\.NET (\d+)(?:\.0)? SR(\d+)$') {
        return ([int]$Matches[1]) * 1000 + 400 + ([int]$Matches[2] * 10)
    }
    # Backlog, Planning, Future, or anything we don't recognize — not orderable
    return $null
}

function Compare-MauiMilestone {
    <#
    .SYNOPSIS
        Compares two MAUI release milestones chronologically.
    .DESCRIPTION
        Returns -1 if A is earlier, 0 if same, 1 if A is later.
        Returns $null if either milestone is non-comparable (Backlog/Planning/none).
    .EXAMPLE
        Compare-MauiMilestone '.NET 10 SR6' '.NET 11.0-preview3'  -> -1
        Compare-MauiMilestone '.NET 11.0-preview3' '.NET 10 SR6'  -> 1
        Compare-MauiMilestone '.NET 10 SR6' '.NET 10 SR6'         -> 0
        Compare-MauiMilestone 'Backlog' '.NET 11.0-preview3'      -> $null
    #>
    [CmdletBinding()]
    param(
        [Parameter(Position = 0)][AllowEmptyString()][AllowNull()][string]$A,
        [Parameter(Position = 1)][AllowEmptyString()][AllowNull()][string]$B
    )
    $ka = Get-MilestoneSortKey $A
    $kb = Get-MilestoneSortKey $B
    if ($null -eq $ka -or $null -eq $kb) { return $null }
    if ($ka -lt $kb) { return -1 }
    if ($ka -gt $kb) { return 1 }
    return 0
}

# Explicit exports - keeps internal helpers private if any are added later.
Export-ModuleMember -Function `
    Get-CurrentMajorVersion, `
    Get-MainBranchForVersion, `
    Get-VersionFromGitRef, `
    ConvertTo-Milestone, `
    ConvertBranchToMilestone, `
    Get-TagSortKey, `
    Find-PreviousTag, `
    Get-MilestoneSortKey, `
    Compare-MauiMilestone
