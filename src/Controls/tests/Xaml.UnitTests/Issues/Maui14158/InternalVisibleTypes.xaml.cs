using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class InternalVisibleTypes : ContentPage
{
	public InternalVisibleTypes() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void VerifyCorrectTypesUsed(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(InternalVisibleTypes));

			var page = new InternalVisibleTypes(inflator);

			Assert.IsType<InternalButVisible>(page.internalButVisible);
		}
	}
}
