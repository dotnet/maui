// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.CommandLine;
using System.CommandLine.Parsing;
using Microsoft.Maui.DevTools.Commands;
using Xunit;

namespace Microsoft.Maui.DevTools.Tests;

public class AndroidCommandsTests
{
	[Fact]
	public void InstallCommand_ParsesCommaSeparatedPackages()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var installCommand = androidCommand.Subcommands.First(c => c.Name == "install");
		var packagesOption = installCommand.Options.First(o => o.Name == "packages");

		// Act
		var parseResult = installCommand.Parse("install --packages platform-tools,build-tools;35.0.0,platforms;android-35");

		// Assert
		Assert.Empty(parseResult.Errors);
		var packages = parseResult.GetValueForOption((Option<string[]>)packagesOption);
		Assert.NotNull(packages);
		// The raw value will be a single string with commas - the handler splits it
		Assert.Single(packages);
		Assert.Equal("platform-tools,build-tools;35.0.0,platforms;android-35", packages[0]);
	}

	[Fact]
	public void InstallCommand_ParsesMultiplePackageFlags()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var installCommand = androidCommand.Subcommands.First(c => c.Name == "install");
		var packagesOption = installCommand.Options.First(o => o.Name == "packages");

		// Act
		var parseResult = installCommand.Parse("install --packages platform-tools --packages build-tools;35.0.0");

		// Assert
		Assert.Empty(parseResult.Errors);
		var packages = parseResult.GetValueForOption((Option<string[]>)packagesOption);
		Assert.NotNull(packages);
		Assert.Equal(2, packages.Length);
	}

	[Fact]
	public void InstallCommand_HasCorrectOptions()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var installCommand = androidCommand.Subcommands.First(c => c.Name == "install");

		// Assert
		Assert.Contains(installCommand.Options, o => o.Name == "sdk-path");
		Assert.Contains(installCommand.Options, o => o.Name == "jdk-path");
		Assert.Contains(installCommand.Options, o => o.Name == "jdk-version");
		Assert.Contains(installCommand.Options, o => o.Name == "packages");
	}

	[Fact]
	public void EmulatorCreateCommand_PackageIsOptional()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");
		var createCommand = emulatorCommand.Subcommands.First(c => c.Name == "create");
		var packageOption = createCommand.Options.First(o => o.Name == "package");

		// Assert
		Assert.False(packageOption.IsRequired);
	}

	[Fact]
	public void EmulatorCreateCommand_HasRequiredNameArgument()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");
		var createCommand = emulatorCommand.Subcommands.First(c => c.Name == "create");

		// Assert
		Assert.Single(createCommand.Arguments);
		Assert.Equal("name", createCommand.Arguments.First().Name);
	}

	[Fact]
	public void EmulatorDeleteCommand_Exists()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");

		// Assert
		Assert.Contains(emulatorCommand.Subcommands, c => c.Name == "delete");
	}

	[Fact]
	public void EmulatorCommand_HasStopSubcommand()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");

		// Assert
		Assert.Contains(emulatorCommand.Subcommands, c => c.Name == "stop");
	}

	[Fact]
	public void EmulatorStopCommand_HasRequiredNameArgument()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");
		var stopCommand = emulatorCommand.Subcommands.First(c => c.Name == "stop");

		// Assert
		Assert.Single(stopCommand.Arguments);
		Assert.Equal("name", stopCommand.Arguments.First().Name);
	}

	[Fact]
	public void EmulatorDeleteCommand_HasRequiredNameArgument()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");
		var deleteCommand = emulatorCommand.Subcommands.First(c => c.Name == "delete");

		// Assert
		Assert.Single(deleteCommand.Arguments);
		Assert.Equal("name", deleteCommand.Arguments.First().Name);
	}

	[Fact]
	public void EmulatorStartCommand_HasColdBootOption()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var emulatorCommand = androidCommand.Subcommands.First(c => c.Name == "emulator");
		var startCommand = emulatorCommand.Subcommands.First(c => c.Name == "start");

		// Assert
		Assert.Contains(startCommand.Options, o => o.Name == "cold-boot");
		Assert.Contains(startCommand.Options, o => o.Name == "wait");
	}

	[Fact]
	public void JdkCommand_HasAllSubcommands()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var jdkCommand = androidCommand.Subcommands.First(c => c.Name == "jdk");

		// Assert
		Assert.Contains(jdkCommand.Subcommands, c => c.Name == "check");
		Assert.Contains(jdkCommand.Subcommands, c => c.Name == "install");
		Assert.Contains(jdkCommand.Subcommands, c => c.Name == "list");
	}

	[Fact]
	public void SdkCommand_HasAllSubcommands()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var sdkCommand = androidCommand.Subcommands.First(c => c.Name == "sdk");

		// Assert
		Assert.Contains(sdkCommand.Subcommands, c => c.Name == "check");
		Assert.Contains(sdkCommand.Subcommands, c => c.Name == "install");
		Assert.Contains(sdkCommand.Subcommands, c => c.Name == "list");
		Assert.Contains(sdkCommand.Subcommands, c => c.Name == "accept-licenses");
	}

	[Fact]
	public void SdkListCommand_HasAvailableAndAllOptions()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();
		var sdkCommand = androidCommand.Subcommands.First(c => c.Name == "sdk");
		var listCommand = sdkCommand.Subcommands.First(c => c.Name == "list");

		// Assert
		Assert.Contains(listCommand.Options, o => o.Name == "available");
		Assert.Contains(listCommand.Options, o => o.Name == "all");
	}

	[Fact]
	public void AndroidCommand_HasAllSubcommands()
	{
		// Arrange
		var androidCommand = AndroidCommands.Create();

		// Assert
		Assert.Contains(androidCommand.Subcommands, c => c.Name == "install");
		Assert.Contains(androidCommand.Subcommands, c => c.Name == "jdk");
		Assert.Contains(androidCommand.Subcommands, c => c.Name == "sdk");
		Assert.Contains(androidCommand.Subcommands, c => c.Name == "emulator");
	}
}
