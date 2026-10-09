using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17175 : ContentPage
{
	public Maui17175() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void VisualStatesApplyStyleValuesAndRestoreThem(XamlInflator inflator)
		{
			var page = new Maui17175(inflator);
			var button = page.TestButton;
			for (var i = 0; i < 2; i++)
			{
				button.IsEnabled = false;
				Assert.Equal("State: Disabled", button.Text);
				Assert.Equal(Colors.Gray, button.BackgroundColor);
				Assert.Equal(Colors.DarkGray, button.TextColor);

				button.IsEnabled = true;
				Assert.Equal("State: Normal", button.Text);
				Assert.Equal(Colors.LightBlue, button.BackgroundColor);
				Assert.Equal(Colors.Black, button.TextColor);
				Assert.Equal(new Thickness(20), button.Padding);
			}

			Assert.True(VisualStateManager.GoToState(button, "Selected"));
			Assert.Equal(Colors.Orange, button.BackgroundColor);
			Assert.Equal(FontAttributes.Bold, button.FontAttributes);
			Assert.True(VisualStateManager.GoToState(button, "Normal"));
			Assert.Equal(FontAttributes.None, button.FontAttributes);
			Assert.Equal(Colors.LightBlue, button.BackgroundColor);
		}
	}
}
