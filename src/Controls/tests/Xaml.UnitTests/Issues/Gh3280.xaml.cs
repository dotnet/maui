using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh3280 : ContentPage
{
	public Gh3280() => InitializeComponent();

	public Size Foo { get; set; }


	public class Tests
	{
		[Theory]
		[Values]
		public void SizeHasConverter(XamlInflator inflator)
		{
			Gh3280 layout = null;
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed
			layout = new Gh3280(inflator);
			Assert.Equal(new Size(15, 25), layout.Foo);
		}
	}
}
