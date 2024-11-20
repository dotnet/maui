using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla38978 : _IssuesUITest
{
	public Bugzilla38978(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Cell.ForceUpdateSize issues with row selection/deselection (ViewCell)";

	[Test]
	[FailsOnIOSWhenRunningOnXamarinUITest]
	[Category(UITestCategories.ManualReview)]
	public void Bugzilla38978Test()
	{
		App.WaitForElement("2");
		var beforeRect = App.FindElement("2").GetRect();

		App.Tap("2");
		var afterRect = App.FindElement("2").GetRect();

		Assert.That(afterRect.Width, Is.GreaterThan(beforeRect.Width));
		Assert.That(afterRect.Height, Is.GreaterThan(beforeRect.Height));
	}
}
