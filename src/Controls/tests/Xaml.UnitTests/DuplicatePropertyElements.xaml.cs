using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class DuplicatePropertyElements : BindableObject
	{
		public DuplicatePropertyElements(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public static class Tests
		{
			[Theory]
			public static void ThrowXamlParseException([Values] bool useCompiledXaml)
			{
				if (useCompiledXaml)
				{
					MockCompiler.Compile(typeof(DuplicatePropertyElements), out var md, out var hasLoggedErrors);
					Assert.That(hasLoggedErrors);
				}
				else
					Assert.Throws<XamlParseException>(() => new DuplicatePropertyElements(useCompiledXaml));
			}
		}
	}
}
