using Microsoft.Maui.Appium;
using NUnit.Framework;
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
				catch (Exception e)
				{
					TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureSetup threw an exception. Attempt {retries}/{SetupMaxRetries}.{Environment.NewLine}Exception details: {e}");
					if (retries++ < SetupMaxRetries)
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
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
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