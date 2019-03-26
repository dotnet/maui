using NUnit.Framework;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class MultipleDataTemplateChildElements : BindableObject
	{
		public MultipleDataTemplateChildElements(bool useCompiledXaml)
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
					(TestDelegate)(() => MockCompiler.Compile(typeof(MultipleDataTemplateChildElements))) :
					() => new MultipleDataTemplateChildElements(useCompiledXaml));
			}
		}
	}
}
