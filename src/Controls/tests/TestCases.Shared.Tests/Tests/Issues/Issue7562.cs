using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue7562 : _IssuesUITest
	{
		public override string Issue => "MeasureFirstItem makes items disappear on Android";
		public Issue7562(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void MeasureFirstItemMakesItemsDisappearOnAndroid()
		{
			App.WaitForElement("Issue2");
		}
	}
}
