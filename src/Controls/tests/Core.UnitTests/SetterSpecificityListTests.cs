#nullable disable

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class SetterSpecificityListTests
    {
        [Fact]
        public void NoValues()
        {
            var list = new SetterSpecificityList<object>();

            var pair = list.GetSpecificityAndValue();
            Assert.Null(pair.Value);
            Assert.Equal(default, pair.Key);
        }

        [Fact]
        public void OverridesValueWithSameSpecificity()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.ManualValueSetter] = "initial";

            list[SetterSpecificity.ManualValueSetter] = "new";
            Assert.Equal(1, list.Count);

            var pair = list.GetSpecificityAndValue();
            Assert.Equal("new", pair.Value);
            Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);
        }

        [Fact]
        public async Task RemovingValueDoesNotLeak()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            list[SetterSpecificity.FromHandler] = nameof(SetterSpecificity.FromHandler);
            WeakReference weakReference;

            {
                var o = new object();
                weakReference = new WeakReference(o);
                list[SetterSpecificity.FromBinding] = o;
            }

            list.Remove(SetterSpecificity.FromBinding);

            Assert.False(await weakReference.WaitForCollect());
        }

        [Fact]
        public async Task RemovingLastValueDoesNotLeak()
        {
            var list = new SetterSpecificityList<object>();
            WeakReference weakReference;

            {
                var o = new object();
                weakReference = new WeakReference(o);
                list[SetterSpecificity.ManualValueSetter] = o;
            }

            list.Remove(SetterSpecificity.ManualValueSetter);

            Assert.False(await weakReference.WaitForCollect());
        }

        [Fact]
        public void GetValueForSpecificity()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

            var foundValue = list[SetterSpecificity.DefaultValue];
            Assert.Equal(nameof(SetterSpecificity.DefaultValue), foundValue);
        }

        [Fact]
        public void NullWhenNoValuesMatchSpecificity()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

            var foundValue = list[SetterSpecificity.FromHandler];
            Assert.Null(foundValue);
        }

        [Fact]
        public void OneValue()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

            var pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
            Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

            // Add a "default" value
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
            Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);
            Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), list.GetValue());
            Assert.Equal(SetterSpecificity.ManualValueSetter, list.GetSpecificity());
        }

        [Fact]
        public void TwoValues()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

            var pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
            Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

            // Remove a value
            list.Remove(SetterSpecificity.ManualValueSetter);
            pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.DefaultValue), pair.Value);
            Assert.Equal(SetterSpecificity.DefaultValue, pair.Key);
            Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetValue());
            Assert.Equal(SetterSpecificity.DefaultValue, list.GetSpecificity());
        }

        [Fact]
        public void ThreeValues()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            list[SetterSpecificity.FromBinding] = nameof(SetterSpecificity.FromBinding);
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);

            var pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), pair.Value);
            Assert.Equal(SetterSpecificity.ManualValueSetter, pair.Key);

            // Remove a value
            list.Remove(SetterSpecificity.ManualValueSetter);
            pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.FromBinding), pair.Value);
            Assert.Equal(SetterSpecificity.FromBinding, pair.Key);
            Assert.Equal(nameof(SetterSpecificity.FromBinding), list.GetValue());
            Assert.Equal(SetterSpecificity.FromBinding, list.GetSpecificity());
        }

        [Fact]
        public void ManyValues()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            list[SetterSpecificity.FromBinding] = nameof(SetterSpecificity.FromBinding);
            list[SetterSpecificity.DynamicResourceSetter] = nameof(SetterSpecificity.DynamicResourceSetter);
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);
            list[SetterSpecificity.Trigger] = nameof(SetterSpecificity.Trigger);

            var pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.Trigger), pair.Value);
            Assert.Equal(SetterSpecificity.Trigger, pair.Key);

            // Remove a value
            list.Remove(SetterSpecificity.ManualValueSetter);
            pair = list.GetSpecificityAndValue();
            Assert.Equal(nameof(SetterSpecificity.Trigger), pair.Value);
            Assert.Equal(SetterSpecificity.Trigger, pair.Key);
            Assert.Equal(nameof(SetterSpecificity.Trigger), list.GetValue());
            Assert.Equal(SetterSpecificity.Trigger, list.GetSpecificity());
        }

        [Fact]
        public void GetClearedValue()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            Assert.Equal(default, list.GetClearedValue());
            Assert.Equal(default, list.GetClearedSpecificity());
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);
            Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetClearedValue());
            Assert.Equal(SetterSpecificity.DefaultValue, list.GetClearedSpecificity());
        }

        [Fact]
        public void GetClearedValueForSpecificity()
        {
            var list = new SetterSpecificityList<object>();
            list[SetterSpecificity.DefaultValue] = nameof(SetterSpecificity.DefaultValue);
            Assert.Equal(default, list.GetClearedValue(SetterSpecificity.DefaultValue));
            list[SetterSpecificity.ManualValueSetter] = nameof(SetterSpecificity.ManualValueSetter);
            Assert.Equal(nameof(SetterSpecificity.DefaultValue), list.GetClearedValue(SetterSpecificity.ManualValueSetter));
            Assert.Equal(nameof(SetterSpecificity.ManualValueSetter), list.GetClearedValue(SetterSpecificity.FromHandler));
        }

        /// <summary>
        /// Tests that the parameterized constructor with zero capacity initializes an empty list correctly.
        /// Should use Array.Empty for internal arrays and have Count of 0.
        /// </summary>
        [Fact]
        public void Constructor_WithZeroCapacity_InitializesEmptyList()
        {
            // Arrange & Act
            var list = new SetterSpecificityList<object>(0);

            // Assert
            Assert.Equal(0, list.Count);

            // Verify the list can accept values after construction
            list[SetterSpecificity.ManualValueSetter] = "test";
            Assert.Equal(1, list.Count);
            Assert.Equal("test", list.GetValue());
        }

        /// <summary>
        /// Tests that the parameterized constructor with positive capacity values initializes correctly.
        /// Should create arrays with specified capacity and have Count of 0.
        /// </summary>
        [Theory]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(100)]
        [InlineData(1000)]
        public void Constructor_WithPositiveCapacity_InitializesCorrectly(int initialCapacity)
        {
            // Arrange & Act
            var list = new SetterSpecificityList<object>(initialCapacity);

            // Assert
            Assert.Equal(0, list.Count);

            // Verify the list can accept values after construction
            list[SetterSpecificity.DefaultValue] = "value1";
            list[SetterSpecificity.ManualValueSetter] = "value2";
            Assert.Equal(2, list.Count);
            Assert.Equal("value2", list.GetValue()); // ManualValueSetter has higher specificity
        }

        /// <summary>
        /// Tests that the parameterized constructor throws ArgumentOutOfRangeException for negative capacity values.
        /// Negative array sizes are not allowed and should cause an exception.
        /// </summary>
        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        [InlineData(-100)]
        [InlineData(int.MinValue)]
        public void Constructor_WithNegativeCapacity_ThrowsArgumentOutOfRangeException(int negativeCapacity)
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => new SetterSpecificityList<object>(negativeCapacity));
        }

        /// <summary>
        /// Tests that the parameterized constructor throws OutOfMemoryException for extremely large capacity values.
        /// Attempting to allocate arrays with int.MaxValue size should fail due to memory constraints.
        /// </summary>
        [Fact]
        public void Constructor_WithMaxValueCapacity_ThrowsOutOfMemoryException()
        {
            // Arrange, Act & Assert
            Assert.Throws<OutOfMemoryException>(() => new SetterSpecificityList<object>(int.MaxValue));
        }

        /// <summary>
        /// Tests that the parameterized constructor initializes correctly and maintains proper behavior 
        /// when values are added and removed with different capacity values.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        public void Constructor_WithVariousCapacities_MaintainsCorrectBehaviorAfterOperations(int initialCapacity)
        {
            // Arrange
            var list = new SetterSpecificityList<string>(initialCapacity);

            // Act - Add multiple values
            list[SetterSpecificity.DefaultValue] = "default";
            list[SetterSpecificity.FromBinding] = "binding";
            list[SetterSpecificity.ManualValueSetter] = "manual";

            // Assert - Verify correct behavior
            Assert.Equal(3, list.Count);
            Assert.Equal("manual", list.GetValue()); // Highest specificity
            Assert.Equal(SetterSpecificity.ManualValueSetter, list.GetSpecificity());

            // Act - Remove highest specificity value
            list.Remove(SetterSpecificity.ManualValueSetter);

            // Assert - Next highest should be active
            Assert.Equal(2, list.Count);
            Assert.Equal("binding", list.GetValue());
            Assert.Equal(SetterSpecificity.FromBinding, list.GetSpecificity());
        }

        /// <summary>
        /// Tests that the parameterized constructor works correctly with reference and value types.
        /// Ensures the generic constraint 'where T : class' is properly enforced.
        /// </summary>
        [Fact]
        public void Constructor_WithReferenceTypes_WorksCorrectly()
        {
            // Arrange & Act
            var stringList = new SetterSpecificityList<string>(5);
            var objectList = new SetterSpecificityList<object>(10);

            // Assert
            Assert.Equal(0, stringList.Count);
            Assert.Equal(0, objectList.Count);

            // Verify they accept null values (valid for reference types)
            stringList[SetterSpecificity.DefaultValue] = null;
            objectList[SetterSpecificity.DefaultValue] = null;

            Assert.Equal(1, stringList.Count);
            Assert.Equal(1, objectList.Count);
            Assert.Null(stringList.GetValue());
            Assert.Null(objectList.GetValue());
        }

        /// <summary>
        /// Tests removing a key that doesn't exist in an empty list
        /// </summary>
        [Fact]
        public void Remove_EmptyList_DoesNotThrow()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();

            // Act & Assert
            list.Remove(SetterSpecificity.DefaultValue);
            Assert.Equal(0, list.Count);
        }

        /// <summary>
        /// Tests removing a key that doesn't exist in a populated list
        /// </summary>
        [Fact]
        public void Remove_NonExistentKey_DoesNotChangeList()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "default";
            list[SetterSpecificity.ManualValueSetter] = "manual";
            var originalCount = list.Count;

            // Act
            list.Remove(SetterSpecificity.FromBinding); // Key that doesn't exist

            // Assert
            Assert.Equal(originalCount, list.Count);
            Assert.Equal("manual", list.GetValue());
        }

        /// <summary>
        /// Tests binary search path where search continues in upper half (indexSpecificity < key)
        /// </summary>
        [Fact]
        public void Remove_SearchUpperHalf_RemovesCorrectItem()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "default";
            list[SetterSpecificity.FromBinding] = "binding";
            list[SetterSpecificity.ManualValueSetter] = "manual";
            list[SetterSpecificity.DynamicResourceSetter] = "dynamic";

            // Act - Remove DynamicResourceSetter which should trigger search in upper half
            list.Remove(SetterSpecificity.DynamicResourceSetter);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal("manual", list.GetValue()); // Should now be the highest
        }

        /// <summary>
        /// Tests binary search path where search continues in lower half (indexSpecificity > key)
        /// </summary>
        [Fact]
        public void Remove_SearchLowerHalf_RemovesCorrectItem()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "default";
            list[SetterSpecificity.FromBinding] = "binding";
            list[SetterSpecificity.ManualValueSetter] = "manual";
            list[SetterSpecificity.DynamicResourceSetter] = "dynamic";

            // Act - Remove DefaultValue which should trigger search in lower half
            list.Remove(SetterSpecificity.DefaultValue);

            // Assert
            Assert.Equal(3, list.Count);
            Assert.Equal("dynamic", list.GetValue()); // Should still be the highest
        }

        /// <summary>
        /// Tests removing the first item in a multi-item list
        /// </summary>
        [Fact]
        public void Remove_FirstItem_ShiftsRemainingItems()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "first";
            list[SetterSpecificity.FromBinding] = "second";
            list[SetterSpecificity.ManualValueSetter] = "third";

            // Act
            list.Remove(SetterSpecificity.DefaultValue);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal("third", list.GetValue());
            Assert.Equal("second", list[SetterSpecificity.FromBinding]);
        }

        /// <summary>
        /// Tests removing a middle item in a multi-item list
        /// </summary>
        [Fact]
        public void Remove_MiddleItem_ShiftsRemainingItems()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "first";
            list[SetterSpecificity.FromBinding] = "middle";
            list[SetterSpecificity.ManualValueSetter] = "last";

            // Act
            list.Remove(SetterSpecificity.FromBinding);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal("last", list.GetValue());
            Assert.Equal("first", list[SetterSpecificity.DefaultValue]);
        }

        /// <summary>
        /// Tests removing the last item in a multi-item list
        /// </summary>
        [Fact]
        public void Remove_LastItem_ReducesCount()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "first";
            list[SetterSpecificity.FromBinding] = "second";
            list[SetterSpecificity.ManualValueSetter] = "last";

            // Act
            list.Remove(SetterSpecificity.ManualValueSetter);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Equal("second", list.GetValue());
        }

        /// <summary>
        /// Tests removing the only item in a single-item list
        /// </summary>
        [Fact]
        public void Remove_OnlyItem_EmptiesList()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            list[SetterSpecificity.DefaultValue] = "only";

            // Act
            list.Remove(SetterSpecificity.DefaultValue);

            // Assert
            Assert.Equal(0, list.Count);
        }

        /// <summary>
        /// Tests removing items with different specificity values to exercise all comparison paths
        /// </summary>
        [Theory]
        [InlineData(nameof(SetterSpecificity.DefaultValue))]
        [InlineData(nameof(SetterSpecificity.FromBinding))]
        [InlineData(nameof(SetterSpecificity.ManualValueSetter))]
        [InlineData(nameof(SetterSpecificity.DynamicResourceSetter))]
        public void Remove_DifferentSpecificities_RemovesCorrectItem(string specificityName)
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            var specificities = new Dictionary<string, SetterSpecificity>
            {
                [nameof(SetterSpecificity.DefaultValue)] = SetterSpecificity.DefaultValue,
                [nameof(SetterSpecificity.FromBinding)] = SetterSpecificity.FromBinding,
                [nameof(SetterSpecificity.ManualValueSetter)] = SetterSpecificity.ManualValueSetter,
                [nameof(SetterSpecificity.DynamicResourceSetter)] = SetterSpecificity.DynamicResourceSetter
            };

            // Add all items
            foreach (var kvp in specificities)
            {
                list[kvp.Value] = kvp.Key;
            }

            var initialCount = list.Count;
            var targetSpecificity = specificities[specificityName];

            // Act
            list.Remove(targetSpecificity);

            // Assert
            Assert.Equal(initialCount - 1, list.Count);
            Assert.Null(list[targetSpecificity]); // Removed item should return null
        }

        /// <summary>
        /// Tests binary search with a large number of items to exercise all search paths
        /// </summary>
        [Fact]
        public void Remove_ManyItems_HandlesComplexSearch()
        {
            // Arrange
            var list = new SetterSpecificityList<string>();
            var specificities = new[]
            {
                SetterSpecificity.DefaultValue,
                SetterSpecificity.FromBinding,
                SetterSpecificity.ManualValueSetter,
                SetterSpecificity.DynamicResourceSetter,
                SetterSpecificity.Trigger,
                SetterSpecificity.VisualStateSetter
            };

            // Add all items
            for (int i = 0; i < specificities.Length; i++)
            {
                list[specificities[i]] = $"value{i}";
            }

            var initialCount = list.Count;

            // Act - Remove an item from the middle to exercise binary search
            list.Remove(SetterSpecificity.ManualValueSetter);

            // Assert
            Assert.Equal(initialCount - 1, list.Count);
            Assert.Null(list[SetterSpecificity.ManualValueSetter]);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes successfully and creates an instance with zero count.
        /// </summary>
        [Fact]
        public void Constructor_InitializesSuccessfully_CreatesInstanceWithZeroCount()
        {
            // Act
            var list = new SetterSpecificityList<object>();

            // Assert
            Assert.Equal(0, list.Count);
        }

        /// <summary>
        /// Tests that the parameterless constructor works correctly with different reference types.
        /// </summary>
        [Theory]
        [InlineData(typeof(object))]
        [InlineData(typeof(string))]
        public void Constructor_WithDifferentGenericTypes_InitializesCorrectly(Type genericType)
        {
            // Act & Assert - Using reflection to create instances with different generic types
            var listType = typeof(SetterSpecificityList<>).MakeGenericType(genericType);
            var instance = Activator.CreateInstance(listType);
            var countProperty = listType.GetProperty("Count");

            // Assert
            Assert.NotNull(instance);
            Assert.Equal(0, countProperty.GetValue(instance));
        }

        /// <summary>
        /// Tests that multiple instances created with the parameterless constructor are independent.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_AreIndependent()
        {
            // Act
            var list1 = new SetterSpecificityList<object>();
            var list2 = new SetterSpecificityList<object>();

            // Assert
            Assert.NotSame(list1, list2);
            Assert.Equal(0, list1.Count);
            Assert.Equal(0, list2.Count);
        }

        /// <summary>
        /// Tests that the parameterless constructor initializes the instance to a valid initial state.
        /// </summary>
        [Fact]
        public void Constructor_InitializesValidState_CanAccessIndexer()
        {
            // Act
            var list = new SetterSpecificityList<object>();

            // Assert - Verify we can access the indexer without exceptions (should return null for empty list)
            var value = list[SetterSpecificity.DefaultValue];
            Assert.Null(value);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates an instance that reports empty state correctly.
        /// </summary>
        [Fact]
        public void Constructor_CreatesEmptyList_ReturnsDefaultSpecificityAndValue()
        {
            // Act
            var list = new SetterSpecificityList<object>();
            var pair = list.GetSpecificityAndValue();

            // Assert
            Assert.Null(pair.Value);
            Assert.Equal(default(SetterSpecificity), pair.Key);
        }
    }
}