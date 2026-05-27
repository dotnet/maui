using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue3106 : ContentPage
{
	public Issue3106() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void NewDoesNotThrow(XamlInflator inflator)
		{
			var p = new Issue3106(inflator);
		}
	}
}

