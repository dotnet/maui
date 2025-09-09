using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

	public class Issue20615 : _IssuesUITest
	{
		public Issue20615(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView selecteditem background lost if collectionview (or parent) IsEnabled changed.";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewSelectedItemBackgroundLost()
		{
			App.WaitForElement("CollectionView");
			App.Tap("ParentGridDisableEnableButton");
		    VerifyScreenshot();
		}
	}