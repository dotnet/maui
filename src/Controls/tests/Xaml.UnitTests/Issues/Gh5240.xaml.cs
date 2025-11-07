using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh5240 : ContentPage
{
	public Gh5240() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void FailOnUnresolvedDataType(XamlInflator inflator)
		{
			new Gh5240(inflator);
		}
	}
}
