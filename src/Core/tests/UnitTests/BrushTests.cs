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
	}
}
