using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Issue2062 : ContentPage
	{
		public Issue2062()
		{
			InitializeComponent();
		}

		public Issue2062(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[InlineData(false)]
			[InlineData(true)]
			public void LabelWithoutExplicitPropertyElement(bool useCompiledXaml)
			{
				var layout = new Issue2062(useCompiledXaml);
				Assert.Equal("text explicitly set to Label.Text", layout.label1.Text);
				Assert.Equal("text implicitly set to Text property of Label", layout.label2.Text);
			}
		}
	}
}