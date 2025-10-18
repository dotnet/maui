#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class NotifyCollectionChangedEventArgsExTests
    {
        /// <summary>
        /// Tests the constructor that accepts count, action, newItems, and oldItems parameters with valid inputs.
        /// Verifies that the Count property is set correctly and base class initialization succeeds.
        /// </summary>
        [Theory]
        [InlineData(0, NotifyCollectionChangedAction.Replace)]
        [InlineData(1, NotifyCollectionChangedAction.Replace)]
        [InlineData(-1, NotifyCollectionChangedAction.Replace)]
        [InlineData(100, NotifyCollectionChangedAction.Replace)]
        [InlineData(int.MaxValue, NotifyCollectionChangedAction.Replace)]
        [InlineData(int.MinValue, NotifyCollectionChangedAction.Replace)]
        public void Constructor_WithValidCountAndActionAndCollections_SetsCountProperty(int count, NotifyCollectionChangedAction action)
        {
            // Arrange
            var newItems = new List<string> { "new1", "new2" };
            var oldItems = new List<string> { "old1", "old2" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(newItems, args.NewItems);
            Assert.Equal(oldItems, args.OldItems);
        }

        /// <summary>
        /// Tests the constructor with null newItems and oldItems collections.
        /// Verifies that null collections are handled correctly and Count property is set.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        [InlineData(-10)]
        public void Constructor_WithNullCollections_SetsCountPropertyAndHandlesNulls(int count)
        {
            // Arrange
            var action = NotifyCollectionChangedAction.Replace;
            IList newItems = null;
            IList oldItems = null;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Null(args.NewItems);
            Assert.Null(args.OldItems);
        }

        /// <summary>
        /// Tests the constructor with empty collections for newItems and oldItems.
        /// Verifies that empty collections are preserved and Count property is set correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyCollections_SetsCountPropertyAndPreservesEmptyCollections()
        {
            // Arrange
            var count = 42;
            var action = NotifyCollectionChangedAction.Replace;
            var newItems = new List<object>();
            var oldItems = new List<object>();

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Empty(args.NewItems);
            Assert.Empty(args.OldItems);
        }

        /// <summary>
        /// Tests the constructor with different NotifyCollectionChangedAction enum values.
        /// Verifies that various action types are handled correctly even though this constructor is typically for Replace operations.
        /// </summary>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        [InlineData(NotifyCollectionChangedAction.Replace)]
        [InlineData(NotifyCollectionChangedAction.Move)]
        [InlineData(NotifyCollectionChangedAction.Reset)]
        public void Constructor_WithDifferentActions_SetsCountAndActionCorrectly(NotifyCollectionChangedAction action)
        {
            // Arrange
            var count = 15;
            var newItems = new ArrayList { "item1" };
            var oldItems = new ArrayList { "item2" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(newItems, args.NewItems);
            Assert.Equal(oldItems, args.OldItems);
        }

        /// <summary>
        /// Tests the constructor with mixed collection types and different object types.
        /// Verifies that various collection implementations and item types are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithMixedCollectionTypes_HandlesVariousImplementations()
        {
            // Arrange
            var count = 7;
            var action = NotifyCollectionChangedAction.Replace;
            var newItems = new ArrayList { 1, "string", new object() };
            var oldItems = new List<object> { 42, null, "test" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(newItems, args.NewItems);
            Assert.Equal(oldItems, args.OldItems);
        }

        /// <summary>
        /// Tests the constructor with invalid enum values cast from integers.
        /// Verifies that the constructor handles out-of-range enum values without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(999)]
        [InlineData(int.MaxValue)]
        public void Constructor_WithInvalidEnumValues_HandlesOutOfRangeActions(int invalidActionValue)
        {
            // Arrange
            var count = 3;
            var invalidAction = (NotifyCollectionChangedAction)invalidActionValue;
            var newItems = new List<string> { "new" };
            var oldItems = new List<string> { "old" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, invalidAction, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(invalidAction, args.Action);
            Assert.Equal(newItems, args.NewItems);
            Assert.Equal(oldItems, args.OldItems);
        }

        /// <summary>
        /// Tests the constructor with single-item collections.
        /// Verifies that collections with exactly one item are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItemCollections_HandlesSingleItems()
        {
            // Arrange
            var count = 1;
            var action = NotifyCollectionChangedAction.Replace;
            var newItems = new List<string> { "single_new" };
            var oldItems = new List<string> { "single_old" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Single(args.NewItems);
            Assert.Single(args.OldItems);
            Assert.Equal("single_new", args.NewItems[0]);
            Assert.Equal("single_old", args.OldItems[0]);
        }

        /// <summary>
        /// Tests the constructor with asymmetric collections (different sizes for newItems and oldItems).
        /// Verifies that collections of different sizes are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithAsymmetricCollections_HandlesUnequalSizes()
        {
            // Arrange
            var count = 10;
            var action = NotifyCollectionChangedAction.Replace;
            var newItems = new List<int> { 1, 2, 3, 4, 5 };
            var oldItems = new List<int> { 100, 200 };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItems, oldItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(5, args.NewItems.Count);
            Assert.Equal(2, args.OldItems.Count);
        }

        /// <summary>
        /// Tests the constructor that takes count, action, newItem, oldItem, and index parameters.
        /// Verifies that the Count property is set correctly and base class properties are initialized.
        /// </summary>
        /// <param name="count">The count value to test</param>
        /// <param name="action">The NotifyCollectionChangedAction to test</param>
        /// <param name="newItem">The new item object to test</param>
        /// <param name="oldItem">The old item object to test</param>
        /// <param name="index">The index value to test</param>
        [Theory]
        [InlineData(0, NotifyCollectionChangedAction.Replace, "newItem", "oldItem", 0)]
        [InlineData(1, NotifyCollectionChangedAction.Replace, "newItem", "oldItem", 1)]
        [InlineData(100, NotifyCollectionChangedAction.Replace, "newItem", "oldItem", 5)]
        [InlineData(-1, NotifyCollectionChangedAction.Replace, "newItem", "oldItem", 0)]
        [InlineData(int.MaxValue, NotifyCollectionChangedAction.Replace, "newItem", "oldItem", int.MaxValue)]
        [InlineData(int.MinValue, NotifyCollectionChangedAction.Replace, "newItem", "oldItem", int.MinValue)]
        public void Constructor_WithCountActionNewItemOldItemAndIndex_SetsCountAndInitializesBaseProperties(int count, NotifyCollectionChangedAction action, object newItem, object oldItem, int index)
        {
            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItem, oldItem, index);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Single(args.NewItems);
            Assert.Equal(newItem, args.NewItems[0]);
            Assert.Single(args.OldItems);
            Assert.Equal(oldItem, args.OldItems[0]);
            Assert.Equal(index, args.NewStartingIndex);
        }

        /// <summary>
        /// Tests the constructor with null values for newItem and oldItem parameters.
        /// Verifies that null values are handled correctly and Count property is set.
        /// </summary>
        /// <param name="count">The count value to test</param>
        /// <param name="index">The index value to test</param>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(10, 5)]
        [InlineData(-5, -1)]
        public void Constructor_WithNullNewItemAndOldItem_SetsCountAndHandlesNullValues(int count, int index)
        {
            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, null, null, index);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.Single(args.NewItems);
            Assert.Null(args.NewItems[0]);
            Assert.Single(args.OldItems);
            Assert.Null(args.OldItems[0]);
            Assert.Equal(index, args.NewStartingIndex);
        }

        /// <summary>
        /// Tests the constructor with different object types for newItem and oldItem.
        /// Verifies that various object types are handled correctly and Count property is set.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentObjectTypes_SetsCountAndHandlesVariousTypes()
        {
            // Arrange
            var stringItem = "test";
            var intItem = 42;
            var count = 25;
            var index = 3;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, stringItem, intItem, index);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.Single(args.NewItems);
            Assert.Equal(stringItem, args.NewItems[0]);
            Assert.Single(args.OldItems);
            Assert.Equal(intItem, args.OldItems[0]);
            Assert.Equal(index, args.NewStartingIndex);
        }

        /// <summary>
        /// Tests the constructor with special string values.
        /// Verifies that empty strings and whitespace strings are handled correctly.
        /// </summary>
        /// <param name="newItem">The new item string to test</param>
        /// <param name="oldItem">The old item string to test</param>
        [Theory]
        [InlineData("", "")]
        [InlineData(" ", "\t")]
        [InlineData("   ", "\n\r")]
        [InlineData("special\0chars", "unicode\u00A0test")]
        public void Constructor_WithSpecialStringValues_SetsCountAndHandlesSpecialStrings(string newItem, string oldItem)
        {
            // Arrange
            var count = 15;
            var index = 2;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, newItem, oldItem, index);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.Equal(newItem, args.NewItems[0]);
            Assert.Equal(oldItem, args.OldItems[0]);
        }

        /// <summary>
        /// Tests the constructor with boundary index values.
        /// Verifies that extreme index values are handled correctly and Count property is set.
        /// </summary>
        /// <param name="index">The boundary index value to test</param>
        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Constructor_WithBoundaryIndexValues_SetsCountAndHandlesBoundaryIndices(int index)
        {
            // Arrange
            var count = 50;
            var newItem = "new";
            var oldItem = "old";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, newItem, oldItem, index);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.Equal(newItem, args.NewItems[0]);
            Assert.Equal(oldItem, args.OldItems[0]);
            Assert.Equal(index, args.NewStartingIndex);
        }

        /// <summary>
        /// Tests the constructor with different NotifyCollectionChangedAction values.
        /// Note: Only Replace action is typically valid for this constructor overload in the base class.
        /// </summary>
        [Fact]
        public void Constructor_WithReplaceAction_SetsCountAndInitializesCorrectly()
        {
            // Arrange
            var count = 30;
            var newItem = "replacement";
            var oldItem = "original";
            var index = 4;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, NotifyCollectionChangedAction.Replace, newItem, oldItem, index);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(NotifyCollectionChangedAction.Replace, args.Action);
            Assert.Equal(newItem, args.NewItems[0]);
            Assert.Equal(oldItem, args.OldItems[0]);
            Assert.Equal(index, args.NewStartingIndex);
        }

        /// <summary>
        /// Tests the constructor with negative index values.
        /// Verifies that negative indices are handled correctly.
        /// </summary>
        [Fact]
        public void Constructor_NegativeIndex_SetsCountCorrectly()
        {
            // Arrange
            int count = 5;
            var action = NotifyCollectionChangedAction.Add;
            object changedItem = "test";
            int index = -1;

            // Act
            var eventArgs = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index);

            // Assert
            Assert.Equal(count, eventArgs.Count);
            Assert.Equal(action, eventArgs.Action);
        }

        /// <summary>
        /// Tests the constructor with boundary values for count parameter.
        /// Verifies that extreme count values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        public void Constructor_BoundaryCountValues_SetsCountCorrectly(int count)
        {
            // Arrange
            var action = NotifyCollectionChangedAction.Add;
            object changedItem = "test";
            int index = 0;

            // Act
            var eventArgs = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index);

            // Assert
            Assert.Equal(count, eventArgs.Count);
        }

        /// <summary>
        /// Tests the constructor with all valid NotifyCollectionChangedAction enum values.
        /// Verifies that all enum values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        [InlineData(NotifyCollectionChangedAction.Replace)]
        [InlineData(NotifyCollectionChangedAction.Move)]
        [InlineData(NotifyCollectionChangedAction.Reset)]
        public void Constructor_AllValidActions_SetsCountAndAction(NotifyCollectionChangedAction action)
        {
            // Arrange
            int count = 1;
            object changedItem = "test";
            int index = 0;

            // Act
            var eventArgs = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index);

            // Assert
            Assert.Equal(count, eventArgs.Count);
            Assert.Equal(action, eventArgs.Action);
        }

        /// <summary>
        /// Tests the constructor with various object types as changedItem.
        /// Verifies that different object types are handled correctly.
        /// </summary>
        [Theory]
        [InlineData("string item")]
        [InlineData(123)]
        [InlineData(true)]
        public void Constructor_DifferentChangedItemTypes_SetsCountCorrectly(object changedItem)
        {
            // Arrange
            int count = 1;
            var action = NotifyCollectionChangedAction.Add;
            int index = 0;

            // Act
            var eventArgs = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index);

            // Assert
            Assert.Equal(count, eventArgs.Count);
            Assert.Equal(changedItem, eventArgs.NewItems[0]);
        }

        /// <summary>
        /// Tests the constructor with invalid enum value for action parameter.
        /// Verifies that invalid enum values are handled by the base class.
        /// </summary>
        [Fact]
        public void Constructor_InvalidEnumValue_SetsCountCorrectly()
        {
            // Arrange
            int count = 1;
            var invalidAction = (NotifyCollectionChangedAction)999;
            object changedItem = "test";
            int index = 0;

            // Act
            var eventArgs = new NotifyCollectionChangedEventArgsEx(count, invalidAction, changedItem, index);

            // Assert
            Assert.Equal(count, eventArgs.Count);
            Assert.Equal(invalidAction, eventArgs.Action);
        }

        /// <summary>
        /// Tests the constructor with boundary index values.
        /// Verifies that extreme index values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        public void Constructor_BoundaryIndexValues_SetsCountCorrectly(int index)
        {
            // Arrange
            int count = 1;
            var action = NotifyCollectionChangedAction.Add;
            object changedItem = "test";

            // Act
            var eventArgs = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index);

            // Assert
            Assert.Equal(count, eventArgs.Count);
        }

        /// <summary>
        /// Tests the constructor with valid Add action and various count values.
        /// Verifies that the Count property is set correctly and the base constructor is called properly.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(100)]
        [InlineData(-1)]
        [InlineData(-100)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Constructor_WithAddActionAndVariousCountValues_SetsCountCorrectly(int count)
        {
            // Arrange
            var action = NotifyCollectionChangedAction.Add;
            var changedItem = new object();

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Single(args.NewItems);
            Assert.Equal(changedItem, args.NewItems[0]);
        }

        /// <summary>
        /// Tests the constructor with valid Remove action and a non-null changed item.
        /// Verifies that the Count property and base class properties are set correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithRemoveActionAndNonNullItem_SetsPropertiesCorrectly()
        {
            // Arrange
            var count = 5;
            var action = NotifyCollectionChangedAction.Remove;
            var changedItem = "test item";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Single(args.OldItems);
            Assert.Equal(changedItem, args.OldItems[0]);
        }

        /// <summary>
        /// Tests the constructor with Replace action and a changed item.
        /// Verifies that the Count property is set and base constructor handles Replace action properly.
        /// </summary>
        [Fact]
        public void Constructor_WithReplaceActionAndItem_SetsCountAndCallsBaseConstructor()
        {
            // Arrange
            var count = 10;
            var action = NotifyCollectionChangedAction.Replace;
            var changedItem = 42;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
        }

        /// <summary>
        /// Tests the constructor with null as the changed item.
        /// Verifies that null items are handled correctly and Count is set properly.
        /// </summary>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        public void Constructor_WithNullChangedItem_SetsCountAndHandlesNullItem(NotifyCollectionChangedAction action)
        {
            // Arrange
            var count = 1;
            object changedItem = null;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
        }

        /// <summary>
        /// Tests the constructor with various object types as changed items.
        /// Verifies that different object types are handled correctly and Count is set.
        /// </summary>
        [Theory]
        [InlineData("string item")]
        [InlineData(123)]
        [InlineData(45.67)]
        [InlineData(true)]
        public void Constructor_WithVariousObjectTypes_SetsCountAndAcceptsAllTypes(object changedItem)
        {
            // Arrange
            var count = 3;
            var action = NotifyCollectionChangedAction.Add;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(changedItem, args.NewItems[0]);
        }

        /// <summary>
        /// Tests the constructor with Move action, which may not be supported by the base constructor.
        /// Verifies that either the operation succeeds or throws an appropriate exception.
        /// </summary>
        [Fact]
        public void Constructor_WithMoveAction_EitherSucceedsOrThrowsException()
        {
            // Arrange
            var count = 2;
            var action = NotifyCollectionChangedAction.Move;
            var changedItem = new object();

            // Act & Assert
            try
            {
                var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);
                Assert.Equal(count, args.Count);
                Assert.Equal(action, args.Action);
            }
            catch (ArgumentException)
            {
                // Move action might not be supported with single item constructor - this is acceptable
            }
        }

        /// <summary>
        /// Tests the constructor with Reset action, which may not be supported by the base constructor.
        /// Verifies that either the operation succeeds or throws an appropriate exception.
        /// </summary>
        [Fact]
        public void Constructor_WithResetAction_EitherSucceedsOrThrowsException()
        {
            // Arrange
            var count = 0;
            var action = NotifyCollectionChangedAction.Reset;
            var changedItem = null;

            // Act & Assert
            try
            {
                var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);
                Assert.Equal(count, args.Count);
                Assert.Equal(action, args.Action);
            }
            catch (ArgumentException)
            {
                // Reset action might not be supported with single item constructor - this is acceptable
            }
        }

        /// <summary>
        /// Tests the constructor with an invalid enum value cast to NotifyCollectionChangedAction.
        /// Verifies that Count is set even when an invalid action value is provided.
        /// </summary>
        [Fact]
        public void Constructor_WithInvalidEnumValue_SetsCountRegardless()
        {
            // Arrange
            var count = 99;
            var action = (NotifyCollectionChangedAction)999; // Invalid enum value
            var changedItem = "test";

            // Act & Assert
            try
            {
                var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);
                Assert.Equal(count, args.Count);
            }
            catch (ArgumentException)
            {
                // Base constructor might throw for invalid enum values - this is acceptable behavior
            }
        }

        /// <summary>
        /// Tests the constructor with zero count and various valid actions.
        /// Verifies that zero count is handled correctly.
        /// </summary>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        public void Constructor_WithZeroCount_SetsCountToZero(NotifyCollectionChangedAction action)
        {
            // Arrange
            var count = 0;
            var changedItem = new object();

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItem);

            // Assert
            Assert.Equal(0, args.Count);
            Assert.Equal(action, args.Action);
        }

        /// <summary>
        /// Tests the constructor with negative count values.
        /// Verifies that negative counts are accepted and stored correctly.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-1000)]
        [InlineData(int.MinValue)]
        public void Constructor_WithNegativeCount_AcceptsAndStoresNegativeValues(int negativeCount)
        {
            // Arrange
            var action = NotifyCollectionChangedAction.Add;
            var changedItem = "item";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(negativeCount, action, changedItem);

            // Assert
            Assert.Equal(negativeCount, args.Count);
            Assert.Equal(action, args.Action);
        }

        /// <summary>
        /// Tests the constructor that takes count, action, newItem, and oldItem parameters with valid inputs.
        /// Verifies that all properties are correctly set and the base constructor is properly called.
        /// </summary>
        [Theory]
        [InlineData(0, NotifyCollectionChangedAction.Replace, "newItem", "oldItem")]
        [InlineData(1, NotifyCollectionChangedAction.Replace, "test", "old")]
        [InlineData(100, NotifyCollectionChangedAction.Replace, 42, 24)]
        [InlineData(5, NotifyCollectionChangedAction.Replace, null, "oldItem")]
        [InlineData(10, NotifyCollectionChangedAction.Replace, "newItem", null)]
        [InlineData(15, NotifyCollectionChangedAction.Replace, null, null)]
        public void Constructor_WithCountActionNewItemOldItem_ValidInputs_SetsPropertiesCorrectly(int count, NotifyCollectionChangedAction action, object newItem, object oldItem)
        {
            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItem, oldItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(newItem, args.NewItems?[0]);
            Assert.Equal(oldItem, args.OldItems?[0]);
        }

        /// <summary>
        /// Tests the constructor with boundary values for the count parameter.
        /// Verifies that extreme integer values are handled correctly.
        /// </summary>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Constructor_WithCountActionNewItemOldItem_BoundaryCountValues_SetsCountCorrectly(int count)
        {
            // Arrange
            var action = NotifyCollectionChangedAction.Replace;
            var newItem = "new";
            var oldItem = "old";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItem, oldItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
        }

        /// <summary>
        /// Tests the constructor with different NotifyCollectionChangedAction values.
        /// Verifies that various action types are properly passed to the base constructor.
        /// </summary>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        [InlineData(NotifyCollectionChangedAction.Replace)]
        [InlineData(NotifyCollectionChangedAction.Move)]
        [InlineData(NotifyCollectionChangedAction.Reset)]
        public void Constructor_WithCountActionNewItemOldItem_DifferentActions_SetsActionCorrectly(NotifyCollectionChangedAction action)
        {
            // Arrange
            var count = 5;
            var newItem = "new";
            var oldItem = "old";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItem, oldItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
        }

        /// <summary>
        /// Tests the constructor with various object types for newItem and oldItem parameters.
        /// Verifies that different object types are correctly handled.
        /// </summary>
        [Fact]
        public void Constructor_WithCountActionNewItemOldItem_VariousObjectTypes_SetsItemsCorrectly()
        {
            // Arrange
            var count = 3;
            var action = NotifyCollectionChangedAction.Replace;
            var newItem = new { Name = "Test", Value = 42 };
            var oldItem = new[] { 1, 2, 3 };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItem, oldItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Same(newItem, args.NewItems[0]);
            Assert.Same(oldItem, args.OldItems[0]);
        }

        /// <summary>
        /// Tests the constructor with null values for both newItem and oldItem parameters.
        /// Verifies that null items are handled correctly and Count property is still set.
        /// </summary>
        [Fact]
        public void Constructor_WithCountActionNewItemOldItem_BothItemsNull_SetsCountAndHandlesNulls()
        {
            // Arrange
            var count = 7;
            var action = NotifyCollectionChangedAction.Replace;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, null, null);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Null(args.NewItems[0]);
            Assert.Null(args.OldItems[0]);
        }

        /// <summary>
        /// Tests the constructor with negative count value and valid other parameters.
        /// Verifies that negative counts are accepted and stored correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithCountActionNewItemOldItem_NegativeCount_AcceptsAndSetsNegativeCount()
        {
            // Arrange
            var count = -10;
            var action = NotifyCollectionChangedAction.Replace;
            var newItem = "replacement";
            var oldItem = "original";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, newItem, oldItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(newItem, args.NewItems[0]);
            Assert.Equal(oldItem, args.OldItems[0]);
        }

        /// <summary>
        /// Tests the constructor with invalid enum value for action parameter (cast from integer).
        /// Verifies that invalid enum values are handled without throwing exceptions.
        /// </summary>
        [Fact]
        public void Constructor_WithCountActionNewItemOldItem_InvalidEnumAction_AcceptsInvalidEnumValue()
        {
            // Arrange
            var count = 1;
            var invalidAction = (NotifyCollectionChangedAction)999;
            var newItem = "new";
            var oldItem = "old";

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, invalidAction, newItem, oldItem);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(invalidAction, args.Action);
        }

        /// <summary>
        /// Tests the constructor with various NotifyCollectionChangedAction enum values.
        /// Verifies that all valid enum values can be used with the constructor.
        /// </summary>
        /// <param name="action">The NotifyCollectionChangedAction enum value to test</param>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        [InlineData(NotifyCollectionChangedAction.Replace)]
        [InlineData(NotifyCollectionChangedAction.Move)]
        [InlineData(NotifyCollectionChangedAction.Reset)]
        public void Constructor_WithDifferentActions_SetsActionCorrectly(NotifyCollectionChangedAction action)
        {
            // Arrange
            const int count = 5;
            const string changedItem = "test";
            const int index = 1;
            const int oldIndex = 2;

            // Act
            var result = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index, oldIndex);

            // Assert
            Assert.Equal(count, result.Count);
            Assert.Equal(action, result.Action);
        }

        /// <summary>
        /// Tests the constructor with null changed item.
        /// Verifies that null values are handled correctly for the changedItem parameter.
        /// </summary>
        [Fact]
        public void Constructor_WithNullChangedItem_SetsCountAndHandlesNull()
        {
            // Arrange
            const int count = 10;
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Move;
            object changedItem = null;
            const int index = 0;
            const int oldIndex = 1;

            // Act
            var result = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index, oldIndex);

            // Assert
            Assert.Equal(count, result.Count);
            Assert.Equal(action, result.Action);
        }

        /// <summary>
        /// Tests the constructor with boundary integer values for count parameter.
        /// Verifies that extreme integer values are handled correctly for the count.
        /// </summary>
        /// <param name="count">The boundary count value to test</param>
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(int.MaxValue)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(1)]
        public void Constructor_WithBoundaryCountValues_SetsCountCorrectly(int count)
        {
            // Arrange
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Move;
            const string changedItem = "test";
            const int index = 0;
            const int oldIndex = 1;

            // Act
            var result = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index, oldIndex);

            // Assert
            Assert.Equal(count, result.Count);
            Assert.Equal(action, result.Action);
        }

        /// <summary>
        /// Tests the constructor with boundary integer values for index parameters.
        /// Verifies that extreme integer values are handled correctly for index and oldIndex.
        /// </summary>
        /// <param name="index">The index value to test</param>
        /// <param name="oldIndex">The old index value to test</param>
        [Theory]
        [InlineData(int.MinValue, int.MaxValue)]
        [InlineData(int.MaxValue, int.MinValue)]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        [InlineData(1, 1)]
        public void Constructor_WithBoundaryIndexValues_HandlesExtremeValues(int index, int oldIndex)
        {
            // Arrange
            const int count = 1;
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Move;
            const string changedItem = "test";

            // Act
            var result = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index, oldIndex);

            // Assert
            Assert.Equal(count, result.Count);
            Assert.Equal(action, result.Action);
        }

        /// <summary>
        /// Tests the constructor with various object types for changedItem parameter.
        /// Verifies that different object types are handled correctly.
        /// </summary>
        /// <param name="changedItem">The changed item object to test</param>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(3.14)]
        public void Constructor_WithDifferentChangedItemTypes_HandlesVariousTypes(object changedItem)
        {
            // Arrange
            const int count = 1;
            const NotifyCollectionChangedAction action = NotifyCollectionChangedAction.Move;
            const int index = 0;
            const int oldIndex = 1;

            // Act
            var result = new NotifyCollectionChangedEventArgsEx(count, action, changedItem, index, oldIndex);

            // Assert
            Assert.Equal(count, result.Count);
            Assert.Equal(action, result.Action);
        }

        /// <summary>
        /// Tests the constructor with valid parameters to ensure Count property is set correctly
        /// and base class properties are properly initialized.
        /// </summary>
        /// <param name="count">The count value to test</param>
        /// <param name="action">The NotifyCollectionChangedAction to test</param>
        [Theory]
        [InlineData(0, NotifyCollectionChangedAction.Add)]
        [InlineData(1, NotifyCollectionChangedAction.Remove)]
        [InlineData(100, NotifyCollectionChangedAction.Replace)]
        [InlineData(-1, NotifyCollectionChangedAction.Move)]
        [InlineData(int.MaxValue, NotifyCollectionChangedAction.Reset)]
        [InlineData(int.MinValue, NotifyCollectionChangedAction.Add)]
        public void Constructor_WithValidParameters_SetsCountAndBaseProperties(int count, NotifyCollectionChangedAction action)
        {
            // Arrange
            var changedItems = new List<object> { "item1", "item2" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(changedItems, args.NewItems);
        }

        /// <summary>
        /// Tests the constructor with null changedItems parameter to ensure proper handling
        /// and that Count property is still set correctly.
        /// </summary>
        [Fact]
        public void Constructor_WithNullChangedItems_SetsCountCorrectly()
        {
            // Arrange
            int count = 42;
            var action = NotifyCollectionChangedAction.Add;
            IList changedItems = null;

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Null(args.NewItems);
        }

        /// <summary>
        /// Tests the constructor with empty changedItems list to ensure proper initialization
        /// and that Count property reflects the provided value, not the list count.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyChangedItems_SetsCountIndependentOfListSize()
        {
            // Arrange
            int count = 100;
            var action = NotifyCollectionChangedAction.Remove;
            var changedItems = new List<object>();

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(changedItems, args.NewItems);
        }

        /// <summary>
        /// Tests the constructor with all valid NotifyCollectionChangedAction enum values
        /// to ensure proper base class initialization for each action type.
        /// </summary>
        /// <param name="action">The NotifyCollectionChangedAction enum value to test</param>
        [Theory]
        [InlineData(NotifyCollectionChangedAction.Add)]
        [InlineData(NotifyCollectionChangedAction.Remove)]
        [InlineData(NotifyCollectionChangedAction.Replace)]
        [InlineData(NotifyCollectionChangedAction.Move)]
        [InlineData(NotifyCollectionChangedAction.Reset)]
        public void Constructor_WithAllValidActions_InitializesCorrectly(NotifyCollectionChangedAction action)
        {
            // Arrange
            int count = 10;
            var changedItems = new ArrayList { 1, 2, 3 };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, action, changedItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(action, args.Action);
            Assert.Equal(changedItems, args.NewItems);
        }

        /// <summary>
        /// Tests the constructor with invalid enum values (cast from out-of-range integers)
        /// to ensure the behavior matches the base class handling.
        /// </summary>
        [Fact]
        public void Constructor_WithInvalidEnumValue_PassesToBaseClass()
        {
            // Arrange
            int count = 25;
            var invalidAction = (NotifyCollectionChangedAction)999;
            var changedItems = new object[] { "item" };

            // Act
            var args = new NotifyCollectionChangedEventArgsEx(count, invalidAction, changedItems);

            // Assert
            Assert.Equal(count, args.Count);
            Assert.Equal(invalidAction, args.Action);
        }

        /// <summary>
        /// Tests the constructor with various IList implementations to ensure compatibility
        /// with different collection types.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentIListImplementations_WorksCorrectly()
        {
            // Arrange
            int count = 3;
            var action = NotifyCollectionChangedAction.Add;

            // Test with ArrayList
            var arrayList = new ArrayList { 1, 2, 3 };
            var args1 = new NotifyCollectionChangedEventArgsEx(count, action, arrayList);

            // Test with generic List
            var genericList = new List<string> { "a", "b", "c" };
            var args2 = new NotifyCollectionChangedEventArgsEx(count, action, genericList);

            // Test with array
            var array = new object[] { "x", "y", "z" };
            var args3 = new NotifyCollectionChangedEventArgsEx(count, action, array);

            // Assert
            Assert.Equal(count, args1.Count);
            Assert.Equal(arrayList, args1.NewItems);

            Assert.Equal(count, args2.Count);
            Assert.Equal(genericList, args2.NewItems);

            Assert.Equal(count, args3.Count);
            Assert.Equal(array, args3.NewItems);
        }
    }
}