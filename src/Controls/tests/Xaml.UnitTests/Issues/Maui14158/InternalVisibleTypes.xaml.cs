using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class InternalVisibleTypes : ContentPage
{
	public InternalVisibleTypes() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void VerifyCorrectTypesUsed([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(InternalVisibleTypes)));

			var page = new InternalVisibleTypes(inflator);

			Assert.IsInstanceOf<InternalButVisible>(page.internalButVisible);
		}
	}
}
