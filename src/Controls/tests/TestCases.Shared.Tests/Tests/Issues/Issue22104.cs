using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22104 : _IssuesUITest
	{
		public Issue22104(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "VisualState Setters not working properly on Windows for a CollectionView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCollectionViewVisualState()
		{
			App.WaitForElement("CollectionView");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}