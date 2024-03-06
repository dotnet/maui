using System.Drawing;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue20920 : _IssuesUITest
{
	public Issue20920(TestDevice device) : base(device) { }

	public override string Issue => "Nested ScrollView does not work in Android";

	[Test]
	public void ScrollingBothDirectionsWithNestedScrollViews()
	{
		// TODO: Correct this test for other platforms
		this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows, TestDevice.iOS });
		var initialPosition = App.WaitForElement("dotnet_bot").GetRect();

		App.ScrollDown("dotnet_bot");
		App.ScrollRight("dotnet_bot");

		var afterScrollPosition = App.WaitForElement("dotnet_bot").GetRect();

		Assert.Less(afterScrollPosition.X, initialPosition.X);
		Assert.Less(afterScrollPosition.Y, initialPosition.Y);
	}
}
