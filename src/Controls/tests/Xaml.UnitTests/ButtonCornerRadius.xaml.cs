using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class ButtonCornerRadius : ContentPage
	{
		public ButtonCornerRadius()
		{
			InitializeComponent();
		}

		public ButtonCornerRadius(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void EscapedStringsAreTreatedAsLiterals(bool useCompiledXaml)
			{
				var layout = new ButtonCornerRadius(useCompiledXaml);
				Assert.Equal(0, layout.Button0.CornerRadius);
			}
		}
	}
}
