using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class WithSuffix : ContentPage
{
	public WithSuffix() => InitializeComponent();

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
				MockCompiler.Compile(typeof(WithSuffix));

			var page = new WithSuffix(inflator);

			Assert.IsType<PublicWithSuffix>(page.publicWithSuffix);
			Assert.IsType<InternalWithSuffix>(page.internalWithSuffix);
		}
	}
}
