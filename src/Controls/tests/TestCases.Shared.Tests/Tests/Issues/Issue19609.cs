using System.Drawing;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium.Interactions;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19609 : _IssuesUITest
{
	public Issue19609(TestDevice device) : base(device) { }

	public override string Issue => "Button clicked event and command will not be occurred in EmptyView of CollectionView";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CanTapButtonOnEmptyView()
	{
		var btnElementId = "btnClick";
		var btnElementCVId = "cv";
		App.WaitForElement(btnElementCVId, $"Can t find the CollectionView {btnElementCVId}", timeout: TimeSpan.FromSeconds(10));
#if WINDOWS
		// on Windows, the button is not on Appium hirearchy, so we need to tap on the center of the CollectionView where the empty view is
		var cv = App.FindElement(btnElementCVId);
		var cvLocation = cv.GetRect();
		var cvCenter = new Point(cvLocation.X + cvLocation.Width / 2, cvLocation.Y + cvLocation.Height / 2);
		App.TapCoordinates(cvCenter.X, cvCenter.Y);
#else
		App.WaitForElement(btnElementId, $"Can t find the button {btnElementId}", timeout: TimeSpan.FromSeconds(10));
		App.Tap(btnElementId);
#endif
		App.WaitForTextToBePresentInElement(btnElementId, "Clicked", timeout: TimeSpan.FromSeconds(10));
	}
}