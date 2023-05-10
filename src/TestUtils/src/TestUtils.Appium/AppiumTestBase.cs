using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.IntegrationTests;
using Microsoft.Maui.IntegrationTests.Android;
using Microsoft.Maui.IntegrationTests.Apple;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Service;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.DevTools.V104.Page;
using OpenQA.Selenium.DevTools.V105.Page;

namespace Microsoft.Maui.Appium
{
	public abstract class AppiumTestBase : BaseBuildTest
	{
		const int Port = 4723;
		protected AppiumLocalService Server;
		protected AppiumDriver? Driver;
		protected AppiumOptions AppiumOptions;
		protected TestConfig? TestConfig;
		protected Simulator TestSimulator = new();
		protected Emulator TestAvd = new();

		public bool IsAndroid => Driver != null && Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Android");
		public bool IsWindows => Driver != null && Driver.Capabilities.GetCapability(MobileCapabilityType.PlatformName).Equals("Windows");

		public AppiumTestBase()
		{
			AppiumOptions = new AppiumOptions();
			Server = GetServer();
			Server.Start();
		}

		public void StartEmulators()
		{
			TestConfig ??= GetTestConfig();

			if (TestConfig.TestDevice == TestDevice.Android)
			{
				AndroidEmulatorStart();
			}

			if (TestConfig.TestDevice == TestDevice.iOS)
			{
				AppleSimulatorStart();
			}
		}

		public void Teardown()
		{
			//this crashes on Android
			if (!IsAndroid && !IsWindows)
				Driver?.ResetApp();
		}

		public void TeardownOneTime()
		{
			Driver?.Quit();
			Server.Dispose();

			if (TestConfig == null)
				return;

			TerminateEmulators(TestConfig);
		}

		void TerminateEmulators(TestConfig testConfig)
		{
			if (testConfig.TestDevice == TestDevice.Android)
			{
				//remove app
				AndroidEmulatorClear();
				AndroidEmulatorStop();
			}
			if (testConfig.TestDevice == TestDevice.iOS)
			{
				AppleSimulatorStop();
			}
		}

		public void InitializeEmulators()
		{
			TestConfig ??= GetTestConfig();

			if (TestConfig.TestDevice == TestDevice.Android)
			{
				AndroidEmulatorInstall();
			}
		}

		public void AppleSimulatorStart()
		{
			if (!TestEnvironment.IsMacOS)
				Assert.Ignore("Running Apple templates is only supported on macOS.");

			TestSimulator.Shutdown();
			Assert.IsTrue(TestSimulator.Launch(), $"Failed to boot simulator with UDID '{TestSimulator.GetUDID()}'.");
			TestSimulator.ShowWindow();
		}

		public void AppleSimulatorStop()
		{
			TestSimulator.Shutdown();
		}

		public void AndroidEmulatorInstall()
		{
			if (TestEnvironment.IsMacOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
				TestAvd.Abi = "arm64-v8a";

			TestAvd.InstallAvd();
		}

		public void AndroidEmulatorStart()
		{
			var emulatorLog = Path.Combine(TestDirectory, $"emulator-launch-{DateTime.UtcNow.ToFileTimeUtc()}.log");
			Directory.CreateDirectory(TestDirectory);
			File.Create(emulatorLog);
			Assert.IsTrue(TestAvd.LaunchAndWaitForAvd(720, emulatorLog), "Failed to launch Test AVD.");
		}

		public void AndroidEmulatorStop()
		{
			Adb.KillEmulator(TestAvd.Id);

			// adb.exe can lock certain files on windows, kill it after tests complete
			if (TestEnvironment.IsWindows)
			{
				Adb.Run("kill-server", deviceId: TestAvd.Id);
				foreach (var p in Process.GetProcessesByName("adb.exe"))
					p.Kill();
			}
		}

		public void AndroidEmulatorClear()
		{
			if (!string.IsNullOrEmpty(TestConfig?.AppId))
				Adb.UninstallPackage(TestConfig.AppId);
		}

		public void BuildProject()
		{
			if (TestConfig == null)
				throw new InvalidOperationException($"You need to provide a {TestConfig}");

			if (string.IsNullOrEmpty(TestConfig.AppProjectPath))
				throw new InvalidOperationException($"You need to specify {nameof(TestConfig.AppProjectPath)}");

			string framework = TestConfig.FrameworkVersion;
			string config = TestConfig.Configuration;
			string target = "run";
			string project = TestConfig.AppProjectPath;

			List<string>? properties = null;
			string appFile = string.Empty;
			string? appFolder = string.Empty;
			if (TestConfig.TestDevice == TestDevice.Android)
			{
				framework = $"{framework}-android";
			}

			if (TestConfig.TestDevice == TestDevice.iOS)
			{
				framework = $"{framework}-ios";
				string simulatorId = TestSimulator.GetUDID();
				var deviceProperty = $"_DeviceName=:v2:udid={simulatorId}";
				properties = new List<string> { deviceProperty };
				appFolder = Path.GetDirectoryName(TestConfig.AppProjectPath.Replace("\\", "//", StringComparison.InvariantCultureIgnoreCase));
				appFile = Path.Combine(appFolder!, "bin", config, framework, "iossimulator-x64", $"{Path.GetFileName(TestConfig.AppProjectPath?.Replace(".csproj", "", StringComparison.InvariantCultureIgnoreCase))}.app");
				target = string.Empty;
			}

			if (TestConfig.TestDevice == TestDevice.Mac)
			{
				framework = $"{framework}-maccatalyst";
			}

			if (TestConfig.TestDevice == TestDevice.Windows)
			{
				//"net7.0-windows10.0.20348";
				framework = $"{framework}-windows10.0.20348";
				target = string.Empty;
			}

			//try build and run the application so it gets registered on the OS or on the Simulator.
			var buildProject = DotnetInternal.Build(project, config, target, framework, properties);
			Assert.IsTrue(buildProject, $"Project {Path.GetFileName(project)} failed to build. Check test output/attachments for errors.");
			if (TestConfig.TestDevice == TestDevice.iOS && !string.IsNullOrEmpty(appFile))
			{
				var resultDir = Path.Combine(appFolder!, "xh-results");
				XHarness.InstallRunAppleForTimeout(appFile, resultDir, TestSimulator.XHarnessID);
			}
		}


		public abstract TestConfig GetTestConfig();

		protected virtual AppiumLocalService GetServer(int port = Port)
		{
			var arguments = new OpenQA.Selenium.Appium.Service.Options.OptionCollector();
			arguments.AddArguments(new KeyValuePair<string, string>("--base-path", "/wd/hub"));

			var logFile = Environment.GetEnvironmentVariable("APPIUM_LOG_FILE") ?? "appium.log";

			var service = new AppiumServiceBuilder()
				.WithArguments(arguments)
				.UsingPort(port)
				.WithLogFile(new FileInfo(logFile))
				.Build();

			service.OutputDataReceived += (s, e) => Debug.WriteLine($"Appium {e.Data}");
			return service;
		}

		protected virtual AppiumDriver? GetDriver(Uri? driverUri = null)
		{
			if (TestConfig == null)
				throw new InvalidOperationException("Make sure to provide a TestConfig");

			SetGeneralAppiumOptions(AppiumOptions, TestConfig);

			if (driverUri == null)
				driverUri = new Uri($"http://localhost:{Port}/wd/hub");

			Task.Delay(1000).Wait();

			while (Server != null && !Server.IsRunning)
			{
				Task.Delay(1000).Wait();
			}
			return TestConfig.TestDevice switch
			{
				TestDevice.Android => new AndroidDriver(driverUri, AppiumOptions),
				TestDevice.iOS => new IOSDriver(driverUri, AppiumOptions),
				TestDevice.Mac => new MacDriver(driverUri, AppiumOptions),
				TestDevice.Windows => new WindowsDriver(driverUri, AppiumOptions),
				_ => null,
			};
		}

		protected virtual void SetPlatformAppiumOptions(AppiumOptions appiumOptions)
		{
			if (TestConfig == null)
				return;
			var appId = TestConfig.BundleId ?? TestConfig.AppId;
			appiumOptions.PlatformName = TestConfig.PlatformName;
			appiumOptions.AutomationName = TestConfig.AutomationName;

			if (!string.IsNullOrEmpty(TestConfig.DeviceName))
				appiumOptions.DeviceName = TestConfig.DeviceName;

			if (!string.IsNullOrEmpty(TestConfig.PlatformVersion))
				appiumOptions.PlatformVersion = TestConfig.PlatformVersion;

			if (!string.IsNullOrEmpty(TestConfig.AppPath))
				appiumOptions.App = TestConfig.AppPath;

			switch (TestConfig.TestDevice)
			{
				case TestDevice.Android:
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppPackage, "com.microsoft.maui.sandbox");
					// activity { com.microsoft.maui.sandbox / crc64fa090d87c1ce7f0b.MainActivity}
					//_appiumOptions.AddAdditionalAppiumOption(AndroidMobileCapabilityType.AppActivity, "MainActivity");
					break;
				case TestDevice.iOS:
					if (string.IsNullOrEmpty(TestConfig.Udid))
						TestConfig.Udid = TestSimulator.GetUDID();
					appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, TestConfig.Udid);
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
					break;
				case TestDevice.Mac:
					appiumOptions.AddAdditionalAppiumOption(IOSMobileCapabilityType.BundleId, appId);
					appiumOptions.AddAdditionalAppiumOption("showServerLogs", true);
					break;
				case TestDevice.Windows:
					break;
			}
		}

		public string GetElementId(string elementId)
		{
			if (IsAndroid)
				return $"{TestConfig?.AppId}:id/{elementId}";

			return elementId;
		}

		public By ByAutomationId(string elementId)
		{
			var id = GetElementId(elementId);
			if (IsWindows)
				return MobileBy.AccessibilityId(id);

			return MobileBy.Id(id);
		}

		static void SetGeneralAppiumOptions(AppiumOptions appiumOptions, TestConfig testConfig)
		{
			appiumOptions.AddAdditionalAppiumOption("reportDirectory", testConfig.ReportDirectory);
			appiumOptions.AddAdditionalAppiumOption("reportFormat", testConfig.ReportFormat);
			if (string.IsNullOrEmpty(testConfig.TestName))
				appiumOptions.AddAdditionalAppiumOption("testName", testConfig.TestName);
			if (testConfig.FullReset)
				appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.FullReset, "true");

			appiumOptions.AddAdditionalAppiumOption(MobileCapabilityType.NewCommandTimeout, 3000);
		}

	}
}