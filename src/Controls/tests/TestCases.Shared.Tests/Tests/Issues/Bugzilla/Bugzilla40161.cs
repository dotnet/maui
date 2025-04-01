#if TEST_FAILS_ON_WINDOWS //Image rendering size inside AbsoluteLayout is inconsistent on Windows. Created a issue report: https://github.com/dotnet/maui/issues/26094.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40161 : _IssuesUITest
{
	public Bugzilla40161(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Issue Bugzilla40161";

	[Test]
	[Category(UITestCategories.Layout)]
	public void Issue1Test()
	{
		App.WaitForElement("REFRESH");

		App.Tap("SWAP");
		App.Tap("REFRESH");

		Assert.That(App.FindElement("counter").GetText(), Is.EqualTo("step=0"));

		Assert.That(App.FindElement("width").GetText(), Is.EqualTo("w=50"));
	}
}
#endif