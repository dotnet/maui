using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11541 : ContentPage
{
	public Gh11541() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void RectangleGeometryDoesntThrow(XamlInflator inflator)
		{
			var ex = Record.Exception(() => new Gh11541(inflator));
			Assert.Null(ex);
		}
	}
}
