using System;
using Xunit;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.UnitTests.Layouts
{
    [Category(TestCategory.Layout)]
    public class GridLayoutManagerDensityTests
    {
        [Fact]
        public void DensityValue_ThreeEqualColumns_WithRemainder()
        {
            // Scenario 1: 293.4dp across 3 columns at density 2.625
            var totalDp = 293.4;
            var density = 2.625;
            var totalPixels = totalDp * density; // 770.175px
            var portions = new double[] { 1.0, 1.0, 1.0 }; // Equal star sizing

            var result = DensityValue.DistributePixels(totalPixels, density, portions);

            // Expected with right-to-left distribution: [256, 257, 257] = 770px
            Assert.Equal(3, result.Length);
            Assert.Equal(256, result[0]);
            Assert.Equal(257, result[1]);
            Assert.Equal(257, result[2]);

            var total = result[0] + result[1] + result[2];
            Assert.Equal(770, total);

            // Verify this is better than naive division
            var naiveSize = (int)Math.Round(totalPixels / 3); // 257
            var naiveTotal = naiveSize * 3; // 771 - too much!
            Assert.True(total < naiveTotal, "DensityValue should provide more accurate allocation than naive rounding");
        }

        [Fact]
        public void DensityValue_ThreeEqualColumns_PerfectDivision()
        {
            // Scenario 2: 290dp across 3 columns at density 3.0 (perfect case)
            var totalDp = 290.0;
            var density = 3.0;
            var totalPixels = totalDp * density; // 870.0px
            var portions = new double[] { 1.0, 1.0, 1.0 };

            var result = DensityValue.DistributePixels(totalPixels, density, portions);

            // Perfect division case
            Assert.Equal(3, result.Length);
            Assert.Equal(290, result[0]);
            Assert.Equal(290, result[1]);
            Assert.Equal(290, result[2]);

            var total = result[0] + result[1] + result[2];
            Assert.Equal(870, total);
        }

        [Fact]
        public void DensityValue_FourEqualColumns_WithRemainder()
        {
            // Scenario 3: 300dp across 4 columns at density 2.625
            var totalDp = 300.0;
            var density = 2.625;
            var totalPixels = totalDp * density; // 787.5px
            var portions = new double[] { 1.0, 1.0, 1.0, 1.0 };

            var result = DensityValue.DistributePixels(totalPixels, density, portions);

            // Expected with right-to-left distribution: [196, 197, 197, 197] = 787px
            Assert.Equal(4, result.Length);
            Assert.Equal(196, result[0]);
            Assert.Equal(197, result[1]);
            Assert.Equal(197, result[2]);
            Assert.Equal(197, result[3]);

            var total = result[0] + result[1] + result[2] + result[3];
            Assert.Equal(787, total);
        }

        [Fact]
        public void DensityValue_WeightedStarSizing_PerfectDivision()
        {
            // Test weighted star sizing: 2*, 1*, 2* across 500 pixels
            var totalPixels = 500.0;
            var density = 2.0;
            var portions = new double[] { 2.0, 1.0, 2.0 }; // 2*, 1*, 2*

            var result = DensityValue.DistributePixels(totalPixels, density, portions);

            // Total weight = 5, so distribution should be:
            // First: 500 * (2/5) = 200 pixels
            // Second: 500 * (1/5) = 100 pixels  
            // Third: 500 * (2/5) = 200 pixels
            Assert.Equal(3, result.Length);
            Assert.Equal(200, result[0]);
            Assert.Equal(100, result[1]);
            Assert.Equal(200, result[2]);

            var total = result[0] + result[1] + result[2];
            Assert.Equal(500, total);
        }

        [Fact]
        public void DensityValue_WeightedStarSizing_WithRounding()
        {
            // Test weighted star sizing with rounding: 3*, 2*, 3* across 333 pixels
            var totalPixels = 333.0;
            var density = 1.5;
            var portions = new double[] { 3.0, 2.0, 3.0 }; // 3*, 2*, 3*

            var result = DensityValue.DistributePixels(totalPixels, density, portions);

            // Total weight = 8, with right-to-left distribution:
            // portions[0]=3: floor(333 * 3/8) = floor(124.875) = 124
            // portions[1]=2: floor(333 * 2/8) = floor(83.25) = 83 
            // portions[2]=3: floor(333 * 3/8) = floor(124.875) = 124
            // Total assigned: 124+83+124 = 331, remainder: 333-331 = 2
            // Right-to-left: portions[2] gets +1, portions[1] gets +1
            // Final: [124, 84, 125]
            Assert.Equal(3, result.Length);
            Assert.Equal(124, result[0]);
            Assert.Equal(84, result[1]);
            Assert.Equal(125, result[2]);

            var total = result[0] + result[1] + result[2];
            Assert.Equal(333, total);
        }

        [Theory]
        [InlineData(100.0, 1.0, new double[] { 1, 1, 1, 1 }, 25)] // Perfect division
        [InlineData(101.0, 1.0, new double[] { 1, 1, 1, 1 }, 25)] // 1 pixel remainder 
        [InlineData(103.0, 1.0, new double[] { 1, 1, 1, 1 }, 25)] // 3 pixel remainder
        public void DensityValue_RemainderPixels_Distribution(double totalPixels, double density, double[] portions, int expectedBase)
        {
            var result = DensityValue.DistributePixels(totalPixels, density, portions);

            // With right-to-left distribution, we expect different behavior:
            if (totalPixels == 103.0)
            {
                // 103/4 = 25.75, floor=25 each, remainder=3
                // Right-to-left: [25, 26, 26, 26]
                Assert.Equal(25, result[0]);
                Assert.Equal(26, result[1]);
                Assert.Equal(26, result[2]);
                Assert.Equal(26, result[3]);
            }
            else if (totalPixels == 101.0)
            {
                // 101/4 = 25.25, floor=25 each, remainder=1 
                // Right-to-left: [25, 25, 25, 26]
                Assert.Equal(25, result[0]);
                Assert.Equal(25, result[1]);
                Assert.Equal(25, result[2]);
                Assert.Equal(26, result[3]);
            }
            else
            {
                // 100/4 = 25 exactly, no remainder
                for (int i = 0; i < result.Length; i++)
                {
                    Assert.Equal(expectedBase, result[i]);
                }
            }

            // Total should match exactly
            var total = 0;
            foreach (var value in result)
            {
                total += value;
            }
            Assert.Equal((int)Math.Round(totalPixels), total);
        }
    }
}