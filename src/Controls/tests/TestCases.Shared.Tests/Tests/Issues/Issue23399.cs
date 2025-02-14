#if ANDROID || IOS //The test fails on Windows and MacCatalyst because the BackgroundApp and ForegroundApp method, which is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23399(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "Closing Modal While App is Backgrounded Fails";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void MakingFragmentRelatedChangesWhileAppIsBackgroundedFails()
		{
			App.WaitForElement("OpenModal");
			App.Tap("OpenModal");
			App.WaitForElement("StartCloseModal");
			App.Tap("StartCloseModal");
			App.BackgroundApp();
			App.WaitForNoElement("StartCloseModal");
			App.ForegroundApp();
			App.WaitForElement("OpenModal");
		}
	}
}
#endif
