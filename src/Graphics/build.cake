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
// #addin "nuget:?package=Cake.Android.SdkManager&version=3.0.2"
// #addin "nuget:?package=Cake.Boots&version=1.0.4.600-preview1"
// #addin "nuget:?package=Cake.AppleSimulator&version=0.2.0"
// #addin "nuget:?package=Cake.FileHelpers&version=3.2.1"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
#tool "nuget:?package=nuget.commandline&version=5.8.1"
#load "eng/cake/dotnet.cake"
#load "eng/cake/helpers.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

string agentName = EnvironmentVariable("AGENT_NAME", "");
bool isCIBuild = !String.IsNullOrWhiteSpace(agentName);
string artifactStagingDirectory = EnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY", ".");
string workingDirectory = EnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY", ".");
string envProgramFiles = EnvironmentVariable("ProgramFiles(x86)");
var configuration = GetBuildVariable("BUILD_CONFIGURATION", GetBuildVariable("configuration", "DEBUG"));
var msbuildPath = GetBuildVariable("msbuild", $"{envProgramFiles}\\Microsoft Visual Studio\\2019\\Enterprise\\MSBuild\\Current\\Bin\\MSBuild.exe");
bool isHostedAgent = agentName.StartsWith("Azure Pipelines") || agentName.StartsWith("Hosted Agent");
string logDirectory = EnvironmentVariable("LogDirectory", $"{artifactStagingDirectory}/logs");
string testResultsDirectory = EnvironmentVariable("TestResultsDirectory", $"{artifactStagingDirectory}/test-results");
string MSBuildExe = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE", ""));
string MSBuildArgumentsENV = EnvironmentVariable("MSBuildArguments", "");
string MSBuildArgumentsARGS = Argument("MSBuildArguments", "");
string MSBuildArguments;
var localDotnet = GetBuildVariable("workloads",  (target == "VS-WINUI") ? "global" : "local") == "local";

var target = Argument("target", "Default");
if(String.IsNullOrWhiteSpace(target))
    target = "Default";

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

// Task("Default").IsDependentOn("dotnet-pack");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);