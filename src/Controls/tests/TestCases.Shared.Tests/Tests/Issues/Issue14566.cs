using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue14566 : _IssuesUITest
	{
		public Issue14566(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SearchBar IsEnabled property not functioning";

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchBarShouldRespectIsEnabled()
		{
			var searchBar = App.WaitForElement("SearchBar");
			Assert.That(searchBar.IsEnabled(), Is.False);
		}
	}
}