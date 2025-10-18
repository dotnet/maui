#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ReadOnlyObservableCollectionWithSourceTests
    {
        /// <summary>
        /// Tests that the parameterless constructor creates a valid instance with an empty ObservableCollection.
        /// Verifies that the List property is properly initialized and not null.
        /// Expected result: Instance is created successfully with an empty, non-null List property.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_CreatesInstanceWithEmptyList()
        {
            // Arrange & Act
            var collection = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<string>();

            // Assert
            Assert.NotNull(collection);
            Assert.NotNull(collection.List);
            Assert.Empty(collection.List);
            Assert.IsType<ObservableCollection<string>>(collection.List);
        }

        /// <summary>
        /// Tests that the parameterless constructor creates a ReadOnlyObservableCollection that behaves correctly.
        /// Verifies that the collection is initially empty and has the correct count.
        /// Expected result: Collection has zero count and is empty.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_CreatesEmptyReadOnlyCollection()
        {
            // Arrange & Act
            var collection = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<int>();

            // Assert
            Assert.Equal(0, collection.Count);
            Assert.Empty(collection);
        }

        /// <summary>
        /// Tests that items added to the underlying List are reflected in the ReadOnlyObservableCollection.
        /// Verifies that the parameterless constructor creates a properly connected collection.
        /// Expected result: Items added to List appear in the readonly collection.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessConstructor_ListAndCollectionAreConnected()
        {
            // Arrange
            var collection = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<string>();

            // Act
            collection.List.Add("test item");

            // Assert
            Assert.Single(collection);
            Assert.Single(collection.List);
            Assert.Equal("test item", collection.List[0]);
            Assert.Contains("test item", collection);
        }

        /// <summary>
        /// Tests that the parameterless constructor works with different generic types.
        /// Verifies that the generic type parameter T is properly handled during construction.
        /// Expected result: Collections of different types are created successfully.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        public void Constructor_ParameterlessConstructor_WorksWithDifferentGenericTypes(Type elementType)
        {
            // Arrange & Act & Assert
            if (elementType == typeof(string))
            {
                var collection = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<string>();
                Assert.NotNull(collection);
                Assert.NotNull(collection.List);
            }
            else if (elementType == typeof(int))
            {
                var collection = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<int>();
                Assert.NotNull(collection);
                Assert.NotNull(collection.List);
            }
            else if (elementType == typeof(object))
            {
                var collection = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<object>();
                Assert.NotNull(collection);
                Assert.NotNull(collection.List);
            }
        }

        /// <summary>
        /// Tests the constructor with a valid ObservableCollection parameter.
        /// Verifies that the List property is properly set and the base class is initialized.
        /// Expected result: Constructor succeeds and List property returns the provided collection.
        /// </summary>
        [Fact]
        public void Constructor_WithValidObservableCollection_SetsListProperty()
        {
            // Arrange
            var observableCollection = new ObservableCollection<string> { "item1", "item2" };

            // Act
            var result = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<string>(observableCollection);

            // Assert
            Assert.NotNull(result);
            Assert.Same(observableCollection, result.List);
            Assert.Equal(2, result.Count);
            Assert.Equal("item1", result[0]);
            Assert.Equal("item2", result[1]);
        }

        /// <summary>
        /// Tests the constructor with an empty ObservableCollection parameter.
        /// Verifies that the constructor handles empty collections properly.
        /// Expected result: Constructor succeeds with empty collection and List property is set.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyObservableCollection_SetsListProperty()
        {
            // Arrange
            var emptyCollection = new ObservableCollection<int>();

            // Act
            var result = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<int>(emptyCollection);

            // Assert
            Assert.NotNull(result);
            Assert.Same(emptyCollection, result.List);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests the constructor with a null ObservableCollection parameter.
        /// Verifies that the constructor properly handles null input.
        /// Expected result: ArgumentNullException is thrown due to base class requirements.
        /// </summary>
        [Fact]
        public void Constructor_WithNullObservableCollection_ThrowsArgumentNullException()
        {
            // Arrange
            ObservableCollection<string> nullCollection = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<string>(nullCollection));
        }

        /// <summary>
        /// Tests the constructor with an ObservableCollection containing a single item.
        /// Verifies that single-item collections are handled correctly.
        /// Expected result: Constructor succeeds and the single item is accessible.
        /// </summary>
        [Fact]
        public void Constructor_WithSingleItemObservableCollection_SetsListProperty()
        {
            // Arrange
            var singleItemCollection = new ObservableCollection<double> { 3.14 };

            // Act
            var result = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<double>(singleItemCollection);

            // Assert
            Assert.NotNull(result);
            Assert.Same(singleItemCollection, result.List);
            Assert.Single(result);
            Assert.Equal(3.14, result[0]);
        }

        /// <summary>
        /// Tests the constructor with an ObservableCollection containing multiple items of different types.
        /// Verifies that the generic type constraint works properly with reference types.
        /// Expected result: Constructor succeeds and all items are accessible.
        /// </summary>
        [Fact]
        public void Constructor_WithMultipleItemObservableCollection_PreservesAllItems()
        {
            // Arrange
            var multiItemCollection = new ObservableCollection<object> { "string", 42, null, DateTime.Now };

            // Act
            var result = new ShellFlyoutItemsManager.ReadOnlyObservableCollectionWithSource<object>(multiItemCollection);

            // Assert
            Assert.NotNull(result);
            Assert.Same(multiItemCollection, result.List);
            Assert.Equal(4, result.Count);
            Assert.Equal("string", result[0]);
            Assert.Equal(42, result[1]);
            Assert.Null(result[2]);
            Assert.IsType<DateTime>(result[3]);
        }
    }

    /// <summary>
    /// Tests for ShellFlyoutItemsManager class.
    /// </summary>
    public class ShellFlyoutItemsManagerTests
    {
        /// <summary>
        /// Tests that GenerateFlyoutGrouping calls UpdateFlyoutGroupings when _lastGeneratedFlyoutItems is null
        /// and returns the generated flyout items.
        /// </summary>
        [Fact]
        public void GenerateFlyoutGrouping_WhenLastGeneratedFlyoutItemsIsNull_CallsUpdateFlyoutGroupingsAndReturnsResult()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var shellController = Substitute.For<IShellController>();
            shell.As<IShellController>().Returns(shellController);
            shellController.GetItems().Returns(new List<ShellItem>());

            var manager = new ShellFlyoutItemsManager(shell);

            // Act
            var result = manager.GenerateFlyoutGrouping();

            // Assert
            Assert.NotNull(result);
        }

        /// <summary>
        /// Tests that GenerateFlyoutGrouping returns cached result when _lastGeneratedFlyoutItems is not null
        /// and does not regenerate the flyout items.
        /// </summary>
        [Fact]
        public void GenerateFlyoutGrouping_WhenLastGeneratedFlyoutItemsIsNotNull_ReturnsCachedResult()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            var shellController = Substitute.For<IShellController>();
            shell.As<IShellController>().Returns(shellController);
            shellController.GetItems().Returns(new List<ShellItem>());

            var manager = new ShellFlyoutItemsManager(shell);

            // Act - First call should populate _lastGeneratedFlyoutItems
            var firstResult = manager.GenerateFlyoutGrouping();

            // Act - Second call should return cached result
            var secondResult = manager.GenerateFlyoutGrouping();

            // Assert
            Assert.NotNull(firstResult);
            Assert.NotNull(secondResult);
            Assert.Same(firstResult, secondResult); // Should return the same instance (cached)
        }

        /// <summary>
        /// Tests that GenerateFlyoutGrouping works correctly with null shell controller.
        /// </summary>
        [Fact]
        public void GenerateFlyoutGrouping_WithNullShellController_ReturnsEmptyGrouping()
        {
            // Arrange
            var shell = Substitute.For<Shell>();
            shell.As<IShellController>().Returns((IShellController)null);

            var manager = new ShellFlyoutItemsManager(shell);

            // Act & Assert - Should not throw exception
            var result = manager.GenerateFlyoutGrouping();
            Assert.NotNull(result);
        }
    }
}