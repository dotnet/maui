using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class McIgnorable : ContentPage
{
	public McIgnorable() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void DoesNotThrow(XamlInflator inflator)
		{
			var layout = new McIgnorable(inflator);
		}
	}
}