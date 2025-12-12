using UITest.Core;
using NUnit.Framework;
using UITest.Appium;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue28968 : _IssuesUITest
{
	public Issue28968(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[iOS] [ActivityIndicator] IsRunning ignores IsVisible when set to true";

    [Test]
	[Category(UITestCategories.ActivityIndicator)]
	public void ActivityIndicatorShouldNotIgnoreIsVisible()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");
        App.WaitForNoElement("MauiActivityIndicator");
	}
}
