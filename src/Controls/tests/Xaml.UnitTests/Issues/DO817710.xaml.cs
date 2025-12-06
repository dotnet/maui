using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DO817710 : ContentPage
{
	public DO817710() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void EmptyResourcesElement(XamlInflator inflator)
		{
			var ex = Record.Exception(() => new DO817710(inflator));
			Assert.Null(ex);
		}
	}
}
