using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests; public class BindingExtensionTests
{
	[Fact]
	public void ProvideValue_Null()
	{
		IMarkupExtension<BindingBase> binding = new BindingExtension { Path = "Foo" };
		binding.ProvideValue(null); // This should not throw
	}
}
