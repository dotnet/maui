using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh5330 : ContentPage
	{
		public Gh5330() => InitializeComponent();
		public Gh5330(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		//[Theory]
		//public void Method(bool useCompiledXaml)
		//{
		//	if (useCompiledXaml)
		//		Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh5330)));
		//}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public void Method(bool useCompiledXaml)
		{
			var layout = new Gh5330(useCompiledXaml) { BindingContext = new Button { Text = "Foo" } };
			Assert.Equal("Foo", layout.label.Text);
		}

	}
}
