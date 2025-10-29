using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class PublicTypes : ContentPage
{
	public PublicTypes()
	{
		InitializeComponent();
	}

	public PublicTypes(bool useCompiledXaml)
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
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(PublicTypes)));

			var page = new PublicTypes(useCompiledXaml);

			Assert.IsInstanceOf<PublicInExternal>(page.publicInExternal);
			Assert.IsInstanceOf<PublicInHidden>(page.publicInHidden);
			Assert.IsInstanceOf<PublicInVisible>(page.publicInVisible);
		}
	}
}
