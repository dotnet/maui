using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public abstract class IssuesUITest : UITest
	{
		public IssuesUITest(TestDevice device) : base(device) { }

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
				if (ResetMainPage)
				{
					Reset();
				}
				else
				{
					this.Back();
				}
			}
			catch (Exception e)
			{
				var name = TestContext.CurrentContext.Test.MethodName ?? TestContext.CurrentContext.Test.Name;
				TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureTeardown threw an exception during {name}.{Environment.NewLine}Exception details: {e}");
			}
		}

		public abstract string Issue { get; }
		public virtual bool ResetMainPage { get; private set; } = true;

		void NavigateToIssue(string issue)
		{
			if (ResetMainPage)
			{
				App.Click("ResetMainPage");

				RunningApp.ClearText("SearchBar");
				RunningApp.EnterText("SearchBar", issue);

				RunningApp.WaitForElement("GoToTestButton");
				RunningApp.Tap("GoToTestButton");
			}
			else
			{
				RunningApp.NavigateToIssues();

				RunningApp.ClearText("SearchBarGo");
				RunningApp.EnterText("SearchBarGo", issue);

				RunningApp.WaitForElement("SearchButton");
				RunningApp.Tap("SearchButton");
			}
		}
	}
}