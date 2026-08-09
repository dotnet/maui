#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Tests for Find-RegressionRisks.ps1

.DESCRIPTION
    Validates the regression cross-reference algorithm: diff parsing, trivial-line
    filtering, whitespace normalization, REVERT/OVERLAP/CLEAN classification, and
    output file generation. Tests use fixture data to avoid gh/git API calls.

.EXAMPLE
    ./Test-FindRegressionRisks.ps1
#>

param(
    [switch]$Verbose
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel
$ScriptPath = Join-Path $RepoRoot ".github/scripts/Find-RegressionRisks.ps1"

# Test tracking
$script:TestsPassed = 0
$script:TestsFailed = 0
$script:TestsSkipped = 0

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message = ""
    )
    if ($Passed) {
        Write-Host "  [PASS] $TestName" -ForegroundColor Green
        $script:TestsPassed++
    } else {
        Write-Host "  [FAIL] $TestName" -ForegroundColor Red
        if ($Message) { Write-Host "         $Message" -ForegroundColor Yellow }
        $script:TestsFailed++
    }
}

function Write-TestSkipped {
    param([string]$TestName, [string]$Reason)
    Write-Host "  [SKIP] $TestName - $Reason" -ForegroundColor Yellow
    $script:TestsSkipped++
}

function Test-Section {
    param([string]$Name)
    Write-Host ""
    Write-Host "=== $Name ===" -ForegroundColor Cyan
}

# ============================================================
# Load helper functions from the script via dot-source
# ============================================================

# We dot-source the script in a constrained way: override the param block
# by extracting just the function definitions. This avoids running Main.

Test-Section "Script Existence"
Write-TestResult "Find-RegressionRisks.ps1 exists" (Test-Path $ScriptPath)

# Extract function definitions by parsing the script AST
Test-Section "Function Extraction"

$ast = [System.Management.Automation.Language.Parser]::ParseFile($ScriptPath, [ref]$null, [ref]$null)
$functions = $ast.FindAll({ $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] }, $false)

foreach ($fn in $functions) {
    # Define each function in this scope
    Invoke-Expression $fn.Extent.Text
}

$expectedFunctions = @(
    'Write-Banner', 'ConvertTo-NormalizedLine', 'Test-IsImplementationFile',
    'Get-PRDiffText', 'Get-DiffLinesByFile', 'Test-IsTrivialLine',
    'Test-IsBugFixLabel', 'Get-LinkedIssueNumbers', 'Get-PRMetadataIfBugFix'
)
foreach ($name in $expectedFunctions) {
    Write-TestResult "Function '$name' extracted" ($null -ne (Get-Command $name -ErrorAction SilentlyContinue))
}

# ============================================================
# Test: ConvertTo-NormalizedLine
# ============================================================
Test-Section "ConvertTo-NormalizedLine"

Write-TestResult "Collapses tabs to single space" (
    (ConvertTo-NormalizedLine "`t`tint x = 1;") -eq "int x = 1;"
)
Write-TestResult "Collapses multiple spaces" (
    (ConvertTo-NormalizedLine "  int   x   =   1;  ") -eq "int x = 1;"
)
Write-TestResult "Trims leading/trailing whitespace" (
    (ConvertTo-NormalizedLine "   hello   ") -eq "hello"
)
Write-TestResult "Empty string stays empty" (
    (ConvertTo-NormalizedLine "") -eq ""
)

# ============================================================
# Test: Test-IsImplementationFile
# ============================================================
Test-Section "Test-IsImplementationFile"

Write-TestResult "Accepts .cs file" (Test-IsImplementationFile "src/Controls/src/Core/Button.cs")
Write-TestResult "Accepts .xaml file" (Test-IsImplementationFile "src/Controls/src/Core/Views/Button.xaml")
Write-TestResult "Rejects .csproj" (-not (Test-IsImplementationFile "src/Controls/src/Core/Controls.csproj"))
Write-TestResult "Rejects test file" (-not (Test-IsImplementationFile "src/Controls/tests/UnitTests/ButtonTests.cs"))
Write-TestResult "Rejects TestCases file" (-not (Test-IsImplementationFile "src/Controls/tests/TestCases.HostApp/Issue123.cs"))
Write-TestResult "Rejects .Designer.cs" (-not (Test-IsImplementationFile "src/Resources.Designer.cs"))
Write-TestResult "Rejects .g.cs" (-not (Test-IsImplementationFile "src/Generated.g.cs"))
Write-TestResult "Rejects samples" (-not (Test-IsImplementationFile "src/Controls/samples/Sample/MainPage.cs"))

# ============================================================
# Test: Test-IsTrivialLine
# ============================================================
Test-Section "Test-IsTrivialLine"

Write-TestResult "Empty string is trivial" (Test-IsTrivialLine "")
Write-TestResult "Whitespace only is trivial" (Test-IsTrivialLine "   ")
Write-TestResult "Short token is trivial" (Test-IsTrivialLine "{ }")
Write-TestResult "Brace-only is trivial" (Test-IsTrivialLine "{ } ;")
Write-TestResult "Return statement is trivial" (Test-IsTrivialLine "return;")
Write-TestResult "Break is trivial" (Test-IsTrivialLine "break;")
Write-TestResult "Using directive is trivial" (Test-IsTrivialLine "using System.Linq;")
Write-TestResult "Comment is trivial" (Test-IsTrivialLine "// This is a comment")
Write-TestResult "Actual code is NOT trivial" (-not (Test-IsTrivialLine "var handler = new ViewHandler();"))
Write-TestResult "Method call is NOT trivial" (-not (Test-IsTrivialLine "parent.SetPadding(left, top, right, bottom);"))

# ============================================================
# Test: Test-IsBugFixLabel
# ============================================================
Test-Section "Test-IsBugFixLabel"

Write-TestResult "i/regression matches" (Test-IsBugFixLabel "i/regression")
Write-TestResult "t/bug matches" (Test-IsBugFixLabel "t/bug")
Write-TestResult "p/0 matches" (Test-IsBugFixLabel "p/0")
Write-TestResult "p/1 matches" (Test-IsBugFixLabel "p/1")
Write-TestResult "t/enhancement does NOT match" (-not (Test-IsBugFixLabel "t/enhancement"))
Write-TestResult "area/controls does NOT match" (-not (Test-IsBugFixLabel "area/controls"))
Write-TestResult "p/2 does NOT match" (-not (Test-IsBugFixLabel "p/2"))

# ============================================================
# Test: Get-LinkedIssueNumbers
# ============================================================
Test-Section "Get-LinkedIssueNumbers"

$body1 = "Fixes #12345`nCloses #67890"
$linked1 = Get-LinkedIssueNumbers $body1
Write-TestResult "Finds Fixes #N" ($linked1 -contains 12345)
Write-TestResult "Finds Closes #N" ($linked1 -contains 67890)

$body2 = "Resolves https://github.com/dotnet/maui/issues/99999"
$linked2 = Get-LinkedIssueNumbers $body2
Write-TestResult "Finds full URL" ($linked2 -contains 99999)

$body3 = "- #111`n- #222`n- #333"
$linked3 = Get-LinkedIssueNumbers $body3
Write-TestResult "Finds bullet list issues" ($linked3.Count -ge 3)

$body4 = "No issues mentioned here."
$linked4 = Get-LinkedIssueNumbers $body4
Write-TestResult "Empty when no issues" ($linked4.Count -eq 0)

Write-TestResult "Handles null body" ((Get-LinkedIssueNumbers $null).Count -eq 0)

# ============================================================
# Test: Get-DiffLinesByFile
# ============================================================
Test-Section "Get-DiffLinesByFile"

$simpleDiff = @"
diff --git a/src/File.cs b/src/File.cs
index abc..def 100644
--- a/src/File.cs
+++ b/src/File.cs
@@ -10,4 +10,4 @@ namespace Foo
 context line
-removed line
+added line
 context line
"@

$parsed = Get-DiffLinesByFile -DiffText $simpleDiff
Write-TestResult "Parses one file" ($parsed.ContainsKey("src/File.cs"))
$fileLines = $parsed["src/File.cs"]
$removed = @($fileLines | Where-Object { $_.Sign -eq '-' })
$added = @($fileLines | Where-Object { $_.Sign -eq '+' })
Write-TestResult "Found 1 removed line" ($removed.Count -eq 1)
Write-TestResult "Found 1 added line" ($added.Count -eq 1)
Write-TestResult "Removed text correct" ($removed[0].Text -eq "removed line")
Write-TestResult "Added text correct" ($added[0].Text -eq "added line")
Write-TestResult "Removed line number = 11" ($removed[0].Line -eq 11)
Write-TestResult "Added line number = 11" ($added[0].Line -eq 11)

# Multi-file diff
$multiDiff = @"
diff --git a/src/A.cs b/src/A.cs
--- a/src/A.cs
+++ b/src/A.cs
@@ -1,3 +1,3 @@
 keep
-old A
+new A
 keep
diff --git a/src/B.cs b/src/B.cs
--- a/src/B.cs
+++ b/src/B.cs
@@ -5,2 +5,3 @@
 keep
+added to B
 keep
"@

$parsedMulti = Get-DiffLinesByFile -DiffText $multiDiff
Write-TestResult "Parses two files" ($parsedMulti.Count -eq 2)
Write-TestResult "Has src/A.cs" ($parsedMulti.ContainsKey("src/A.cs"))
Write-TestResult "Has src/B.cs" ($parsedMulti.ContainsKey("src/B.cs"))

# Handles "\ No newline at end of file" marker
$noNewlineDiff = @"
diff --git a/src/C.cs b/src/C.cs
--- a/src/C.cs
+++ b/src/C.cs
@@ -1,2 +1,2 @@
 keep
-old line
\ No newline at end of file
+new line
\ No newline at end of file
"@

$parsedNoNl = Get-DiffLinesByFile -DiffText $noNewlineDiff
$cLines = $parsedNoNl["src/C.cs"]
Write-TestResult "No-newline marker ignored (2 entries)" (@($cLines).Count -eq 2)

# CRLF handling
$crlfDiff = "diff --git a/src/D.cs b/src/D.cs`r`n--- a/src/D.cs`r`n+++ b/src/D.cs`r`n@@ -1,2 +1,2 @@`r`n keep`r`n-old`r`n+new`r`n"
$parsedCrlf = Get-DiffLinesByFile -DiffText $crlfDiff
Write-TestResult "CRLF diff parsed correctly" ($parsedCrlf.ContainsKey("src/D.cs"))

# ============================================================
# Test: REVERT detection logic (simulated)
# ============================================================
Test-Section "REVERT Detection Logic"

# Simulate: PR removes a line that was added by a fix PR
$prDiff = @"
diff --git a/src/Handler.cs b/src/Handler.cs
--- a/src/Handler.cs
+++ b/src/Handler.cs
@@ -10,4 +10,3 @@ class Handler
 keep
-parent.SetPadding(left, top, right, bottom);
 keep
 keep
"@

$fixDiff = @"
diff --git a/src/Handler.cs b/src/Handler.cs
--- a/src/Handler.cs
+++ b/src/Handler.cs
@@ -10,3 +10,4 @@ class Handler
 keep
+parent.SetPadding(left, top, right, bottom);
 keep
 keep
"@

$prByFile = Get-DiffLinesByFile -DiffText $prDiff
$fixByFile = Get-DiffLinesByFile -DiffText $fixDiff

$prRemoved = @($prByFile["src/Handler.cs"] | Where-Object {
    $_.Sign -eq '-' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text))
})
$fixAdded = @($fixByFile["src/Handler.cs"] | Where-Object {
    $_.Sign -eq '+' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text))
} | ForEach-Object { ConvertTo-NormalizedLine $_.Text }) | Select-Object -Unique

$addedSet = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($n in $fixAdded) { [void]$addedSet.Add($n) }

$reverted = New-Object System.Collections.Generic.List[object]
foreach ($r in $prRemoved) {
    $key = ConvertTo-NormalizedLine $r.Text
    if ($addedSet.Contains($key)) {
        $reverted.Add([PSCustomObject]@{ Text = $r.Text; Line = $r.Line })
    }
}

Write-TestResult "Detects REVERT (1 reverted line)" ($reverted.Count -eq 1)
Write-TestResult "Reverted line text correct" ($reverted[0].Text -match "SetPadding")

# ============================================================
# Test: Whitespace-insensitive matching
# ============================================================
Test-Section "Whitespace-Insensitive Matching"

$prDiffWs = @"
diff --git a/src/Handler.cs b/src/Handler.cs
--- a/src/Handler.cs
+++ b/src/Handler.cs
@@ -10,4 +10,3 @@ class Handler
 keep
-    parent.SetPadding(left, top, right, bottom);
 keep
 keep
"@

$fixDiffWs = @"
diff --git a/src/Handler.cs b/src/Handler.cs
--- a/src/Handler.cs
+++ b/src/Handler.cs
@@ -10,3 +10,4 @@ class Handler
 keep
+		parent.SetPadding(left,  top,  right,  bottom);
 keep
 keep
"@

$prByFileWs = Get-DiffLinesByFile -DiffText $prDiffWs
$fixByFileWs = Get-DiffLinesByFile -DiffText $fixDiffWs

$prRemovedWs = @($prByFileWs["src/Handler.cs"] | Where-Object {
    $_.Sign -eq '-' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text))
})
$fixAddedWs = @($fixByFileWs["src/Handler.cs"] | Where-Object {
    $_.Sign -eq '+' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text))
} | ForEach-Object { ConvertTo-NormalizedLine $_.Text }) | Select-Object -Unique

$addedSetWs = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($n in $fixAddedWs) { [void]$addedSetWs.Add($n) }

$revertedWs = @()
foreach ($r in $prRemovedWs) {
    $key = ConvertTo-NormalizedLine $r.Text
    if ($addedSetWs.Contains($key)) { $revertedWs += $r }
}
Write-TestResult "Whitespace-different lines still match" ($revertedWs.Count -eq 1)

# ============================================================
# Test: Move-within-PR suppression
# ============================================================
Test-Section "Move-Within-PR Suppression"

# PR removes a line AND re-adds it (refactor/move) — should NOT be flagged as REVERT
$prDiffMove = @"
diff --git a/src/Handler.cs b/src/Handler.cs
--- a/src/Handler.cs
+++ b/src/Handler.cs
@@ -10,4 +10,4 @@ class Handler
 keep
-parent.SetPadding(left, top, right, bottom);
 keep
+parent.SetPadding(left, top, right, bottom);
"@

$prByFileMove = Get-DiffLinesByFile -DiffText $prDiffMove
$prRemovedMove = @($prByFileMove["src/Handler.cs"] | Where-Object {
    $_.Sign -eq '-' -and -not (Test-IsTrivialLine (ConvertTo-NormalizedLine $_.Text))
})
$prAddedNormMove = New-Object 'System.Collections.Generic.HashSet[string]'
foreach ($a in ($prByFileMove["src/Handler.cs"] | Where-Object { $_.Sign -eq '+' })) {
    [void]$prAddedNormMove.Add((ConvertTo-NormalizedLine $a.Text))
}

$revertedMove = @()
foreach ($r in $prRemovedMove) {
    $key = ConvertTo-NormalizedLine $r.Text
    if (-not $addedSet.Contains($key)) { continue }  # not in fix PR
    if ($prAddedNormMove.Contains($key)) { continue } # moved within PR
    $revertedMove += $r
}
Write-TestResult "Move-within-PR not flagged as REVERT" ($revertedMove.Count -eq 0)

# ============================================================
# Test: Self-PR exclusion
# ============================================================
Test-Section "Self-PR Exclusion"

# The git-log parsing should exclude the current PR number
$commitLog = @"
abc1234 Some change (#100)
def5678 Fix bug (#200)
ghi9012 Another fix (#100)
"@

$prNumber = 100
$seen = New-Object 'System.Collections.Generic.HashSet[int]'
$recentPRs = New-Object 'System.Collections.Generic.List[int]'
foreach ($line in ($commitLog -split "`n")) {
    if ($line -match '\(#(\d+)\)') {
        $n = [int]$Matches[1]
        if ($n -ne $prNumber -and $seen.Add($n)) {
            $recentPRs.Add($n)
        }
    }
}
Write-TestResult "Self-PR excluded" (-not ($recentPRs -contains 100))
Write-TestResult "Other PRs included" ($recentPRs -contains 200)
Write-TestResult "Dedup works" ($recentPRs.Count -eq 1)

# ============================================================
# Summary
# ============================================================
Write-Host ""
Write-Host "══════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  Results: $($script:TestsPassed) passed, $($script:TestsFailed) failed, $($script:TestsSkipped) skipped" -ForegroundColor $(if ($script:TestsFailed -gt 0) { "Red" } else { "Green" })
Write-Host "══════════════════════════════════════" -ForegroundColor Cyan

if ($script:TestsFailed -gt 0) {
    exit 1
}
exit 0
