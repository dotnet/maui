#nullable disable

using System;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the GroupableItemsView class.
    /// </summary>
    public class GroupableItemsViewTests
    {
        /// <summary>
        /// Tests that GroupHeaderTemplate getter returns null by default.
        /// Verifies the initial state of the GroupHeaderTemplate property.
        /// Expected result: Property should return null when not explicitly set.
        /// </summary>
        [Fact]
        public void GroupHeaderTemplate_DefaultValue_ReturnsNull()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();

            // Act
            var result = groupableItemsView.GroupHeaderTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GroupHeaderTemplate getter returns the value that was set.
        /// Verifies that the property correctly stores and retrieves DataTemplate instances.
        /// Expected result: Property should return the exact DataTemplate instance that was set.
        /// </summary>
        [Fact]
        public void GroupHeaderTemplate_SetValidDataTemplate_ReturnsSetValue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate(typeof(string));

            // Act
            groupableItemsView.GroupHeaderTemplate = dataTemplate;
            var result = groupableItemsView.GroupHeaderTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that GroupHeaderTemplate setter accepts null values.
        /// Verifies that the property can be explicitly set to null.
        /// Expected result: Property should accept null and return null when retrieved.
        /// </summary>
        [Fact]
        public void GroupHeaderTemplate_SetNull_ReturnsNull()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate(typeof(string));
            groupableItemsView.GroupHeaderTemplate = dataTemplate;

            // Act
            groupableItemsView.GroupHeaderTemplate = null;
            var result = groupableItemsView.GroupHeaderTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GroupHeaderTemplate can be set multiple times with different values.
        /// Verifies that the property correctly updates when set to different DataTemplate instances.
        /// Expected result: Property should return the most recently set DataTemplate instance.
        /// </summary>
        [Fact]
        public void GroupHeaderTemplate_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var firstTemplate = new DataTemplate(typeof(string));
            var secondTemplate = new DataTemplate(typeof(int));

            // Act
            groupableItemsView.GroupHeaderTemplate = firstTemplate;
            groupableItemsView.GroupHeaderTemplate = secondTemplate;
            var result = groupableItemsView.GroupHeaderTemplate;

            // Assert
            Assert.Same(secondTemplate, result);
        }

        /// <summary>
        /// Tests that GroupHeaderTemplate works with DataTemplate created using Func constructor.
        /// Verifies that the property works with all DataTemplate constructor variants.
        /// Expected result: Property should correctly store and retrieve DataTemplate created with Func.
        /// </summary>
        [Fact]
        public void GroupHeaderTemplate_SetDataTemplateWithFunc_ReturnsSetValue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate(() => new object());

            // Act
            groupableItemsView.GroupHeaderTemplate = dataTemplate;
            var result = groupableItemsView.GroupHeaderTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that GroupHeaderTemplate works with parameterless DataTemplate constructor.
        /// Verifies that the property works with DataTemplate instances created using the default constructor.
        /// Expected result: Property should correctly store and retrieve DataTemplate created with default constructor.
        /// </summary>
        [Fact]
        public void GroupHeaderTemplate_SetDataTemplateWithDefaultConstructor_ReturnsSetValue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate();

            // Act
            groupableItemsView.GroupHeaderTemplate = dataTemplate;
            var result = groupableItemsView.GroupHeaderTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that GroupFooterTemplate getter returns null when not set.
        /// Verifies the default behavior when no value has been assigned to the property.
        /// </summary>
        [Fact]
        public void GroupFooterTemplate_GetWhenNotSet_ReturnsNull()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();

            // Act
            var result = groupableItemsView.GroupFooterTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GroupFooterTemplate getter returns the correct DataTemplate when set.
        /// Verifies the property correctly retrieves a previously set DataTemplate value.
        /// </summary>
        [Fact]
        public void GroupFooterTemplate_GetWhenSet_ReturnsCorrectValue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate();

            // Act
            groupableItemsView.GroupFooterTemplate = dataTemplate;
            var result = groupableItemsView.GroupFooterTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that GroupFooterTemplate getter returns null after being explicitly set to null.
        /// Verifies the property correctly handles null assignment and retrieval.
        /// </summary>
        [Fact]
        public void GroupFooterTemplate_GetAfterSetToNull_ReturnsNull()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate();
            groupableItemsView.GroupFooterTemplate = dataTemplate;

            // Act
            groupableItemsView.GroupFooterTemplate = null;
            var result = groupableItemsView.GroupFooterTemplate;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GroupFooterTemplate setter accepts valid DataTemplate instances.
        /// Verifies the property correctly stores different types of DataTemplate instances.
        /// </summary>
        [Theory]
        [InlineData(true)]  // Test with default constructor
        [InlineData(false)] // Test with type constructor
        public void GroupFooterTemplate_SetValidDataTemplate_StoresCorrectly(bool useDefaultConstructor)
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = useDefaultConstructor
                ? new DataTemplate()
                : new DataTemplate(typeof(object));

            // Act
            groupableItemsView.GroupFooterTemplate = dataTemplate;
            var result = groupableItemsView.GroupFooterTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that GroupFooterTemplate setter accepts null values.
        /// Verifies the property correctly handles null assignment.
        /// </summary>
        [Fact]
        public void GroupFooterTemplate_SetToNull_AcceptsNull()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();

            // Act & Assert (should not throw)
            groupableItemsView.GroupFooterTemplate = null;
            Assert.Null(groupableItemsView.GroupFooterTemplate);
        }

        /// <summary>
        /// Tests that GroupFooterTemplate uses the correct BindableProperty.
        /// Verifies the property is backed by the expected GroupFooterTemplateProperty.
        /// </summary>
        [Fact]
        public void GroupFooterTemplate_UsesCorrectBindableProperty_ReturnsExpectedValue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            var dataTemplate = new DataTemplate();

            // Act
            groupableItemsView.SetValue(GroupableItemsView.GroupFooterTemplateProperty, dataTemplate);
            var result = groupableItemsView.GroupFooterTemplate;

            // Assert
            Assert.Same(dataTemplate, result);
        }

        /// <summary>
        /// Tests that IsGrouped property returns the default value of false when not explicitly set.
        /// Verifies the getter functionality and default BindableProperty behavior.
        /// </summary>
        [Fact]
        public void IsGrouped_DefaultValue_ReturnsFalse()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();

            // Act
            bool result = groupableItemsView.IsGrouped;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsGrouped property can be set to true and returns true when retrieved.
        /// Verifies both setter and getter functionality with true value.
        /// </summary>
        [Fact]
        public void IsGrouped_SetToTrue_ReturnsTrue()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();

            // Act
            groupableItemsView.IsGrouped = true;
            bool result = groupableItemsView.IsGrouped;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsGrouped property can be set to false and returns false when retrieved.
        /// Verifies both setter and getter functionality with false value.
        /// </summary>
        [Fact]
        public void IsGrouped_SetToFalse_ReturnsFalse()
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();

            // Act
            groupableItemsView.IsGrouped = false;
            bool result = groupableItemsView.IsGrouped;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsGrouped property maintains its value after multiple get operations.
        /// Verifies the getter consistently returns the same value without side effects.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsGrouped_MultipleGets_ReturnsSameValue(bool setValue)
        {
            // Arrange
            var groupableItemsView = new GroupableItemsView();
            groupableItemsView.IsGrouped = setValue;

            // Act & Assert
            Assert.Equal(setValue, groupableItemsView.IsGrouped);
            Assert.Equal(setValue, groupableItemsView.IsGrouped);
            Assert.Equal(setValue, groupableItemsView.IsGrouped);
        }
    }
}