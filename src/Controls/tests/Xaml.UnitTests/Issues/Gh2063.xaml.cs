using Microsoft.Maui.Controls.Build.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Gh2063 : ContentPage
	{
		public Gh2063()
		{
			InitializeComponent();
		}

		public Gh2063(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(false), InlineData(true)]
			public void DetailedErrorMessageOnMissingXmlnsDeclaration(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Gh2063)));
				else
					Assert.Throws<XamlParseException>(() => new Gh2063(useCompiledXaml));
			}
		}
	}
}