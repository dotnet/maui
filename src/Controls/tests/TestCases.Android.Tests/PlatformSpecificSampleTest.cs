using NUnit.Framework;
using OpenQA.Selenium.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class PlatformSpecificSampleTest : UITest
{
	public PlatformSpecificSampleTest(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	public void SampleTest()
	{
		if (App is AppiumDriver driver)
		{
			driver?.GetScreenshot().SaveAsFile($"{nameof(SampleTest)}.png");
		}
	}
}