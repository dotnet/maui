namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class CustomButtonNoBaseClass //DO NOT add base class here
{
	public CustomButtonNoBaseClass()
	{
		InitializeComponent();
	}
}
