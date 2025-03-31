using System;
using Microsoft.Maui.Controls.Shapes;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class StrokeShapeTests : BaseTestFixture
	{
		StrokeShapeTypeConverter _strokeShapeTypeConverter;

		public StrokeShapeTests()
		{
			_strokeShapeTypeConverter = new StrokeShapeTypeConverter();
		}

		[Theory]
		[InlineData("rectangle")]
		[InlineData("Rectangle")]
		public void TestRectangleConstructor(string value)
		{
			Rectangle rectangle = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Rectangle;

			Assert.NotNull(rectangle);
		}

		[Theory]
		[InlineData("roundRectangle")]
		[InlineData("RoundRectangle")]
		[InlineData("roundRectangle 12")]
		[InlineData("roundRectangle 12, 6, 24, 36")]
		[InlineData("roundRectangle 12, 12, 24, 12")]
		[InlineData("RoundRectangle 12")]
		[InlineData("RoundRectangle 12, 6, 24, 36")]
		[InlineData("RoundRectangle 12, 12, 24, 12")]
		public void TestRoundRectangleConstructor(string value)
		{
			RoundRectangle roundRectangle = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as RoundRectangle;

			Assert.NotNull(roundRectangle);

			if (!string.Equals("roundRectangle", value, StringComparison.OrdinalIgnoreCase))
			{
				Assert.NotEqual(0, roundRectangle.CornerRadius.TopLeft);
				Assert.NotEqual(0, roundRectangle.CornerRadius.TopRight);
				Assert.NotEqual(0, roundRectangle.CornerRadius.BottomLeft);
				Assert.NotEqual(0, roundRectangle.CornerRadius.BottomRight);
			}
		}

		[Theory]
		[InlineData("path")]
		[InlineData("Path")]
		[InlineData("path M8.4580019,25.5C8.4580019,26.747002 10.050002,27.758995 12.013003,27.758995 13.977001,27.758995 15.569004,26.747002 15.569004,25.5z M19.000005,10C16.861005,9.9469986 14.527004,12.903999 14.822002,22.133995 14.822002,22.133995 26.036002,15.072998 20.689,10.681999 20.183003,10.265999 19.599004,10.014999 19.000005,10z M4.2539991,10C3.6549998,10.014999 3.0710002,10.265999 2.5649996,10.681999 -2.7820019,15.072998 8.4320009,22.133995 8.4320009,22.133995 8.7270001,12.903999 6.3929995,9.9469986 4.2539991,10z M11.643,0C18.073003,0 23.286002,5.8619995 23.286002,13.091995 23.286002,20.321999 18.684003,32 12.254,32 5.8239992,32 1.8224728E-07,20.321999 0,13.091995 1.8224728E-07,5.8619995 5.2129987,0 11.643,0z")]
		[InlineData("path M16.484421,0.73799322C20.831404,0.7379931 24.353395,1.1259904 24.353395,1.6049905 24.353395,2.0839829 20.831404,2.4719803 16.484421,2.47198 12.138443,2.4719803 8.6154527,2.0839829 8.6154527,1.6049905 8.6154527,1.1259904 12.138443,0.7379931 16.484421,0.73799322z M1.9454784,0.061995983C2.7564723,5.2449602 12.246436,11.341911 12.246436,11.341911 13.248431,19.240842 9.6454477,17.915854 9.6454477,17.915854 7.9604563,18.897849 6.5314603,17.171859 6.5314603,17.171859 4.1084647,18.29585 3.279473,15.359877 3.2794733,15.359877 0.82348057,15.291876 1.2804796,11.362907 1.2804799,11.362907 -1.573514,10.239915 1.2344746,6.3909473 1.2344746,6.3909473 -1.3255138,4.9869594 1.9454782,0.061996057 1.9454784,0.061995983z M30.054371,0C30.054371,9.8700468E-08 33.325355,4.9249634 30.765367,6.3289513 30.765367,6.3289513 33.574364,10.177919 30.71837,11.30191 30.71837,11.30191 31.175369,15.22988 28.721384,15.297872 28.721384,15.297872 27.892376,18.232854 25.468389,17.110862 25.468389,17.110862 24.040392,18.835847 22.355402,17.853852 22.355402,17.853852 18.752417,19.178845 19.753414,11.279907 19.753414,11.279907 29.243385,5.1829566 30.054371,0z")]
		[InlineData("Path M8.4580019,25.5C8.4580019,26.747002 10.050002,27.758995 12.013003,27.758995 13.977001,27.758995 15.569004,26.747002 15.569004,25.5z M19.000005,10C16.861005,9.9469986 14.527004,12.903999 14.822002,22.133995 14.822002,22.133995 26.036002,15.072998 20.689,10.681999 20.183003,10.265999 19.599004,10.014999 19.000005,10z M4.2539991,10C3.6549998,10.014999 3.0710002,10.265999 2.5649996,10.681999 -2.7820019,15.072998 8.4320009,22.133995 8.4320009,22.133995 8.7270001,12.903999 6.3929995,9.9469986 4.2539991,10z M11.643,0C18.073003,0 23.286002,5.8619995 23.286002,13.091995 23.286002,20.321999 18.684003,32 12.254,32 5.8239992,32 1.8224728E-07,20.321999 0,13.091995 1.8224728E-07,5.8619995 5.2129987,0 11.643,0z")]
		[InlineData("Path M16.484421,0.73799322C20.831404,0.7379931 24.353395,1.1259904 24.353395,1.6049905 24.353395,2.0839829 20.831404,2.4719803 16.484421,2.47198 12.138443,2.4719803 8.6154527,2.0839829 8.6154527,1.6049905 8.6154527,1.1259904 12.138443,0.7379931 16.484421,0.73799322z M1.9454784,0.061995983C2.7564723,5.2449602 12.246436,11.341911 12.246436,11.341911 13.248431,19.240842 9.6454477,17.915854 9.6454477,17.915854 7.9604563,18.897849 6.5314603,17.171859 6.5314603,17.171859 4.1084647,18.29585 3.279473,15.359877 3.2794733,15.359877 0.82348057,15.291876 1.2804796,11.362907 1.2804799,11.362907 -1.573514,10.239915 1.2344746,6.3909473 1.2344746,6.3909473 -1.3255138,4.9869594 1.9454782,0.061996057 1.9454784,0.061995983z M30.054371,0C30.054371,9.8700468E-08 33.325355,4.9249634 30.765367,6.3289513 30.765367,6.3289513 33.574364,10.177919 30.71837,11.30191 30.71837,11.30191 31.175369,15.22988 28.721384,15.297872 28.721384,15.297872 27.892376,18.232854 25.468389,17.110862 25.468389,17.110862 24.040392,18.835847 22.355402,17.853852 22.355402,17.853852 18.752417,19.178845 19.753414,11.279907 19.753414,11.279907 29.243385,5.1829566 30.054371,0z")]
		public void TestPathConstructor(string value)
		{
			Path path = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Path;

			Assert.NotNull(path);
			if (!string.Equals("path", value, StringComparison.OrdinalIgnoreCase))
			{
				Assert.NotNull(path.Data);
			}
		}

		[Theory]
		[InlineData("polygon")]
		[InlineData("Polygon")]
		[InlineData("polygon 10,110 60,10 110,110")]
		[InlineData("polygon 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
		[InlineData("Polygon 10,110 60,10 110,110")]
		[InlineData("Polygon 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
		public void TestPolygonConstructor(string value)
		{
			Polygon polygon = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Polygon;

			Assert.NotNull(polygon);
			if (!string.Equals("polygon", value, StringComparison.OrdinalIgnoreCase))
			{
				Assert.NotEmpty(polygon.Points);
			}
		}

		[Theory]
		[InlineData("line")]
		[InlineData("Line")]
		[InlineData("line 1 2")]
		[InlineData("Line 1 2 3 4")]
		public void TestLineConstructor(string value)
		{
			Line line = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Line;

			Assert.NotNull(line);
			if (!string.Equals("line", value, StringComparison.OrdinalIgnoreCase))
			{
				Assert.True(line.X1 != 0);
				Assert.True(line.Y1 != 0);
			}
		}

		[Theory]
		[InlineData("polyline")]
		[InlineData("Polyline")]
		[InlineData("polyline 10,110 60,10 110,110")]
		[InlineData("polyline 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
		[InlineData("Polyline 10,110 60,10 110,110")]
		[InlineData("Polyline 0 48, 0 144, 96 150, 100 0, 192 0, 192 96, 50 96, 48 192, 150 200 144 48")]
		public void TestPolylineConstructor(string value)
		{
			Polyline polyline = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Polyline;

			Assert.NotNull(polyline);
			if (!string.Equals("polyline", value, StringComparison.OrdinalIgnoreCase))
			{
				Assert.NotEmpty(polyline.Points);
			}
		}

		[Theory]
		[InlineData("ellipse")]
		[InlineData("Ellipse")]
		public void TestEllipseConstructor(string value)
		{
			Ellipse ellipse = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as Ellipse;

			Assert.NotNull(ellipse);
		}

		[Theory]
		[InlineData("20")]
		public void TestRoundRectangleSingleValue(string value)
		{
			RoundRectangle roundRectangle = _strokeShapeTypeConverter.ConvertFromInvariantString(value) as RoundRectangle;

			Assert.NotNull(roundRectangle);
			Assert.NotEqual(0, roundRectangle.CornerRadius.TopLeft);
			Assert.NotEqual(0, roundRectangle.CornerRadius.TopRight);
			Assert.NotEqual(0, roundRectangle.CornerRadius.BottomLeft);
			Assert.NotEqual(0, roundRectangle.CornerRadius.BottomRight);
		}
	}
}