using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class InlineCSS : ContentPage
{
	public InlineCSS() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void InlineCSSParsed(XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void InitialValue(XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.Equal(Colors.Green, layout.BackgroundColor);
			Assert.Equal(Colors.Green, layout.stack.BackgroundColor);
			Assert.Equal(Colors.Green, layout.button.BackgroundColor);
			Assert.Equal(VisualElement.BackgroundColorProperty.DefaultValue, layout.label.BackgroundColor);
			Assert.Equal(TextTransform.Uppercase, layout.label.TextTransform);
		}
	}
}
