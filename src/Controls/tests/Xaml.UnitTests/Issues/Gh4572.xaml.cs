using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh4572 : ContentPage
	{
		public Gh4572() => InitializeComponent();
		public Gh4572(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), InlineData(false)]
			public void BindingAsElement(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh4572)));
				var layout = new Gh4572(useCompiledXaml) { BindingContext = new { labeltext = "Foo" } };
				Assert.Equal("Foo", layout.label.Text);
			}
		}
	}
}
