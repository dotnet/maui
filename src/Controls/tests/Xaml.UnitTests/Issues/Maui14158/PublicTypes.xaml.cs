using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class PublicTypes : ContentPage
{
	public PublicTypes() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : BaseTestFixture
	{
		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void VerifyCorrectTypesUsed(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(PublicTypes));

			var page = new PublicTypes(inflator);

			Assert.IsType<PublicInExternal>(page.publicInExternal);
			Assert.IsType<PublicInHidden>(page.publicInHidden);
			Assert.IsType<PublicInVisible>(page.publicInVisible);
		}
	}
}
