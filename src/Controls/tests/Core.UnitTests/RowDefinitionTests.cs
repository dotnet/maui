using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class RowDefinitionTests
    {
        [Fact]
        public void DefaultMinHeightIsNegativeOne()
        {
            var row = new RowDefinition();
            Assert.Equal(-1d, row.MinHeight);
        }

        [Fact]
        public void DefaultMaxHeightIsNegativeOne()
        {
            var row = new RowDefinition();
            Assert.Equal(-1d, row.MaxHeight);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(double.PositiveInfinity)]
        public void CanSetMinHeight(double value)
        {
            var row = new RowDefinition();
            row.MinHeight = value;
            Assert.Equal(value, row.MinHeight);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(double.PositiveInfinity)]
        public void CanSetMaxHeight(double value)
        {
            var row = new RowDefinition();
            row.MaxHeight = value;
            Assert.Equal(value, row.MaxHeight);
        }

        [Fact]
        public void MinHeightChangeInvalidatesLayout()
        {
            bool invalidated = false;
            var row = new RowDefinition();
            row.SizeChanged += (sender, args) => invalidated = true;

            row.MinHeight = 100;
            Assert.True(invalidated);
        }

        [Fact]
        public void MaxHeightChangeInvalidatesLayout()
        {
            bool invalidated = false;
            var row = new RowDefinition();
            row.SizeChanged += (sender, args) => invalidated = true;

            row.MaxHeight = 200;
            Assert.True(invalidated);
        }

        [Fact]
        public void HeightChangeInvalidatesLayoutWithMinMaxConstraints()
        {
            bool invalidated = false;
            var row = new RowDefinition();
            row.MinHeight = 50;
            row.MaxHeight = 150;
            row.SizeChanged += (sender, args) => invalidated = true;

            row.Height = new GridLength(100);
            Assert.True(invalidated);
        }

        [Fact]
        public void MinMaxHeightAreBindableProperties()
        {
            var row = new RowDefinition();
            
            bool minHeightChanged = false;
            bool maxHeightChanged = false;
            
            row.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(RowDefinition.MinHeight))
                    minHeightChanged = true;
                if (args.PropertyName == nameof(RowDefinition.MaxHeight))
                    maxHeightChanged = true;
            };
            
            row.MinHeight = 100;
            row.MaxHeight = 200;
            
            Assert.True(minHeightChanged);
            Assert.True(maxHeightChanged);
        }

        [Theory]
        [InlineData(-10)] // Negative values other than -1 should be coerced to -1
        [InlineData(-2)]
        public void NegativeMinHeightIsCoerced(double value)
        {
            var row = new RowDefinition();
            row.MinHeight = value;
            Assert.Equal(-1d, row.MinHeight);
        }

        [Theory]
        [InlineData(-10)] // Negative values other than -1 should be coerced to -1
        [InlineData(-2)]
        public void NegativeMaxHeightIsCoerced(double value)
        {
            var row = new RowDefinition();
            row.MaxHeight = value;
            Assert.Equal(-1d, row.MaxHeight);
        }
    }
}