using NUnit.Framework;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;

namespace UITests;

public abstract class UITestBase
{
	protected AppiumDriver App => AppiumSetup.App;

	[SetUp]
	public void RecordTestSetup()
	{
		var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
		TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Start");
	}

	[TearDown]
	public void RecordTestTeardown()
	{
		var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
		TestContext.Progress.WriteLine($">>>>> {DateTime.Now} {name} Stop");
	}

	// This could also be an extension method to AppiumDriver if you prefer
	protected AppiumElement FindUIElement(string id)
	{
		if (App is WindowsDriver)
		{
			return App.FindElement(MobileBy.AccessibilityId(id));
		}

		return App.FindElement(MobileBy.Id(id));
	}

	protected void NavigateToGallery(string galleryName)
	{
		App.FindElement(MobileBy.AccessibilityId(galleryName)).Click();
	}

	protected void NavigateToIssuesList()
	{
		App.FindElement(MobileBy.Id("com.microsoft.mauicompatibilitygallery:id/GoToTestButton")).Click();
	}
}