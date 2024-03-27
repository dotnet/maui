using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue20273 : _IssuesUITest
{
	public override string Issue => "Previously selected item cannot be reselected";

	public Issue20273(TestDevice device)
		: base(device)
	{ }

    [Test]
	public void PreviouslySelectedItemShouldBeReselected()
	{
		var firstItem = App.WaitForElement("FirstItem");
		firstItem.Click();
		App.WaitForElement("FirstItem");
		firstItem.Click();
		var numberOfNavigations = App.WaitForElement("numberOfNavigations").GetText();

		Assert.AreEqual(int.Parse(numberOfNavigations!), 2);
	}
}
