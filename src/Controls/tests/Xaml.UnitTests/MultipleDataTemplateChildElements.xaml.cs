using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class MultipleDataTemplateChildElements : BindableObject
	{
		public MultipleDataTemplateChildElements(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public static class Tests
		{
			[Fact]
			public static void ThrowXamlParseException([Values] bool useCompiledXaml)
			{
				if (useCompiledXaml)
				{
					MockCompiler.Compile(typeof(MultipleDataTemplateChildElements), out var md, out var hasLoggedErrors);
					Assert.That(hasLoggedErrors);
				}
				else
					Assert.Throws<XamlParseException>(() => new MultipleDataTemplateChildElements(useCompiledXaml));
			}
		}
	}
}
