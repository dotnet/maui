using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Layouts;
using System;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class FlexExtensionsTests
    {
        /// <summary>
        /// Tests GetConstraints with an item that has no parent.
        /// Should return Size(-1, -1) and hit the break statement when parent is null.
        /// </summary>
        [Fact]
        public void GetConstraints_ItemWithNoParent_ReturnsNegativeOneConstraints()
        {
            // Arrange
            var item = new Flex.Item();

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(-1d, result.Width);
            Assert.Equal(-1d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with a parent that has both valid width and height.
        /// Should return the parent's dimensions immediately.
        /// </summary>
        [Fact]
        public void GetConstraints_ParentWithValidWidthAndHeight_ReturnsParentDimensions()
        {
            // Arrange
            var parent = new Flex.Item(100f, 200f);
            var item = new Flex.Item();
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(100d, result.Width);
            Assert.Equal(200d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with a parent that has NaN width and height.
        /// Should return Size(-1, -1) as no valid constraints are found.
        /// </summary>
        [Fact]
        public void GetConstraints_ParentWithNaNDimensions_ReturnsNegativeOneConstraints()
        {
            // Arrange
            var parent = new Flex.Item(); // Default Width and Height are NaN
            var item = new Flex.Item();
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(-1d, result.Width);
            Assert.Equal(-1d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with a parent that has valid width but NaN height.
        /// Should find width constraint but height remains -1.
        /// </summary>
        [Fact]
        public void GetConstraints_ParentWithValidWidthNaNHeight_ReturnsWidthConstraintOnly()
        {
            // Arrange
            var parent = new Flex.Item { Width = 150f }; // Height remains NaN
            var item = new Flex.Item();
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(150d, result.Width);
            Assert.Equal(-1d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with a parent that has NaN width but valid height.
        /// Should find height constraint but width remains -1.
        /// </summary>
        [Fact]
        public void GetConstraints_ParentWithNaNWidthValidHeight_ReturnsHeightConstraintOnly()
        {
            // Arrange
            var parent = new Flex.Item { Height = 250f }; // Width remains NaN
            var item = new Flex.Item();
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(-1d, result.Width);
            Assert.Equal(250d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with a multi-level parent hierarchy where constraints are found at different levels.
        /// Grandparent has width, parent has height - should find both constraints.
        /// </summary>
        [Fact]
        public void GetConstraints_MultiLevelHierarchyWithDifferentConstraints_FindsBothConstraints()
        {
            // Arrange
            var grandparent = new Flex.Item { Width = 300f }; // Has width, no height
            var parent = new Flex.Item { Height = 400f }; // Has height, no width
            var item = new Flex.Item();

            grandparent.Add(parent);
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(300d, result.Width);
            Assert.Equal(400d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with a deep parent hierarchy with mixed NaN and valid values.
        /// Should traverse until both constraints are found.
        /// </summary>
        [Fact]
        public void GetConstraints_DeepHierarchyWithMixedValues_FindsConstraintsFromDifferentLevels()
        {
            // Arrange
            var greatGrandparent = new Flex.Item(500f, 600f);
            var grandparent = new Flex.Item(); // NaN dimensions
            var parent = new Flex.Item { Width = 350f }; // Only width
            var item = new Flex.Item();

            greatGrandparent.Add(grandparent);
            grandparent.Add(parent);
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(350d, result.Width); // Found from parent
            Assert.Equal(600d, result.Height); // Found from great-grandparent
        }

        /// <summary>
        /// Tests GetConstraints with extreme float values to ensure proper conversion to double.
        /// Should handle float.MaxValue and very small positive values correctly.
        /// </summary>
        [Theory]
        [InlineData(float.MaxValue, float.MinValue)]
        [InlineData(float.Epsilon, 1.0f)]
        [InlineData(0.0f, 0.0f)]
        public void GetConstraints_ExtremeFloatValues_ConvertsToDoubleCorrectly(float width, float height)
        {
            // Arrange
            var parent = new Flex.Item { Width = width, Height = height };
            var item = new Flex.Item();
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal((double)width, result.Width);
            Assert.Equal((double)height, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints when constraints are found immediately (both width and height in direct parent).
        /// Should stop traversing after finding both constraints in the first parent.
        /// </summary>
        [Fact]
        public void GetConstraints_ConstraintsFoundImmediately_StopsTraversing()
        {
            // Arrange
            var grandparent = new Flex.Item(999f, 888f); // These should not be used
            var parent = new Flex.Item(100f, 200f); // These should be used
            var item = new Flex.Item();

            grandparent.Add(parent);
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(100d, result.Width); // From direct parent, not grandparent
            Assert.Equal(200d, result.Height); // From direct parent, not grandparent
        }

        /// <summary>
        /// Tests GetConstraints with a hierarchy where no constraints are ever found.
        /// All parents have NaN dimensions, should return Size(-1, -1).
        /// </summary>
        [Fact]
        public void GetConstraints_NoConstraintsInHierarchy_ReturnsNegativeOneConstraints()
        {
            // Arrange
            var grandparent = new Flex.Item(); // NaN dimensions
            var parent = new Flex.Item(); // NaN dimensions
            var item = new Flex.Item();

            grandparent.Add(parent);
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal(-1d, result.Width);
            Assert.Equal(-1d, result.Height);
        }

        /// <summary>
        /// Tests GetConstraints with positive infinity and negative infinity values.
        /// Should handle special float values correctly.
        /// </summary>
        [Theory]
        [InlineData(float.PositiveInfinity, float.NegativeInfinity)]
        [InlineData(float.NegativeInfinity, float.PositiveInfinity)]
        public void GetConstraints_InfinityValues_HandlesSpecialFloatValues(float width, float height)
        {
            // Arrange
            var parent = new Flex.Item { Width = width, Height = height };
            var item = new Flex.Item();
            parent.Add(item);

            // Act
            var result = item.GetConstraints();

            // Assert
            Assert.Equal((double)width, result.Width);
            Assert.Equal((double)height, result.Height);
        }
    }
}
