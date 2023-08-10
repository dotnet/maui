using Microsoft.Maui.Appium;
using OpenQA.Selenium.Support.UI;

namespace Microsoft.Maui.AppiumTests
{
	public abstract class _IssuesUITest : UITestBase
	{
		public _IssuesUITest(TestDevice device) : base(device) { }

		protected override void FixtureSetup()
		{
			int retries = 0;
			while (true)
			{
				try
				{
					base.FixtureSetup();
					NavigateToIssue(Issue);
					break;
				}
				catch (Exception)
				{
					if (retries++ < 1)
					{
						Reset();
					}
					else
					{
						throw;
					}
				}
			}
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			try
			{
				App.NavigateBack();
				App.Tap("GoBackToGalleriesButton");
			}
			catch (Exception)
			{ 
			}
		}

		public abstract string Issue { get; }

		private static void NavigateToIssue(string issue)
		{
			App.NavigateToIssues();

			App.EnterText("SearchBarGo", issue);

			App.WaitForElement("SearchButton");
			App.Tap("SearchButton");
		}
	}
}