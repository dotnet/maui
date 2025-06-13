#if TEST_FAILS_ON_WINDOWS //Image rendering size inside AbsoluteLayout is inconsistent on Windows. Created a issue report: https://github.com/dotnet/maui/issues/26094.
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla40161 : _IssuesUITest
{
	public Bugzilla40161(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Issue Bugzilla40161";

	[Fact]
	[Category(UITestCategories.Layout)]
	public void Issue1Test()
	{
		App.WaitForElement("REFRESH");

		App.Tap("SWAP");
		App.Tap("REFRESH");

		Assert.Equal("step=0", App.FindElement("counter").GetText());

		Assert.Equal("w=50", App.FindElement("width").GetText());
	}
}
#endif