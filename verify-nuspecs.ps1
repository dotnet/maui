[CmdletBinding()]
param ( )

# Namespace for msbuild csproj files
$namespace = @{msb="http://schemas.microsoft.com/developer/msbuild/2003"}

# The various Windows projects and their nuspec targets
$projectTargets = @{
    "Xamarin.Forms.Platform.WinRT.Phone" = @("lib\wpa81\Xamarin.Forms.Platform.WinRT.Phone");
    "Xamarin.Forms.Platform.WinRT" = @("lib\wpa81\Xamarin.Forms.Platform.WinRT", "lib\win81\Xamarin.Forms.Platform.WinRT");
    "Xamarin.Forms.Platform.WinRT.Tablet" = @("lib\win81\Xamarin.Forms.Platform.WinRT.Tablet");
    "Xamarin.Forms.Platform.UAP" = @("lib\uap10.0\Xamarin.Forms.Platform.UAP");
}

# Iterate over the Windows projects, load their csproj files,
# and build a hashtable of the required .xbf files and their targets

$projectNames = @()
$projectTargets.Keys | % {
    $projectNames += $_
}

$requirements = @{}

$projectNames  | % {

    $name = $_

    # Find the csproj file
    $csproj = (Get-ChildItem -r ($_ + '.csproj')).FullName

    # Load it up
    [xml]$proj = Get-Content $csproj 
    
    # Check for XAML files as part of control with codebehind files 
    $dependentUpon = Select-Xml -Xml $proj -XPath "//msb:Compile/msb:DependentUpon" -Namespace $namespace | Select-Object -ExpandProperty Node 
    $dependentUpon | % { 
        $filename = $_.InnerText 

        Write-Verbose "Found $filename for project $name"; 
        
        # Build the .xbf source file name that should be in the nuspec
        $xbf = (Split-Path $filename -leaf).Replace(".xaml", ".xbf")
        $xbf = "..\$name\bin\`$Configuration`$\$xbf"

        # Add this .xbf to our requirements
        $requirements[$xbf] = $projectTargets[$name] 
    }

    # Check for XAML files included as Pages (Resources files, styles, etc.)
    $pageInclude = Select-Xml -Xml $proj -XPath "//msb:Page" -Namespace $namespace | Select-Object -ExpandProperty Node
    $pageInclude | % { 

        Write-Verbose "Found $($_.Include) for project $name";
                
        # Build the .xbf source file name that should be in the nuspec
        $xbf = (Split-Path $_.Include -leaf).Replace(".xaml", ".xbf")
        $xbf = "..\$name\bin\`$Configuration`$\$xbf"

        # Add this .xbf to our requirements
        $requirements[$xbf] = $projectTargets[$name]  
    }
}


# load up the nuspec file
[xml]$nuspec = Get-Content .\.nuspec\Xamarin.Forms.nuspec

# Keep track of which requirements aren't being met so we can display that in the build output
$failedRequirements = @()

# Also keep track of extra XBF entries which aren't required so we can display that in the build output
$extraEntries = @()

# Find all the xbf files listed in the nuspec
$nuspecFiles = $nuspec.package.files.file | ? { $_.src.EndsWith(".xbf") } 

# Iterate over the requirements and track each one that isn't met
Write-Verbose "Verifying that required XAML file has a corresponding XBF in nuspec..."
$requirements.Keys | % {
    $xbf = $_

    $requirements[$_] | % {
        $target = $_
     
        Write-Verbose "Checking for nuspec entry file = $xbf with target $target"
       
        $entries = $nuspecFiles | ? {
            ($_.src -eq $xbf) -and ($_.target -eq $target)
        }

        if(!$entries) {
           $failedRequirements +=  "Missing nuspec entry for $xbf with target $target"
        }
    }
}

# Iterate over the xbf entries and track each one that isn't a requirement
Write-Verbose "Verifying that each XBF entry in nuspec has an actual XAML file..."
$nuspecFiles | % {
    $entry = $_

    Write-Verbose "Checking entry with src = $($entry.src) and target = $($entry.target)"
       
    $srcMatch = $requirements.Keys | ? { $_ -eq $entry.src }

    if($srcMatch) {
        $requirements[$entry.src] | % { Write-Verbose $_ }
        $targetMatch = $requirements[$entry.src] | ? { $_ -eq $entry.target }
        if(-not $targetMatch) {
             $extraEntries += "XBF entry $($entry.src) doesn't have a corresponding XAML file"
        }
    } else {
         $extraEntries += "XBF entry $($entry.src) doesn't have a corresponding XAML file"
    }
}


# Emit the failed requirements and extra entries so they show up in build ouput
$failedRequirements
$extraEntries

if($failedRequirements -or $extraEntries) {
    # D'oh!
    exit 13
} else {
    # Woohoo!
    exit 0
}

