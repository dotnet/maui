using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class MergedResourceDictionaries : ContentPage
{
	public MergedResourceDictionaries() => InitializeComponent();

	public class Tests
	{
		[Theory]
		[Values]
		public void MergedResourcesAreFound(XamlInflator inflator)
		{
			MockCompiler.Compile(typeof(MergedResourceDictionaries));
			var layout = new MergedResourceDictionaries(inflator);
			Assert.Equal("Foo", layout.label0.Text);
			Assert.Equal(Colors.Pink, layout.label0.TextColor);
			Assert.Equal(Color.FromArgb("#111"), layout.label0.BackgroundColor);
		}
	}
}