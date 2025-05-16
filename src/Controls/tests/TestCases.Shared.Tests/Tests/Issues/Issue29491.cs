using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue29491 : _IssuesUITest
{
	public override string Issue => "[CV2][CollectionView] Changing CollectionView's ItemsSource in runtime removes elements' parent seemingly random";

	public Issue29491(TestDevice device)
	: base(device)
	{ }

	[Category(UITestCategories.CollectionView)]
	public void VerifyDataTemplateParentisNotNull()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		App.WaitForElement("Micorosoft.Maui.Controls.CollectionView");
	}
}