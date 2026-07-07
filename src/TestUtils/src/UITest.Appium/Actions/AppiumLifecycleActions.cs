using System.Diagnostics;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumLifecycleActions : ICommandExecutionGroup
	{
		const string LaunchAppCommand = "launchApp";
		const string BackgroundAppCommand = "backgroundApp";
		const string ForegroundAppCommand = "foregroundApp";
		const string ResetAppCommand = "resetApp";
		const string CloseAppCommand = "closeApp";
		const string ForceCloseAppCommand = "forceCloseApp";
		const string RecoverFromCrashCommand = "recoverFromCrash";
		const string BackCommand = "back";
		const string RefreshCommand = "refresh";

		protected readonly AppiumApp _app;

		readonly List<string> _commands = new()
		{
			LaunchAppCommand,
			ForegroundAppCommand,
			BackgroundAppCommand,
			ResetAppCommand,
			CloseAppCommand,
			ForceCloseAppCommand,
			RecoverFromCrashCommand,
			BackCommand,
			RefreshCommand
		};

		public AppiumLifecycleActions(AppiumApp app)
		{
			_app = app;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				LaunchAppCommand => LaunchApp(parameters),
				ForegroundAppCommand => ForegroundApp(parameters),
				BackgroundAppCommand => BackgroundApp(parameters),
				ResetAppCommand => ResetApp(parameters),
				CloseAppCommand => CloseApp(parameters),
				ForceCloseAppCommand => ForceCloseApp(parameters),
				RecoverFromCrashCommand => RecoverFromCrash(parameters),
				BackCommand => Back(parameters),
				RefreshCommand => Refresh(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse LaunchApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			if (_app.GetTestDevice() == TestDevice.Mac)
			{		
				var args = _app.Config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs") ?? new Dictionary<string, string>();

				if (args.ContainsKey("test") && parameters.ContainsKey("testName") && parameters["testName"] is string testName && !string.IsNullOrEmpty(testName))
				{
					args["test"] = testName;
				}

				_app.Driver.ExecuteScript("macos: launchApp", new Dictionary<string, object>
				{
					{ "bundleId", _app.GetAppId() },
					{ "environment", args},
				});
			}
			else if (_app.Driver is WindowsDriver windowsDriver)
			{
				// Appium driver removed the LaunchApp method in 5.0.0, so we need to use the executeScript method instead
				// Currently the appium-windows-driver reports the following commands as compatible:
				//   startRecordingScreen,stopRecordingScreen,launchApp,closeApp,deleteFile,deleteFolder,
				//   click,scroll,clickAndDrag,hover,keys,setClipboard,getClipboard
				windowsDriver.ExecuteScript("windows: launchApp", [_app.GetAppId()]);
			}
			else if (_app.Driver is IOSDriver iOSDriver)
			{
				var args = _app.Config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs") ?? new Dictionary<string, string>();
				iOSDriver.ExecuteScript("mobile: launchApp", new Dictionary<string, object>
				{
					{ "bundleId", _app.GetAppId() },
					{ "environment", args },
				});
			}
			else
			{
				_app.Driver.ActivateApp(_app.GetAppId());
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse ForegroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			if (_app.Driver is WindowsDriver wd)
			{
				wd.SwitchTo().Window(wd.WindowHandles.First());
			}
			else
			{
				_app.Driver.ActivateApp(_app.GetAppId());
				// Give it time for the animation to settle, otherwise there's a risk
				// of picking wrong elements coordinates and `Tap`s will fail silently.
				Thread.Sleep(100);
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse BackgroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			_app.Driver.BackgroundApp();
			if (_app.GetTestDevice() == TestDevice.Android)
				Thread.Sleep(500);

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse ResetApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			CloseApp(parameters);
			LaunchApp(parameters);

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse CloseApp(IDictionary<string, object> parameters)
		{
			try
			{
				if (_app is null || _app.Driver is null)
					return CommandResponse.FailedEmptyResponse;

				if (_app.AppState == ApplicationState.NotRunning)
					return CommandResponse.SuccessEmptyResponse;
			}
			catch (Exception)
			{
				// App might be too locked up to even report state — try force close
				return ForceCloseApp(parameters);
			}

			// Try normal Appium termination with a timeout to prevent hanging when app is unresponsive
			try
			{
				var closeTask = Task.Run(() =>
				{
					if (_app.GetTestDevice() == TestDevice.Mac)
					{
						_app.Driver.ExecuteScript("macos: terminateApp", new Dictionary<string, object>
						{
							{ "bundleId", _app.GetAppId() },
						});
					}
					else if (_app.Driver is WindowsDriver windowsDriver)
					{
						windowsDriver.CloseApp();
					}
					else
					{
						_app.Driver.TerminateApp(_app.GetAppId());
					}
				});

				if (closeTask.Wait(TimeSpan.FromSeconds(15)))
				{
					return CommandResponse.SuccessEmptyResponse;
				}

				// Normal close timed out — app is likely unresponsive, use force close
				Debug.WriteLine(">>>>> CloseApp timed out after 15s, falling back to ForceCloseApp");
			}
			catch (Exception ex)
			{
				// Normal close failed — fall through to force close
				Debug.WriteLine($">>>>> CloseApp threw an exception, falling back to ForceCloseApp: {ex.Message}");
			}

			return ForceCloseApp(parameters);
		}

		/// <summary>
		/// Force-terminates the app using platform-specific OS commands.
		/// This bypasses Appium/WDA which may be stuck waiting for the app to become idle.
		/// Use when the app is unresponsive (e.g., stuck in an infinite layout loop).
		/// </summary>
		CommandResponse ForceCloseApp(IDictionary<string, object> parameters)
		{
			try
			{
				var appId = _app.GetAppId();
				var testDevice = _app.GetTestDevice();

				if (testDevice == TestDevice.iOS)
				{
					var udid = _app.Config.GetProperty<string>("Udid");
					if (!string.IsNullOrEmpty(udid))
					{
						Debug.WriteLine($">>>>> ForceCloseApp: xcrun simctl terminate {udid} {appId}");
						using var process = Process.Start(new ProcessStartInfo
						{
							FileName = "xcrun",
							ArgumentList = { "simctl", "terminate", udid, appId },
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							UseShellExecute = false,
						});
						if (process is not null && !process.WaitForExit(10000))
						{
							Debug.WriteLine(">>>>> ForceCloseApp: xcrun simctl terminate timed out, killing process");
							try { process.Kill(); } catch { }
						}
						return CommandResponse.SuccessEmptyResponse;
					}
					else
					{
						Debug.WriteLine(">>>>> ForceCloseApp: iOS UDID not available, cannot force-terminate via simctl");
					}
				}
				else if (testDevice == TestDevice.Android)
				{
					Debug.WriteLine($">>>>> ForceCloseApp: adb shell am force-stop {appId}");
					using var process = Process.Start(new ProcessStartInfo
					{
						FileName = "adb",
						ArgumentList = { "shell", "am", "force-stop", appId },
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
					});
					if (process is not null && !process.WaitForExit(10000))
					{
						Debug.WriteLine(">>>>> ForceCloseApp: adb force-stop timed out, killing process");
						try { process.Kill(); } catch { }
					}
					return CommandResponse.SuccessEmptyResponse;
				}
				else if (testDevice == TestDevice.Mac)
				{
					// Use pkill -9 instead of osascript "tell app to quit" because a hung app
					// won't process the cooperative Apple Event from osascript.
					Debug.WriteLine($">>>>> ForceCloseApp: macOS force-kill for {appId}");
					using var process = Process.Start(new ProcessStartInfo
					{
						FileName = "pkill",
						ArgumentList = { "-9", "-f", appId },
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
					});
					if (process is not null && !process.WaitForExit(10000))
					{
						Debug.WriteLine(">>>>> ForceCloseApp: pkill timed out, killing process");
						try { process.Kill(); } catch { }
					}
					return CommandResponse.SuccessEmptyResponse;
				}
				else if (testDevice == TestDevice.Windows)
				{
					Debug.WriteLine($">>>>> ForceCloseApp: Windows taskkill for {appId}");
					using var process = Process.Start(new ProcessStartInfo
					{
						FileName = "taskkill",
						ArgumentList = { "/F", "/IM", $"{appId}.exe" },
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
					});
					if (process is not null && !process.WaitForExit(10000))
					{
						Debug.WriteLine(">>>>> ForceCloseApp: taskkill timed out, killing process");
						try { process.Kill(); } catch { }
					}
					return CommandResponse.SuccessEmptyResponse;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($">>>>> ForceCloseApp failed: {ex.Message}");
			}

			return CommandResponse.FailedEmptyResponse;
		}

		/// <summary>
		/// Attempts to recover from a HostApp crash so a single crash doesn't cascade into
		/// every following fixture failing OneTimeSetUp. On Android a crash can leave the
		/// system showing an "App keeps stopping" / "isn't responding" dialog that blocks the
		/// relaunched activity (so every subsequent "Go To Test button" wait times out).
		/// This dismisses that dialog, force-stops the app, optionally clears its data
		/// ("hard" — breaks a corrupt-state startup crash loop), then relaunches it.
		/// </summary>
		CommandResponse RecoverFromCrash(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			var hard = parameters.TryGetValue("hard", out var h) && h is bool hb && hb;

			// Only Android exhibits the system "keeps stopping" dialog that blocks relaunch.
			// For other platforms a force-close + relaunch is the best available recovery.
			if (_app.GetTestDevice() != TestDevice.Android)
			{
				ForceCloseApp(parameters);
				try { LaunchApp(parameters); } catch (Exception ex) { Debug.WriteLine($">>>>> RecoverFromCrash relaunch failed: {ex.Message}"); }
				return CommandResponse.SuccessEmptyResponse;
			}

			var appId = _app.GetAppId();
			try
			{
				// 1. Dismiss any Android system crash/ANR dialog so it can't block the
				//    relaunched activity (BACK closes the dialog, HOME clears the surface).
				RunAdb("shell", "input", "keyevent", "KEYCODE_BACK");
				RunAdb("shell", "input", "keyevent", "KEYCODE_HOME");

				// 2. Force-stop the crashed/looping app to clear its pending-crash state.
				RunAdb("shell", "am", "force-stop", appId);

				// 3. On a hard recovery, clear app data to break a corrupt-state startup
				//    crash loop — the app then starts completely fresh on the next launch.
				if (hard)
					RunAdb("shell", "pm", "clear", appId);

				// 4. Relaunch the app's main activity. Prefer Appium; fall back to adb in
				//    case the driver session is wedged from the crash.
				try
				{
					_app.Driver.ActivateApp(appId);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($">>>>> RecoverFromCrash ActivateApp failed, falling back to monkey: {ex.Message}");
					RunAdb("shell", "monkey", "-p", appId, "-c", "android.intent.category.LAUNCHER", "1");
				}

				return CommandResponse.SuccessEmptyResponse;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($">>>>> RecoverFromCrash failed: {ex.Message}");
			}

			return CommandResponse.FailedEmptyResponse;
		}

		/// <summary>
		/// Runs an <c>adb</c> command with a hard timeout so a wedged emulator can't hang the
		/// test run. Best-effort: failures are logged and swallowed.
		/// </summary>
		static void RunAdb(params string[] args)
		{
			try
			{
				var psi = new ProcessStartInfo
				{
					FileName = "adb",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
				};
				foreach (var a in args)
					psi.ArgumentList.Add(a);

				using var process = Process.Start(psi);
				if (process is not null)
				{
					// Drain stdout/stderr so a chatty command (e.g. `adb shell monkey`) can't fill
					// the pipe buffer and deadlock WaitForExit. The output is unused, so discard it.
					process.OutputDataReceived += static (_, _) => { };
					process.ErrorDataReceived += static (_, _) => { };
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					if (!process.WaitForExit(15000))
					{
						Debug.WriteLine($">>>>> RunAdb timed out: adb {string.Join(' ', args)}");
						try { process.Kill(); } catch { /* best effort */ }
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($">>>>> RunAdb failed (adb {string.Join(' ', args)}): {ex.Message}");
			}
		}

		CommandResponse Back(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			try
			{
				// Navigate backwards in the history, if possible.
				_app.Driver.Navigate().Back();

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse Refresh(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			// Refresh the current page.
			_app.Driver.Navigate().Refresh();

			return CommandResponse.SuccessEmptyResponse;
		}
	}
}