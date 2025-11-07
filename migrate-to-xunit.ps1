# Script to migrate NUnit tests to XUnit in the Xaml.UnitTests project

$projectPath = "C:\repos\dotnet\maui\src\Controls\tests\Xaml.UnitTests"

# Get all C# files excluding Generated, obj, bin directories
$csFiles = Get-ChildItem -Path $projectPath -Filter "*.cs" -Recurse | Where-Object { 
    $_.Directory.Name -ne 'obj' -and 
    $_.Directory.Name -ne 'bin' -and 
    $_.Directory.Name -ne 'Generated' -and
    $_.FullName -notlike '*\obj\*' -and
    $_.FullName -notlike '*\bin\*' -and
    $_.FullName -notlike '*\Generated\*'
}

Write-Host "Processing $($csFiles.Count) files..."
$modifiedCount = 0

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    $originalContent = $content
    
    # Skip if no NUnit references
    if ($content -notmatch 'NUnit\.Framework') {
        continue
    }
    
    # Replace using statements
    $content = $content -replace 'using NUnit\.Framework;', 'using Xunit;'
    $content = $content -replace 'using NUnit\.Framework\.Constraints;', 'using Xunit;'
    
    # Replace attributes
    $content = $content -replace '\[TestFixture\]', ''
    $content = $content -replace '\[Test\]', '[Fact]'
    $content = $content -replace '\[SetUp\]', '[MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor'
    $content = $content -replace '\[TearDown\]', '// TODO: Convert to IDisposable.Dispose()'
    $content = $content -replace '\[OneTimeSetUp\]', '// TODO: Convert to IClassFixture or static constructor'
    $content = $content -replace '\[OneTimeTearDown\]', '// TODO: Convert to IClassFixture'
    $content = $content -replace '\[SetUpFixture\]', '// TODO: Convert to AssemblyFixture or ICollectionFixture'
    
    # Replace TestCase with InlineData
    $content = $content -replace '\[TestCase\((.*?)\)\]', '[InlineData($1)]'
    
    # Replace TestCaseSource with MemberData
    $content = $content -replace '\[TestCaseSource\(nameof\((.*?)\)\)\]', '[MemberData(nameof($1))]'
    $content = $content -replace '\[TestCaseSource\("(.*?)"\)\]', '[MemberData(nameof($1))]'
    
    # Replace Category with Trait
    $content = $content -replace '\[Category\("(.*?)"\)\]', '[Trait("Category", "$1")]'
    
    # Replace Ignore with Skip
    $content = $content -replace '\[Ignore\("(.*?)"\)\]', '[Fact(Skip = "$1")]'
    
    # Replace common assertions
    $content = $content -replace 'Assert\.AreEqual\(', 'Assert.Equal('
    $content = $content -replace 'Assert\.AreNotEqual\(', 'Assert.NotEqual('
    $content = $content -replace 'Assert\.AreSame\(', 'Assert.Same('
    $content = $content -replace 'Assert\.AreNotSame\(', 'Assert.NotSame('
    $content = $content -replace 'Assert\.IsTrue\(', 'Assert.True('
    $content = $content -replace 'Assert\.IsFalse\(', 'Assert.False('
    $content = $content -replace 'Assert\.IsNull\(', 'Assert.Null('
    $content = $content -replace 'Assert\.IsNotNull\(', 'Assert.NotNull('
    $content = $content -replace 'Assert\.IsEmpty\(', 'Assert.Empty('
    $content = $content -replace 'Assert\.IsNotEmpty\(', 'Assert.NotEmpty('
    $content = $content -replace 'Assert\.That\((.*?), Is\.TypeOf<(.*?)>\(\)\)', 'Assert.IsType<$2>($1)'
    $content = $content -replace 'Assert\.That\((.*?), Is\.Not\.Null\)', 'Assert.NotNull($1)'
    $content = $content -replace 'Assert\.That\((.*?), Is\.Null\)', 'Assert.Null($1)'
    $content = $content -replace 'Assert\.That\((.*?), Is\.True\)', 'Assert.True($1)'
    $content = $content -replace 'Assert\.That\((.*?), Is\.False\)', 'Assert.False($1)'
    $content = $content -replace 'Assert\.That\((.*?), Is\.EqualTo\((.*?)\)\)', 'Assert.Equal($2, $1)'
    $content = $content -replace 'Assert\.That\((.*?), Is\.Not\.EqualTo\((.*?)\)\)', 'Assert.NotEqual($2, $1)'
    
    # Replace Assert.Throws patterns
    $content = $content -replace 'Assert\.Throws<(.*?)>\(\(\) =>', 'Assert.Throws<$1>(() =>'
    $content = $content -replace 'Assert\.Throws\(new (.*?)\(\), \(\) =>', '// TODO: XUnit does not support custom constraint objects like $1. Use Assert.Throws<ExceptionType>(() =>'
    $content = $content -replace 'Assert\.DoesNotThrow\(\(\) =>', '// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() =>'
    
    # Replace Contains assertions
    $content = $content -replace 'Assert\.Contains\((.*?), (.*?)\)', 'Assert.Contains($1, $2)'
    
    # Clean up multiple blank lines
    $content = $content -replace '(\r?\n){3,}', "`r`n`r`n"
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -NoNewline
        $modifiedCount++
        Write-Host "Modified: $($file.Name)"
    }
}

Write-Host "`nMigration complete! Modified $modifiedCount files."
Write-Host "Please review TODOs and custom constraint replacements manually."
