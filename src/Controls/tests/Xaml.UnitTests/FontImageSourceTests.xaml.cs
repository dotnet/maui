using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class FontImageSourceTests : ContentPage
	{
		public FontImageSourceTests() => InitializeComponent();
		public FontImageSourceTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(false), TestCase(true)]

			public void FontImageWithDynamicResource(bool useCompiledXaml)
			{
				var layout = new FontImageSourceTests(useCompiledXaml);
				Assert.That((layout.staticImage.Source as FontImageSource).Color, Is.EqualTo(Colors.HotPink));
				Assert.That((layout.dynamicWithExtension.Source as FontImageSource).Color, Is.EqualTo(Colors.HotPink));
				Assert.That((layout.dynamicWithElement.Source as FontImageSource).Color, Is.EqualTo(Colors.HotPink));
			}

			[TestCase(false), TestCase(true)]
			public void FontImageWithDynamicResourceChanged(bool useCompiledXaml)
			{
				var layout = new FontImageSourceTests(useCompiledXaml);
				var rd = layout.Resources;
				rd["PrimaryColor"] = Colors.Chartreuse;
				Assert.That((layout.staticImage.Source as FontImageSource).Color, Is.EqualTo(Colors.HotPink));
				Assert.That((layout.dynamicWithExtension.Source as FontImageSource).Color, Is.EqualTo(Colors.Chartreuse));
				Assert.That((layout.dynamicWithElement.Source as FontImageSource).Color, Is.EqualTo(Colors.Chartreuse));
			}
		}
	}
}