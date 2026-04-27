using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34832 : _IssuesUITest
{
	public override string Issue => "SwipeItem.IsVisible doesn't properly refresh native swipe items when binding value changes dynamically";

	public Issue34832(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemBecomesHiddenAfterBindingChanges()
	{
		Exception? exception = null;
		App.WaitForElement("OpenSwipeButton");
		App.Tap("OpenSwipeButton");

		VerifyScreenshotOrSetException(ref exception, "SwipeOpen_DeleteVisible");

		App.Tap("CloseSwipeButton");
		App.Tap("ToggleVisibilityButton");
		App.Tap("OpenSwipeButton");
		
		VerifyScreenshotOrSetException(ref exception, "SwipeOpen_DeleteHidden");
		if (exception is not null)
		{
			throw exception;
		}
	}
}
