using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh12025NavPage : NavigationPage
{
	public static new readonly BindableProperty IconColorProperty = BindableProperty.CreateAttached("IconColor", typeof(Color), typeof(Page), null);
	public static void SetIconColor(Page page, Color barTintColor) => page.SetValue(IconColorProperty, barTintColor);
	public static Color GetIconColor(Page page) => (Color)page.GetValue(IconColorProperty);
}

public partial class Gh12025 : ContentPage
{
	public Gh12025() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void FindMostDerivedABP(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				Assert.Null(Record.Exception(() => MockCompiler.Compile(typeof(Gh12025))));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class Gh12025NavPage : NavigationPage
{
	public static new readonly BindableProperty IconColorProperty = BindableProperty.CreateAttached("IconColor", typeof(Color), typeof(Page), null);
	public static void SetIconColor(Page page, Color barTintColor) => page.SetValue(IconColorProperty, barTintColor);
	public static Color GetIconColor(Page page) => (Color)page.GetValue(IconColorProperty);
}

public partial class Gh12025 : ContentPage
{
	public Gh12025() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Gh12025));
				Assert.Empty(result.Diagnostics);
			}
			var layout = new Gh12025(inflator);
			Assert.Equal(NavigationPage.IconColorProperty.DefaultValue, NavigationPage.GetIconColor(layout));
			Assert.Equal(Colors.HotPink, Gh12025NavPage.GetIconColor(layout));
		}
	}
}