using System;
using Xunit;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.UnitTests.Layouts
{
	[Category(TestCategory.Layout)]
	public class DensityValueTests
	{
		[Fact]
		public void Constructor_SetsPropertiesCorrectly()
		{
			var density = 2.625;
			var dp = 100.0;
			var value = new DensityValue(dp, density);

			Assert.Equal(dp, value.Dp);
			Assert.Equal(density, value.Density);
			Assert.Equal(dp * density, value.RawPx);
			Assert.Equal((int)Math.Round(dp * density), (int)Math.Round(value.RawPx));
		}

		[Fact]
		public void FromPixels_CalculatesDpCorrectly()
		{
			var pixels = 262.5;
			var density = 2.625;
			var expectedDp = pixels / density; // 100.0

			var value = DensityValue.FromPixels(pixels, density);

			Assert.Equal(expectedDp, value.Dp, precision: 5);
			Assert.Equal(density, value.Density);
		}

		[Fact]
		public void Addition_WorksWithSameDensity()
		{
			var value1 = new DensityValue(50.0, 2.0);
			var value2 = new DensityValue(30.0, 2.0);

			var result = value1 + value2;

			Assert.Equal(80.0, result.Dp);
			Assert.Equal(2.0, result.Density);
		}

		[Fact]
		public void Addition_ThrowsWithDifferentDensities()
		{
			var value1 = new DensityValue(50.0, 2.0);
			var value2 = new DensityValue(30.0, 3.0);

			Assert.Throws<ArgumentException>(() => value1 + value2);
		}

		[Fact]
		public void Multiplication_WorksCorrectly()
		{
			var value = new DensityValue(100.0, 2.5);
			var scalar = 1.5;

			var result = value * scalar;

			Assert.Equal(150.0, result.Dp);
			Assert.Equal(2.5, result.Density);
		}

		[Fact]
		public void ImplicitConversion_ReturnsDp()
		{
			var value = new DensityValue(123.45, 2.0);
			double dp = value;

			Assert.Equal(123.45, dp);
		}

		[Theory]
		[InlineData(770.175, 2.625, new double[] { 1, 1, 1 }, new int[] { 256, 257, 257 })]
		[InlineData(870.0, 3.0, new double[] { 1, 1, 1 }, new int[] { 290, 290, 290 })]
		[InlineData(787.5, 2.625, new double[] { 1, 1, 1, 1 }, new int[] { 196, 197, 197, 197 })]
		public void DistributePixels_HandlesRoundingCorrectly(double totalPixels, double density, double[] portions, int[] expected)
		{
			var result = DensityValue.DistributePixels(totalPixels, density, portions);

			Assert.Equal(expected.Length, result.Length);
			for (int i = 0; i < expected.Length; i++)
			{
				Assert.Equal(expected[i], result[i]);
			}

			// Verify that the total adds up correctly 
			var sum = 0;
			foreach (var value in result)
			{
				sum += value;
			}
			
			// The total should be Math.Floor(totalPixels) for right-to-left distribution
			var expectedTotal = (int)Math.Floor(totalPixels);
			Assert.Equal(expectedTotal, sum);
		}

		[Fact]
		public void DistributePixels_HandlesWeightedPortions()
		{
			// 100 pixels distributed as 2*, 1*, 2* (5 total weight)
			var totalPixels = 100.0;
			var density = 1.0;
			var portions = new double[] { 2.0, 1.0, 2.0 };

			var result = DensityValue.DistributePixels(totalPixels, density, portions);

			// Expected: 40, 20, 40
			Assert.Equal(40, result[0]);
			Assert.Equal(20, result[1]);
			Assert.Equal(40, result[2]);
		}

		[Fact]
		public void DistributePixels_HandlesEmptyArray()
		{
			var result = DensityValue.DistributePixels(100.0, 1.0, Array.Empty<double>());
			Assert.Empty(result);
		}

		[Fact]
		public void DistributePixels_HandlesZeroPortions()
		{
			var portions = new double[] { 0.0, 0.0, 0.0 };
			var result = DensityValue.DistributePixels(100.0, 1.0, portions);

			Assert.Equal(3, result.Length);
			Assert.All(result, value => Assert.Equal(0, value));
		}

		/// <summary>
		/// Tests the specific scenario from the issue: 293.4dp across 3 columns at density 2.625
		/// </summary>
		[Fact]
		public void DistributePixels_IssueScenario1()
		{
			// 293.4dp * 2.625 = 770.175px across 3 equal columns
			var totalDp = 293.4;
			var density = 2.625;
			var totalPixels = totalDp * density; // 770.175
			var portions = new double[] { 1.0, 1.0, 1.0 };

			var result = DensityValue.DistributePixels(totalPixels, density, portions);

			// Expected allocation with right-to-left distribution: 256, 257, 257 (total 770)
			Assert.Equal(256, result[0]);
			Assert.Equal(257, result[1]);
			Assert.Equal(257, result[2]);

			var totalAllocated = result[0] + result[1] + result[2];
			Assert.Equal(770, totalAllocated);
		}

		[Fact]
		public void Equality_WorksCorrectly()
		{
			var value1 = new DensityValue(100.0, 2.0);
			var value2 = new DensityValue(100.0, 2.0);
			var value3 = new DensityValue(100.1, 2.0);

			Assert.True(value1.Equals(value2));
			Assert.False(value1.Equals(value3));
			Assert.True(value1 == value2);
			Assert.False(value1 == value3);
		}

		[Fact]
		public void ToString_ProvidesMeaningfulOutput()
		{
			var value = new DensityValue(100.0, 2.5);
			var result = value.ToString();

			Assert.Contains("100.00dp", result, StringComparison.Ordinal);
			Assert.Contains("250.00px", result, StringComparison.Ordinal);
			Assert.Contains("2.50x", result, StringComparison.Ordinal);
		}
	}
}