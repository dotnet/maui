// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// examples
/*

Windows CMD:
build.cmd -Target dotnet-pack
build.cmd -Target dotnet-pack -ScriptArgs '--packageVersion="9.9.9-custom"','--configuration="Release"'

PowerShell:
./build.ps1 -Target dotnet-pack
./build.ps1 -Target dotnet-pack -ScriptArgs '--packageVersion="9.9.9-custom"'

 */
//////////////////////////////////////////////////////////////////////
// ADDINS
//////////////////////////////////////////////////////////////////////
#addin "nuget:?package=Cake.Android.SdkManager&version=3.0.2"
#addin "nuget:?package=Cake.AppleSimulator&version=0.2.0"
#addin "nuget:?package=Cake.FileHelpers&version=3.2.1"
#load "eng/cake/dotnet.cake"
#load "eng/cake/helpers.cake"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:?package=nuget.commandline&version=6.6.1"

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("GenerateCgManifest")
    .Description("Generates the cgmanifest.json file with versions from Versions.props")
    .Does(() => 
{
    Information("Generating cgmanifest.json from Versions.props");
    
    // Use pwsh on all platforms
    var pwshExecutable = "pwsh";
    
    // Check if pwsh is available
    try {
        if (IsRunningOnWindows()) {
            var exitCode = StartProcess("where", new ProcessSettings {
                Arguments = "pwsh",
                RedirectStandardOutput = true, 
                RedirectStandardError = true
            });
            if (exitCode != 0) {
                Information("pwsh not found, falling back to powershell");
                pwshExecutable = "powershell";
            }
        } else {
            var exitCode = StartProcess("which", new ProcessSettings {
                Arguments = "pwsh",
                RedirectStandardOutput = true,
                RedirectStandardError = true
            });
            if (exitCode != 0) {
                throw new Exception("PowerShell Core (pwsh) is not installed. Please install it to continue.");
            }
        }
    } catch (Exception ex) when (!IsRunningOnWindows()) {
        Error("Error checking for pwsh: " + ex.Message);
        throw new Exception("PowerShell Core (pwsh) is required on non-Windows platforms. Please install it and try again.");
    }
    
    // Execute the PowerShell script
    StartProcess(pwshExecutable, new ProcessSettings {
        Arguments = "-NonInteractive -ExecutionPolicy Bypass -File ./eng/scripts/update-cgmanifest.ps1"
    });
});

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////


var target = Argument<string>("target", "Default");
if(String.IsNullOrWhiteSpace(target))
    target = "Default";

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("dotnet").IsDependentOn("dotnet-pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);