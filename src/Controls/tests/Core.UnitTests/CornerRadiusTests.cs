using System;
using Microsoft.Maui.Converters;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class CornerRadiusTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var cornerRadius = new CornerRadius();

			Assert.Equal(0, cornerRadius.TopLeft);
			Assert.Equal(0, cornerRadius.TopRight);
			Assert.Equal(0, cornerRadius.BottomLeft);
			Assert.Equal(0, cornerRadius.BottomRight);
		}

		[Fact]
		public void UniformParameterizedConstructor()
		{
			var cornerRadius = new CornerRadius(3);

			Assert.Equal(3, cornerRadius.TopLeft);
			Assert.Equal(3, cornerRadius.TopRight);
			Assert.Equal(3, cornerRadius.BottomLeft);
			Assert.Equal(3, cornerRadius.BottomRight);
		}

		[Fact]
		public void ParameterizedConstructor()
		{
			var cornerRadius = new CornerRadius(1, 2, 3, 4);

			Assert.Equal(1, cornerRadius.TopLeft);
			Assert.Equal(2, cornerRadius.TopRight);
			Assert.Equal(3, cornerRadius.BottomLeft);
			Assert.Equal(4, cornerRadius.BottomRight);
		}

		[Fact]
		public void ParameterizedConstuctorDoubles()
		{
			var cornerRadius = new CornerRadius(1.2, 3.3, 4.2, 10.66);
			Assert.Equal(1.2, cornerRadius.TopLeft);
			Assert.Equal(3.3, cornerRadius.TopRight);
			Assert.Equal(4.2, cornerRadius.BottomLeft);
			Assert.Equal(10.66, cornerRadius.BottomRight);
		}

		[Fact]
		public void Equality()
		{
			Assert.False(new CornerRadius().Equals(null));
			Assert.False(new CornerRadius().Equals("CornerRadius"));
			Assert.False(new CornerRadius().Equals(new CornerRadius(1, 2, 3, 4)));
			Assert.True(new CornerRadius().Equals(new CornerRadius()));

			Assert.True(new CornerRadius() == new CornerRadius());
			Assert.True(new CornerRadius(4, 3, 2, 1) != new CornerRadius(1, 2, 3, 4));
		}

		[Theory, MemberData(nameof(TestDataHelpers.Range), 3, 4, 8, MemberType = typeof(TestDataHelpers))]
		public void HashCode(double l1, double t1, double r1, double b1,
							  double l2, double t2, double r2, double b2)
		{
			bool result = new CornerRadius(l1, t1, r1, b1).GetHashCode() == new CornerRadius(l2, t2, r2, b2).GetHashCode();
			if (l1 == l2 && t1 == t2 && r1 == r2 && b1 == b2)
				Assert.True(result);
			else
				Assert.False(result);
		}

		[Fact]
		public void TestThicknessTypeConverter()
		{
			var converter = new CornerRadiusTypeConverter();
			Assert.True(converter.CanConvertFrom(typeof(string)));
			Assert.Equal(new CornerRadius(1), converter.ConvertFromInvariantString("1"));
			Assert.Equal(new CornerRadius(1), converter.ConvertFromInvariantString("1, 2"));
			Assert.Equal(new CornerRadius(1), converter.ConvertFromInvariantString("1, 2, 3"));
			Assert.Equal(new CornerRadius(1, 2, 3, 4), converter.ConvertFromInvariantString("1, 2, 3, 4"));
			Assert.Equal(new CornerRadius(1.1), converter.ConvertFromInvariantString("1.1,2"));
			Assert.Throws<InvalidOperationException>(() => converter.ConvertFromInvariantString(""));
		}

		[Fact]
		public void ThicknessTypeConverterDoubles()
		{
			var converter = new CornerRadiusTypeConverter();
			Assert.Equal(new CornerRadius(1.3), converter.ConvertFromInvariantString("1.3"));
			Assert.Equal(new CornerRadius(1.4), converter.ConvertFromInvariantString("1.4, 2.8"));
			Assert.Equal(new CornerRadius(1.2), converter.ConvertFromInvariantString("1.2, 3.4,2"));
			Assert.Equal(new CornerRadius(1.6, 2.1, 3.8, 4.2), converter.ConvertFromInvariantString(" 1.6 , 2.1, 3.8, 4.2"));
			Assert.Equal(new CornerRadius(1.1), converter.ConvertFromInvariantString("1.1,2"));
		}
	}
}
