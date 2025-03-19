using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue19313 : _IssuesUITest
{
	public Issue19313(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Checkbox broken in Android";

    [Test]
	[Category(UITestCategories.CheckBox)]
	public void CheckBoxWithTapGestureShouldBeChecked()
	{
		App.WaitForElement("MauiCheckBox");
		App.Tap("MauiCheckBox");

		var text1 = App.WaitForElement("MauiLabel").GetText();

        Assert.That(text1,Is.EqualTo("Tapped"));
        VerifyScreenshot();
	}
}
