#if IOS
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23892 : _IssuesUITest
	{
		public Issue23892(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Using long-press navigation on back button using shell does not update the shell's current page";

		[Fact]
		[Category(UITestCategories.Shell)]
		public void ShellBackButtonShouldWorkOnLongPress()
		{
			App.WaitForElement("button");
			App.Click("button");
			App.LongPress("Back");
			var text = App.FindElement("label").GetText();

			Assert.Equal("OnAppearing count: 2", text);
		}
	}
}
#endif