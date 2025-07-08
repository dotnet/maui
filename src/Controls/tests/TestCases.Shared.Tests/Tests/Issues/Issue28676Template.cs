using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
[Category(UITestCategories.CollectionView)]
public class Issue28676Template : _IssuesUITest
{
	public override string Issue => "Android Dynamic Updates to CollectionView Header and FooterTemplates Are Not Displayed";

	public Issue28676Template(TestDevice device) : base(device)
	{
	}

	[Test]
	public void CollectionViewHeaderTemplateShouldChangeDynamically()
	{
		App.WaitForElement("Issue28676TemplateHeader");
		App.WaitForElement("Issue28676ChangeHeaderTemplate");
		App.Tap("Issue28676ChangeHeaderTemplate");
		App.WaitForElement("Issue28676ChangedHeaderTemplate");
	}

	[Test]
	public void CollectionViewFooterTemplateShouldChangeDynamically()
	{
		App.WaitForElement("Issue28676TemplateFooter");
		App.WaitForElement("Issue28676ChangeFooterTemplate");
		App.Tap("Issue28676ChangeFooterTemplate");
		App.WaitForElement("Issue28676ChangedFooterTemplate");
	}
}