#if TEST_FAILS_ON_ANDROID // Skipping test: Unable to retrieve the native refresh color on Android.
using Xunit;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16973 : _IssuesUITest
{
	public Issue16973(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "RefreshView RefreshColor is not working";
	[Fact]
	[Category(UITestCategories.RefreshView)]
	public void Issue16973ValidateRefreshColor()
	{
		App.WaitForElement("label");
		App.Tap("button");
		var text = App.FindElement("label").GetText();
		Assert.Equal("Color matches", text);
	}
}
#endif