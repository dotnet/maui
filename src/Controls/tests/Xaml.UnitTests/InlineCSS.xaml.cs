using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class InlineCSS : ContentPage
{
	public InlineCSS() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void InlineCSSParsed(XamlInflator inflator)
		{
			var layout = new InlineCSS(inflator);
			Assert.Equal(Colors.Pink, layout.label.TextColor);
		}

		[Theory]
		[Values]
		public void InitialValue(XamlInflator inflator)
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
