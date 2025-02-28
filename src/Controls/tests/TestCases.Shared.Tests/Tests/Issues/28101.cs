using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28101 : _IssuesUITest
{
	public Issue28101(TestDevice device) : base(device)
	{
	}
	public override string Issue => "CollectionView Footer Becomes Scrollable When EmptyView is Active on Android";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void FooterTemplateShouldNotScrollWhenEmptyViewIsDisplayed()
	{
		App.WaitForElement("This Is A Footer");
	}
}