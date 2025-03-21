#if TEST_FAILS_ON_ANDROID // Destructor not triggered on Android: https://github.com/dotnet/maui/issues/28549
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28201 : _IssuesUITest
{
	public Issue28201(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "TitleView disposed when Page is disposed";

	[Test]
	[Category(UITestCategories.TitleView)]
	public void TitleViewDisposed()
	{
		string pushButton = "pushButton";
		string popButton = "popButton";
		for (int i = 0; i < 3; i++)
		{
			App.WaitForElement(pushButton);
			App.Tap(pushButton);
			App.WaitForElement(popButton);
			App.Tap(popButton);
		}
		var label = App.WaitForElement("label");
		App.Tap("checkStatusButton");
		Assert.That(label.GetText(), Is.EqualTo("Page Destroyed"));
	}
}
#endif