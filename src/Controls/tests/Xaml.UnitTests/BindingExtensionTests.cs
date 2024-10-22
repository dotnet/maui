using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[TestFixture]
public class BindingExtensionTests
{
	[Test]
	public void ProvideValue_Null()
	{
		IMarkupExtension<BindingBase> binding = new BindingExtension { Path = "Foo" };
		binding.ProvideValue(null); // This should not throw
	}
}
