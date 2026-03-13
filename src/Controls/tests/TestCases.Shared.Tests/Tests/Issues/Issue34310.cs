using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34310 : _IssuesUITest
{
	public override string Issue => "Loaded event not called for MAUI View added to native View";

	public Issue34310(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.LifeCycle)]
	public void LoadedEventFiresForNativeHostedView()
	{
		App.WaitForElement("LoadedStatus");
		Assert.That(App.FindElement("LoadedStatus").GetText(), Is.EqualTo("Loaded"),
			"Loaded event should fire for a MAUI View added to a native container via ToPlatform().");
	}
}
