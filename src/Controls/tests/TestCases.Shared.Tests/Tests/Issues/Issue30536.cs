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
		App.Tap("NewWindowButton");
		App.WaitForElement("MinimizeSecondWindowButton");
		App.Tap("MinimizeSecondWindowButton");
		App.WaitForElement("BorderButton");
		App.Tap("BorderButton");
		var pointerEnterCount = App.FindElement("PointerEnterCountLabel");
		Assert.That(pointerEnterCount.GetText(), Is.EqualTo("Pointer Enter Count: 1"));
 
		var pointerExitCount = App.FindElement("PointerExitCountLabel");
		Assert.That(pointerExitCount.GetText(), Is.EqualTo("Pointer Exit Count: 0"));
		//once verified that pointer enter and exit counts are correct, close the newly created window
		App.WaitForElement("CloseNewWindowButton");
		App.Tap("CloseNewWindowButton");
	}
}
#endif