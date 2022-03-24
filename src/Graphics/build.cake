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
#load "eng/cake/dotnet.cake"
#load "eng/cake/helpers.cake"

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////
#tool nuget:?package=NUnit.ConsoleRunner&version=3.11.1
#tool "nuget:?package=nuget.commandline&version=5.8.1"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

string agentName = EnvironmentVariable("AGENT_NAME", "");
bool isCIBuild = !String.IsNullOrWhiteSpace(agentName);
string workingDirectory = EnvironmentVariable("SYSTEM_DEFAULTWORKINGDIRECTORY", ".");
string envProgramFiles = EnvironmentVariable("ProgramFiles(x86)");
var configuration = GetBuildVariable("BUILD_CONFIGURATION", GetBuildVariable("configuration", "DEBUG"));
bool isHostedAgent = agentName.StartsWith("Azure Pipelines") || agentName.StartsWith("Hosted Agent");

var dotnetInstallDirectory = EnvironmentVariable("DOTNET_ROOT");

var localDotnet = GetBuildVariable("workloads",  (target == "VS-WINUI") ? "global" : "local") == "local";
var vsVersion = GetBuildVariable("VS", "");

var installDotNet = GetBuildVariable("installdotnet", "true");;

DirectoryPath artifactStagingDirectory = MakeAbsolute(Directory(EnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY", "artifacts")));
DirectoryPath logDirectory = MakeAbsolute(Directory(EnvironmentVariable("LogDirectory", $"{artifactStagingDirectory}/logs")));
DirectoryPath testResultsDirectory = MakeAbsolute(Directory(EnvironmentVariable("TestResultsDirectory", $"{artifactStagingDirectory}/test-results")));
DirectoryPath diffDirectory = MakeAbsolute(Directory(EnvironmentVariable("ApiDiffDirectory", $"{artifactStagingDirectory}/api-diff")));
DirectoryPath tempDirectory = MakeAbsolute(Directory(EnvironmentVariable("AGENT_TEMPDIRECTORY", EnvironmentVariable("TEMP", EnvironmentVariable("TMPDIR", "../maui-temp")) + "/" + Guid.NewGuid())));

string MSBuildExe = Argument("msbuild", EnvironmentVariable("MSBUILD_EXE", ""));
string MSBuildArgumentsENV = EnvironmentVariable("MSBuildArguments", "");
string MSBuildArgumentsARGS = Argument("MSBuildArguments", "");
string MSBuildArguments;

var target = Argument("target", "Default");
if(String.IsNullOrWhiteSpace(target))
    target = "Default";

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default").IsDependentOn("dotnet");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);