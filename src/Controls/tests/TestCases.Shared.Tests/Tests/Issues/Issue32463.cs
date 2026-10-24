#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

// Use UITest rather than _IssuesUITest so setup cannot navigate to the test page.
public class Issue32463 : UITest
{
	public Issue32463(TestDevice device) : base(device)
	{
	}

	[TestCase("Android direct test launch", "DirectLaunchLabel")]
	[TestCase("android DIRECT test launch", "DirectLaunchLabel")]
	[TestCase("Button Feature Matrix", "ButtonControl")]
	[Category(UITestCategories.LifeCycle)]
	public void LaunchDirectlyToTest(string test, string expectedElement)
	{
		LaunchWithTest(test);
		App.WaitForElement(expectedElement);
		Assert.That(App.FindElements("GoToTestButton"), Is.Empty);
	}

	[TestCase(null)]
	[TestCase("")]
	[TestCase("Unknown startup test")]
	[Category(UITestCategories.LifeCycle)]
	public void LaunchWithoutMatchingTestShowsGallery(string? test)
	{
		LaunchWithTest(test);
		App.WaitForElement("GoToTestButton");
	}

	void LaunchWithTest(string? test)
	{
		var arguments = new Dictionary<string, object>
		{
			{ "intent", "com.microsoft.maui.uitests/com.microsoft.maui.uitests.MainActivity" },
			{ "stop", true }
		};

		if (test is not null)
		{
			arguments["extras"] = new[] { new[] { "s", "test", test } };
		}

		((AppiumApp)App).Driver.ExecuteScript("mobile: startActivity", arguments);
	}
}
#endif
