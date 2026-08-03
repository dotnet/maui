#!/usr/bin/env pwsh

[CmdletBinding()]
param(
    [string]$SourcePath = '',
    [string]$TargetPath = '',
    [string]$AncestorPath = ''
)

$ErrorActionPreference = 'Stop'

function Read-Utf8TextFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $resolvedPath = (Resolve-Path -LiteralPath $Path -ErrorAction Stop).Path
    $bytes = [System.IO.File]::ReadAllBytes($resolvedPath)
    $hasBom = $bytes.Length -ge 3 -and
        $bytes[0] -eq 0xEF -and
        $bytes[1] -eq 0xBB -and
        $bytes[2] -eq 0xBF
    $offset = if ($hasBom) { 3 } else { 0 }
    $encoding = [System.Text.UTF8Encoding]::new($false, $true)

    try {
        $text = $encoding.GetString($bytes, $offset, $bytes.Length - $offset)
    }
    catch {
        throw "File '$resolvedPath' is not valid UTF-8. $($_.Exception.Message)"
    }

    [pscustomobject]@{
        Path   = $resolvedPath
        Text   = $text
        HasBom = $hasBom
    }
}

function Read-SafeXmlDocument {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $settings = [System.Xml.XmlReaderSettings]::new()
    $settings.DtdProcessing = [System.Xml.DtdProcessing]::Prohibit
    $settings.XmlResolver = $null

    $stringReader = [System.IO.StringReader]::new($Text)
    $reader = $null

    try {
        $reader = [System.Xml.XmlReader]::Create($stringReader, $settings)
        $document = [System.Xml.XmlDocument]::new()
        $document.PreserveWhitespace = $true
        $document.XmlResolver = $null
        $document.Load($reader)
    }
    catch {
        throw "$Description is not valid safe XML. $($_.Exception.Message)"
    }
    finally {
        if ($null -ne $reader) {
            $reader.Dispose()
        }
        $stringReader.Dispose()
    }

    if ($document.DocumentElement.LocalName -ne 'Project') {
        throw "$Description must have a Project root element."
    }

    return $document
}

function Get-DirectPropertyGroup {
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlDocument]$Document
    )

    $groups = [System.Collections.Generic.List[object]]::new()

    foreach ($child in $Document.DocumentElement.ChildNodes) {
        if ($child.NodeType -ne [System.Xml.XmlNodeType]::Element -or
            $child.LocalName -ne 'PropertyGroup') {
            continue
        }

        $properties = [System.Collections.Generic.List[object]]::new()
        foreach ($property in $child.ChildNodes) {
            if ($property.NodeType -eq [System.Xml.XmlNodeType]::Element) {
                $properties.Add([pscustomobject]@{
                    Name  = $property.LocalName
                    Order = $properties.Count
                })
            }
        }

        $groups.Add([pscustomobject]@{
            Index      = $groups.Count
            Properties = $properties.ToArray()
        })
    }

    return $groups.ToArray()
}

function ConvertTo-LineModel {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $hasCrLf = $Text.Contains("`r`n")
    $withoutCrLf = $Text.Replace("`r`n", '')
    if ($hasCrLf -and $withoutCrLf.Contains("`n")) {
        throw "$Description contains mixed line endings."
    }

    $newLine = if ($hasCrLf) { "`r`n" } else { "`n" }
    $endsWithNewLine = $Text.EndsWith($newLine, [System.StringComparison]::Ordinal)
    $lines = [System.Collections.Generic.List[string]]::new()
    $splitLines = [System.Text.RegularExpressions.Regex]::Split($Text, "\r?\n")
    $lineCount = if ($endsWithNewLine) { $splitLines.Length - 1 } else { $splitLines.Length }

    for ($index = 0; $index -lt $lineCount; $index++) {
        $lines.Add($splitLines[$index])
    }

    [pscustomobject]@{
        Lines           = $lines
        NewLine         = $newLine
        EndsWithNewLine = $endsWithNewLine
    }
}

function Get-PropertyGroupLineLayout {
    param(
        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.List[string]]$Lines,
        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $groups = [System.Collections.Generic.List[object]]::new()
    $currentGroup = $null
    $propertyPattern = '^\s*<(?<name>[A-Za-z_][A-Za-z0-9_.-]*)(?:\s[^>]*)?>.*</\k<name>>\s*$'

    for ($lineIndex = 0; $lineIndex -lt $Lines.Count; $lineIndex++) {
        $line = $Lines[$lineIndex]

        if ($line -match '^\s*<PropertyGroup(?:\s|>)') {
            if ($null -ne $currentGroup) {
                throw "$Description contains a nested PropertyGroup at line $($lineIndex + 1)."
            }

            $currentGroup = [pscustomobject]@{
                Index      = $groups.Count
                StartLine  = $lineIndex
                EndLine    = -1
                Properties = @{}
            }
            $groups.Add($currentGroup)
            continue
        }

        if ($line -match '^\s*</PropertyGroup>\s*$') {
            if ($null -eq $currentGroup) {
                throw "$Description contains an unmatched PropertyGroup close tag at line $($lineIndex + 1)."
            }

            $currentGroup.EndLine = $lineIndex
            $currentGroup = $null
            continue
        }

        if ($null -ne $currentGroup -and $line -match $propertyPattern) {
            $key = $Matches.name.ToLowerInvariant()
            if (-not $currentGroup.Properties.ContainsKey($key)) {
                $currentGroup.Properties[$key] = [System.Collections.Generic.List[int]]::new()
            }
            $currentGroup.Properties[$key].Add($lineIndex)
        }
    }

    if ($null -ne $currentGroup) {
        throw "$Description contains an unclosed PropertyGroup."
    }

    foreach ($group in $groups) {
        if ($group.EndLine -lt 0) {
            throw "$Description contains an incomplete PropertyGroup."
        }
    }

    return $groups.ToArray()
}

function Get-SourcePropertyLine {
    param(
        [Parameter(Mandatory = $true)]
        [System.Collections.Generic.List[string]]$Lines,
        [Parameter(Mandatory = $true)]
        [object]$Group,
        [Parameter(Mandatory = $true)]
        [string]$PropertyName
    )

    $key = $PropertyName.ToLowerInvariant()
    if (-not $Group.Properties.ContainsKey($key) -or $Group.Properties[$key].Count -ne 1) {
        throw "Source-only property '$PropertyName' must be represented by exactly one single-line element."
    }

    $lineIndex = $Group.Properties[$key][0]
    [pscustomobject]@{
        Index = $lineIndex
        Text  = $Lines[$lineIndex]
    }
}

function Merge-SourceOnlyVersionProperty {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$SourcePath,
        [Parameter(Mandatory = $true)]
        [string]$TargetPath,
        # Optional merge-base (three-way) version of the target file. When supplied, a source
        # property missing from the target is only restored if it is ALSO absent from this ancestor
        # (i.e. genuinely added on the source branch). A property present in the ancestor but absent
        # from the target was intentionally deleted on the target branch and must NOT be resurrected.
        [string]$AncestorPath = ''
    )

    $sourceFile = Read-Utf8TextFile -Path $SourcePath
    $targetFile = Read-Utf8TextFile -Path $TargetPath
    $sourceDocument = Read-SafeXmlDocument -Text $sourceFile.Text -Description "Source file '$($sourceFile.Path)'"
    $targetDocument = Read-SafeXmlDocument -Text $targetFile.Text -Description "Target file '$($targetFile.Path)'"
    $sourceGroups = @(Get-DirectPropertyGroup -Document $sourceDocument)
    $targetGroups = @(Get-DirectPropertyGroup -Document $targetDocument)

    $targetNames = [System.Collections.Generic.HashSet[string]]::new(
        [System.StringComparer]::OrdinalIgnoreCase)
    foreach ($group in $targetGroups) {
        foreach ($property in $group.Properties) {
            [void]$targetNames.Add($property.Name)
        }
    }

    # Three-way provenance: names present in the merge-base ancestor. A property missing from the
    # target that IS in the ancestor was deleted on the target branch, so it must not be restored.
    $ancestorNames = $null
    if (-not [string]::IsNullOrWhiteSpace($AncestorPath)) {
        $ancestorFile = Read-Utf8TextFile -Path $AncestorPath
        $ancestorDocument = Read-SafeXmlDocument -Text $ancestorFile.Text -Description "Ancestor file '$($ancestorFile.Path)'"
        $ancestorGroups = @(Get-DirectPropertyGroup -Document $ancestorDocument)
        $ancestorNames = [System.Collections.Generic.HashSet[string]]::new(
            [System.StringComparer]::OrdinalIgnoreCase)
        foreach ($group in $ancestorGroups) {
            foreach ($property in $group.Properties) {
                [void]$ancestorNames.Add($property.Name)
            }
        }
    }

    $sourceCounts = @{}
    foreach ($group in $sourceGroups) {
        foreach ($property in $group.Properties) {
            $key = $property.Name.ToLowerInvariant()
            if ($sourceCounts.ContainsKey($key)) {
                $sourceCounts[$key]++
            }
            else {
                $sourceCounts[$key] = 1
            }
        }
    }

    $missingProperties = [System.Collections.Generic.List[object]]::new()
    foreach ($group in $sourceGroups) {
        foreach ($property in $group.Properties) {
            if ($targetNames.Contains($property.Name)) {
                continue
            }

            # Deleted on the target branch (present in ancestor, absent from target): do not resurrect.
            if ($null -ne $ancestorNames -and $ancestorNames.Contains($property.Name)) {
                continue
            }

            $key = $property.Name.ToLowerInvariant()
            if ($sourceCounts[$key] -ne 1) {
                throw "Source-only property '$($property.Name)' is declared more than once and cannot be merged safely."
            }

            $missingProperties.Add([pscustomobject]@{
                Name       = $property.Name
                GroupIndex = $group.Index
                Order      = $property.Order
            })
        }
    }

    if ($missingProperties.Count -eq 0) {
        return [pscustomobject]@{
            Changed         = $false
            AddedProperties = @()
        }
    }

    $sourceLines = ConvertTo-LineModel -Text $sourceFile.Text -Description "Source file '$($sourceFile.Path)'"
    $targetLines = ConvertTo-LineModel -Text $targetFile.Text -Description "Target file '$($targetFile.Path)'"
    $sourceLayout = @(Get-PropertyGroupLineLayout -Lines $sourceLines.Lines -Description "Source file '$($sourceFile.Path)'")

    if ($sourceLayout.Count -ne $sourceGroups.Count) {
        throw "Source XML PropertyGroup structure does not match its line layout."
    }

    $addedProperties = [System.Collections.Generic.List[string]]::new()

    foreach ($missing in $missingProperties) {
        $targetLayout = @(Get-PropertyGroupLineLayout -Lines $targetLines.Lines -Description "Target file '$($targetFile.Path)'")
        if ($missing.GroupIndex -ge $targetLayout.Count) {
            throw "Target file has no PropertyGroup matching source group $($missing.GroupIndex)."
        }

        $sourceGroup = $sourceGroups[$missing.GroupIndex]
        $sourceLineGroup = $sourceLayout[$missing.GroupIndex]
        $targetGroup = $targetLayout[$missing.GroupIndex]
        $sourcePropertyLine = Get-SourcePropertyLine `
            -Lines $sourceLines.Lines `
            -Group $sourceLineGroup `
            -PropertyName $missing.Name

        $previousAnchor = $null
        for ($index = $missing.Order - 1; $index -ge 0; $index--) {
            $candidate = $sourceGroup.Properties[$index].Name.ToLowerInvariant()
            if ($targetGroup.Properties.ContainsKey($candidate)) {
                $previousAnchor = [pscustomobject]@{
                    Name = $candidate
                    Line = $targetGroup.Properties[$candidate][-1]
                }
                break
            }
        }

        $nextAnchor = $null
        for ($index = $missing.Order + 1; $index -lt $sourceGroup.Properties.Count; $index++) {
            $candidate = $sourceGroup.Properties[$index].Name.ToLowerInvariant()
            if ($targetGroup.Properties.ContainsKey($candidate)) {
                $nextAnchor = [pscustomobject]@{
                    Name = $candidate
                    Line = $targetGroup.Properties[$candidate][0]
                }
                break
            }
        }

        $preferNextAnchor = $false
        if ($null -ne $previousAnchor) {
            if (-not $sourceLineGroup.Properties.ContainsKey($previousAnchor.Name)) {
                throw "Cannot place source-only property '$($missing.Name)' because its previous placement anchor '$($previousAnchor.Name)' is not a single-line element."
            }

            $sourcePreviousLine = $sourceLineGroup.Properties[$previousAnchor.Name][-1]
            for ($lineIndex = $sourcePreviousLine + 1; $lineIndex -lt $sourcePropertyLine.Index; $lineIndex++) {
                if (-not [string]::IsNullOrWhiteSpace($sourceLines.Lines[$lineIndex])) {
                    $preferNextAnchor = $true
                    break
                }
            }
        }

        if ($preferNextAnchor -and $null -ne $nextAnchor) {
            $insertAt = $nextAnchor.Line
        }
        elseif ($null -ne $previousAnchor) {
            $insertAt = $previousAnchor.Line + 1
        }
        elseif ($null -ne $nextAnchor) {
            $insertAt = $nextAnchor.Line
        }
        else {
            # No property from the source group is present in the target group at this index, so the
            # Nth-group correspondence cannot be verified. Fail closed rather than risk inserting into
            # an unrelated group (which would still produce well-formed XML).
            throw "Cannot place source-only property '$($missing.Name)' because target PropertyGroup $($missing.GroupIndex) shares no property with the corresponding source group, so their correspondence cannot be verified."
        }

        $targetLines.Lines.Insert($insertAt, $sourcePropertyLine.Text)
        [void]$targetNames.Add($missing.Name)
        $addedProperties.Add($missing.Name)
    }

    $updatedText = [string]::Join($targetLines.NewLine, $targetLines.Lines)
    if ($targetLines.EndsWithNewLine) {
        $updatedText += $targetLines.NewLine
    }

    [void](Read-SafeXmlDocument -Text $updatedText -Description "Updated target file '$($targetFile.Path)'")
    $encoding = [System.Text.UTF8Encoding]::new($targetFile.HasBom)
    [System.IO.File]::WriteAllText($targetFile.Path, $updatedText, $encoding)

    [pscustomobject]@{
        Changed         = $true
        AddedProperties = $addedProperties.ToArray()
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    if ([string]::IsNullOrWhiteSpace($SourcePath) -or [string]::IsNullOrWhiteSpace($TargetPath)) {
        throw 'SourcePath and TargetPath are required.'
    }

    $result = Merge-SourceOnlyVersionProperty -SourcePath $SourcePath -TargetPath $TargetPath -AncestorPath $AncestorPath
    if ($result.Changed) {
        Write-Output "Added source-only version properties: $($result.AddedProperties -join ', ')"
    }
    else {
        Write-Output 'No source-only version properties needed reconciliation.'
    }
}
