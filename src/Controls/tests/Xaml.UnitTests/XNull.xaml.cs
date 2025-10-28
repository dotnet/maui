using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class XNull : ContentPage
	{
		public XNull()
		{
			InitializeComponent();
		}

		public XNull(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void SupportsXNull(bool useCompiledXaml)
			{
				var layout = new XNull(useCompiledXaml);
				Assert.True(layout.Resources.ContainsKey("null"));
				Assert.Null(layout.Resources["null"]);
			}
		}
	}
}