using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue3090 : ContentPage
{
	public Issue3090() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void NewDoesNotThrow(XamlInflator inflator)
		{
			var p = new Issue3090(inflator);
		}
	}
}