using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class McIgnorable : ContentPage
	{
		public McIgnorable()
		{
			InitializeComponent();
		}

		public McIgnorable(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public public class Tests
		{
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void DoesNotThrow(bool useCompiledXaml)
			{
				var layout = new McIgnorable(useCompiledXaml);
			}
		}
	}
}