using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20273 : _IssuesUITest
{
	public override string Issue => "Previously selected item cannot be reselected";

	public Issue20273(TestDevice device)
		: base(device)
	{ }

    [Test]
	public async Task PreviouslySelectedItemShouldBeReselected()
	{
		var firstItem = App.WaitForElement("FirstItem");
		firstItem.Click();
		App.WaitForElement("FirstItem");
		firstItem.Click();
		await Task.Delay(250);
		var numberOfNavigations = App.WaitForElement("numberOfNavigations").GetText();

		ClassicAssert.AreEqual(int.Parse(numberOfNavigations!), 2);
	}
}
