using Microsoft.Maui.Controls.Xaml.UnitTests.Issues.Maui14158;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests.Maui14158;

public partial class InternalVisibleTypes : ContentPage
{
	public InternalVisibleTypes()
	{
		InitializeComponent();
	}

	public InternalVisibleTypes(bool useCompiledXaml)
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
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(InternalVisibleTypes)));

			var page = new InternalVisibleTypes(useCompiledXaml);

			Assert.IsInstanceOf<InternalButVisible>(page.internalButVisible);
		}
	}
}
