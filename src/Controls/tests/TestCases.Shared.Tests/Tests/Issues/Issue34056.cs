using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34056(TestDevice testDevice) : _IssuesUITest(testDevice)
{
	public override string Issue => "iOS wrong trimmed Relative bindings under config XamlInflator and AOT";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void RelativeAncestorBindingCommandShouldExecute()
	{
		// Verify the page loaded and StatusLabel shows initial state
		App.WaitForElement("StatusLabel");

		// Tap a CollectionView item button whose AutomationId is bound to ProductName
		App.WaitForElement("Monkey Doll");
		App.Tap("Monkey Doll");

		// The Button's Command is bound via RelativeSource AncestorType to the page ViewModel.
		// With MauiXamlInflator + AOT, the binding was trimmed incorrectly and the command
		// would not execute. Verify the label was updated by the command.
		var labelText = App.WaitForElement("StatusLabel").GetText();
		Assert.That(labelText, Is.EqualTo("Command Executed"),
			"Button command bound via RelativeSource AncestorType should execute and update the label.");
	}
}
