using Microsoft.Maui.Graphics;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void FindMostDerivedABP([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Gh12025)));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
using Microsoft.Maui.Graphics;
using NUnit.Framework;

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
				Assert.That(result.Diagnostics, Is.Empty);
			}
			var layout = new Gh12025(inflator);
			Assert.That(NavigationPage.GetIconColor(layout), Is.EqualTo(NavigationPage.IconColorProperty.DefaultValue));
			Assert.That(Gh12025NavPage.GetIconColor(layout), Is.EqualTo(Colors.HotPink));
		}
	}
}