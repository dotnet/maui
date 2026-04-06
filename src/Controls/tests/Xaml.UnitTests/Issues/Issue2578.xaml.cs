using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue2578 : ContentPage
{
	public Issue2578() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory(Skip = "[Bug] NamedSizes don't work in triggers: https://github.com/xamarin/Microsoft.Maui.Controls/issues/13831")]
		[XamlInflatorData]
		internal void MultipleTriggers(XamlInflator inflator)
		{
			Issue2578 layout = new Issue2578(inflator);

			Assert.Null(layout.label.Text);
			Assert.Null(layout.label.BackgroundColor);
			Assert.Equal(Colors.Olive, layout.label.TextColor);
			layout.label.Text = "Foo";
			Assert.Equal(Colors.Red, layout.label.BackgroundColor);
		}
	}
}