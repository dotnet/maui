using System;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ThicknessTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var thickness = new Thickness();

			Assert.Equal(0, thickness.Left);
			Assert.Equal(0, thickness.Top);
			Assert.Equal(0, thickness.Right);
			Assert.Equal(0, thickness.Bottom);
			Assert.Equal(0, thickness.HorizontalThickness);
			Assert.Equal(0, thickness.VerticalThickness);
		}

		[Fact]
		public void UniformParameterizedConstructor()
		{
			var thickness = new Thickness(3);

			Assert.Equal(3, thickness.Left);
			Assert.Equal(3, thickness.Top);
			Assert.Equal(3, thickness.Right);
			Assert.Equal(3, thickness.Bottom);
			Assert.Equal(6, thickness.HorizontalThickness);
			Assert.Equal(6, thickness.VerticalThickness);
		}

		[Fact]
		public void HorizontalVerticalParameterizedConstructor()
		{
			var thickness = new Thickness(4, 5);

			Assert.Equal(4, thickness.Left);
			Assert.Equal(5, thickness.Top);
			Assert.Equal(4, thickness.Right);
			Assert.Equal(5, thickness.Bottom);
			Assert.Equal(8, thickness.HorizontalThickness);
			Assert.Equal(10, thickness.VerticalThickness);
		}

		[Fact]
		public void ParameterizedConstructor()
		{
			var thickness = new Thickness(1, 2, 3, 4);

			Assert.Equal(1, thickness.Left);
			Assert.Equal(2, thickness.Top);
			Assert.Equal(3, thickness.Right);
			Assert.Equal(4, thickness.Bottom);
			Assert.Equal(4, thickness.HorizontalThickness);
			Assert.Equal(6, thickness.VerticalThickness);
		}

		[Fact]
		public void ParameterizedConstuctorDoubles()
		{
			var thickness = new Thickness(1.2, 3.3, 4.2, 10.66);
			Assert.Equal(1.2, thickness.Left);
			Assert.Equal(3.3, thickness.Top);
			Assert.Equal(4.2, thickness.Right);
			Assert.Equal(10.66, thickness.Bottom);
			Assert.Equal(5.4, thickness.HorizontalThickness);
			Assert.Equal(13.96, thickness.VerticalThickness);
		}

		[Fact]
		public void Equality()
		{
			Assert.False(new Thickness().Equals(null));
			Assert.False(new Thickness().Equals("Thickness"));
			Assert.False(new Thickness().Equals(new Thickness(1, 2, 3, 4)));
			Assert.True(new Thickness().Equals(new Thickness()));

			Assert.True(new Thickness() == new Thickness());
			Assert.True(new Thickness(4, 3, 2, 1) != new Thickness(1, 2, 3, 4));
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 3, 4, 8, MemberType = typeof(TestDataHelpers))]
		public void HashCode(double l1, double t1, double r1, double b1,
							  double l2, double t2, double r2, double b2)
		{
			bool result = new Thickness(l1, t1, r1, b1).GetHashCode() == new Thickness(l2, t2, r2, b2).GetHashCode();
			if (l1 == l2 && t1 == t2 && r1 == r2 && b1 == b2)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Fact]
		public void ImplicitConversionFromSize()
		{
			Thickness thickness = new Thickness();
			thickness = new Size(42, 84);
			Assert.Equal(new Thickness(42, 84), thickness);

			thickness = 42;
			Assert.Equal(new Thickness(42), thickness);
		}

		[Fact]
		public void TestThicknessTypeConverter()
		{
			var converter = new ThicknessTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.Equal(new Thickness(1), converter.ConvertFromInvariantString("1"));
			Assert.Equal(new Thickness(1, 2), converter.ConvertFromInvariantString("1, 2"));
			Assert.Equal(new Thickness(1, 2, 3, 4), converter.ConvertFromInvariantString("1, 2, 3, 4"));
			Assert.Equal(new Thickness(1.1, 2), converter.ConvertFromInvariantString("1.1,2"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(""));
		}

		[Fact]
		public void ThicknessTypeConverterDoubles()
		{
			var converter = new ThicknessTypeConverter();
			Assert.Equal(new Thickness(1.3), converter.ConvertFromInvariantString("1.3"));
			Assert.Equal(new Thickness(1.4, 2.8), converter.ConvertFromInvariantString("1.4, 2.8"));
			Assert.Equal(new Thickness(1.6, 2.1, 3.8, 4.2), converter.ConvertFromInvariantString(" 1.6 , 2.1, 3.8, 4.2"));
			Assert.Equal(new Thickness(1.1, 2), converter.ConvertFromInvariantString("1.1,2"));
		}
	}
}
