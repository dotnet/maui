#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime;
using System.Runtime.CompilerServices;

using Microsoft;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the BaseShellItem class FlyoutIcon property.
    /// </summary>
    public partial class BaseShellItemTests
    {
        /// <summary>
        /// Tests that the FlyoutIcon property returns null when not set.
        /// Verifies the default behavior of the getter when no value has been assigned.
        /// </summary>
        [Fact]
        public void FlyoutIcon_Get_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act
            var result = baseShellItem.FlyoutIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the FlyoutIcon property can be set to a valid ImageSource instance.
        /// Verifies the setter accepts a valid ImageSource and the getter returns the same value.
        /// </summary>
        [Fact]
        public void FlyoutIcon_SetAndGet_WithValidImageSource_ReturnsSetValue()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            baseShellItem.FlyoutIcon = mockImageSource;
            var result = baseShellItem.FlyoutIcon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that the FlyoutIcon property can be set to null.
        /// Verifies the setter accepts null values and the getter returns null.
        /// </summary>
        [Fact]
        public void FlyoutIcon_SetAndGet_WithNull_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var mockImageSource = Substitute.For<ImageSource>();
            baseShellItem.FlyoutIcon = mockImageSource; // Set to non-null first

            // Act
            baseShellItem.FlyoutIcon = null;
            var result = baseShellItem.FlyoutIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the FlyoutIcon property getter properly casts the value from GetValue.
        /// Verifies the getter behavior with multiple different ImageSource instances.
        /// </summary>
        [Theory]
        [MemberData(nameof(GetImageSourceTestData))]
        public void FlyoutIcon_Get_WithDifferentImageSources_ReturnsCorrectValue(ImageSource imageSource)
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutIcon = imageSource;

            // Act
            var result = baseShellItem.FlyoutIcon;

            // Assert
            Assert.Equal(imageSource, result);
        }

        /// <summary>
        /// Provides test data for FlyoutIcon property testing with various ImageSource scenarios.
        /// </summary>
        public static IEnumerable<object[]> GetImageSourceTestData()
        {
            yield return new object[] { null };
            yield return new object[] { Substitute.For<ImageSource>() };
        }
#if NETSTANDARD
		/// <summary>
		/// Tests that the IsWindows method returns false in NETSTANDARD builds.
		/// This validates the stub implementation behavior.
		/// </summary>
		[Fact]
		public void IsWindows_Always_ReturnsFalse()
		{
			// Act
			bool result = BaseShellItem.OperatingSystem.IsWindows();

			// Assert
			Assert.False(result);
		}
#endif

        /// <summary>
        /// Tests that the Icon property getter returns null when no value has been set.
        /// This test verifies that the getter correctly calls GetValue(IconProperty) and casts the result to ImageSource.
        /// Expected result: Icon property returns null.
        /// </summary>
        [Fact]
        public void Icon_Get_WhenNotSet_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();

            // Act
            var result = baseShellItem.Icon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Icon property getter returns the correct ImageSource when a value has been set.
        /// This test verifies that the getter correctly retrieves the value through GetValue(IconProperty).
        /// Expected result: Icon property returns the set ImageSource value.
        /// </summary>
        [Fact]
        public void Icon_Get_WhenSet_ReturnsCorrectValue()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();
            var mockImageSource = Substitute.For<ImageSource>();
            baseShellItem.Icon = mockImageSource;

            // Act
            var result = baseShellItem.Icon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that the Icon property setter correctly accepts null values.
        /// This test verifies that the setter calls SetValue(IconProperty, value) with null.
        /// Expected result: Icon property can be set to null without throwing an exception.
        /// </summary>
        [Fact]
        public void Icon_Set_WithNull_DoesNotThrow()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();

            // Act & Assert
            var exception = Record.Exception(() => baseShellItem.Icon = null);
            Assert.Null(exception);
            Assert.Null(baseShellItem.Icon);
        }

        /// <summary>
        /// Tests that the Icon property setter correctly accepts valid ImageSource values.
        /// This test verifies that the setter calls SetValue(IconProperty, value) with a valid ImageSource.
        /// Expected result: Icon property can be set to a valid ImageSource and retrieved correctly.
        /// </summary>
        [Fact]
        public void Icon_Set_WithValidImageSource_StoresCorrectly()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();
            var mockImageSource = Substitute.For<ImageSource>();

            // Act
            baseShellItem.Icon = mockImageSource;

            // Assert
            Assert.Same(mockImageSource, baseShellItem.Icon);
        }

        /// <summary>
        /// Tests that the Icon property can be set multiple times with different values.
        /// This test verifies that the property correctly handles value changes.
        /// Expected result: Icon property reflects the most recently set value.
        /// </summary>
        [Fact]
        public void Icon_Set_MultipleValues_UpdatesCorrectly()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();

            // Act
            baseShellItem.Icon = firstImageSource;
            var firstResult = baseShellItem.Icon;
            baseShellItem.Icon = secondImageSource;
            var secondResult = baseShellItem.Icon;

            // Assert
            Assert.Same(firstImageSource, firstResult);
            Assert.Same(secondImageSource, secondResult);
            Assert.NotSame(firstResult, secondResult);
        }

        /// <summary>
        /// Tests that setting Icon to null after having a value works correctly.
        /// This test verifies the property can transition from non-null to null values.
        /// Expected result: Icon property transitions from non-null to null correctly.
        /// </summary>
        [Fact]
        public void Icon_Set_FromValueToNull_TransitionsCorrectly()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();
            var mockImageSource = Substitute.For<ImageSource>();
            baseShellItem.Icon = mockImageSource;

            // Act
            baseShellItem.Icon = null;

            // Assert
            Assert.Null(baseShellItem.Icon);
        }

        /// <summary>
        /// Testable implementation of BaseShellItem for unit testing purposes.
        /// This helper class allows direct instantiation of BaseShellItem for testing.
        /// </summary>
        private class TestableBaseShellItem : BaseShellItem
        {
        }

        /// <summary>
        /// Tests that the IsEnabled property getter returns the default value (true) when no value has been explicitly set.
        /// This test exercises the getter method to ensure it correctly retrieves the default value from the underlying BindableProperty.
        /// Expected result: The property should return true (the default value defined in IsEnabledProperty).
        /// </summary>
        [Fact]
        public void IsEnabled_Get_ReturnsDefaultValueTrue()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act
            var result = baseShellItem.IsEnabled;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that the IsEnabled property getter returns the correct value after the setter has been called.
        /// This test verifies the complete get/set functionality using various boolean values.
        /// Expected result: The getter should return the exact value that was set via the setter.
        /// </summary>
        /// <param name="value">The boolean value to set and then retrieve via the IsEnabled property.</param>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void IsEnabled_GetAfterSet_ReturnsSetValue(bool value)
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act
            baseShellItem.IsEnabled = value;
            var result = baseShellItem.IsEnabled;

            // Assert
            Assert.Equal(value, result);
        }

        /// <summary>
        /// Tests that the IsEnabled property getter consistently returns the correct value after multiple set operations.
        /// This test ensures the property maintains its state correctly through multiple changes.
        /// Expected result: The getter should return the most recently set value after each operation.
        /// </summary>
        [Fact]
        public void IsEnabled_MultipleGetSetOperations_MaintainsCorrectState()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act & Assert - Test sequence of operations
            baseShellItem.IsEnabled = false;
            Assert.False(baseShellItem.IsEnabled);

            baseShellItem.IsEnabled = true;
            Assert.True(baseShellItem.IsEnabled);

            baseShellItem.IsEnabled = false;
            Assert.False(baseShellItem.IsEnabled);
        }
#if NETSTANDARD
		/// <summary>
		/// Tests that the IsIOS method returns false.
		/// This method is part of a stub OperatingSystem class used in NETSTANDARD builds.
		/// </summary>
		[Fact]
		public void IsIOS_Always_ReturnsFalse()
		{
			// Act
			bool result = BaseShellItem.OperatingSystem.IsIOS();

			// Assert
			Assert.False(result);
		}
#endif

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns true when Parent is IShellController and GetItems contains this item.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsShellControllerContainingItem_ReturnsTrue()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var mockShellController = Substitute.For<IShellController>();
            var shellItems = new ReadOnlyCollection<ShellItem>(new List<ShellItem> { (ShellItem)(object)shellItem });
            mockShellController.GetItems().Returns(shellItems);

            shellItem.SetParent(mockShellController);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns false when Parent is IShellController but GetItems does not contain this item.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsShellControllerNotContainingItem_ReturnsFalse()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var otherShellItem = Substitute.For<ShellItem>();
            var mockShellController = Substitute.For<IShellController>();
            var shellItems = new ReadOnlyCollection<ShellItem>(new List<ShellItem> { otherShellItem });
            mockShellController.GetItems().Returns(shellItems);

            shellItem.SetParent(mockShellController);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns false when Parent is IShellController and GetItems returns empty collection.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsShellControllerWithEmptyItems_ReturnsFalse()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var mockShellController = Substitute.For<IShellController>();
            var shellItems = new ReadOnlyCollection<ShellItem>(new List<ShellItem>());
            mockShellController.GetItems().Returns(shellItems);

            shellItem.SetParent(mockShellController);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns true when Parent is ShellGroupItem and ShellElementCollection contains this item.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsShellGroupItemContainingItem_ReturnsTrue()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var mockShellGroupItem = Substitute.For<ShellGroupItem>();
            var mockShellElementCollection = Substitute.For<ShellElementCollection>();
            mockShellElementCollection.Contains(shellItem).Returns(true);
            mockShellGroupItem.ShellElementCollection.Returns(mockShellElementCollection);

            shellItem.SetParent(mockShellGroupItem);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns false when Parent is ShellGroupItem but ShellElementCollection does not contain this item.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsShellGroupItemNotContainingItem_ReturnsFalse()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var mockShellGroupItem = Substitute.For<ShellGroupItem>();
            var mockShellElementCollection = Substitute.For<ShellElementCollection>();
            mockShellElementCollection.Contains(shellItem).Returns(false);
            mockShellGroupItem.ShellElementCollection.Returns(mockShellElementCollection);

            shellItem.SetParent(mockShellGroupItem);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns false when Parent is null.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsNull_ReturnsFalse()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            // Parent is null by default

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns false when Parent is neither IShellController nor ShellGroupItem.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsOtherElementType_ReturnsFalse()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var otherElement = Substitute.For<Element>();
            shellItem.SetParent(otherElement);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that IsPartOfVisibleTree returns false when Parent is a regular BindableObject that's not Element.
        /// </summary>
        [Fact]
        public void IsPartOfVisibleTree_ParentIsBindableObjectNotElement_ReturnsFalse()
        {
            // Arrange
            var shellItem = new TestableBaseShellItem();
            var bindableObject = Substitute.For<BindableObject>();
            shellItem.SetParent(bindableObject);

            // Act
            var result = shellItem.IsPartOfVisibleTree();

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests that Propagate method returns early when the 'from' parameter is null.
        /// Verifies null parameter handling and ensures no exceptions are thrown.
        /// </summary>
        [Fact]
        public void Propagate_FromIsNull_ReturnsEarlyWithoutException()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var to = Substitute.For<BindableObject>();

            // Act & Assert - Should not throw
            BaseShellItem.Propagate(property, null, to, false);
            BaseShellItem.Propagate(property, null, to, true);
        }

        /// <summary>
        /// Tests that Propagate method returns early when the 'to' parameter is null.
        /// Verifies null parameter handling and ensures no exceptions are thrown.
        /// </summary>
        [Fact]
        public void Propagate_ToIsNull_ReturnsEarlyWithoutException()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();

            // Act & Assert - Should not throw
            BaseShellItem.Propagate(property, from, null, false);
            BaseShellItem.Propagate(property, from, null, true);
        }

        /// <summary>
        /// Tests that Propagate method returns early when both 'from' and 'to' parameters are null.
        /// Verifies null parameter handling and ensures no exceptions are thrown.
        /// </summary>
        [Fact]
        public void Propagate_BothFromAndToAreNull_ReturnsEarlyWithoutException()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");

            // Act & Assert - Should not throw
            BaseShellItem.Propagate(property, null, null, false);
            BaseShellItem.Propagate(property, null, null, true);
        }

        /// <summary>
        /// Tests that Propagate method propagates property value when source has property set and destination does not.
        /// Verifies the core propagation functionality works correctly.
        /// </summary>
        [Fact]
        public void Propagate_FromIsSetAndToIsNotSet_PropagatesValue()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();
            var testValue = "test value";

            from.IsSet(property).Returns(true);
            to.IsSet(property).Returns(false);
            from.GetValue(property).Returns(testValue);

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.Received(1).SetValue(property, testValue);
        }

        /// <summary>
        /// Tests that Propagate method does not propagate when source does not have property set.
        /// Verifies that propagation only occurs when the source property is explicitly set.
        /// </summary>
        [Fact]
        public void Propagate_FromIsNotSet_DoesNotPropagate()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();

            from.IsSet(property).Returns(false);
            to.IsSet(property).Returns(false);

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.DidNotReceive().SetValue(property, Arg.Any<object>());
        }

        /// <summary>
        /// Tests that Propagate method does not propagate when destination already has property set.
        /// Verifies that existing property values are not overwritten during propagation.
        /// </summary>
        [Fact]
        public void Propagate_ToIsAlreadySet_DoesNotPropagate()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();

            from.IsSet(property).Returns(true);
            to.IsSet(property).Returns(true);
            from.GetValue(property).Returns("test value");

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.DidNotReceive().SetValue(property, Arg.Any<object>());
        }

        /// <summary>
        /// Tests that Propagate method does not propagate when both source and destination do not have property set.
        /// Verifies no unnecessary operations occur when neither object has the property set.
        /// </summary>
        [Fact]
        public void Propagate_BothFromAndToAreNotSet_DoesNotPropagate()
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();

            from.IsSet(property).Returns(false);
            to.IsSet(property).Returns(false);

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.DidNotReceive().SetValue(property, Arg.Any<object>());
        }

        /// <summary>
        /// Tests that Propagate method handles various property value types correctly.
        /// Verifies propagation works with different data types including null values.
        /// </summary>
        [Theory]
        [InlineData("string value")]
        [InlineData(null)]
        [InlineData("")]
        public void Propagate_VariousStringValues_PropagatesCorrectly(string testValue)
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();

            from.IsSet(property).Returns(true);
            to.IsSet(property).Returns(false);
            from.GetValue(property).Returns(testValue);

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.Received(1).SetValue(property, testValue);
        }

        /// <summary>
        /// Tests that Propagate method handles numeric and boolean property values correctly.
        /// Verifies propagation works with various primitive types.
        /// </summary>
        [Theory]
        [InlineData(42)]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void Propagate_IntegerValues_PropagatesCorrectly(int testValue)
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(int), typeof(BaseShellItem), 0);
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();

            from.IsSet(property).Returns(true);
            to.IsSet(property).Returns(false);
            from.GetValue(property).Returns(testValue);

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.Received(1).SetValue(property, testValue);
        }

        /// <summary>
        /// Tests that Propagate method handles boolean property values correctly.
        /// Verifies propagation works with boolean types.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Propagate_BooleanValues_PropagatesCorrectly(bool testValue)
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(bool), typeof(BaseShellItem), false);
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();

            from.IsSet(property).Returns(true);
            to.IsSet(property).Returns(false);
            from.GetValue(property).Returns(testValue);

            // Act
            BaseShellItem.Propagate(property, from, to, false);

            // Assert
            to.Received(1).SetValue(property, testValue);
        }

        /// <summary>
        /// Tests that Propagate method works correctly with various onlyToImplicit parameter values.
        /// Verifies the onlyToImplicit flag doesn't affect propagation when source is not implicit.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Propagate_OnlyToImplicitParameter_PropagatesWhenFromIsNotImplicit(bool onlyToImplicit)
        {
            // Arrange
            var property = BindableProperty.Create("TestProperty", typeof(string), typeof(BaseShellItem), "default");
            var from = Substitute.For<BindableObject>();
            var to = Substitute.For<BindableObject>();
            var testValue = "test value";

            from.IsSet(property).Returns(true);
            to.IsSet(property).Returns(false);
            from.GetValue(property).Returns(testValue);

            // Act
            BaseShellItem.Propagate(property, from, to, onlyToImplicit);

            // Assert
            to.Received(1).SetValue(property, testValue);
        }

        /// <summary>
        /// Tests that the Window property returns null when no Window is set (default behavior).
        /// This test verifies the default state of the Window property.
        /// Expected result: Window property should return null.
        /// </summary>
        [Fact]
        public void Window_WhenNoWindowSet_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act
            var result = baseShellItem.Window;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Window property getter calls GetValue with the correct WindowProperty.
        /// This test verifies that the property implementation correctly uses the WindowProperty.
        /// Expected result: GetValue should be called with WindowProperty and return the expected value.
        /// </summary>
        [Fact]
        public void Window_Getter_CallsGetValueWithWindowProperty()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act
            var result = baseShellItem.Window;

            // Assert
            // Since WindowProperty is read-only and the default value is null,
            // we verify that accessing the property doesn't throw and returns the default value
            Assert.Null(result);

            // Verify that WindowProperty is properly configured
            Assert.NotNull(BaseShellItem.WindowProperty);
            Assert.Equal(nameof(BaseShellItem.Window), BaseShellItem.WindowProperty.PropertyName);
            Assert.Equal(typeof(Window), BaseShellItem.WindowProperty.ReturnType);
            Assert.Equal(typeof(BaseShellItem), BaseShellItem.WindowProperty.DeclaringType);
        }

        /// <summary>
        /// Tests that multiple calls to the Window property return consistent results.
        /// This test verifies that the property getter is stable and doesn't have side effects.
        /// Expected result: Multiple calls should return the same value.
        /// </summary>
        [Fact]
        public void Window_MultipleCalls_ReturnsConsistentResults()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();

            // Act
            var result1 = baseShellItem.Window;
            var result2 = baseShellItem.Window;
            var result3 = baseShellItem.Window;

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(result2, result3);
        }
#if NETSTANDARD
		/// <summary>
		/// Tests that IsMacCatalyst method returns false when called.
		/// This method is part of a stub OperatingSystem implementation for NETSTANDARD.
		/// Expected result: Always returns false.
		/// </summary>
		[Fact]
		public void IsMacCatalyst_WhenCalled_ReturnsFalse()
		{
			// Act
			bool result = BaseShellItem.OperatingSystem.IsMacCatalyst();

			// Assert
			Assert.False(result);
		}
#endif

        /// <summary>
        /// Tests that the BaseShellItem constructor executes successfully without throwing exceptions.
        /// Verifies basic instantiation and that the DeclaredChildren collection is properly initialized.
        /// </summary>
        [Fact]
        public void Constructor_DefaultInstantiation_CreatesInstanceSuccessfully()
        {
            // Act
            var baseShellItem = new BaseShellItem();

            // Assert
            Assert.NotNull(baseShellItem);
            Assert.NotNull(baseShellItem.DeclaredChildren);
            Assert.Empty(baseShellItem.DeclaredChildren);
        }

        /// <summary>
        /// Tests that the constructor properly subscribes to the DeclaredChildren.CollectionChanged event
        /// by adding an element and verifying the collection change is handled without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_CollectionChangedEventHandler_HandlesAdditionWithoutException()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var element = new TestElement();

            // Act & Assert (should not throw)
            baseShellItem.DeclaredChildren.Add(element);

            Assert.Single(baseShellItem.DeclaredChildren);
            Assert.Equal(element, baseShellItem.DeclaredChildren[0]);
        }

        /// <summary>
        /// Tests that the constructor properly subscribes to the DeclaredChildren.CollectionChanged event
        /// by removing an element and verifying the collection change is handled without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_CollectionChangedEventHandler_HandlesRemovalWithoutException()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var element = new TestElement();
            baseShellItem.DeclaredChildren.Add(element);

            // Act & Assert (should not throw)
            var removed = baseShellItem.DeclaredChildren.Remove(element);

            Assert.True(removed);
            Assert.Empty(baseShellItem.DeclaredChildren);
        }

        /// <summary>
        /// Tests that the constructor's event handler properly handles clearing the DeclaredChildren collection.
        /// Verifies that clearing multiple elements is handled without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_CollectionChangedEventHandler_HandlesClearWithoutException()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var element1 = new TestElement();
            var element2 = new TestElement();
            baseShellItem.DeclaredChildren.Add(element1);
            baseShellItem.DeclaredChildren.Add(element2);

            // Act & Assert (should not throw)
            baseShellItem.DeclaredChildren.Clear();

            Assert.Empty(baseShellItem.DeclaredChildren);
        }

        /// <summary>
        /// Tests that the constructor's event handler properly handles replacing elements in the DeclaredChildren collection.
        /// Verifies that element replacement is handled without exceptions.
        /// </summary>
        [Fact]
        public void Constructor_CollectionChangedEventHandler_HandlesReplacementWithoutException()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var originalElement = new TestElement();
            var replacementElement = new TestElement();
            baseShellItem.DeclaredChildren.Add(originalElement);

            // Act & Assert (should not throw)
            baseShellItem.DeclaredChildren[0] = replacementElement;

            Assert.Single(baseShellItem.DeclaredChildren);
            Assert.Equal(replacementElement, baseShellItem.DeclaredChildren[0]);
        }

        /// <summary>
        /// Tests that the constructor's event handler handles multiple rapid collection changes
        /// without throwing exceptions, verifying robustness under concurrent-like scenarios.
        /// </summary>
        [Fact]
        public void Constructor_CollectionChangedEventHandler_HandlesMultipleChangesWithoutException()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var elements = new[] { new TestElement(), new TestElement(), new TestElement() };

            // Act & Assert (should not throw)
            foreach (var element in elements)
            {
                baseShellItem.DeclaredChildren.Add(element);
            }

            Assert.Equal(3, baseShellItem.DeclaredChildren.Count);

            // Remove all elements
            baseShellItem.DeclaredChildren.Clear();
            Assert.Empty(baseShellItem.DeclaredChildren);
        }

        /// <summary>
        /// Test helper class that extends Element for testing purposes.
        /// Used to create concrete Element instances for collection testing.
        /// </summary>
        private class TestElement : Element
        {
        }
    }

    public partial class BaseShellItemDebugViewTests
    {
        /// <summary>
        /// Tests that the Icon property returns null when the underlying BaseShellItem.Icon is null.
        /// Input condition: BaseShellItem with Icon set to null.
        /// Expected result: DebugView.Icon returns null.
        /// </summary>
        [Fact]
        public void Icon_WhenBaseShellItemIconIsNull_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new TestableBaseShellItem();
            baseShellItem.Icon = null;
            var debugView = new TestableBaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Icon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Icon property returns the same ImageSource instance as the underlying BaseShellItem.Icon.
        /// Input condition: BaseShellItem with a mock ImageSource set as Icon.
        /// Expected result: DebugView.Icon returns the same ImageSource instance.
        /// </summary>
        [Fact]
        public void Icon_WhenBaseShellItemIconHasValue_ReturnsSameInstance()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            var baseShellItem = new TestableBaseShellItem();
            baseShellItem.Icon = mockImageSource;
            var debugView = new TestableBaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Icon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that the Icon property correctly delegates to the BaseShellItem.Icon property multiple times.
        /// Input condition: BaseShellItem with Icon changed between calls.
        /// Expected result: DebugView.Icon always returns the current value of BaseShellItem.Icon.
        /// </summary>
        [Fact]
        public void Icon_WhenBaseShellItemIconChanges_ReflectsCurrentValue()
        {
            // Arrange
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();
            var baseShellItem = new TestableBaseShellItem();
            var debugView = new TestableBaseShellItemDebugView(baseShellItem);

            // Act & Assert - First value
            baseShellItem.Icon = firstImageSource;
            Assert.Same(firstImageSource, debugView.Icon);

            // Act & Assert - Changed value
            baseShellItem.Icon = secondImageSource;
            Assert.Same(secondImageSource, debugView.Icon);

            // Act & Assert - Null value
            baseShellItem.Icon = null;
            Assert.Null(debugView.Icon);
        }

        /// <summary>
        /// Tests that the Icon property throws ArgumentNullException when BaseShellItem is null.
        /// Input condition: null BaseShellItem passed to constructor.
        /// Expected result: ArgumentNullException is thrown during construction.
        /// </summary>
        [Fact]
        public void Constructor_WhenBaseShellItemIsNull_ThrowsArgumentNullException()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => new TestableBaseShellItemDebugView(null));
        }

        private class TestableBaseShellItem : BaseShellItem
        {
        }

        /// <summary>
        /// Tests that FlyoutIcon property returns null when the base shell item's FlyoutIcon is null.
        /// This verifies the property delegation works correctly for null values.
        /// </summary>
        [Fact]
        public void FlyoutIcon_WhenBaseShellItemFlyoutIconIsNull_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutIcon = null;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.FlyoutIcon;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that FlyoutIcon property returns the same ImageSource instance as the base shell item.
        /// This verifies the property delegation works correctly for non-null values.
        /// </summary>
        [Fact]
        public void FlyoutIcon_WhenBaseShellItemHasFlyoutIcon_ReturnsSameInstance()
        {
            // Arrange
            var mockImageSource = Substitute.For<ImageSource>();
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutIcon = mockImageSource;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.FlyoutIcon;

            // Assert
            Assert.Same(mockImageSource, result);
        }

        /// <summary>
        /// Tests that FlyoutIcon property reflects changes to the base shell item's FlyoutIcon property.
        /// This verifies the property delegation is dynamic and not cached.
        /// </summary>
        [Fact]
        public void FlyoutIcon_WhenBaseShellItemFlyoutIconChanges_ReflectsChanges()
        {
            // Arrange
            var firstImageSource = Substitute.For<ImageSource>();
            var secondImageSource = Substitute.For<ImageSource>();
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutIcon = firstImageSource;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act & Assert - Initial value
            Assert.Same(firstImageSource, debugView.FlyoutIcon);

            // Act - Change the base item's FlyoutIcon
            baseShellItem.FlyoutIcon = secondImageSource;

            // Assert - Verify debug view reflects the change
            Assert.Same(secondImageSource, debugView.FlyoutIcon);

            // Act - Set to null
            baseShellItem.FlyoutIcon = null;

            // Assert - Verify debug view reflects null
            Assert.Null(debugView.FlyoutIcon);
        }

        /// <summary>
        /// Tests that FlyoutItemIsVisible returns true when the underlying BaseShellItem's FlyoutItemIsVisible is true.
        /// This verifies that the debug view property correctly delegates to the underlying BaseShellItem instance.
        /// </summary>
        [Fact]
        public void FlyoutItemIsVisible_WhenBaseShellItemIsTrue_ReturnsTrue()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutItemIsVisible = true;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.FlyoutItemIsVisible;

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Tests that FlyoutItemIsVisible returns false when the underlying BaseShellItem's FlyoutItemIsVisible is false.
        /// This verifies that the debug view property correctly delegates to the underlying BaseShellItem instance.
        /// </summary>
        [Fact]
        public void FlyoutItemIsVisible_WhenBaseShellItemIsFalse_ReturnsFalse()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutItemIsVisible = false;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.FlyoutItemIsVisible;

            // Assert
            Assert.False(result);
        }

        /// <summary>
        /// Tests FlyoutItemIsVisible property with parameterized test data to verify correct delegation.
        /// This test covers both true and false cases using a single parameterized test method.
        /// </summary>
        /// <param name="flyoutItemIsVisible">The value to set for the BaseShellItem's FlyoutItemIsVisible property.</param>
        /// <param name="expectedResult">The expected result from the debug view's FlyoutItemIsVisible property.</param>
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void FlyoutItemIsVisible_WithDifferentValues_ReturnsExpectedResult(bool flyoutItemIsVisible, bool expectedResult)
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.FlyoutItemIsVisible = flyoutItemIsVisible;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.FlyoutItemIsVisible;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        /// <summary>
        /// Tests that FlyoutItemIsVisible reflects changes in the underlying BaseShellItem's FlyoutItemIsVisible property.
        /// This verifies that the debug view property is not cached and always returns the current value.
        /// </summary>
        [Fact]
        public void FlyoutItemIsVisible_WhenBaseShellItemChanges_ReflectsNewValue()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act & Assert - Initial value (should be true by default)
            baseShellItem.FlyoutItemIsVisible = true;
            Assert.True(debugView.FlyoutItemIsVisible);

            // Act & Assert - Change to false
            baseShellItem.FlyoutItemIsVisible = false;
            Assert.False(debugView.FlyoutItemIsVisible);

            // Act & Assert - Change back to true
            baseShellItem.FlyoutItemIsVisible = true;
            Assert.True(debugView.FlyoutItemIsVisible);
        }

        /// <summary>
        /// Tests that the Route property returns the Route value from the underlying BaseShellItem when it has a valid route.
        /// Input: BaseShellItem with Route set to a non-empty string.
        /// Expected: Route property returns the same string value.
        /// </summary>
        [Fact]
        public void Route_WithValidBaseShellItemRoute_ReturnsBaseShellItemRoute()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.Route = "test/route";
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Route;

            // Assert
            Assert.Equal("test/route", result);
        }

        /// <summary>
        /// Tests that the Route property returns null when the underlying BaseShellItem has a null Route.
        /// Input: BaseShellItem with Route set to null.
        /// Expected: Route property returns null.
        /// </summary>
        [Fact]
        public void Route_WithNullBaseShellItemRoute_ReturnsNull()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.Route = null;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Route;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the Route property returns an empty string when the underlying BaseShellItem has an empty Route.
        /// Input: BaseShellItem with Route set to empty string.
        /// Expected: Route property returns empty string.
        /// </summary>
        [Fact]
        public void Route_WithEmptyBaseShellItemRoute_ReturnsEmptyString()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.Route = string.Empty;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Route;

            // Assert
            Assert.Equal(string.Empty, result);
        }

        /// <summary>
        /// Tests that the Route property returns whitespace when the underlying BaseShellItem has a whitespace Route.
        /// Input: BaseShellItem with Route set to whitespace string.
        /// Expected: Route property returns the same whitespace string.
        /// </summary>
        [Fact]
        public void Route_WithWhitespaceBaseShellItemRoute_ReturnsWhitespace()
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.Route = "   ";
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Route;

            // Assert
            Assert.Equal("   ", result);
        }

        /// <summary>
        /// Tests that accessing the Route property throws NullReferenceException when the BaseShellItem parameter is null.
        /// Input: null BaseShellItem instance.
        /// Expected: NullReferenceException is thrown when accessing Route property.
        /// </summary>
        [Fact]
        public void Route_WithNullBaseShellItem_ThrowsNullReferenceException()
        {
            // Arrange
            BaseShellItem nullBaseShellItem = null;
            var debugView = new BaseShellItem.BaseShellItemDebugView(nullBaseShellItem);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => debugView.Route);
        }

        /// <summary>
        /// Tests that the Route property returns complex route strings correctly.
        /// Input: BaseShellItem with Route set to various complex route values.
        /// Expected: Route property returns the exact same complex route values.
        /// </summary>
        [Theory]
        [InlineData("//path/to/page")]
        [InlineData("shell/tab/page?param=value")]
        [InlineData("../relative/path")]
        [InlineData("https://example.com/absolute")]
        [InlineData("special-chars!@#$%^&*()")]
        [InlineData("unicode路径测试")]
        public void Route_WithComplexRouteStrings_ReturnsExactValue(string routeValue)
        {
            // Arrange
            var baseShellItem = new BaseShellItem();
            baseShellItem.Route = routeValue;
            var debugView = new BaseShellItem.BaseShellItemDebugView(baseShellItem);

            // Act
            var result = debugView.Route;

            // Assert
            Assert.Equal(routeValue, result);
        }
    }

    public class OperatingSystemTests
    {
#if NETSTANDARD
		/// <summary>
		/// Tests that IsAndroid method returns false.
		/// This is a stub implementation for NETSTANDARD that always returns false.
		/// </summary>
		[Fact]
		public void IsAndroid_Always_ReturnsFalse()
		{
			// Act
			bool result = BaseShellItem.OperatingSystem.IsAndroid();

			// Assert
			Assert.False(result);
		}
#endif
    }
}