using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23293 : _IssuesUITest
{
	public Issue23293(TestDevice device) : base(device) { }

	public override string Issue => "Grouping collection view without data template results in displaying the default string representation of the object";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewWithoutDataTemplate()
	{
		App.WaitForElement("CollectionViewWithoutDataTemplate");
		VerifyScreenshot();
	}
}