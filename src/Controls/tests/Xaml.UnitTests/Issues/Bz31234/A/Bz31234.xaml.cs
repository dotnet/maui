using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.A;

public partial class Bz31234 : ContentPage
{
	public Bz31234() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void ShouldPass(XamlInflator inflator)
		{
			new Bz31234(inflator);
		}
	}
}