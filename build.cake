// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Test change: Adding test modifications for log testing


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
// ARGUMENTS
//////////////////////////////////////////////////////////////////////


var target = Argument<string>("target", "Default");
if(String.IsNullOrWhiteSpace(target))
    target = "Default";

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("dotnet").IsDependentOn("dotnet-pack");

// Test task with intentional compiler error for log testing
Task("test-compiler-error")
    .Description("Test task that intentionally causes a compiler error to test logging")
    .Does(() =>
    {
        Information("Testing compiler error logging...");
        
        try 
        {
            // This should fail due to syntax error in TestCompilerError.cs
            DotNetBuild("./TestCompilerError.csproj", new DotNetBuildSettings
            {
                Configuration = "Debug",
                MSBuildSettings = new DotNetMSBuildSettings()
                    .EnableBinaryLogger($"{GetLogDirectory()}/test-compiler-error-{DateTime.UtcNow.ToFileTimeUtc()}.binlog")
            });
        }
        catch (Exception ex)
        {
            Information("Expected compiler error occurred for log testing: " + ex.Message);
            throw; // Re-throw to ensure the error is logged properly
        }
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);