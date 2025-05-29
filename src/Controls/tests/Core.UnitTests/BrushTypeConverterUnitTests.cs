using System;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class BrushTypeConverterUnitTests : BaseTestFixture
	{
		private readonly BrushTypeConverter _converter = new();

		[Fact]
		public void ConvertNullTest()
		{
			var result = _converter.ConvertFromInvariantString(null);
			Assert.NotNull(result);
			var brush = Assert.IsType<SolidColorBrush>(result);
			Assert.Null(brush.Color);
		}

		[Theory]
		[InlineData("rgb(6, 201, 198)")]
		[InlineData("rgba(6, 201, 188, 0.2)")]
		[InlineData("hsl(6, 20%, 45%)")]
		[InlineData("hsla(6, 20%, 45%,0.75)")]
		[InlineData("rgb(100%, 32%, 64%)")]
		[InlineData("rgba(100%, 32%, 64%,0.27)")]
		public void TestBrushTypeConverterWithColorDefinition(string colorDefinition)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(colorDefinition));
		}

		[Theory]
		[InlineData("#ff00ff")]
		[InlineData("#00FF33")]
		[InlineData("#00FFff 40%")]
		public void TestBrushTypeConverterWithColorHex(string colorHex)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(colorHex));
		}

		[Theory]
		[InlineData("linear-gradient(90deg, rgb(255, 0, 0),rgb(255, 153, 51))")]
		[InlineData("radial-gradient(circle, rgb(255, 0, 0) 25%, rgb(0, 255, 0) 50%, rgb(0, 0, 255) 75%)")]
		public void TestBrushTypeConverterWithBrush(string brush)
		{
			Assert.True(_converter.CanConvertFrom(typeof(string)));
			Assert.NotNull(_converter.ConvertFromInvariantString(brush));
		}

		[Fact]
		public void TestBindingContextPropagation()
		{
			var context = new object();
			var linearGradientBrush = new LinearGradientBrush();

			var firstStop = new GradientStop { Offset = 0.1f, Color = Colors.Red };
			var secondStop = new GradientStop { Offset = 1.0f, Color = Colors.Blue };

			linearGradientBrush.GradientStops.Add(firstStop);
			linearGradientBrush.GradientStops.Add(secondStop);

			linearGradientBrush.BindingContext = context;

			Assert.Same(context, firstStop.BindingContext);
			Assert.Same(context, secondStop.BindingContext);
		}

		[Fact]
		public void TestBrushBindingContext()
		{
			var context = new object();

			var parent = new Grid
			{
				BindingContext = context
			};

			var linearGradientBrush = new LinearGradientBrush();

			var firstStop = new GradientStop { Offset = 0.1f, Color = Colors.Red };
			var secondStop = new GradientStop { Offset = 1.0f, Color = Colors.Blue };

			linearGradientBrush.GradientStops.Add(firstStop);
			linearGradientBrush.GradientStops.Add(secondStop);

			parent.Background = linearGradientBrush;

			Assert.Same(context, parent.Background.BindingContext);
		}

		[Fact]
		public void TestGetGradientStopHashCode()
		{
			var gradientStop = new GradientStop();
			_ = gradientStop.GetHashCode();
			// This test is just validating that calling `GetHashCode` doesn't throw
		}

		[Fact]
		public void ImmutableBrushDoesntSetParent()
		{
			var grid = new Grid();
			grid.Background = SolidColorBrush.Green;
			Assert.Null(SolidColorBrush.Green.Parent);
		}

		[Fact]
		public void InvalidOperationExceptionWhenSettingParentOnImmutableBrush()
		{
			Assert.Throws<InvalidOperationException>(() => SolidColorBrush.Green.Parent = new Grid());
		}
	}
}