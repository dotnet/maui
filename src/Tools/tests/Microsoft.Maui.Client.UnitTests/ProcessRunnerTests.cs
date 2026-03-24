// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Client.Errors;
using Microsoft.Maui.Client.Providers.Android;
using Microsoft.Maui.Client.Services;
using Microsoft.Maui.Client.Utils;
using Xunit;

namespace Microsoft.Maui.Client.UnitTests;

public class ProcessRunnerTests
{
	[Fact]
	public void SanitizeArg_NullInput_ThrowsArgumentNullException()
	{
		Assert.Throws<ArgumentNullException>(() => ProcessRunner.SanitizeArg(null!));
	}

	[Theory]
	[InlineData(";")]
	[InlineData("&")]
	[InlineData("|")]
	[InlineData("`")]
	[InlineData("$")]
	[InlineData("\n")]
	[InlineData("\r")]
	[InlineData("\0")]
	public void SanitizeArg_ForbiddenCharacter_ThrowsArgumentException(string forbidden)
	{
		var ex = Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg($"test{forbidden}value"));
		Assert.Contains("forbidden characters", ex.Message);
	}

	[Fact]
	public void SanitizeArg_CommandInjection_ThrowsArgumentException()
	{
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("test; rm -rf /"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("test && evil"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("test | cat /etc/passwd"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("$(whoami)"));
		Assert.Throws<ArgumentException>(() => ProcessRunner.SanitizeArg("`whoami`"));
	}

	[Fact]
	public void SanitizeArg_ValueWithSpaces_ReturnsQuoted()
	{
		var result = ProcessRunner.SanitizeArg("hello world");
		Assert.Equal("\"hello world\"", result);
	}

	[Fact]
	public void SanitizeArg_SimpleValue_ReturnsUnchanged()
	{
		Assert.Equal("emulator-5554", ProcessRunner.SanitizeArg("emulator-5554"));
		Assert.Equal("ActivityManager:I", ProcessRunner.SanitizeArg("ActivityManager:I"));
		Assert.Equal("test123", ProcessRunner.SanitizeArg("test123"));
	}

	[Fact]
	public void SanitizeArg_EmptyString_ReturnsEmpty()
	{
		Assert.Equal("", ProcessRunner.SanitizeArg(""));
	}
}

public class DoctorServiceParseCommandTests
{
	[Fact]
	public void ParseCommand_SingleWord_ReturnsFileNameOnly()
	{
		var (fileName, arguments) = DoctorService.ParseCommand("dotnet");
		Assert.Equal("dotnet", fileName);
		Assert.Equal(string.Empty, arguments);
	}

	[Fact]
	public void ParseCommand_MultipleWords_SplitsCorrectly()
	{
		var (fileName, arguments) = DoctorService.ParseCommand("dotnet workload install maui");
		Assert.Equal("dotnet", fileName);
		Assert.Equal("workload install maui", arguments);
	}

	[Fact]
	public void ParseCommand_QuotedExecutable_SplitsCorrectly()
	{
		var (fileName, arguments) = DoctorService.ParseCommand("\"C:\\Program Files\\tool.exe\" --flag value");
		Assert.Equal("C:\\Program Files\\tool.exe", fileName);
		Assert.Equal("--flag value", arguments);
	}

	[Fact]
	public void ParseCommand_QuotedExecutableOnly_ReturnsEmptyArgs()
	{
		var (fileName, arguments) = DoctorService.ParseCommand("\"my tool\"");
		Assert.Equal("my tool", fileName);
		Assert.Equal(string.Empty, arguments);
	}

	[Theory]
	[InlineData("maui android jdk install", "maui", "android jdk install")]
	[InlineData("maui android install --accept-licenses", "maui", "android install --accept-licenses")]
	[InlineData("maui android sdk install platform-tools", "maui", "android sdk install platform-tools")]
	[InlineData("maui android sdk accept-licenses", "maui", "android sdk accept-licenses")]
	[InlineData("xcode-select --install", "xcode-select", "--install")]
	public void ParseCommand_RealFixCommands_SplitsCorrectly(string command, string expectedFile, string expectedArgs)
	{
		var (fileName, arguments) = DoctorService.ParseCommand(command);
		Assert.Equal(expectedFile, fileName);
		Assert.Equal(expectedArgs, arguments);
	}

	[Fact]
	public void ParseCommand_WhitespaceAroundCommand_TrimsCorrectly()
	{
		var (fileName, arguments) = DoctorService.ParseCommand("  dotnet  build  ");
		Assert.Equal("dotnet", fileName);
		Assert.Equal(" build", arguments);
	}
}

public class JdkManagerValidateInstallPathTests
{
	[Fact]
	public void ValidateInstallPath_HomeDirectory_Throws()
	{
		var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var ex = Assert.Throws<MauiToolException>(() => JdkManager.ValidateInstallPath(homePath));
		Assert.Contains("home directory", ex.Message);
	}

	[Theory]
	[InlineData("/")]
	[InlineData("/usr")]
	[InlineData("/bin")]
	[InlineData("/etc")]
	[InlineData("/tmp")]
	public void ValidateInstallPath_SystemDirectories_Throws(string path)
	{
		if (OperatingSystem.IsWindows())
			return; // Unix paths on Windows resolve differently

		var ex = Assert.Throws<MauiToolException>(() => JdkManager.ValidateInstallPath(path));
		Assert.Contains("system directory", ex.Message, StringComparison.OrdinalIgnoreCase);
	}

	[Fact]
	public void ValidateInstallPath_ShallowPath_Throws()
	{
		// A 2-segment path like "/Users/foo" or "C:\Users" should be rejected
		var shallowPath = OperatingSystem.IsWindows() ? "C:\\Users" : "/Users";
		var ex = Assert.Throws<MauiToolException>(() => JdkManager.ValidateInstallPath(shallowPath));
		Assert.Contains("too shallow", ex.Message);
	}

	[Fact]
	public void ValidateInstallPath_DeepEnoughPath_DoesNotThrow()
	{
		// A 3+ segment path should be accepted
		var validPath = OperatingSystem.IsWindows()
			? "C:\\Users\\test\\jdk-17"
			: "/Users/test/jdk-17";
		JdkManager.ValidateInstallPath(validPath); // Should not throw
	}
}
