using Microsoft.Maui.Controls.CustomAttributes;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Support.UI;

namespace UITests
{
    public class Issue11311 : BaseTest
    {
		[SetUp]
		public void NavigateToTest()
		{
			var issueAttribute = TestContext.CurrentContext.Test.Method?.GetCustomAttributes<IssueAttribute>(true)[0];

			if (issueAttribute is null)
			{
				return;
			}

			_ = App.TerminateApp("com.microsoft.mauicompatibilitygallery");
			App.ActivateApp("com.microsoft.mauicompatibilitygallery");
			
			WebDriverWait wait = new WebDriverWait(App, TimeSpan.FromSeconds(30));
			wait.Until(x => FindUIElement("com.microsoft.mauicompatibilitygallery:id/SearchBar"));
			
			//NavigateToGallery("CollectionView GalleryAutomationId");
			NavigateToIssuesList();

			var issuesSearchBar = wait.Until(x => FindUIElement("com.microsoft.mauicompatibilitygallery:id/SearchBarGo"));

			issuesSearchBar.Click();
			issuesSearchBar.SendKeys(issueAttribute.DisplayName);
			FindUIElement("com.microsoft.mauicompatibilitygallery:id/SearchButton").Click();
		}

		[Test]
		[Issue(IssueTracker.None, 11311, "[Regression] CollectionView NSRangeException", PlatformAffected.iOS)]
		//[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.TabbedPage)]
		public void CollectionViewWithFooterShouldNotCrashOnDisplay()
		{
			// If this hasn't already crashed, the test is passing
			App.FindElement(By.XPath("//*[@text=\"Success\"]"));
		}
	}
}
