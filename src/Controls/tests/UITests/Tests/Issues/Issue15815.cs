using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue15815 : _IssuesUITest
{
	public Issue15815(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "MeasureAllItems not working in Horizontal CollectionView";

	[Test]
	public void LastItemIsVisilbe()
	{
		var lastItem = App.WaitForElement("Item 2");
		Assert.IsNotNull(lastItem);
	}
}
