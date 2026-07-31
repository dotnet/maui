using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue36422 : _IssuesUITest
	{
		public Issue36422(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "Changing ItemSpacing at runtime shifts CollectionView's ContentOffset, hiding the first item";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ChangingItemSpacingDoesNotShiftFirstItemOutOfView()
		{
			App.WaitForElement("IncreaseSpacingButton");
			App.Tap("IncreaseSpacingButton");
			VerifyScreenshot();
		}
	}
}
