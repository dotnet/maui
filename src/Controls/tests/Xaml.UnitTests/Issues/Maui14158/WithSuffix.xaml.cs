using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class WithSuffix : ContentPage
{
	public WithSuffix()
	{
		InitializeComponent();
	}

	public WithSuffix(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Tests
	{
		[InlineData(true)]
		[Theory]
		[InlineData(false)]
		public void VerifyCorrectTypesUsed(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(WithSuffix)));

			var page = new WithSuffix(useCompiledXaml);

			Assert.IsInstanceOf<PublicWithSuffix>(page.publicWithSuffix);
			Assert.IsInstanceOf<InternalWithSuffix>(page.internalWithSuffix);
		}
	}
}
