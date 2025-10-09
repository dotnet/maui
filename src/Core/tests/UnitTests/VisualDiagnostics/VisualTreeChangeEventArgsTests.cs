// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using Microsoft.Maui;
using Xunit;


namespace Microsoft.Maui.UnitTests
{
    /// <summary>
    /// Unit tests for the VisualTreeChangeEventArgs class.
    /// </summary>
    public class VisualTreeChangeEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly assigns all parameters to their corresponding properties.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithValidParameters_AssignsPropertiesCorrectly()
        {
            // Arrange
            var parent = new object();
            var child = new object();
            int childIndex = 5;
            var changeType = VisualTreeChangeType.Add;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, changeType);

            // Assert
            Assert.Same(parent, eventArgs.Parent);
            Assert.Same(child, eventArgs.Child);
            Assert.Equal(childIndex, eventArgs.ChildIndex);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }

        /// <summary>
        /// Tests that the constructor accepts null parent parameter and assigns it correctly.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithNullParent_AssignsNullParentCorrectly()
        {
            // Arrange
            object parent = null;
            var child = new object();
            int childIndex = 0;
            var changeType = VisualTreeChangeType.Remove;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, changeType);

            // Assert
            Assert.Null(eventArgs.Parent);
            Assert.Same(child, eventArgs.Child);
            Assert.Equal(childIndex, eventArgs.ChildIndex);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }

        /// <summary>
        /// Tests the constructor with boundary values for childIndex parameter.
        /// </summary>
        /// <param name="childIndex">The child index value to test.</param>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithBoundaryChildIndexValues_AssignsChildIndexCorrectly(int childIndex)
        {
            // Arrange
            var parent = new object();
            var child = new object();
            var changeType = VisualTreeChangeType.Add;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, changeType);

            // Assert
            Assert.Same(parent, eventArgs.Parent);
            Assert.Same(child, eventArgs.Child);
            Assert.Equal(childIndex, eventArgs.ChildIndex);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }

        /// <summary>
        /// Tests the constructor with different VisualTreeChangeType enum values.
        /// </summary>
        /// <param name="changeType">The change type to test.</param>
        [Theory]
        [InlineData(VisualTreeChangeType.Add)]
        [InlineData(VisualTreeChangeType.Remove)]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithDifferentChangeTypes_AssignsChangeTypeCorrectly(VisualTreeChangeType changeType)
        {
            // Arrange
            var parent = new object();
            var child = new object();
            int childIndex = 2;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, changeType);

            // Assert
            Assert.Same(parent, eventArgs.Parent);
            Assert.Same(child, eventArgs.Child);
            Assert.Equal(childIndex, eventArgs.ChildIndex);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }

        /// <summary>
        /// Tests the constructor with an invalid enum value cast to VisualTreeChangeType.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithInvalidEnumValue_AssignsValueCorrectly()
        {
            // Arrange
            var parent = new object();
            var child = new object();
            int childIndex = 1;
            var invalidChangeType = (VisualTreeChangeType)999;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, invalidChangeType);

            // Assert
            Assert.Same(parent, eventArgs.Parent);
            Assert.Same(child, eventArgs.Child);
            Assert.Equal(childIndex, eventArgs.ChildIndex);
            Assert.Equal(invalidChangeType, eventArgs.ChangeType);
        }

        /// <summary>
        /// Tests that the constructor creates an instance that inherits from EventArgs.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_CreatesInstanceInheritingFromEventArgs()
        {
            // Arrange
            var parent = new object();
            var child = new object();
            int childIndex = 0;
            var changeType = VisualTreeChangeType.Add;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, changeType);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }

        /// <summary>
        /// Tests the constructor with different object types for parent and child parameters.
        /// </summary>
        [Fact]
        [Trait("Owner", "Code Testing Agent 0.4.133-alpha+a413c4336c")]
        [Trait("Category", "auto-generated")]
        public void Constructor_WithDifferentObjectTypes_AssignsCorrectly()
        {
            // Arrange
            string parent = "parent";
            int child = 42;
            int childIndex = 3;
            var changeType = VisualTreeChangeType.Remove;

            // Act
            var eventArgs = new VisualTreeChangeEventArgs(parent, child, childIndex, changeType);

            // Assert
            Assert.Same(parent, eventArgs.Parent);
            Assert.Equal(child, eventArgs.Child);
            Assert.Equal(childIndex, eventArgs.ChildIndex);
            Assert.Equal(changeType, eventArgs.ChangeType);
        }
    }
}
