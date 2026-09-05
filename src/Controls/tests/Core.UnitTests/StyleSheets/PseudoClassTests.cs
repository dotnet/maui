using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{
	[Collection("StyleSheet")]
	public class PseudoClassTests : BaseTestFixture
	{
		public PseudoClassTests()
		{
			ApplicationExtensions.CreateAndSetMockApplication();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				Application.ClearCurrent();

			base.Dispose(disposing);
		}

		[Fact]
		public void PseudoClassSelectorParsing()
		{
			Assert.NotEqual(Selector.Invalid, Selector.Parse("button:hover"));
			Assert.NotEqual(Selector.Invalid, Selector.Parse("input:focus"));
			Assert.NotEqual(Selector.Invalid, Selector.Parse(".btn:disabled"));
		}

		[Fact]
		public void PseudoClassVsmMapping()
		{
			var button = new Button { Text = "Click Me" };
			var app = new MockApplication();

			var stylesheet = StyleSheet.FromString(@"
				.primary { background-color: blue; }
				.primary:hover { background-color: darkblue; }
				.primary:focus { border-width: 2; }
				.primary:disabled { opacity: 0.5; }
			");

			button.StyleClass = new[] { "primary" };
			app.Resources.Add(stylesheet);
			app.LoadPage(new ContentPage { Content = button });

			var commonStates = VisualStateManager.GetVisualStateGroups(button)
				.FirstOrDefault(group => group.Name == "CommonStates");

			Assert.NotNull(commonStates);
			Assert.NotEmpty(commonStates.States.First(state => state.Name == VisualStateManager.CommonStates.PointerOver).Setters);
			Assert.NotEmpty(commonStates.States.First(state => state.Name == VisualStateManager.CommonStates.Focused).Setters);
			Assert.NotEmpty(commonStates.States.First(state => state.Name == VisualStateManager.CommonStates.Disabled).Setters);
		}
	}
}
