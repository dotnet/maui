using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class Issue32225 : _IssuesUITest
	{
		public override string Issue => "CollectionView FlowDirection RightToLeft not working in iOS horizontal layouts";

		public Issue32225(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HorizontalCollectionViewFlowDirectionShouldWork()
		{

            App.WaitForElement("Issue32225HorizontalCollectionView");
            App.WaitForElement("Issue32225SetRtlButton");
            App.Click("Issue32225SetRtlButton");
            App.WaitForElement("First");
            App.WaitForElement("Issue32225SetLtrButton");
            App.Click("Issue32225SetLtrButton");
            App.Click("Issue32225SetRtlButton");
            VerifyScreenshot();
		}
	}
}