using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5240 : ContentPage
{
	public Gh5240() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void FailOnUnresolvedDataType(XamlInflator inflator)
		{
			new Gh5240(inflator);
		}
	}
}
