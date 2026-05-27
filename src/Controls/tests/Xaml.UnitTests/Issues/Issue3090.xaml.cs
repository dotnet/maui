using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue3090 : ContentPage
{
	public Issue3090() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void NewDoesNotThrow(XamlInflator inflator)
		{
			var p = new Issue3090(inflator);
		}
	}
}