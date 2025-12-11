using System;
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleTests : ContentPage
{
	public StyleTests() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IClassFixture<ApplicationFixture>
	{
		public Tests(ApplicationFixture fixture) { }

		[Theory]
		[XamlInflatorData]
		internal void TestStyle(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Assert.IsType<Style>(layout.style0);
			Assert.Same(layout.style0, layout.label0.Style);
			Assert.Equal("FooBar", layout.label0.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestConversionOnSetters(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Style style = layout.style1;
			Setter setter;

			//Test built-in conversions
			setter = style.Setters.Single(s => s.Property == HeightProperty);
			Assert.IsType<double>(setter.Value);
			Assert.Equal(42d, (double)setter.Value);

			//Test TypeConverters
			setter = style.Setters.Single(s => s.Property == BackgroundColorProperty);
			Assert.IsType<Color>(setter.Value);
			Assert.Equal(Colors.Pink, (Color)setter.Value);

			//Test implicit cast operator
			setter = style.Setters.Single(s => s.Property == Image.SourceProperty);
			Assert.IsType<FileImageSource>(setter.Value);
			Assert.Equal("foo.png", ((FileImageSource)setter.Value).File);
		}

		[Theory]
		[XamlInflatorData]
		internal void ImplicitStyleAreApplied(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Assert.Equal(Colors.Red, layout.label1.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		internal void PropertyDoesNotNeedTypes(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Style style2 = layout.style2;
			var s0 = style2.Setters[0];
			var s1 = style2.Setters[1];
			Assert.Equal(Label.TextProperty, s0.Property);
			Assert.Equal(BackgroundColorProperty, s1.Property);
			Assert.Equal(Colors.Red, s1.Value);
		}

		[Theory]
		[XamlInflatorData]
		//issue #2406
		internal void StylesDerivedFromDynamicStylesThroughStaticResource(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Application.Current.LoadPage(layout);

			var label = layout.labelWithStyleDerivedFromDynamic_StaticResource;

			Assert.Equal(50, label.FontSize);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Theory]
		[XamlInflatorData]
		//issue #2406
		internal void StylesDerivedFromDynamicStylesThroughDynamicResource(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Application.Current.LoadPage(layout);

			var label = layout.labelWithStyleDerivedFromDynamic_DynamicResource;

			Assert.Equal(50, label.FontSize);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		internal void StyleCtorIsInvokedWithType()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleTests : ContentPage
{
	public StyleTests() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(StyleTests));
			Assert.False(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			Assert.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Label))", initComp, StringComparison.InvariantCulture);
		}
	}
}