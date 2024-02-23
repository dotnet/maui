using NUnit.Framework;
using OpenQA.Selenium.Support.UI;

namespace UITests;
public abstract class IssuesUITest : UITestBase
{
	public abstract string Issue { get; }

	[SetUp]
	public void FixtureSetup()
	{
		try
		{
			RecordTestSetup();
			NavigateToIssue(Issue);
		}
		catch (Exception e)
		{
			TestContext.Error.WriteLine($">>>>> {DateTime.Now} The FixtureSetup threw an exception. {Environment.NewLine}Exception details: {e}");
		}
	}

	public void NavigateToIssue(string issue)
	{
		_ = App.TerminateApp("com.microsoft.mauicompatibilitygallery");
		App.ActivateApp("com.microsoft.mauicompatibilitygallery");

		WebDriverWait wait = new WebDriverWait(App, TimeSpan.FromSeconds(30));
		wait.Until(x => FindUIElement("com.microsoft.mauicompatibilitygallery:id/SearchBar"));

		NavigateToIssuesList();

		var issuesSearchBar = wait.Until(x => FindUIElement("com.microsoft.mauicompatibilitygallery:id/SearchBarGo"));

		issuesSearchBar.Click();
		issuesSearchBar.SendKeys(issue);
		FindUIElement("com.microsoft.mauicompatibilitygallery:id/SearchButton").Click();
	}
}