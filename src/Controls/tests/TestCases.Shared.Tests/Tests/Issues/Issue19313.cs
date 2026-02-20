#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //https://github.com/dotnet/maui/issues/19313
using System;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

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
		var text2 = App.WaitForElement("CheckedStatusLabel").GetText();

        Assert.That(text1,Is.EqualTo("Tapped"));
		Assert.That(text2, Is.EqualTo("Checked"));
	}
}
#endif