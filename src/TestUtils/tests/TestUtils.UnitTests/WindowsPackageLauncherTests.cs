using System.Diagnostics;
using System.Text.Json;
using Microsoft.Maui.TestUtils.WindowsTesting;
using Xunit;

namespace Microsoft.Maui.TestUtils.UnitTests;

public class WindowsPackageLauncherTests
{
	public static bool IsWindows => OperatingSystem.IsWindows();

	static PackagedTestState State(int timeout = 10)
	{
		var owner = Guid.NewGuid().ToString("N");
		var name = "maui.tests." + owner;
		return new()
		{
			OwnerId = owner,
			IdentityName = name,
			Publisher = "CN=MauiTests-" + owner,
			PackageFullName = name + "_1.0.0.0_x64__publisher",
			PackageFamilyName = name + "_publisher",
			AppUserModelId = name + "_publisher!App",
			InstallLocation = Path.GetFullPath("installed-test-package"),
			TimeoutSeconds = timeout
		};
	}

	static Process Child(int? exitCode = null)
	{
		var windows = OperatingSystem.IsWindows();
		var start = new ProcessStartInfo(windows ? Path.Combine(Environment.SystemDirectory, "cmd.exe") : "/bin/sh")
		{
			UseShellExecute = false,
			RedirectStandardInput = true
		};
		// Hold the native process until its PID/start time are recorded; no managed
		// child runtime or sleeping grandchild is needed for timeout/cancellation.
		var command = windows ? "set /p fixture=" : "read fixture";
		if (exitCode.HasValue)
			command += windows ? $" & exit {exitCode.Value}" : $"; exit {exitCode.Value}";
		if (windows)
			start.ArgumentList.Add("/d");
		start.ArgumentList.Add(windows ? "/c" : "-c");
		start.ArgumentList.Add(command);
		return Process.Start(start)!;
	}

	[Theory]
	[InlineData("", "\"\"")]
	[InlineData("two words", "\"two words\"")]
	[InlineData("trailing\\", "\"trailing\\\\\"")]
	[InlineData("a\"b", "\"a\\\"b\"")]
	public void UsesWindowsArgvQuoting(string value, string expected) =>
		Assert.Equal(expected, WindowsTestCommandLine.Quote(value));

	[Fact(SkipUnless = nameof(IsWindows), Skip = "CommandLineToArgvW runtime verification requires Windows.")]
	public void ActualCommandLineToArgvWRoundtripsTheCompleteMtpArgumentVector()
	{
		string[] arguments = ["--server", "--dotnet-test-pipe", "pipe-42", "--results-directory",
			@"C:\path with spaces\", "--report-trx-filename", "quote\"name.trx", "", "Ω", "slash\\\"quote"];
		Assert.Equal(arguments, WindowsTestCommandLine.ParseActivation(WindowsTestCommandLine.Format(arguments)));
	}

	[Fact]
	public void RejectsNulBeforeCallingNativeActivation() =>
		Assert.Throws<ArgumentException>(() => WindowsTestCommandLine.Format(["bad\0argument"]));

	[Fact]
	public void RejectsAStateFileThatNamesAnotherOwnersPackage()
	{
		var state = State();
		state.IdentityName = "another-package";
		Assert.Throws<InvalidOperationException>(() => PackagedTestSession.Validate(state));
	}

	[Fact]
	public void LauncherStateJsonPreservesExactOwnershipAndPidTicks()
	{
		var state = State();
		state.ProcessId = 12345;
		state.ProcessStartUtcTicks = DateTime.UtcNow.Ticks;
		var restored = JsonSerializer.Deserialize<PackagedTestState>(JsonSerializer.Serialize(state))!;
		PackagedTestSession.Validate(restored);
		Assert.Equal(state.Publisher, restored.Publisher);
		Assert.Equal(state.AppUserModelId, restored.AppUserModelId);
		Assert.Equal(state.ProcessStartUtcTicks, restored.ProcessStartUtcTicks);
		Assert.Equal(state.ProcessId, restored.ProcessId);
	}

	[Theory]
	[InlineData(0)]
	[InlineData(23)]
	public void ForwardsEveryMtpArgumentAndPropagatesTheActualActivatedPidExit(int exitCode)
	{
		var state = State();
		state.ProcessExited = true;
		string[] arguments = ["--server", "--dotnet-test-pipe", "the-pipe", "--results-directory", "with spaces"];
		var saved = 0;
		Process? child = null;
		var result = PackagedTestSession.Run(state, arguments,
			(aumid, commandLine) =>
			{
				Assert.Equal(state.AppUserModelId, aumid);
				Assert.Equal(WindowsTestCommandLine.Format(arguments), commandLine);
				return child = Child(exitCode);
			}, processId =>
			{
				Assert.Equal(child!.Id, processId);
				Assert.Equal(processId, state.ProcessId);
				Assert.True(state.ProcessStartUtcTicks > 0);
				child!.StandardInput.WriteLine("finish");
				child.StandardInput.Close();
				return state.PackageFullName;
			}, _ => state.Publisher, value =>
			{
				if (saved++ == 0)
					Assert.False(value.ProcessExited);
			});
		Assert.Equal(exitCode, result);
		Assert.True(state.ProcessId > 0);
		Assert.True(state.ProcessStartUtcTicks > 0);
		Assert.True(state.ProcessIdentityVerified);
		Assert.True(state.ProcessExited);
		Assert.True(saved >= 3);
	}

	[Fact]
	public void TimeoutStopsTheRecordedActivatedPidAndFails()
	{
		var state = State(timeout: 1);
		var activatedPid = 0;
		var result = PackagedTestSession.Run(state, ["--server"],
			(_, _) =>
			{
				var child = Child();
				activatedPid = child.Id;
				return child;
			},
			_ => state.PackageFullName, _ => state.Publisher, _ => { });
		Assert.Equal(124, result);
		Assert.Equal(activatedPid, state.ProcessId);
		Assert.True(state.ProcessExited);
	}

	[Fact]
	public void CancellationStopsOnlyTheRecordedActivatedPidAndFails()
	{
		var state = State();
		using var cancellation = new CancellationTokenSource();
		cancellation.Cancel();
		var activatedPid = 0;
		var result = PackagedTestSession.Run(state, ["--server"],
			(_, _) =>
			{
				var child = Child();
				activatedPid = child.Id;
				return child;
			},
			_ => state.PackageFullName, _ => state.Publisher, _ => { }, cancellation.Token);
		Assert.Equal(130, result);
		Assert.Equal(activatedPid, state.ProcessId);
		Assert.True(state.ProcessExited);
	}

	[Fact]
	public void AProcessFromAnotherPackageIsNeverTerminated()
	{
		var state = State();
		using var child = Child();
		try
		{
			Assert.Throws<InvalidOperationException>(() => PackagedTestSession.Run(state, ["--server"],
				(_, _) => Process.GetProcessById(child.Id), _ => "another-package", _ => state.Publisher, _ => { }));
			Assert.False(child.HasExited);
			Assert.False(state.ProcessIdentityVerified);
		}
		finally
		{
			if (!child.HasExited)
			{ child.Kill(); child.WaitForExit(); }
		}
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void RejectsForeignInstalledPackageBeforeActivation(bool consistentForeignIdentifiers)
	{
		var state = State();
		if (consistentForeignIdentifiers)
		{
			state.IdentityName = "foreign.application";
			state.PackageFullName = "foreign.application_1.0.0.0_x64__foreign";
			state.PackageFamilyName = "foreign.application_foreign";
			state.AppUserModelId = state.PackageFamilyName + "!App";
		}
		var activated = false;
		Assert.Throws<InvalidOperationException>(() => PackagedTestSession.Run(state, [],
			(_, _) => { activated = true; throw new InvalidOperationException("Must not activate."); },
			_ => state.PackageFullName, _ => "CN=Foreign publisher", _ => { }));
		Assert.False(activated);
	}

	[Fact]
	public void RefusesTerminationWhenRegisteredPublisherNoLongerMatches()
	{
		var state = State(timeout: 1);
		using var child = Child();
		var publisherQueries = 0;
		try
		{
			Assert.Throws<InvalidOperationException>(() => PackagedTestSession.Run(state, [],
				(_, _) => Process.GetProcessById(child.Id), _ => state.PackageFullName,
				_ => publisherQueries++ == 0 ? state.Publisher : "CN=Foreign publisher", _ => { }));
			Assert.False(child.HasExited);
		}
		finally
		{
			if (!child.HasExited)
			{ child.Kill(); child.WaitForExit(); }
		}
	}
}
