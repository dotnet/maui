#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28201 : _IssuesUITest
{
	public Issue28201(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "TiteView disposed when Page is disposed";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleViewDisposed()
	{
		for (int i = 0; i < 3; i++)
		{
			App.WaitForElement("pushButton");
			App.Tap("pushButton");
			App.WaitForElement("popButton");
			App.Tap("popButton");
		}
		var label = App.WaitForElement("label");
		App.Tap("checkStatusButton");
		Assert.That(label.GetText(), Is.EqualTo("Page Destroyed"));
	}
}
#endif