using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19783 : _IssuesUITest
	{
		public Issue19783(TestDevice device) : base(device) { }

		public override string Issue => "ShellContent IsVisible=True Does not display Title";

		[Fact]
		[Trait("Category", UITestCategories.Shell)]
		public void Issue19783Shell()
		{
			App.WaitForElement("click to open pane");
			App.Tap("OpenPane");
			App.WaitForElement("Toggle Profile Page");
			App.Tap("ToggleProfilePage");
			App.WaitForElement("Profile Page");
		}
	}
}