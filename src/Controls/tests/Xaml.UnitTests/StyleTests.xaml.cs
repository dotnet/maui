using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.UnitTests;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class StyleTests : ContentPage
	{
		public StyleTests()
		{
			InitializeComponent();
		}

		public StyleTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Application.Current = new MockApplication();
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TestStyle(bool useCompiledXaml)
			{
				var layout = new StyleTests(useCompiledXaml);
				Assert.That(layout.style0, Is.InstanceOf<Style>());
				Assert.AreSame(layout.style0, layout.label0.Style);
				Assert.Equal("FooBar", layout.label0.Text);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TestConversionOnSetters(bool useCompiledXaml)
			{
				var layout = new StyleTests(useCompiledXaml);
				Style style = layout.style1;
				Setter setter;

				//Test built-in conversions
				setter = style.Setters.Single(s => s.Property == HeightProperty);
				Assert.That(setter.Value, Is.TypeOf<double>());
				Assert.Equal(42d, (double)setter.Value);

				//Test TypeConverters
				setter = style.Setters.Single(s => s.Property == BackgroundColorProperty);
				Assert.That(setter.Value, Is.TypeOf<Color>());
				Assert.Equal(Colors.Pink, (Color)setter.Value);

				//Test implicit cast operator
				setter = style.Setters.Single(s => s.Property == Image.SourceProperty);
				Assert.That(setter.Value, Is.TypeOf<FileImageSource>());
				Assert.Equal("foo.png", ((FileImageSource)setter.Value).File);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ImplicitStyleAreApplied(bool useCompiledXaml)
			{
				var layout = new StyleTests(useCompiledXaml);
				Assert.Equal(Colors.Red, layout.label1.TextColor);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void PropertyDoesNotNeedTypes(bool useCompiledXaml)
			{
				var layout = new StyleTests(useCompiledXaml);
				Style style2 = layout.style2;
				var s0 = style2.Setters[0];
				var s1 = style2.Setters[1];
				Assert.Equal(Label.TextProperty, s0.Property);
				Assert.Equal(BackgroundColorProperty, s1.Property);
				Assert.Equal(Colors.Red, s1.Value);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			//issue #2406
			public void StylesDerivedFromDynamicStylesThroughStaticResource(bool useCompiledXaml)
			{
				var layout = new StyleTests(useCompiledXaml);
				Application.Current.LoadPage(layout);

				var label = layout.labelWithStyleDerivedFromDynamic_StaticResource;

				Assert.Equal(50, label.FontSize);
				Assert.Equal(Colors.Red, label.TextColor);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			//issue #2406
			public void StylesDerivedFromDynamicStylesThroughDynamicResource(bool useCompiledXaml)
			{
				var layout = new StyleTests(useCompiledXaml);
				Application.Current.LoadPage(layout);

				var label = layout.labelWithStyleDerivedFromDynamic_DynamicResource;

				Assert.Equal(50, label.FontSize);
				Assert.Equal(Colors.Red, label.TextColor);
			}
		}
	}
}