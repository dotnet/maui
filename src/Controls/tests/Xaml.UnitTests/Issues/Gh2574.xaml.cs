using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh2574 : ContentPage
	{
		public Gh2574()
		{
			InitializeComponent();
		}

		public Gh2574(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{

			[InlineData(false), TestCase(true)]
			public void xNameOnRoot(bool useCompiledXaml)
			{
				var layout = new Gh2574(useCompiledXaml);
				Assert.Equal(layout, layout.page);
			}
		}
	}
}
