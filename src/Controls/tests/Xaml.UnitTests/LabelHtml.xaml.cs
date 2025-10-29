using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class LabelHtml : ContentPage
	{
		public LabelHtml() => InitializeComponent();
		public LabelHtml(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method(bool useCompiledXaml)
			{
				var html = "<h1>Hello World!</h1><br/>SecondLine";
				var layout = new LabelHtml(useCompiledXaml);
				Assert.Equal(html, layout.label0.Text);
				Assert.Equal(html, layout.label1.Text);
				Assert.Equal(html, layout.label2.Text);
				Assert.Equal(html, layout.label3.Text);
				Assert.Equal(html, layout.label4.Text);
			}
		}
	}
}