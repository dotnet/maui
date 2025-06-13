using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla29363 : _IssuesUITest
	{
		public Bugzilla29363(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PushModal followed immediate by PopModal crashes";

		[Fact]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public void PushButton()
		{
			App.WaitForElement("ModalPushPopTest");
			App.Tap("ModalPushPopTest");
			App.WaitForElementTillPageNavigationSettled("ModalPushPopTest");
		}
	}
}