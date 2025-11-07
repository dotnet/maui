using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class McIgnorable : ContentPage
{
	public McIgnorable() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void DoesNotThrow(XamlInflator inflator)
		{
			var layout = new McIgnorable(inflator);
		}
	}
}