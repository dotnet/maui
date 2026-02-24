using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Unreported003 : ContentPage
{
	public Unreported003() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void AllowCtorArgsForValueTypes(XamlInflator inflator)
		{
			var page = new Unreported003(inflator);
		}
	}
}