using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue6738 : _IssuesUITest
	{
		const string PushAutomationId = "PushPageButton";
		const string InsertAutomationId = "InsertPageButton";
		const string ReturnAutomationId = "ReturnPageButton";
		const string FlyoutMainId = "Main";
		const string FlyoutOtherId = "OtherPage";

		public Issue6738(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Flyout Navigation fails when coupled with tabs that have a stack";

		// src/Compatibility/ControlGallery/src/Issues.Shared/Issue6738.cs
		[Test]
		public void FlyoutNavigationBetweenItemsWithNavigationStacks()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

			App.WaitForElement(PushAutomationId);
			App.Click(PushAutomationId);
			App.WaitForElement(InsertAutomationId);
			App.Click(InsertAutomationId);

			TapInFlyout(FlyoutOtherId);
			TapInFlyout(FlyoutMainId);

			App.WaitForElement(ReturnAutomationId);
			App.Click(ReturnAutomationId);
			App.Back();
			App.Back();
		}

		void TapInFlyout(string text)
		{
			Thread.Sleep(500);

			CheckIfOpen();

			try
			{
				App.Click(text);
			}
			catch
			{
				// Give it one more try
				CheckIfOpen();
				App.Click(text);
			}

			void CheckIfOpen()
			{
				if (App.FindElement(text) is null)
				{
					App.Click(72, 72); // Tap in the menu icon to show the Flyout.
					App.WaitForElement(text);
				}
			}
		}
	}
}