#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/35216
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
	[Order(1)]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemInitiallyHiddenBecomesVisibleAfterBindingChanges()
	{
		Exception? exception = null;
		App.WaitForElement("OpenSwipeButton");
		App.Tap("OpenSwipeButton");

		VerifyScreenshotOrSetException(ref exception, "SwipeOpen_InitiallyHidden");

		App.Tap("ToggleVisibilityButton");

		VerifyScreenshotOrSetException(ref exception, "SwipeOpen_BecomeVisible");

		App.Tap("TestSwipeView");
		App.Tap("ResetButton");

		if (exception is not null)
		{
			throw exception;
		}
	}

	[Test]
	[Order(2)]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemBecomesHiddenAfterBindingChanges()
	{
		Exception? exception = null;
		App.WaitForElement("ToggleVisibilityButton");
		App.Tap("ToggleVisibilityButton");
		App.Tap("OpenSwipeButton");

		VerifyScreenshotOrSetException(ref exception, "SwipeOpen_DeleteVisible");

		App.Tap("ToggleVisibilityButton");

		VerifyScreenshotOrSetException(ref exception, "SwipeOpen_DeleteHidden");

		App.Tap("TestSwipeView");
		App.Tap("ResetButton");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
#endif
