using Microsoft.Maui.Controls;
using Xunit;
namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class FakeView : View
	{
		public string NonBindable { get; set; }
	}

	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SetterOnNonBP : ContentPage
	{
		public SetterOnNonBP()
		{
			InitializeComponent();
		}

		public SetterOnNonBP(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class SetterOnNonBPTests
		{
			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ShouldThrow(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.Throws(new BuildExceptionConstraint(10, 13), () => MockCompiler.Compile(typeof(SetterOnNonBP)));
				else
					Assert.Throws(new XamlParseExceptionConstraint(10, 13), () => new SetterOnNonBP(useCompiledXaml));
			}
		}
	}
}