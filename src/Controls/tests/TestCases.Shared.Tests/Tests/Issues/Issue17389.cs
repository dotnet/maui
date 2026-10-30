using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17389 : _IssuesUITest
{
	protected override bool ResetAfterEachTest => true;

	public Issue17389(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "InputTransparent should not affect background color on Windows layouts";

	[Test, Order(1)]
	[Category(UITestCategories.Layout)]
	public void ValidateBackgroundColorDoesNotAffectInputTransparent()
	{
		App.WaitForElement("ToggleInputTransparentButton");

		string[] layouts = new[]
		{
			 "RedGrid",
			 "GreenGrid",
			 "BlueBorder",
			 "PurpleContent"
		};

		for (var index = 0; index < layouts.Length; index++)
		{
			TapLayout(layouts[index]);
			App.WaitForElement($"Tap count: {index + 1}");
		}

		App.WaitForElement("Tap count: 4");
		App.Click("ToggleInputTransparentButton");
		Assert.That(App.WaitForTextToBePresentInElement("RedGrid", "InputTransparent=True"), Is.True);

		foreach (var layout in layouts)
		{
			TapLayout(layout);
		}

		App.WaitForElement("Tap count: 0");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Layout)]
	public void ValidateInputTransparentBackgroundColorToggle()
	{
		Exception? exception = null;

		App.WaitForElement("ToggleInputTransparentButton");
		App.Tap("ToggleInputTransparentButton");
		Assert.That(App.WaitForTextToBePresentInElement("RedGrid", "InputTransparent=True"), Is.True);
		Assert.That(App.WaitForTextToBePresentInElement("TapCountLabel", "Tap count: 0"), Is.True);

		App.WaitForElement("ToggleBackgroundColorsButton");
		VerifyScreenshotOrSetException(ref exception, "BeforeToggleBackgroundColors");
		App.WaitForElement("ToggleBackgroundColorsButton");
		App.Tap("ToggleBackgroundColorsButton");
		VerifyScreenshotOrSetException(ref exception, "AfterToggleBackgroundColors");

		if (exception != null)
		{
			throw exception;
		}
	}

	void TapLayout(string automationId)
	{
		if (App is ICatalystApp)
		{
			// Exercise parent gesture routing with real pointer input, not a static-text accessibility action.
			var rect = App.WaitForElement(automationId).GetRect();
			App.TapCoordinates(rect.CenterX(), rect.CenterY());
		}
		else
		{
			App.Tap(automationId);
		}
	}
}