using System.Linq;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class StyleTests : ContentPage
{
	public StyleTests() => InitializeComponent();

	class Tests
	{
		[SetUp] public void SetUp() => Application.Current = new MockApplication();

		[Test]
		public void TestStyle([Values] XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Assert.That(layout.style0, Is.InstanceOf<Style>());
			Assert.AreSame(layout.style0, layout.label0.Style);
			Assert.AreEqual("FooBar", layout.label0.Text);
		}

		[Test]
		public void TestConversionOnSetters([Values] XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Style style = layout.style1;
			Setter setter;

			//Test built-in conversions
			setter = style.Setters.Single(s => s.Property == HeightProperty);
			Assert.That(setter.Value, Is.TypeOf<double>());
			Assert.AreEqual(42d, (double)setter.Value);

			//Test TypeConverters
			setter = style.Setters.Single(s => s.Property == BackgroundColorProperty);
			Assert.That(setter.Value, Is.TypeOf<Color>());
			Assert.AreEqual(Colors.Pink, (Color)setter.Value);

			//Test implicit cast operator
			setter = style.Setters.Single(s => s.Property == Image.SourceProperty);
			Assert.That(setter.Value, Is.TypeOf<FileImageSource>());
			Assert.AreEqual("foo.png", ((FileImageSource)setter.Value).File);
		}

		[Test]
		public void ImplicitStyleAreApplied([Values] XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Assert.AreEqual(Colors.Red, layout.label1.TextColor);
		}

		[Test]
		public void PropertyDoesNotNeedTypes([Values] XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Style style2 = layout.style2;
			var s0 = style2.Setters[0];
			var s1 = style2.Setters[1];
			Assert.AreEqual(Label.TextProperty, s0.Property);
			Assert.AreEqual(BackgroundColorProperty, s1.Property);
			Assert.AreEqual(Colors.Red, s1.Value);
		}

		[Test]
		//issue #2406
		public void StylesDerivedFromDynamicStylesThroughStaticResource([Values] XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Application.Current.LoadPage(layout);

			var label = layout.labelWithStyleDerivedFromDynamic_StaticResource;

			Assert.AreEqual(50, label.FontSize);
			Assert.AreEqual(Colors.Red, label.TextColor);
		}

		[Test]
		//issue #2406
		public void StylesDerivedFromDynamicStylesThroughDynamicResource([Values] XamlInflator inflator)
		{
			var layout = new StyleTests(inflator);
			Application.Current.LoadPage(layout);

			var label = layout.labelWithStyleDerivedFromDynamic_DynamicResource;

			Assert.AreEqual(50, label.FontSize);
			Assert.AreEqual(Colors.Red, label.TextColor);
		}
	}
}