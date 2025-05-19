using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ColumnDefinitionTests
    {
        [Fact]
        public void DefaultMinWidthIsNegativeOne()
        {
            var column = new ColumnDefinition();
            Assert.Equal(-1d, column.MinWidth);
        }

        [Fact]
        public void DefaultMaxWidthIsNegativeOne()
        {
            var column = new ColumnDefinition();
            Assert.Equal(-1d, column.MaxWidth);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(double.PositiveInfinity)]
        public void CanSetMinWidth(double value)
        {
            var column = new ColumnDefinition();
            column.MinWidth = value;
            Assert.Equal(value, column.MinWidth);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(double.PositiveInfinity)]
        public void CanSetMaxWidth(double value)
        {
            var column = new ColumnDefinition();
            column.MaxWidth = value;
            Assert.Equal(value, column.MaxWidth);
        }

        [Fact]
        public void MinWidthChangeInvalidatesLayout()
        {
            bool invalidated = false;
            var column = new ColumnDefinition();
            column.SizeChanged += (sender, args) => invalidated = true;

            column.MinWidth = 100;
            Assert.True(invalidated);
        }

        [Fact]
        public void MaxWidthChangeInvalidatesLayout()
        {
            bool invalidated = false;
            var column = new ColumnDefinition();
            column.SizeChanged += (sender, args) => invalidated = true;

            column.MaxWidth = 200;
            Assert.True(invalidated);
        }

        [Fact]
        public void WidthChangeInvalidatesLayoutWithMinMaxConstraints()
        {
            bool invalidated = false;
            var column = new ColumnDefinition();
            column.MinWidth = 50;
            column.MaxWidth = 150;
            column.SizeChanged += (sender, args) => invalidated = true;

            column.Width = new GridLength(100);
            Assert.True(invalidated);
        }

        [Fact]
        public void MinMaxWidthAreBindableProperties()
        {
            var column = new ColumnDefinition();
            
            bool minWidthChanged = false;
            bool maxWidthChanged = false;
            
            column.PropertyChanged += (sender, args) => 
            {
                if (args.PropertyName == nameof(ColumnDefinition.MinWidth))
                    minWidthChanged = true;
                if (args.PropertyName == nameof(ColumnDefinition.MaxWidth))
                    maxWidthChanged = true;
            };
            
            column.MinWidth = 100;
            column.MaxWidth = 200;
            
            Assert.True(minWidthChanged);
            Assert.True(maxWidthChanged);
        }

        [Theory]
        [InlineData(-10)] // Negative values other than -1 should be coerced to -1
        [InlineData(-2)]
        public void NegativeMinWidthIsCoerced(double value)
        {
            var column = new ColumnDefinition();
            column.MinWidth = value;
            Assert.Equal(-1d, column.MinWidth);
        }

        [Theory]
        [InlineData(-10)] // Negative values other than -1 should be coerced to -1
        [InlineData(-2)]
        public void NegativeMaxWidthIsCoerced(double value)
        {
            var column = new ColumnDefinition();
            column.MaxWidth = value;
            Assert.Equal(-1d, column.MaxWidth);
        }
    }
}