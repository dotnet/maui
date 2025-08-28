using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19771 : _IssuesUITest
{
	public Issue19771(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView IsEnabled=false allows touch interactions on iOS and Android";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewIsEnabledFalsePreventsInteractions()
	{
		App.WaitForElement("Item 1");
		App.Tap("Item 1");
        App.WaitForElement("DisableButton");
        App.Tap("DisableButton");
        App.WaitForElement("Item 3");
		App.Tap("Item 3");
		var text = App.WaitForElement("InteractionCountLabel").GetText();
		Assert.That(text, Is.EqualTo("Interaction Count: 1"));
	}
}