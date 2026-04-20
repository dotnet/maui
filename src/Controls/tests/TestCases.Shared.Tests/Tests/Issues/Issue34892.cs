using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34892 : _IssuesUITest
{
	public Issue34892(TestDevice device) : base(device)
	{
	}

	public override string Issue => "ContentPage with ToolbarItem Clicked event leaks when presented as modal page";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void ToolbarItemClickedHandlerDoesNotLeakModalPage()
	{
		App.WaitForElement("PushLeakPageModalButton");
		App.Tap("PushLeakPageModalButton");
		App.WaitForElement("LeakPageCloseButton");
		App.Tap("LeakPageCloseButton");
		App.WaitForElement("ForceGCButton");
		App.Tap("ForceGCButton");
		App.WaitForElement("StatusLabel");
		Thread.Sleep(1000); // Allow some time for the GC to finalize objects
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Still alive: 0"));

		App.WaitForElement("PushLeakPageModalButton");
		App.Tap("PushLeakPageModalButton");
		App.WaitForElement("LeakPageCloseButton");
		App.Tap("LeakPageCloseButton");
		App.WaitForElement("ForceGCButton");
		App.Tap("ForceGCButton");
		App.WaitForElement("StatusLabel");
		Thread.Sleep(1000); // Allow some time for the GC to finalize objects
		Assert.That(App.WaitForElement("StatusLabel").GetText(), Is.EqualTo("Still alive: 0"));
	}
}
