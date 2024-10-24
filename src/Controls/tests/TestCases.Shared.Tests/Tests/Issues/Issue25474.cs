using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25474 : _IssuesUITest
	{
		public Issue25474(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView can't scroll to the last item when TabBar is visible";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void LastItemShouldBeVisible()
		{
			App.WaitForElement("Item1");
			App.Click("Item1");
			App.WaitForElement("Item10");
		}
	}
}