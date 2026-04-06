#if IOS
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20098: _IssuesUITest
{
	public Issue20098(TestDevice device) : base(device)
	{
	}

	public override string Issue => "iOS-specific Slider.UpdateOnTab";

	[Test]
	[Category(UITestCategories.Picker)]
	public void UpdateOnTabShouldWork()
	{
		_ = App.WaitForElement("slider");
		var pickerRect = App.FindElement("slider").GetRect();
		var label = App.FindElement("label");
		App.Click(pickerRect.X + pickerRect.Width / 2, pickerRect.Y);
		var text = label.GetText();

		ClassicAssert.AreNotEqual("0", text);
	}
}
#endif