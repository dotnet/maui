#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the SelectableItemsView.SelectedItem property.
    /// </summary>
    public class SelectableItemsViewTests
    {
        /// <summary>
        /// Tests that SelectedItem getter returns the default value when no value has been set.
        /// Should retrieve the default value from the underlying BindableProperty system.
        /// Expected result: Returns the default value (null for object type).
        /// </summary>
        [Fact]
        public void SelectedItem_Get_WhenNoValueSet_ReturnsDefaultValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            var result = selectableItemsView.SelectedItem;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SelectedItem setter and getter work correctly with various object types.
        /// Should properly store and retrieve different types of objects through the BindableProperty system.
        /// Expected result: The getter returns the exact same value that was set.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("test string")]
        [InlineData(42)]
        [InlineData(3.14)]
        [InlineData(true)]
        [InlineData(false)]
        public void SelectedItem_SetAndGet_WithVariousValues_ReturnsSetValue(object expectedValue)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectedItem = expectedValue;
            var result = selectableItemsView.SelectedItem;

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that SelectedItem can be set to null explicitly.
        /// Should properly handle null assignment and retrieval through the BindableProperty system.
        /// Expected result: The getter returns null after setting to null.
        /// </summary>
        [Fact]
        public void SelectedItem_SetToNull_ReturnsNull()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            selectableItemsView.SelectedItem = "initial value";

            // Act
            selectableItemsView.SelectedItem = null;
            var result = selectableItemsView.SelectedItem;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SelectedItem can handle complex object types.
        /// Should properly store and retrieve custom objects through the BindableProperty system.
        /// Expected result: The getter returns the exact same custom object instance that was set.
        /// </summary>
        [Fact]
        public void SelectedItem_SetComplexObject_ReturnsExactSameInstance()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var customObject = new TestObject { Name = "Test", Id = 123 };

            // Act
            selectableItemsView.SelectedItem = customObject;
            var result = selectableItemsView.SelectedItem;

            // Assert
            Assert.Same(customObject, result);
        }

        /// <summary>
        /// Tests that SelectedItem can be overwritten with different values.
        /// Should properly handle multiple assignments through the BindableProperty system.
        /// Expected result: The getter always returns the most recently set value.
        /// </summary>
        [Fact]
        public void SelectedItem_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var firstValue = "first";
            var secondValue = "second";
            var thirdValue = 42;

            // Act & Assert - First value
            selectableItemsView.SelectedItem = firstValue;
            Assert.Equal(firstValue, selectableItemsView.SelectedItem);

            // Act & Assert - Second value
            selectableItemsView.SelectedItem = secondValue;
            Assert.Equal(secondValue, selectableItemsView.SelectedItem);

            // Act & Assert - Third value
            selectableItemsView.SelectedItem = thirdValue;
            Assert.Equal(thirdValue, selectableItemsView.SelectedItem);
        }

        /// <summary>
        /// Tests that SelectedItem handles extreme values correctly.
        /// Should properly store and retrieve boundary values through the BindableProperty system.
        /// Expected result: All extreme values are properly stored and retrieved.
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        [InlineData(double.MaxValue)]
        [InlineData(double.MinValue)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        [InlineData(double.NegativeInfinity)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void SelectedItem_SetExtremeValues_ReturnsCorrectValue(object extremeValue)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectedItem = extremeValue;
            var result = selectableItemsView.SelectedItem;

            // Assert
            if (extremeValue is double doubleValue && double.IsNaN(doubleValue))
            {
                Assert.True(double.IsNaN((double)result));
            }
            else
            {
                Assert.Equal(extremeValue, result);
            }
        }

        /// <summary>
        /// Helper class for testing complex object storage and retrieval.
        /// </summary>
        private class TestObject
        {
            public string Name { get; set; }
            public int Id { get; set; }
        }

        /// <summary>
        /// Tests that SelectionChangedCommand getter returns the default value (null) when no command is set.
        /// Input: New SelectableItemsView instance with no command set.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void SelectionChangedCommand_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            var result = selectableItemsView.SelectionChangedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SelectionChangedCommand getter returns the command that was previously set.
        /// Input: SelectableItemsView instance with a mock ICommand set.
        /// Expected: Returns the same ICommand instance that was set.
        /// </summary>
        [Fact]
        public void SelectionChangedCommand_WhenSet_ReturnsSetCommand()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var mockCommand = Substitute.For<ICommand>();

            // Act
            selectableItemsView.SelectionChangedCommand = mockCommand;
            var result = selectableItemsView.SelectionChangedCommand;

            // Assert
            Assert.Same(mockCommand, result);
        }

        /// <summary>
        /// Tests that SelectionChangedCommand getter returns null when explicitly set to null.
        /// Input: SelectableItemsView instance with command set to null.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void SelectionChangedCommand_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var mockCommand = Substitute.For<ICommand>();
            selectableItemsView.SelectionChangedCommand = mockCommand;

            // Act
            selectableItemsView.SelectionChangedCommand = null;
            var result = selectableItemsView.SelectionChangedCommand;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that SelectionChangedCommand getter properly handles multiple different ICommand implementations.
        /// Input: SelectableItemsView instance with different ICommand implementations set sequentially.
        /// Expected: Returns the correct ICommand instance for each assignment.
        /// </summary>
        [Fact]
        public void SelectionChangedCommand_WithDifferentCommands_ReturnsCorrectCommand()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var firstCommand = Substitute.For<ICommand>();
            var secondCommand = Substitute.For<ICommand>();

            // Act & Assert - First command
            selectableItemsView.SelectionChangedCommand = firstCommand;
            Assert.Same(firstCommand, selectableItemsView.SelectionChangedCommand);

            // Act & Assert - Second command
            selectableItemsView.SelectionChangedCommand = secondCommand;
            Assert.Same(secondCommand, selectableItemsView.SelectionChangedCommand);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property returns null by default when no value has been set.
        /// </summary>
        [Fact]
        public void SelectionChangedCommandParameter_Get_ReturnsDefaultValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property can be set to null and returns null when retrieved.
        /// </summary>
        [Fact]
        public void SelectionChangedCommandParameter_SetNull_ReturnsNull()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectionChangedCommandParameter = null;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property correctly stores and retrieves various object types.
        /// </summary>
        /// <param name="value">The test value to set and retrieve</param>
        /// <param name="description">Description of the test case for debugging purposes</param>
        [Theory]
        [InlineData("test string", "String value")]
        [InlineData(42, "Integer value")]
        [InlineData(3.14, "Double value")]
        [InlineData(true, "Boolean value")]
        [InlineData('c', "Character value")]
        public void SelectionChangedCommandParameter_SetAndGet_ReturnsCorrectValue(object value, string description)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectionChangedCommandParameter = value;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property correctly handles complex object types.
        /// </summary>
        [Fact]
        public void SelectionChangedCommandParameter_SetComplexObject_ReturnsCorrectValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var complexObject = new { Name = "Test", Value = 123 };

            // Act
            selectableItemsView.SelectionChangedCommandParameter = complexObject;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Same(complexObject, result);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property correctly handles array objects.
        /// </summary>
        [Fact]
        public void SelectionChangedCommandParameter_SetArray_ReturnsCorrectValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var arrayValue = new[] { "item1", "item2", "item3" };

            // Act
            selectableItemsView.SelectionChangedCommandParameter = arrayValue;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Same(arrayValue, result);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property correctly handles collection objects.
        /// </summary>
        [Fact]
        public void SelectionChangedCommandParameter_SetCollection_ReturnsCorrectValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var collectionValue = new List<string> { "item1", "item2" };

            // Act
            selectableItemsView.SelectionChangedCommandParameter = collectionValue;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Same(collectionValue, result);
        }

        /// <summary>
        /// Tests that setting the SelectionChangedCommandParameter property multiple times with different values works correctly.
        /// </summary>
        [Fact]
        public void SelectionChangedCommandParameter_SetMultipleTimes_ReturnsLatestValue()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();
            var firstValue = "first";
            var secondValue = 42;
            var thirdValue = true;

            // Act & Assert - First value
            selectableItemsView.SelectionChangedCommandParameter = firstValue;
            Assert.Equal(firstValue, selectableItemsView.SelectionChangedCommandParameter);

            // Act & Assert - Second value
            selectableItemsView.SelectionChangedCommandParameter = secondValue;
            Assert.Equal(secondValue, selectableItemsView.SelectionChangedCommandParameter);

            // Act & Assert - Third value
            selectableItemsView.SelectionChangedCommandParameter = thirdValue;
            Assert.Equal(thirdValue, selectableItemsView.SelectionChangedCommandParameter);
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property handles extreme numeric values correctly.
        /// </summary>
        /// <param name="value">The extreme numeric value to test</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData(int.MinValue, "Integer minimum value")]
        [InlineData(int.MaxValue, "Integer maximum value")]
        [InlineData(long.MinValue, "Long minimum value")]
        [InlineData(long.MaxValue, "Long maximum value")]
        [InlineData(double.MinValue, "Double minimum value")]
        [InlineData(double.MaxValue, "Double maximum value")]
        [InlineData(double.NaN, "Double NaN")]
        [InlineData(double.PositiveInfinity, "Double positive infinity")]
        [InlineData(double.NegativeInfinity, "Double negative infinity")]
        public void SelectionChangedCommandParameter_SetExtremeValues_ReturnsCorrectValue(object value, string description)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectionChangedCommandParameter = value;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            if (value is double doubleValue && double.IsNaN(doubleValue))
            {
                Assert.True(double.IsNaN((double)result));
            }
            else
            {
                Assert.Equal(value, result);
            }
        }

        /// <summary>
        /// Tests that the SelectionChangedCommandParameter property handles string edge cases correctly.
        /// </summary>
        /// <param name="value">The string value to test</param>
        /// <param name="description">Description of the test case</param>
        [Theory]
        [InlineData("", "Empty string")]
        [InlineData(" ", "Whitespace string")]
        [InlineData("\t\n\r", "String with control characters")]
        [InlineData("🚀🌟💯", "String with Unicode emojis")]
        public void SelectionChangedCommandParameter_SetStringEdgeCases_ReturnsCorrectValue(string value, string description)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectionChangedCommandParameter = value;
            var result = selectableItemsView.SelectionChangedCommandParameter;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the SelectionMode property getter returns the default value of SelectionMode.None when no value has been explicitly set.
        /// </summary>
        [Fact]
        public void SelectionMode_DefaultValue_ReturnsNone()
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            var selectionMode = selectableItemsView.SelectionMode;

            // Assert
            Assert.Equal(SelectionMode.None, selectionMode);
        }

        /// <summary>
        /// Tests that the SelectionMode property getter returns the correct value after setting valid SelectionMode enum values.
        /// Validates that the getter properly casts the underlying bindable property value to SelectionMode.
        /// </summary>
        /// <param name="expectedMode">The SelectionMode value to set and verify</param>
        [Theory]
        [InlineData(SelectionMode.None)]
        [InlineData(SelectionMode.Single)]
        [InlineData(SelectionMode.Multiple)]
        public void SelectionMode_SetValidValue_ReturnsCorrectValue(SelectionMode expectedMode)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SelectionMode = expectedMode;
            var actualMode = selectableItemsView.SelectionMode;

            // Assert
            Assert.Equal(expectedMode, actualMode);
        }

        /// <summary>
        /// Tests that the SelectionMode property getter can handle boundary enum values and cast them correctly.
        /// Verifies the casting behavior when dealing with enum values at the edges of the defined range.
        /// </summary>
        /// <param name="enumValue">The enum value to test, including boundary values</param>
        [Theory]
        [InlineData((SelectionMode)0)] // None
        [InlineData((SelectionMode)1)] // Single
        [InlineData((SelectionMode)2)] // Multiple
        public void SelectionMode_BoundaryEnumValues_ReturnsCorrectValue(SelectionMode enumValue)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SetValue(SelectableItemsView.SelectionModeProperty, enumValue);
            var actualMode = selectableItemsView.SelectionMode;

            // Assert
            Assert.Equal(enumValue, actualMode);
        }

        /// <summary>
        /// Tests that the SelectionMode property getter handles undefined enum values by casting them correctly.
        /// Verifies that the getter can cast values outside the defined enum range without throwing exceptions.
        /// </summary>
        /// <param name="undefinedValue">An undefined SelectionMode enum value</param>
        [Theory]
        [InlineData((SelectionMode)(-1))]
        [InlineData((SelectionMode)3)]
        [InlineData((SelectionMode)int.MaxValue)]
        [InlineData((SelectionMode)int.MinValue)]
        public void SelectionMode_UndefinedEnumValues_ReturnsValue(SelectionMode undefinedValue)
        {
            // Arrange
            var selectableItemsView = new SelectableItemsView();

            // Act
            selectableItemsView.SetValue(SelectableItemsView.SelectionModeProperty, undefinedValue);
            var actualMode = selectableItemsView.SelectionMode;

            // Assert
            Assert.Equal(undefinedValue, actualMode);
        }

        /// <summary>
        /// Creates a testable SelectableItemsView instance that exposes internal methods for testing.
        /// </summary>
        private class TestableSelectableItemsView : SelectableItemsView
        {
            public bool SuppressSelectionChangeNotificationValue => _suppressSelectionChangeNotification;

            public void CallSelectedItemsPropertyChanged(IList<object> oldSelection, IList<object> newSelection)
            {
                SelectedItemsPropertyChanged(oldSelection, newSelection);
            }
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with null newSelection parameter.
        /// Should clear existing items and call SelectedItemsPropertyChanged with empty collections.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithNullNewSelection_ClearsSelectedItemsAndNotifiesChange()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var initialItems = new List<object> { "item1", "item2" };

            // Add some initial items to SelectedItems
            foreach (var item in initialItems)
            {
                selectableItemsView.SelectedItems.Add(item);
            }

            // Act
            selectableItemsView.UpdateSelectedItems(null);

            // Assert
            Assert.Empty(selectableItemsView.SelectedItems);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with empty newSelection parameter.
        /// Should clear existing items and call SelectedItemsPropertyChanged with empty collections.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithEmptyNewSelection_ClearsSelectedItemsAndNotifiesChange()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var initialItems = new List<object> { "item1", "item2" };
            var emptyNewSelection = new List<object>();

            // Add some initial items to SelectedItems
            foreach (var item in initialItems)
            {
                selectableItemsView.SelectedItems.Add(item);
            }

            // Act
            selectableItemsView.UpdateSelectedItems(emptyNewSelection);

            // Assert
            Assert.Empty(selectableItemsView.SelectedItems);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with single item newSelection parameter.
        /// Should clear existing items, add new item, and call SelectedItemsPropertyChanged.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithSingleItemNewSelection_UpdatesSelectedItemsCorrectly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var initialItems = new List<object> { "oldItem1", "oldItem2" };
            var newItem = "newItem";
            var newSelection = new List<object> { newItem };

            // Add some initial items to SelectedItems
            foreach (var item in initialItems)
            {
                selectableItemsView.SelectedItems.Add(item);
            }

            // Act
            selectableItemsView.UpdateSelectedItems(newSelection);

            // Assert
            Assert.Single(selectableItemsView.SelectedItems);
            Assert.Equal(newItem, selectableItemsView.SelectedItems[0]);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with multiple items newSelection parameter.
        /// Should clear existing items, add all new items, and call SelectedItemsPropertyChanged.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithMultipleItemsNewSelection_UpdatesSelectedItemsCorrectly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var initialItems = new List<object> { "oldItem1" };
            var newSelection = new List<object> { "newItem1", "newItem2", "newItem3" };

            // Add some initial items to SelectedItems
            foreach (var item in initialItems)
            {
                selectableItemsView.SelectedItems.Add(item);
            }

            // Act
            selectableItemsView.UpdateSelectedItems(newSelection);

            // Assert
            Assert.Equal(3, selectableItemsView.SelectedItems.Count);
            Assert.Equal("newItem1", selectableItemsView.SelectedItems[0]);
            Assert.Equal("newItem2", selectableItemsView.SelectedItems[1]);
            Assert.Equal("newItem3", selectableItemsView.SelectedItems[2]);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method when SelectedItems is initially empty.
        /// Should add new items and call SelectedItemsPropertyChanged with correct parameters.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithEmptyInitialSelectedItems_AddsNewItemsCorrectly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var newSelection = new List<object> { "item1", "item2" };

            // Act
            selectableItemsView.UpdateSelectedItems(newSelection);

            // Assert
            Assert.Equal(2, selectableItemsView.SelectedItems.Count);
            Assert.Equal("item1", selectableItemsView.SelectedItems[0]);
            Assert.Equal("item2", selectableItemsView.SelectedItems[1]);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with mixed object types in newSelection.
        /// Should handle different object types correctly as the collection accepts any object.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithMixedObjectTypes_HandlesAllTypesCorrectly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var stringItem = "stringItem";
            var intItem = 42;
            var customObject = new { Name = "Test" };
            var newSelection = new List<object> { stringItem, intItem, customObject };

            // Act
            selectableItemsView.UpdateSelectedItems(newSelection);

            // Assert
            Assert.Equal(3, selectableItemsView.SelectedItems.Count);
            Assert.Equal(stringItem, selectableItemsView.SelectedItems[0]);
            Assert.Equal(intItem, selectableItemsView.SelectedItems[1]);
            Assert.Equal(customObject, selectableItemsView.SelectedItems[2]);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with duplicate items in newSelection.
        /// Should add all items including duplicates since IList allows duplicates.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithDuplicateItems_AddsDuplicatesCorrectly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var duplicateItem = "duplicateItem";
            var newSelection = new List<object> { duplicateItem, "otherItem", duplicateItem };

            // Act
            selectableItemsView.UpdateSelectedItems(newSelection);

            // Assert
            Assert.Equal(3, selectableItemsView.SelectedItems.Count);
            Assert.Equal(duplicateItem, selectableItemsView.SelectedItems[0]);
            Assert.Equal("otherItem", selectableItemsView.SelectedItems[1]);
            Assert.Equal(duplicateItem, selectableItemsView.SelectedItems[2]);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests UpdateSelectedItems method with null items in newSelection.
        /// Should handle null values correctly as the collection accepts null objects.
        /// </summary>
        [Fact]
        public void UpdateSelectedItems_WithNullItemsInCollection_HandlesNullValuesCorrectly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var newSelection = new List<object> { "item1", null, "item3" };

            // Act
            selectableItemsView.UpdateSelectedItems(newSelection);

            // Assert
            Assert.Equal(3, selectableItemsView.SelectedItems.Count);
            Assert.Equal("item1", selectableItemsView.SelectedItems[0]);
            Assert.Null(selectableItemsView.SelectedItems[1]);
            Assert.Equal("item3", selectableItemsView.SelectedItems[2]);
            Assert.False(selectableItemsView.SuppressSelectionChangeNotificationValue);
        }

        /// <summary>
        /// Tests that OnSelectionChanged can be called with a valid SelectionChangedEventArgs parameter
        /// without throwing exceptions. The method has an empty implementation so we verify basic functionality.
        /// </summary>
        [Fact]
        public void OnSelectionChanged_WithValidArgs_DoesNotThrow()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var args = new SelectionChangedEventArgs(null, "selectedItem");

            // Act & Assert
            var exception = Record.Exception(() => selectableItemsView.CallOnSelectionChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSelectionChanged throws ArgumentNullException when called with null arguments.
        /// Even though the method body is empty, we verify proper null parameter handling.
        /// </summary>
        [Fact]
        public void OnSelectionChanged_WithNullArgs_ThrowsArgumentNullException()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => selectableItemsView.CallOnSelectionChanged(null));
        }

        /// <summary>
        /// Tests that OnSelectionChanged can be called with SelectionChangedEventArgs containing empty selections
        /// without throwing exceptions. Verifies handling of empty selection scenarios.
        /// </summary>
        [Fact]
        public void OnSelectionChanged_WithEmptySelections_DoesNotThrow()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var args = new SelectionChangedEventArgs(null, null);

            // Act & Assert
            var exception = Record.Exception(() => selectableItemsView.CallOnSelectionChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSelectionChanged can be called with SelectionChangedEventArgs containing multiple selections
        /// without throwing exceptions. Verifies handling of complex selection scenarios.
        /// </summary>
        [Fact]
        public void OnSelectionChanged_WithMultipleSelections_DoesNotThrow()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var previousSelection = new List<object> { "item1", "item2" };
            var currentSelection = new List<object> { "item3", "item4", "item5" };
            var args = new SelectionChangedEventArgs(previousSelection, currentSelection);

            // Act & Assert
            var exception = Record.Exception(() => selectableItemsView.CallOnSelectionChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnSelectionChanged can be called with SelectionChangedEventArgs where previous selection
        /// transitions from single item to multiple items without throwing exceptions.
        /// </summary>
        [Fact]
        public void OnSelectionChanged_TransitionSingleToMultiple_DoesNotThrow()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            var args = new SelectionChangedEventArgs("previousItem", new List<object> { "item1", "item2" });

            // Act & Assert
            var exception = Record.Exception(() => selectableItemsView.CallOnSelectionChanged(args));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged returns early when _suppressSelectionChangeNotification is true.
        /// Verifies that no property change notifications are sent and no further processing occurs.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WhenSuppressSelectionChangeNotificationIsTrue_ReturnsEarly()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(true);
            var oldSelection = new List<object> { "item1" };
            var newSelection = new List<object> { "item2" };

            // Act
            selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection);

            // Assert
            Assert.False(selectableItemsView.PropertyChangedWasCalled);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged processes normally when _suppressSelectionChangeNotification is false.
        /// Verifies that OnPropertyChanged is called with the correct property name.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WhenSuppressSelectionChangeNotificationIsFalse_CallsOnPropertyChanged()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(false);
            var oldSelection = new List<object> { "item1" };
            var newSelection = new List<object> { "item2" };

            // Act
            selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection);

            // Assert
            Assert.True(selectableItemsView.PropertyChangedWasCalled);
            Assert.Equal("SelectedItems", selectableItemsView.LastPropertyChangedName);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged handles empty collections correctly.
        /// Verifies that the method processes empty selections without throwing exceptions.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WithEmptyCollections_ProcessesSuccessfully()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(false);
            var oldSelection = new List<object>();
            var newSelection = new List<object>();

            // Act
            selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection);

            // Assert
            Assert.True(selectableItemsView.PropertyChangedWasCalled);
            Assert.Equal("SelectedItems", selectableItemsView.LastPropertyChangedName);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged throws ArgumentNullException when oldSelection is null.
        /// Verifies that null parameter validation occurs in the SelectionChangedEventArgs constructor.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WithNullOldSelection_ThrowsArgumentNullException()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(false);
            IList<object> oldSelection = null;
            var newSelection = new List<object> { "item1" };

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection));

            Assert.Equal("previousSelection", exception.ParamName);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged throws ArgumentNullException when newSelection is null.
        /// Verifies that null parameter validation occurs in the SelectionChangedEventArgs constructor.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WithNullNewSelection_ThrowsArgumentNullException()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(false);
            var oldSelection = new List<object> { "item1" };
            IList<object> newSelection = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection));

            Assert.Equal("currentSelection", exception.ParamName);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged throws ArgumentNullException when both selections are null.
        /// Verifies that null parameter validation occurs in the SelectionChangedEventArgs constructor.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WithBothSelectionsNull_ThrowsArgumentNullException()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(false);
            IList<object> oldSelection = null;
            IList<object> newSelection = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection));

            Assert.Equal("previousSelection", exception.ParamName);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged handles collections with various object types.
        /// Verifies that the method processes different types of objects in the selection collections.
        /// </summary>
        [Theory]
        [InlineData("string1", "string2")]
        [InlineData(42, 84)]
        public void SelectedItemsPropertyChanged_WithVariousObjectTypes_ProcessesSuccessfully(object oldItem, object newItem)
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(false);
            var oldSelection = new List<object> { oldItem };
            var newSelection = new List<object> { newItem };

            // Act
            selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection);

            // Assert
            Assert.True(selectableItemsView.PropertyChangedWasCalled);
            Assert.Equal("SelectedItems", selectableItemsView.LastPropertyChangedName);
        }

        /// <summary>
        /// Tests that SelectedItemsPropertyChanged does not call OnPropertyChanged when suppression is enabled, even with null parameters.
        /// Verifies that the early return prevents any exception from being thrown.
        /// </summary>
        [Fact]
        public void SelectedItemsPropertyChanged_WithSuppressionEnabledAndNullParameters_ReturnsEarlyWithoutException()
        {
            // Arrange
            var selectableItemsView = new TestableSelectableItemsView();
            selectableItemsView.SetSuppressSelectionChangeNotification(true);
            IList<object> oldSelection = null;
            IList<object> newSelection = null;

            // Act
            selectableItemsView.CallSelectedItemsPropertyChanged(oldSelection, newSelection);

            // Assert
            Assert.False(selectableItemsView.PropertyChangedWasCalled);
        }

        /// <summary>
        /// Tests that the SelectableItemsView constructor initializes all properties to their expected default values.
        /// Verifies that the object is created successfully and inherits from StructuredItemsView.
        /// Expected result: All properties should be initialized to their bindable property default values.
        /// </summary>
        [Fact]
        public void Constructor_Default_InitializesPropertiesCorrectly()
        {
            // Act
            var selectableItemsView = new SelectableItemsView();

            // Assert
            Assert.NotNull(selectableItemsView);
            Assert.IsType<SelectableItemsView>(selectableItemsView);
            Assert.IsAssignableFrom<StructuredItemsView>(selectableItemsView);

            // Verify default property values
            Assert.Equal(SelectionMode.None, selectableItemsView.SelectionMode);
            Assert.Null(selectableItemsView.SelectedItem);
            Assert.NotNull(selectableItemsView.SelectedItems);
            Assert.Null(selectableItemsView.SelectionChangedCommand);
            Assert.Null(selectableItemsView.SelectionChangedCommandParameter);
        }
    }
}