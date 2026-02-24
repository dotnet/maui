using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[Collection("Xaml Inflation")]
public class BindingExtensionTests
{
	[Fact]
	public void ProvideValue_Null()
	{
		BindingExtension binding = new BindingExtension { Path = "Foo" };
		((IMarkupExtension<BindingBase>)binding).ProvideValue(null); // This should not throw
	}
}
