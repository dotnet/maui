using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11541 : ContentPage
{
	public Gh11541() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void RectangleGeometryDoesntThrow(XamlInflator inflator)
		{
			Assert.Null(Record.Exception(() => new Gh11541(inflator)));
		}
	}
}
