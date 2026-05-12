#if WINDOWS // Existing PR for iOS & Android: https://github.com/dotnet/maui/pull/35217
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35216 : _IssuesUITest
{
	public override string Issue => "SwipeItem IsVisible should properly refresh native swipe items when binding value changes dynamically";

	public Issue35216(TestDevice device) : base(device)
	{
	}

	[Test]
	[Order(1)]
	[Category(UITestCategories.SwipeView)]
	public void Issue35216SwipeItemInitiallyHiddenBecomesVisibleAfterBindingChanges()
	{
		Exception? exception = null;
		var rect = App.WaitForElement("SwipeContent").GetRect();
		var centerX = rect.X + rect.Width / 2;
		var centerY = rect.Y + rect.Height / 2;

		App.DragCoordinates(centerX, centerY, centerX + 200, centerY);

		VerifyScreenshotOrSetException(ref exception, "Issue35216SwipeOpen_InitiallyHidden");

		App.Tap("ToggleVisibilityButton");

		App.DragCoordinates(centerX, centerY, centerX + 200, centerY);
		VerifyScreenshotOrSetException(ref exception, "Issue35216SwipeOpen_BecomeVisible");

		App.Tap("ResetButton");

		if (exception is not null)
		{
			throw exception;
		}
	}

	[Test]
	[Order(2)]
	[Category(UITestCategories.SwipeView)]
	public void Issue35216SwipeItemBecomesHiddenAfterBindingChanges()
	{
		Exception? exception = null;
		var rect = App.WaitForElement("SwipeContent").GetRect();
		var centerX = rect.X + rect.Width / 2;
		var centerY = rect.Y + rect.Height / 2;

		App.Tap("ToggleVisibilityButton");
		App.DragCoordinates(centerX, centerY, centerX + 200, centerY);

		VerifyScreenshotOrSetException(ref exception, "Issue35216SwipeOpen_DeleteVisible");

		App.Tap("ToggleVisibilityButton");

		App.DragCoordinates(centerX, centerY, centerX + 200, centerY);
		VerifyScreenshotOrSetException(ref exception, "Issue35216SwipeOpen_DeleteHidden");

		App.Tap("ResetButton");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
#endif
