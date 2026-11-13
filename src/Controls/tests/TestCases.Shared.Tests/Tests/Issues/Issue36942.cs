#if TEST_FAILS_ON_WINDOWS //Issue Link : https://github.com/dotnet/maui/issues/4715
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue36942 : _IssuesUITest
	{
		public Issue36942(TestDevice device) : base(device)
		{
		}
		public override string Issue => "Border with Shadow breaks descendant BackgroundColor UI updates on Android";

		[Test]
		[Category(UITestCategories.Shadow)]
		public void TappingDescendantInsideShadowedBorderShouldUpdateBackgroundColor()
		{
			App.WaitForElement("Issue36942Page");
			App.Tap("ToggleTarget");
			App.WaitForTextToBePresentInElement("ViewModelState", "Activated: True");
			VerifyScreenshot();
		}
	}
}
#endif