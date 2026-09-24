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
		var newWindowButton = App.WaitForElement("NewWindowButton").GetRect();
		try
		{
			// Use mouse coordinates so the pointer starts outside the border and actually enters it.
			App.Click(newWindowButton.CenterX(), newWindowButton.CenterY());
			App.WaitForElement("MinimizeSecondWindowButton");
			App.Tap("MinimizeSecondWindowButton");
			var borderButton = App.WaitForElement("BorderButton").GetRect();
			App.Click(borderButton.CenterX(), borderButton.CenterY());
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
}
#endif