using System.Reflection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class BrushTests
	{
		[Fact]
		public void BrushColorsAreImmutable()
		{
			Brush.Red.Color = Colors.Blue;
			Assert.Equal(Brush.Red.Color, Colors.Red);

			Brush.Red.SetValue(SolidColorBrush.ColorProperty, Colors.Blue);
			Assert.Equal(Brush.Red.Color, Colors.Red);
		}

		[Fact]
		public void BrushValuesMatchColorValues()
		{
			//Brush.Red property is a SolidColorBrush that maps to Colors.Red field

			var flags = BindingFlags.Static | BindingFlags.Public;
			foreach (var property in typeof(Brush).GetProperties(flags))
			{
				if (property.PropertyType != typeof(SolidColorBrush))
					continue;
				var brush = (SolidColorBrush)property.GetValue(null);
				Assert.NotNull(brush);
				var expected = typeof(Colors).GetField(property.Name, flags);
				Assert.NotNull(expected);
				Assert.Equal((Color)expected.GetValue(null), brush.Color);
			}
		}

		[Fact]
		public void SolidColorBrushComparison()
		{
			var brush1 = new SolidColorBrush(Colors.Red);
			var brush2 = new SolidColorBrush(Colors.Red);

			Assert.False(brush1 == brush2);
			Assert.True(brush1.Equals(brush2));
			Assert.True(brush1.Color == brush2.Color);
		}

		[Fact]
		public void LinearGradientBrushComparison()
		{
			var brush1 = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, },
					new GradientStop { Color = Colors.Green }
				}
			};
			var brush2 = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, },
					new GradientStop { Color = Colors.Green }
				}
			};

			Assert.False(brush1 == brush2);
			Assert.True(brush1.Equals(brush2));
			Assert.True(brush1.GradientStops[0].Color == brush2.GradientStops[0].Color);
			Assert.True(brush1.GradientStops[1].Color == brush2.GradientStops[1].Color);
		}

		[Fact]
		public void RadialGradientBrushComparison()
		{
			var brush1 = new RadialGradientBrush
			{
				Center = new Point(0.5, 0.5),
				Radius = 10,
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, },
					new GradientStop { Color = Colors.Green }
				}
			};
			var brush2 = new RadialGradientBrush
			{
				Center = new Point(0.5, 0.5),
				Radius = 10,
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, },
					new GradientStop { Color = Colors.Green }
				}
			};

			Assert.False(brush1 == brush2);
			Assert.True(brush1.Equals(brush2));
			Assert.True(brush1.GradientStops[0].Color == brush2.GradientStops[0].Color);
			Assert.True(brush1.GradientStops[1].Color == brush2.GradientStops[1].Color);
		}
	}
}
