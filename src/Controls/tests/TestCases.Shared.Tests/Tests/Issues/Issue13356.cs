#if ANDROID // This Issue is only reproduced in Android platform
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue13356 : _IssuesUITest
{
	public Issue13356(TestDevice testDevice) : base(testDevice)
	{
	}

	protected override bool ResetAfterEachTest => true;
	public override string Issue => "Java.Lang.IllegalArgumentException: The style on this component requires your app theme to be Theme.MaterialComponent.";

	[Test]
	[Category(UITestCategories.Button)]
	public void DialogButtonAppearsWhenMaterialButtonIsTapped()
	{
		App.WaitForElement("showButtonDialogButton");
		App.Tap("showButtonDialogButton");
		App.WaitForElement("Dialog Button");
	}

	[Test]
	[Category(UITestCategories.CheckBox)]
	public void DialogButtonAppearsWhenMaterialCheckBoxIsTapped()
	{
		App.WaitForElement("showCheckBoxDialogButton");
		App.Tap("showCheckBoxDialogButton");
		App.WaitForElement("DialogCheckBox");
	}
}
#endif