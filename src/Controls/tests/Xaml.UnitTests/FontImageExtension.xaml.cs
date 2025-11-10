using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FontImageExtension : TabBar
{
	public FontImageExtension() => InitializeComponent();

	public static string FontFamily => "MyFontFamily";
	public static string Glyph => "MyGlyph";
	public static Color Color => Colors.Black;
	public static double Size = 50d;


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
		[Theory]
		[Values]
		public void FontImageExtension_Positive(XamlInflator inflator)
		{
			var layout = new FontImageExtension(inflator);
			var tabs = ((IVisualTreeElement)layout).GetVisualChildren();

			int i = 0;
			foreach (var tab in tabs)
			{
				Tab myTab = (Tab)tab;
				if (myTab == null)
					continue;

				Assert.IsType<FontImageSource>(myTab.Icon);

				var fontImage = (FontImageSource)myTab.Icon;
				Assert.Equal(FontFamily, fontImage.FontFamily);
				Assert.Equal(Glyph, fontImage.Glyph);

				if (i == 3)
					Assert.Equal(30d, fontImage.Size);
				else
					Assert.Equal(Size, fontImage.Size);

				Assert.Equal(Color, fontImage.Color);
				i++;
			}
		}

		[Theory]
		[Values]
		public void FontImageExtension_Negative(XamlInflator inflator)
		{
			var layout = new FontImageExtension(inflator);
			var tabs = ((IVisualTreeElement)layout).GetVisualChildren();

			foreach (var tab in tabs)
			{
				Tab myTab = (Tab)tab;
				if (myTab == null)
					continue;

				Assert.IsNotType<ImageSource>(myTab.Icon);
			}
		}
	}
}