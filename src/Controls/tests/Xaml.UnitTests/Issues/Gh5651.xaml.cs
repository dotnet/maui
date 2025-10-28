using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh5651VM : IViewModel5651
	{
		public Gh5651VM()
		{
			this.SelectedItem = "test";
		}

		public string SelectedItem { get; set; }
	}

	public interface IViewModel5651 : IEditViewModel5651<string> { }

	public interface IEditViewModel5651<T> : IBaseViewModel5651<T> { }

	public interface IBaseViewModel5651<T>
	{
		T SelectedItem { get; set; }
	}

	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Gh5651 : ContentPage
	{
		public Gh5651()
		{
			InitializeComponent();
		}

		public Gh5651(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[InlineData(true), TestCase(false)]
			public void GenericBaseInterfaceResolution(bool useCompiledXaml)
			{
				if (useCompiledXaml)
					Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh5651)));
				var layout = new Gh5651(useCompiledXaml) { BindingContext = new Gh5651VM() };
				Assert.Equal("test", layout.label.Text);
			}
		}
	}
}
