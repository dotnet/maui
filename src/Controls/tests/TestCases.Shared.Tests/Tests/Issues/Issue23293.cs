using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

class Issue23293 : _IssuesUITest
{
	public Issue23293(TestDevice device) : base(device) { }

	public override string Issue => "'Grouping for Vertical list without DataTemplates' page loading exception";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewWithoutDataTemplate()
	{
		App.WaitForElement("CollectionViewWithoutDataTemplate");
		VerifyScreenshot();
	}
}

