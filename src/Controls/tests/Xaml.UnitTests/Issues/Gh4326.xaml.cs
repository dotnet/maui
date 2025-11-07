using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh4326 : ContentPage
{
	public static string Foo = "Foo";
	internal static string Bar = "Bar";

	public Gh4326() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void FindStaticInternal(XamlInflator inflator)
		{
			var layout = new Gh4326(inflator);

			Assert.Equal("Foo", layout.labelfoo.Text);
			Assert.Equal("Bar", layout.labelbar.Text);
			Assert.Equal(Style.StyleClassPrefix, layout.labelinternalvisibleto.Text);
		}
	}
}
