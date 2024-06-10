using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

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
		ClassicAssert.AreEqual("Item 2", lastItem.GetText());
	}
}
