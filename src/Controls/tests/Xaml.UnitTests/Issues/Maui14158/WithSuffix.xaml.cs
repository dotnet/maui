using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class WithSuffix : ContentPage
{
	public WithSuffix() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void VerifyCorrectTypesUsed([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(WithSuffix)));

			var page = new WithSuffix(inflator);

			Assert.IsInstanceOf<PublicWithSuffix>(page.publicWithSuffix);
			Assert.IsInstanceOf<InternalWithSuffix>(page.internalWithSuffix);
		}
	}
}
