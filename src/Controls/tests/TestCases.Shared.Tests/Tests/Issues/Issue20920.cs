#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20920 : _IssuesUITest
{
	public Issue20920(TestDevice device) : base(device) { }

	public override string Issue => "Nested ScrollView does not work in Android";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollingBothDirectionsWithNestedScrollViews()
	{
		// TODO: Correct this test for other platforms
		var initialPosition = App.WaitForElement("dotnet_bot").GetRect();

		App.ScrollDown("dotnet_bot");
		App.ScrollRight("dotnet_bot");

		var afterScrollPosition = App.WaitForElement("dotnet_bot").GetRect();

		ClassicAssert.Less(afterScrollPosition.X, initialPosition.X);
		ClassicAssert.Less(afterScrollPosition.Y, initialPosition.Y);
	}
}
#endif