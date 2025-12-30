using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3280 : ContentPage
{
	public Gh3280() => InitializeComponent();

	public Size Foo { get; set; }

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void SizeHasConverter(XamlInflator inflator)
		{
			Gh3280 layout = null;
			var ex = Record.Exception(() => layout = new Gh3280(inflator));
			Assert.Null(ex);
			Assert.Equal(new Size(15, 25), layout.Foo);
		}
	}
}
