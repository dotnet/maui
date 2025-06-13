using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19127 : _IssuesUITest
	{
		public override string Issue => "Triggers are not working on Frame control";

		public Issue19127(TestDevice device) : base(device)
		{
		}

		[Fact]
		[Category(UITestCategories.Frame)]
		public void ContentOfFrameShouldChange()
		{
			_ = App.WaitForElement("button");

			var textBeforeClick = App.FindElement("label1").GetText();

			App.Click("button");

			var textAfterClick = App.FindElement("label2").GetText();

			Assert.Equal("Camera is Disabled", textBeforeClick!);
			Assert.Equal("Camera is Enabled", textAfterClick!);
		}
	}
}
