using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16432 : ContentPage
{
	public Maui16432() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[XamlInflatorData]
		internal void SpanStyleClassesAreApplied(XamlInflator inflator)
		{
			var page = new Maui16432(inflator);
			var spans = page.label.FormattedText.Spans;

			Assert.Equal(Colors.Blue, spans[0].TextColor);
			Assert.Equal(Colors.Red, spans[1].TextColor);
			Assert.Equal(Colors.Blue, spans[2].TextColor);
			Assert.Equal(FontAttributes.Bold, spans[2].FontAttributes);
			Assert.Equal(new[] { "accent", "emphasis" }, spans[2].StyleClass);
			Assert.Same(spans[1].StyleClass, spans[1].@class);
		}
	}
}
