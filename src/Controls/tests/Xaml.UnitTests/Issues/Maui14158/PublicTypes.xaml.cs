using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class PublicTypes : ContentPage
{
	public PublicTypes() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void VerifyCorrectTypesUsed([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(PublicTypes)));

			var page = new PublicTypes(inflator);

			Assert.IsInstanceOf<PublicInExternal>(page.publicInExternal);
			Assert.IsInstanceOf<PublicInHidden>(page.publicInHidden);
			Assert.IsInstanceOf<PublicInVisible>(page.publicInVisible);
		}
	}
}
