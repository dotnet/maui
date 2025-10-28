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
		}		public class SetterOnNonBPTests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void ShouldThrow(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					new BuildExceptionConstraint(10, 13).Validate(() => MockCompiler.Compile(typeof(SetterOnNonBP)));
				else
					new XamlParseExceptionConstraint(10, 13).Validate(() => new SetterOnNonBP(useCompiledXaml));
			}
		}
	}
}