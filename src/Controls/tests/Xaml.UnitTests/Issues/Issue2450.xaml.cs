using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	//this covers Issue2125 as well
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class Issue2450 : ContentPage
	{
		public Issue2450()
		{
			InitializeComponent();
		}

		public Issue2450(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			public void ThrowMeaningfulExceptionOnDuplicateXName(bool useCompiledXaml)
			{
				var layout = new Issue2450(useCompiledXaml);
				new XamlParseExceptionConstraint(11, 13, m => m == "An element with the name \"label0\" already exists in this NameScope").Validate(() => (layout.Resources["foo"] as Microsoft.Maui.Controls.DataTemplate).CreateContent());
			}
		}
	}
}