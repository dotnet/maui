using NUnit.Framework;

using Xamarin.Forms;
namespace Xamarin.Forms.Xaml.UnitTests
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

		[TestFixture]
		public class SetterOnNonBPTests
		{
			[TestCase(false)]
			[TestCase(true)]
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