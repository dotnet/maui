#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30536 : _IssuesUITest
{
	public Issue30536(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] PointerGestureRecognizer behaves incorrectly when multiple windows are open";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void PointerGesturesShouldWorkProperlyOnMultiWindows()
	{
		App.WaitForElement("NewWindowButton");
		try
		{
			// Use mouse coordinates so the pointer starts outside the border and actually enters it.
			ClickWithMouse("NewWindowButton");
			Assert.That(() => App.FindElement("SecondWindowStateLabel").GetText(),
				Is.EqualTo("Created").After(5000, 100));
			App.Tap("MinimizeSecondWindowButton");
			Assert.That(() => App.FindElement("SecondWindowStateLabel").GetText(),
				Is.EqualTo("Minimized").After(5000, 100));
			ClickWithMouse("BorderButton");
			Assert.That(() => App.FindElement("PointerEnterCountLabel").GetText(),
				Is.EqualTo("Pointer Enter Count: 1").After(5000, 100));
			Assert.That(App.FindElement("PointerExitCountLabel").GetText(),
				Is.EqualTo("Pointer Exit Count: 0"));
		}
		finally
		{
			App.WaitForElement("CloseNewWindowButton");
			App.Tap("CloseNewWindowButton");
		}
	}

	void ClickWithMouse(string automationId)
	{
		var bounds = App.WaitForElement(automationId).GetRect();
		var driver = ((AppiumApp)App).Driver;
		var windowPosition = driver.Manage().Window.Position;
		// WinAppDriver bounds are window-relative; native mouse input uses screen coordinates.
		driver.ExecuteScript("windows: click", new Dictionary<string, object>
		{
			["x"] = windowPosition.X + bounds.CenterX(),
			["y"] = windowPosition.Y + bounds.CenterY()
		});
	}
}
#endif