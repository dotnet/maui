#if TEST_FAILS_ON_ANDROID // More information: https://github.com/dotnet/maui/issues/28640
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10222 : _IssuesUITest
{
	public Issue10222(TestDevice device) : base(device)
	{
	}
	public override string Issue => "[CollectionView] ObjectDisposedException if the page is closed during scrolling";

	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void Issue10222Test()
	{
		App.WaitForElement("goTo");
		App.Tap("goTo");
		App.WaitForElement("items1", timeout: TimeSpan.FromSeconds(1));
		App.WaitForElement("goTo", timeout: TimeSpan.FromSeconds(2));
	}
}
#endif