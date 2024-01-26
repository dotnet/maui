using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue15815 : _IssuesUITest
{
	public Issue15815(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Horizontal CollectionView does not show the last element under some condition";

	[Test]
	public void LastItemIsVisilbe()
	{
		var lastItem = App.WaitForElement("id-2");
		Assert.AreEqual("Item 2", lastItem.GetText());
	}
}
