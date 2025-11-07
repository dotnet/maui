param($filePath)

$content = Get-Content $filePath -Raw

# Check if file needs processing
if ($content -notmatch "public void TearDown\(\)") {
    return $false
}

# Pattern 1: Find the class declaration and check if it already has IDisposable
$classPattern = 'public class Tests\s*(?::\s*IDisposable)?'
if ($content -match $classPattern) {
    $classMatch = $Matches[0]
    # Add IDisposable if not present
    if ($classMatch -notmatch "IDisposable") {
        $content = $content -replace 'public class Tests\s*\r?\n?\s*\{', "public class Tests : IDisposable`n`t{"
    }
}

# Pattern 2: Convert TearDown() to Dispose()
$content = $content -replace 'public void TearDown\(\)', 'public void Dispose()'

# Pattern 3: Remove the TODO comment for TearDown
$content = $content -replace '\s*// TODO: Convert to IDisposable\.Dispose\(\).*?\r?\n', "`n"

# Save the file
Set-Content -Path $filePath -Value $content -NoNewline

return $true
