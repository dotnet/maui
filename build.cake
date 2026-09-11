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
#addin "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json?package=Cake.Android.SdkManager&version=3.0.2"
#addin "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json?package=Cake.AppleSimulator&version=0.2.0"
#addin "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json?package=Cake.FileHelpers&version=3.2.1"
#load "eng/cake/dotnet.cake"
#load "eng/cake/helpers.cake"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool "nuget:https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json?package=nuget.commandline&version=6.6.1"


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