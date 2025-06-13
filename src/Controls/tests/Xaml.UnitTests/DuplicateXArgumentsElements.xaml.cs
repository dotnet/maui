using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class DuplicateXArgumentsElements : BindableObject
	{
		public DuplicateXArgumentsElements(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		public static class Tests
		{
			[Fact]
			public static void ThrowXamlParseException([Values] bool useCompiledXaml)
			{
				if (useCompiledXaml)
				{
					MockCompiler.Compile(typeof(DuplicateXArgumentsElements), out var md, out var hasLoggedErrors);
					Assert.That(hasLoggedErrors);
				}
				else
					Assert.Throws<XamlParseException>(() => new DuplicateXArgumentsElements(useCompiledXaml));
			}
		}
	}
}
