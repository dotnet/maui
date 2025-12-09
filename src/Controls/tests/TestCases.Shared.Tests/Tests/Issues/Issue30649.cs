#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/27195 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30649 : _IssuesUITest
{
	public override string Issue => "GraphicsView event handlers are triggered even when IsEnabled is set to False";

	public Issue30649(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void TestGraphicsViewTouchEventsIgnoredWhenIsEnabledFalse()
	{
		App.WaitForElement("Issue30649_GraphicsView");
		App.Tap("Issue30649_GraphicsView");
		Assert.That(App.WaitForElement("Issue30649_Label").GetText(), Is.EqualTo("Success"));
	}
}
#endif