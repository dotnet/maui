using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[TestFixture]
public class BindingExtensionTests
{
	[Test]
	public void ProvideValue_Null()
	{
		BindingExtension binding = new BindingExtension { Path = "Foo" };
		((IMarkupExtension<BindingBase>)binding).ProvideValue(null); // This should not throw
	}
}
