using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class ButtonUITests : _ViewUITests
	{
		const string ButtonGallery = "Button Gallery";

		public ButtonUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(ButtonGallery);
		}

		[Fact]
		[Trait("Category", UITestCategories.Button)]
		public void Clicked()
		{
			var remote = GoToEventRemote();

			var textBeforeClick = remote.GetEventLabel().GetText();
			Assert.Equal("Event: Clicked (none)", textBeforeClick);

			// Click Button
			remote.TapView();

			var textAfterClick = remote.GetEventLabel().GetText();
			Assert.Equal("Event: Clicked (fired 1)", textAfterClick);
		}
	}
}
