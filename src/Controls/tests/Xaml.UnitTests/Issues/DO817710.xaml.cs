using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class DO817710 : ContentPage
{
	public DO817710() => InitializeComponent();


	[Theory]
	[Values]
	public void EmptyResourcesElement(XamlInflator inflator)
	{
		Assert.Null(Record.Exception(() => new DO817710(inflator)));
	}

}
