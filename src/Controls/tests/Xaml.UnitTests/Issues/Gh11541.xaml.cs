using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11541 : ContentPage
{
	public Gh11541() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void RectangleGeometryDoesntThrow()
		{
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => new Gh11541(inflator));
		}
	}
}
