#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29542 : _IssuesUITest
{
	public Issue29542(TestDevice device) : base(device) { }

	public override string Issue => "I1_Vertical_list_for_Multiple_Rows - Rotating the emulator would cause clipping on the description text.";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelShouldSizeProperlyOnCollectionView()
	{
		App.WaitForElement("TestCollectionView");
		App.Tap("ScrollToDownButton");
		App.SetOrientationLandscape();
		App.WaitForElement("TestCollectionView");
		App.Tap("ScrollToDownButton");
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("Label_12");
		var label12Width = App.WaitForElement("Label_12").GetRect().Width;
		App.WaitForElement("Label_13");
		var label13Width = App.WaitForElement("Label_13").GetRect().Width;
		App.WaitForElement("Label_14");
		var label14Width = App.WaitForElement("Label_14").GetRect().Width;
		App.WaitForElement("Label_15");
		var label15Width = App.WaitForElement("Label_15").GetRect().Width;

		Assert.That(label15Width, Is.EqualTo(label14Width));
		Assert.That(label14Width, Is.EqualTo(label13Width));
		Assert.That(label13Width, Is.EqualTo(label12Width));
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif