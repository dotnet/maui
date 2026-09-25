using System.Diagnostics;
using System.Security.Cryptography;
using ImageMagick;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.UiEvidence;

static class UiEvidenceRunCommand
{
	const string AppId = "com.microsoft.maui.uitests";

	public static async Task<int> ExecuteAsync(CommandLineOptions options)
	{
		var platform = options.Required("platform");
		if (platform is not ("android" or "windows"))
			throw new ArgumentException("Platform must be 'android' or 'windows'.");
		var deviceId = platform == "android" ? RequiredDeviceId(options) : string.Empty;

		var variant = options.Required("variant");
		if (variant is not ("base" or "head"))
			throw new ArgumentException("Variant must be 'base' or 'head'.");

		var appPath = Path.GetFullPath(options.Required("app"));
		if (!File.Exists(appPath))
			throw new FileNotFoundException("App artifact was not found.", appPath);

		var output = Path.GetFullPath(options.Required("output"));
		Directory.CreateDirectory(output);
		var registry = UiEvidenceJson.Read<ScenarioRegistry>(options.Required("registry"));
		var scenarioId = options.Required("scenario");
		var scenario = registry.Scenarios.SingleOrDefault(item => item.Id == scenarioId)
			?? throw new ArgumentException($"Unknown scenario '{scenarioId}'.");
		if (!scenario.Platforms.Contains(platform, StringComparer.Ordinal))
			throw new ArgumentException($"Scenario '{scenarioId}' does not support platform '{platform}'.");

		var startedAt = DateTimeOffset.UtcNow;
		var result = new UiEvidenceRunResult
		{
			RequestKey = options.Required("request-key"),
			ScenarioId = scenarioId,
			Platform = platform,
			Variant = variant,
			VariantRunOrdinal = options.OptionalInt("run-ordinal", 1),
			SequenceOrdinal = options.OptionalInt("sequence-ordinal", 1),
			CommitSha = options.Required("commit-sha").ToLowerInvariant(),
			HarnessSha = options.Required("harness-sha").ToLowerInvariant(),
			Status = "harness-failed",
			StartedAtUtc = startedAt.ToString("O"),
			FinishedAtUtc = startedAt.ToString("O"),
			AppArtifactSha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(appPath))).ToLowerInvariant(),
			Environment = CreateEnvironment(platform, options, inspectDevice: false)
		};

		AppiumApp? app = null;
		var forwardedPort = 0;
		try
		{
			result.Environment = CreateEnvironment(platform, options);
			if (platform == "android")
				ResetAndroidApp(appPath, deviceId);
			else
				EnsureWindowsAppNotRunning(appPath);

			app = CreateApp(platform, appPath, output, options);
			var allAssertionsPassed = true;
			foreach (var checkpoint in scenario.Checkpoints)
			{
				foreach (var automationId in checkpoint.RequiredAutomationIds)
				{
					try
					{
						var element = app.WaitForElement(
							automationId,
							$"Timed out waiting for trusted UI evidence element '{automationId}'.",
							TimeSpan.FromSeconds(30));
						var rect = element.GetRect();
						var passed = rect.Width > 0 && rect.Height > 0;
						allAssertionsPassed &= passed;
						result.Assertions.Add(new UiEvidenceAssertion(
							automationId,
							passed ? "passed" : "failed",
							new UiEvidenceRect(rect.X, rect.Y, rect.Width, rect.Height)));
					}
					catch (TimeoutException)
					{
						allAssertionsPassed = false;
						result.Assertions.Add(new UiEvidenceAssertion(automationId, "failed", null));
					}
				}

				var screenshotPath = Path.Combine(output, "screenshots", $"{checkpoint.Id}.png");
				Directory.CreateDirectory(Path.GetDirectoryName(screenshotPath)!);
				app.Screenshot(screenshotPath);
				using var image = new MagickImage(screenshotPath);
				image.Strip();
				image.ColorSpace = ColorSpace.sRGB;
				image.Format = MagickFormat.Png;
				image.Write(screenshotPath);
				result.Checkpoints.Add(new UiEvidenceCheckpoint(
					checkpoint.Id,
					Path.GetRelativePath(output, screenshotPath).Replace('\\', '/'),
					Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(screenshotPath))).ToLowerInvariant(),
					checked((int)image.Width),
					checked((int)image.Height)));
			}

			if (options.Flag("devflow"))
			{
				var devFlowPort = options.OptionalInt("devflow-port", 9223);
				if (platform == "android")
				{
					ForwardAndroidPort(deviceId, devFlowPort);
					forwardedPort = devFlowPort;
				}
				result.DevFlow = await DevFlowEvidenceCollector.CaptureAsync(output, devFlowPort);
			}

			result.Status = allAssertionsPassed ? "passed" : "scenario-failed";
		}
		catch (TimeoutException)
		{
			result.Status = "timed-out";
			result.ErrorCodes.Add("ui-evidence-timeout");
		}
		catch (Exception ex)
		{
			result.Status = "harness-failed";
			result.ErrorCodes.Add($"ui-evidence-{ex.GetType().Name.ToLowerInvariant()}");
		}
		finally
		{
			void Cleanup(Action action, string code)
			{
				try
				{
					action();
				}
				catch (Exception ex)
				{
					result.Status = "harness-failed";
					result.ErrorCodes.Add(code);
					Console.Error.WriteLine($"{code}: {ex.Message}");
				}
			}
			if (app is not null)
			{
				Cleanup(() => app.CloseApp(), "ui-evidence-app-close-failed");
				Cleanup(app.Dispose, "ui-evidence-app-dispose-failed");
			}
			if (forwardedPort != 0)
				Cleanup(() => RunProcess("adb", ["-s", deviceId, "forward", "--remove", $"tcp:{forwardedPort}"], false),
					"ui-evidence-forward-cleanup-failed");
			result.FinishedAtUtc = DateTimeOffset.UtcNow.ToString("O");
			UiEvidenceJson.Write(Path.Combine(output, "run-result.json"), result);
		}

		return 0;
	}

	static AppiumApp CreateApp(
		string platform,
		string appPath,
		string output,
		CommandLineOptions options)
	{
		var config = CreateAppConfig(platform, appPath, output, options);
		var remoteAddress = new Uri(options.Optional("appium-url", "http://127.0.0.1:4723/wd/hub"));
		return platform == "android"
			? AppiumAndroidApp.CreateAndroidApp(remoteAddress, config)
			: new AppiumWindowsApp(remoteAddress, config);
	}

	internal static string RequiredDeviceId(CommandLineOptions options)
	{
		var deviceId = options.Required("device-id");
		if (deviceId == "true" || deviceId.Any(char.IsWhiteSpace) || deviceId.StartsWith('-'))
			throw new ArgumentException("Option '--device-id' must identify an explicit Android device.");
		return deviceId;
	}

	internal static Config CreateAppConfig(string platform, string appPath, string output, CommandLineOptions options)
	{
		var config = new Config();
		config.SetProperty("AppPath", appPath);
		config.SetProperty("AppId", AppId);
		config.SetProperty("ReportDirectory", output);
		config.SetProperty("ReportFormat", "xml");
		config.SetProperty("TestName", $"ui-evidence-{options.Required("variant")}-{options.OptionalInt("run-ordinal", 1)}");
		config.SetProperty("FullReset", false);
		config.SetProperty("NoReset", false);
		config.SetProperty("Headless", options.Flag("headless"));
		config.SetProperty("DeviceName", options.Optional("device-name", string.Empty));
		if (platform == "android")
			config.SetProperty("Udid", RequiredDeviceId(options));
		config.SetProperty("PlatformVersion", options.Optional("platform-version", string.Empty));
		config.SetProperty("EnableDebugPopup", false);
		config.SetProperty("AvdForceInstall", false);

		return config;
	}

	static void ResetAndroidApp(string appPath, string deviceId)
	{
		List<string> Arguments(params string[] values)
		{
			var arguments = new List<string>();
			if (!string.IsNullOrWhiteSpace(deviceId))
				arguments.AddRange(["-s", deviceId]);
			arguments.AddRange(values);
			return arguments;
		}

		var installed = RunProcessCapture("adb", Arguments("shell", "pm", "list", "packages", AppId))
			.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
			.Contains($"package:{AppId}", StringComparer.Ordinal);
		if (installed)
			RunProcess("adb", Arguments("uninstall", AppId), allowFailure: false);

		RunProcess("adb", Arguments("install", "-r", "-t", appPath), allowFailure: false);
		if (string.IsNullOrWhiteSpace(
			RunProcessCapture("adb", Arguments("shell", "pm", "path", AppId))))
		{
			throw new InvalidOperationException("The requested Android app was not installed.");
		}
	}

	static void ForwardAndroidPort(string deviceId, int port)
	{
		var arguments = new List<string>();
		if (!string.IsNullOrWhiteSpace(deviceId))
		{
			arguments.Add("-s");
			arguments.Add(deviceId);
		}
		arguments.AddRange(["forward", "--no-rebind", $"tcp:{port}", $"tcp:{port}"]);
		RunProcess("adb", arguments, allowFailure: false);
	}

	internal static void EnsureWindowsAppNotRunning(string appPath)
	{
		var processName = Path.GetFileNameWithoutExtension(appPath);
		foreach (var process in Process.GetProcessesByName(processName))
		{
			try
			{
				if (!process.HasExited)
					throw new InvalidOperationException($"A same-name app is already running (PID {process.Id}); refusing to terminate an unowned process.");
			}
			finally
			{
				process.Dispose();
			}
		}
	}

	static void RunProcess(string fileName, IEnumerable<string> arguments, bool allowFailure)
	{
		var result = UiEvidenceProcess.RunAsync(fileName, arguments).GetAwaiter().GetResult();
		if (!allowFailure && result.ExitCode != 0)
			throw new InvalidOperationException($"{fileName} exited with code {result.ExitCode}: {result.Error.Trim()}");
	}

	static UiEvidenceEnvironment CreateEnvironment(string platform, CommandLineOptions options, bool inspectDevice = true)
	{
		var deviceId = options.Optional("device-id", string.Empty);
		if (platform == "android" && inspectDevice)
		{
			string Adb(params string[] arguments)
			{
				var effectiveArguments = new List<string>();
				if (!string.IsNullOrWhiteSpace(deviceId))
					effectiveArguments.AddRange(["-s", deviceId]);
				effectiveArguments.AddRange(arguments);
				return RunProcessCapture("adb", effectiveArguments);
			}

			return new UiEvidenceEnvironment(
				"android",
				Environment.OSVersion.VersionString,
				Environment.Version.ToString(),
				Environment.MachineName,
				string.IsNullOrWhiteSpace(deviceId) ? null : deviceId,
				Adb("shell", "getprop", "ro.product.model"),
				Adb("shell", "getprop", "ro.build.version.sdk"),
				Adb("shell", "wm", "size"),
				Adb("shell", "wm", "density"),
				Adb("shell", "settings", "get", "system", "user_rotation"),
				options.Optional("appium-url", "http://127.0.0.1:4723/wd/hub"));
		}

		return new UiEvidenceEnvironment(
			platform,
			Environment.OSVersion.VersionString,
			Environment.Version.ToString(),
			Environment.MachineName,
			string.IsNullOrWhiteSpace(deviceId) ? null : deviceId,
			platform == "windows" ? Environment.MachineName : null,
			platform == "windows" ? Environment.OSVersion.Version.ToString() : null,
			null,
			null,
			null,
			options.Optional("appium-url", "http://127.0.0.1:4723/wd/hub"));
	}

	static string RunProcessCapture(string fileName, IEnumerable<string> arguments)
	{
		var result = UiEvidenceProcess.RunAsync(fileName, arguments).GetAwaiter().GetResult();
		if (result.ExitCode != 0)
			throw new InvalidOperationException($"{fileName} failed while collecting device identity: {result.Error.Trim()}");
		return result.Output.Replace("\r", string.Empty, StringComparison.Ordinal).Trim();
	}
}
