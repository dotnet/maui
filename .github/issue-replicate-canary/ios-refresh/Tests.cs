#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39080 : _IssuesUITest
{
	public Issue39080(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Disabled RefreshView allows pull gesture on iOS";

	[Test]
	[Category(UITestCategories.RefreshView)]
	public void DisabledRefreshViewDoesNotAllowPullGesture()
	{
		var result = App.WaitForElement("Result");
		Assert.That(result.GetText(), Is.EqualTo("No pull movement"));
		var target = App.WaitForElement("GestureTarget").GetRect();
		var centerX = (float)(target.X + target.Width / 2);
		var startY = (float)(target.Y + target.Height * 0.35f);
		var endY = (float)(target.Y + target.Height * 0.75f);
		App.DragCoordinates(centerX, startY, centerX, endY);
		Assert.That(App.WaitForElement("Result").GetText(), Is.EqualTo("No pull movement"),
			"A disabled RefreshView allowed its ScrollView content to be pulled down.");
	}
}
#endif
