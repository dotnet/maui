using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Bz43450 : ContentPage
	{
		public Bz43450()
		{
			InitializeComponent();
		}

		public Bz43450(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true)]
			[InlineData(false)]
			public void DoesNotAllowGridRowDefinition(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws<BuildException>(() => MockCompiler.Compile(typeof(Bz43450)));
				else
					Assert.Throws<XamlParseException>(() => new Bz43450(useCompiledXaml));
			}
		}
	}
}