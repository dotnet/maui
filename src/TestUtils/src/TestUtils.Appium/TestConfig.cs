using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Enums;

namespace Microsoft.Maui.Appium
{
	public class TestConfig
	{
		public TestConfig(TestDevice testDevice, string appID)
		{
			TestDevice = testDevice;
			switch (TestDevice)
			{
				case TestDevice.Android:
					PlatformName = "Android";
					AutomationName = "UiAutomator2";
					break;
				case TestDevice.iOS:
					PlatformName = "iOS";
					AutomationName = "XCUITest";
					break;
				case TestDevice.Mac:
					PlatformName = "mac";
					AutomationName = "mac2";
					break;
				case TestDevice.Windows:
					PlatformName = "Windows";
					DeviceName = "WindowsPC";
					AutomationName = "Windows";
					break;
			}
			AppId = appID;
			FrameworkVersion = "net7.0";
#if DEBUG
			Configuration = "Debug";
#else
			Configuration = "Release";
#endif
		}

		public string PlatformName { get; private set; } = string.Empty;
		public string AutomationName { get; private set; } = string.Empty;

		public string? DeviceName { get; set; }
		public string? PlatformVersion { get; set; }
		public string AppId { get; set; }
		public string? BundleId { get; set; }
		public string? AppPath { get; set; }
		public string? Udid { get; set; }
		public string ReportDirectory { get; set; } = "reports";
		public string ReportFormat { get; set; } = "xml";

		public string? TestName { get; set; }

		public bool FullReset { get; set; }

		public TestDevice TestDevice { get; private set; }

		public string? AppProjectPath { get; set; }

		public string Configuration { get; set; }

		public string FrameworkVersion { get; set; }
	}
}
