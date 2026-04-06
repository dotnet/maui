using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class InternalVisibleTypes : ContentPage
{
	public InternalVisibleTypes() => InitializeComponent();

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
				MockCompiler.Compile(typeof(InternalVisibleTypes));

			var page = new InternalVisibleTypes(inflator);

			Assert.IsType<InternalButVisible>(page.internalButVisible);
		}
	}
}
