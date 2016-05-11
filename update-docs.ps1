param(
    [Parameter(Position = 0)]
    [String]$MdocPath = ".\tools\mdoc\mdoc.exe",
    [Parameter(Position = 1)]
    [String]$ProfilePath = "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile259",
    [Parameter(Position = 2)]
    [switch]$FailOnChanges
)

function Update
{
    param
    ( 
        [string]$dllPath, 
        [string]$docsPath
    )
    
    Write-Host "Updating docs for $dllPath ..."
    if(Test-Path $dllPath) {
        & $MdocPath update --delete $dllPath -L $ProfilePath --out $docsPath
    } else {
        Write-Warning "$dllPath was not found; you may need to rebuild"
    }
}

function ReportChanges
{
    param
    ( 
        [string]$dllPath, 
        [string]$docsPath,
        [string[]]$changes
    )

    $docsPath = $docsPath.Replace("\", "/")

    if($changes.Length -eq 0){
        return
    }

    $changes | % {$n=0} { 

        if($changes[$n+1] -match "Member Added:" -or $changes[$n+1] -match "Member Removed:") {
        
            if($changes[$n] -match "^Updating: (.*)") {
                $modified = "$($docsPath)/$(ClassToXMLPath $matches[1])"
                Write-Host "$modified was modified"
            }
        } 

        if($changes[$n] -match "^New Type: (.*)") {
            $modified = "$($docsPath)/$(ClassToXMLPath $matches[1])"
            Write-Host "$modified was added"
        }

         if($changes[$n] -match "^Class no longer present; file renamed: (.*)") {
            $modified = $matches[1].Replace(".remove", "")
            Write-Host "$modified was removed"
        }

        $n = $n + 1
    }
}

function ClassToXMLPath
{
    param([string]$class)
         
    $lastDot = $class.LastIndexOf(".")
    $output = $class.Substring(0, $lastDot) + "/" + $class.Substring($lastDot + 1, $class.Length - $lastDot - 1) + ".xml"
    
    # Unfortunate but necessary fix for IOpenGlViewController
    # Because git is case sensitive but Windows isn't, and Windows has the filename
    # set to IOpenGlViewController by default
    $output = $output.Replace("IOpenGlViewController", "IOpenGLViewController")

    return $output
}

# Resets the line endings changed by mdoc
# So we don't see a lot of 'modified docs' which really aren't changed
function FixLineEndings
{
    param
    ( 
        [string[]]$changes,
        [string]$docsPath
    )

    Write-Host "Resetting line endings modified by mdoc..."

    $docsPath = $docsPath.Replace("\", "/")

    $changes | % { 
        if($_ -match "^Updating: (.*)"){
            $modified = "$($docsPath)/$(ClassToXMLPath $matches[1])"

            (Get-Content $modified).Replace("`n", "`r`n") | Set-Content $modified
        }
    }
}

function CheckForFailure
{
    param( [string]$docsPath )

    if(-not $FailOnChanges){
        return $false
    }

    $docsPath = $docsPath.Replace("\", "/")
    $diffs = git diff $($docsPath) 2> $null
    if($diffs){
        return $true
    }

    return $false
}

$failed = $false

# Core
$dllPath = "Xamarin.Forms.Core\bin\Debug\Xamarin.Forms.Core.dll"
$docsPath = "docs\Xamarin.Forms.Core"
$changes = Update $dllPath $docsPath
$changes = ($changes | % { $_.Replace("/", "+") }) # Fix-up for nested types
ReportChanges $dllPath $docsPath $changes
FixLineEndings $changes $docsPath
if(-not $failed){
    $failed = CheckForFailure $docsPath
}

Write-Host

# Xaml
$dllPath = "Xamarin.Forms.Xaml\bin\Debug\Xamarin.Forms.Xaml.dll"
$docsPath = "docs\Xamarin.Forms.Xaml"
$changes = Update $dllPath $docsPath
$changes = ($changes | % { $_.Replace("/", "+") }) # Fix-up for nested types
ReportChanges $dllPath $docsPath $changes
FixLineEndings $changes $docsPath
if(-not $failed){
    $failed = CheckForFailure $docsPath
}

Write-Host

# Maps
$dllPath = "Xamarin.Forms.Maps\bin\Debug\Xamarin.Forms.Maps.dll"
$docsPath = "docs\Xamarin.Forms.Maps"
$changes = Update $dllPath $docsPath
$changes = ($changes | % { $_.Replace("/", "+") }) # Fix-up for nested types
ReportChanges $dllPath $docsPath $changes
FixLineEndings $changes $docsPath
if(-not $failed){
    $failed = CheckForFailure $docsPath
}

if($failed){
    Write-Warning "Not all of the documentation changes have been committed"
    exit 1
} else {
    exit 0
} 
