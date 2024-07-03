#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23399(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "Closing Modal While App is Backgrounded Fails"; 


		[Test]
		public void MakingFragmentRelatedChangesWhileAppIsBackgroundedFails()
		{
			try
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
			catch
			{
				SaveUIDiagnosticInfo();

				// Just in case these tests leave the app in an unreliable state
				App.ResetApp();
				FixtureSetup();
				throw;
			}
		}
	}
}
#endif