using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class LazyResourceDictionary : ContentPage
{
	public LazyResourceDictionary() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Fact]
		public void SharedResourceReturnsSameInstance()
		{
			// This test only works with SourceGen (the only inflator that supports LazyRD)
			var page = new LazyResourceDictionary();

			// Verify keyed style resource is accessible
			Assert.True(page.Resources.TryGetValue("MyLabelStyle", out var style));
			Assert.IsType<Style>(style);
			Assert.Equal(typeof(Label), ((Style)style).TargetType);

			// Accessing the same key returns the same instance (shared=true is default)
			Assert.True(page.Resources.TryGetValue("MyLabelStyle", out var style2));
			Assert.Same(style, style2);
		}

		[Fact]
		public void NamedElementsStillWork()
		{
			var page = new LazyResourceDictionary();

			// Verify named elements work
			Assert.NotNull(page.label1);
			Assert.Equal("Test Label", page.label1.Text);
		}

		[Fact]
		public void StaticResourceBindingWorks()
		{
			var page = new LazyResourceDictionary();

			// Verify the label has the style applied via StaticResource
			Assert.NotNull(page.label1.Style);
			Assert.Equal(typeof(Label), page.label1.Style.TargetType);
		}

		[Fact]
		public void ColorResourceWorks()
		{
			var page = new LazyResourceDictionary();

			// Verify color resource is accessible (lazy)
			Assert.True(page.Resources.TryGetValue("PrimaryColor", out var color));
			Assert.IsType<Color>(color);
			Assert.Equal(Colors.Blue, color);
		}

		[Fact]
		public void ImplicitStyleIsInResourceDictionary()
		{
			var page = new LazyResourceDictionary();

			// Verify implicit style is in the resource dictionary
			// Implicit styles use the TargetType.FullName as the key
			Assert.True(page.Resources.TryGetValue(typeof(Button).FullName, out var implicitStyle));
			Assert.IsType<Style>(implicitStyle);
			Assert.Equal(typeof(Button), ((Style)implicitStyle).TargetType);
		}

		[Fact]
		public void NonSharedResourceReturnsNewInstanceEachTime()
		{
			var page = new LazyResourceDictionary();

			// Access the x:Shared="false" resource multiple times
			Assert.True(page.Resources.TryGetValue("Handle", out var obj1));
			Assert.True(page.Resources.TryGetValue("Handle", out var obj2));
			Assert.True(page.Resources.TryGetValue("Handle", out var obj3));

			// Each access should return a NEW instance
			Assert.NotSame(obj1, obj2);
			Assert.NotSame(obj2, obj3);
			Assert.NotSame(obj1, obj3);
		}
	}
}

