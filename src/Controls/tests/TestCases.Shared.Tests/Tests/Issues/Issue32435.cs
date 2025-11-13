#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS    //Issue reproduce only when rotating device.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues

public class Issue32435 : _IssuesUITest
{
	public Issue32435(TestDevice device) : base(device) { }

	public override string Issue => "Rotating the Simulator causes the text on the collection view to disappear";
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyCollectionViewTextShouldAppearAfterRotatingTheDevice()
	{
		App.WaitForElement("InstructionLabel");
		App.Tap("AddButton");
		App.SetOrientationLandscape();
		App.SetOrientationPortrait();
		VerifyScreenshot();
	}
}
#endif