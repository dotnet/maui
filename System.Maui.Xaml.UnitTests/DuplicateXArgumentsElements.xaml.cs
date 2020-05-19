using NUnit.Framework;

namespace System.Maui.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class DuplicateXArgumentsElements : BindableObject
	{
		public DuplicateXArgumentsElements(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public static class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
			public static void ThrowXamlParseException(bool useCompiledXaml)
			{
				Assert.Throws<XamlParseException>(useCompiledXaml ?
					(TestDelegate)(() => MockCompiler.Compile(typeof(DuplicateXArgumentsElements))) :
					() => new DuplicateXArgumentsElements(useCompiledXaml));
			}
		}
	}
}
