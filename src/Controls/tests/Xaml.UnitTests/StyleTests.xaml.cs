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

	public class Tests
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void SetUp() => Application.Current = new MockApplication();

		[Theory]
		[Values]
		public void TestStyle(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Assert.IsType<Style>(layout.style0);
			Assert.Same(layout.style0, layout.label0.Style);
			Assert.Equal("FooBar", layout.label0.Text);
		}

		[Theory]
		[Values]
		public void TestConversionOnSetters(XamlInflator inflator)
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
		[Values]
		public void ImplicitStyleAreApplied(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Assert.Equal(Colors.Red, layout.label1.TextColor);
		}

		[Theory]
		[Values]
		public void PropertyDoesNotNeedTypes(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Style style2 = layout.style2;
			var s0 = style2.Setters[0];
			var s1 = style2.Setters[1];
			Assert.Equal(Label.TextProperty, s0.Property);
			Assert.Equal(BackgroundColorProperty, s1.Property);
			Assert.Equal(Colors.Red, s1.Value);
		}

		[Fact]
		//issue #2406
		public void StylesDerivedFromDynamicStylesThroughStaticResource(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Application.Current.LoadPage(layout);

			var label = layout.labelWithStyleDerivedFromDynamic_StaticResource;

			Assert.Equal(50, label.FontSize);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		//issue #2406
		public void StylesDerivedFromDynamicStylesThroughDynamicResource(XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Application.Current.LoadPage(layout);

			var label = layout.labelWithStyleDerivedFromDynamic_DynamicResource;

			Assert.Equal(50, label.FontSize);
			Assert.Equal(Colors.Red, label.TextColor);
		}

		[Fact]
		public void StyleCtorIsInvokedWithType() // TODO: Fix parameters - see comment above] XamlInflator inflator)
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleTests : ContentPage
{
	public StyleTests() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(StyleTests));
			Assert.False(result.Diagnostics.Any());
			var initComp = result.GeneratedInitializeComponent();
			Assert.True(initComp.Contains("new global::Microsoft.Maui.Controls.Style(typeof(global::Microsoft.Maui.Controls.Label))", StringComparison.InvariantCulture));
		}
	}
}