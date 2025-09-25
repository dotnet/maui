using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27117 : _IssuesUITest
	{
		public Issue27117(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView ScrollTo not working under android";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue27117Test()
		{
			App.WaitForElement("collectionView");
			VerifyScreenshot();
		}
	}
}
