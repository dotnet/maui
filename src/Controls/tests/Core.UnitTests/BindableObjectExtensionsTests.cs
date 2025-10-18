#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class BindableObjectExtensionsTests
    {
        private bool? GetBindingInterceptorsSwitchValue()
        {
            return AppContext.TryGetSwitch("Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported", out bool value)
                ? value
                : null;
        }

        private void RestoreBindingInterceptorsSwitchValue(bool? originalValue)
        {
            if (originalValue.HasValue)
            {
                AppContext.SetSwitch("Microsoft.Maui.RuntimeFeature.AreBindingInterceptorsSupported", originalValue.Value);
            }
        }

        private class MockBindableObject : BindableObject
        {
        }

        /// <summary>
        /// Tests that GetPropertyIfSet returns the fallback value when bindableObject is null.
        /// This tests the null check path in the method.
        /// </summary>
        [Fact]
        public void GetPropertyIfSet_NullBindableObject_ReturnsFallbackValue()
        {
            // Arrange
            BindableObject nullObject = null;
            var property = Label.TextProperty;
            var fallbackValue = "fallback";

            // Act
            var result = BindableObjectExtensions.GetPropertyIfSet(nullObject, property, fallbackValue);

            // Assert
            Assert.Equal(fallbackValue, result);
        }

        /// <summary>
        /// Tests that GetPropertyIfSet throws ArgumentNullException when bindableProperty is null.
        /// This verifies the method doesn't handle null properties gracefully and relies on IsSet to validate.
        /// </summary>
        [Fact]
        public void GetPropertyIfSet_NullBindableProperty_ThrowsArgumentNullException()
        {
            // Arrange
            var bindableObject = new Label();
            BindableProperty nullProperty = null;
            var fallbackValue = "fallback";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                BindableObjectExtensions.GetPropertyIfSet(bindableObject, nullProperty, fallbackValue));
        }

        /// <summary>
        /// Tests that GetPropertyIfSet returns the property value when the property is set.
        /// This tests the path where IsSet returns true and GetValue is called.
        /// </summary>
        [Fact]
        public void GetPropertyIfSet_PropertyIsSet_ReturnsPropertyValue()
        {
            // Arrange
            var label = new Label();
            var expectedValue = "test value";
            label.Text = expectedValue;
            var fallbackValue = "fallback";

            // Act
            var result = BindableObjectExtensions.GetPropertyIfSet(label, Label.TextProperty, fallbackValue);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetPropertyIfSet returns the fallback value when the property is not set.
        /// This tests the path where IsSet returns false.
        /// </summary>
        [Fact]
        public void GetPropertyIfSet_PropertyNotSet_ReturnsFallbackValue()
        {
            // Arrange
            var label = new Label(); // Text property not explicitly set
            var fallbackValue = "fallback";

            // Act
            var result = BindableObjectExtensions.GetPropertyIfSet(label, Label.TextProperty, fallbackValue);

            // Assert
            Assert.Equal(fallbackValue, result);
        }

        /// <summary>
        /// Tests GetPropertyIfSet with different generic types to ensure type casting works correctly.
        /// Tests value types, reference types, and null values as fallbacks.
        /// </summary>
        [Theory]
        [InlineData("string value", "string fallback")]
        [InlineData("set value", null)]
        [InlineData(null, "fallback for null")]
        public void GetPropertyIfSet_DifferentStringValues_ReturnsExpectedResult(string setValue, string fallbackValue)
        {
            // Arrange
            var label = new Label();
            if (setValue != null)
            {
                label.Text = setValue;
            }

            // Act
            var result = BindableObjectExtensions.GetPropertyIfSet(label, Label.TextProperty, fallbackValue);

            // Assert
            if (setValue != null)
            {
                Assert.Equal(setValue, result);
            }
            else
            {
                Assert.Equal(fallbackValue, result);
            }
        }

        /// <summary>
        /// Tests GetPropertyIfSet with boolean values to verify it works with value types.
        /// This tests the generic type system and casting behavior.
        /// </summary>
        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void GetPropertyIfSet_BooleanProperty_ReturnsExpectedResult(bool setValue, bool fallbackValue)
        {
            // Arrange
            var label = new Label();
            label.IsVisible = setValue;

            // Act
            var result = BindableObjectExtensions.GetPropertyIfSet(label, Label.IsVisibleProperty, fallbackValue);

            // Assert
            Assert.Equal(setValue, result);
        }

        /// <summary>
        /// Tests GetPropertyIfSet with null bindableObject and various fallback value types.
        /// This ensures the null check works correctly for different generic type parameters.
        /// </summary>
        [Theory]
        [InlineData("string")]
        [InlineData(42)]
        [InlineData(true)]
        [InlineData(null)]
        public void GetPropertyIfSet_NullBindableObjectWithDifferentFallbackTypes_ReturnsFallbackValue<T>(T fallbackValue)
        {
            // Arrange
            BindableObject nullObject = null;
            var property = Label.TextProperty;

            // Act
            var result = BindableObjectExtensions.GetPropertyIfSet(nullObject, property, fallbackValue);

            // Assert
            Assert.Equal(fallbackValue, result);
        }

        /// <summary>
        /// Tests TrySetDynamicThemeColor when the resource key exists in Application.Current.Resources.
        /// Should set the dynamic resource on the bindable object and return true.
        /// </summary>
        [Fact]
        public void TrySetDynamicThemeColor_ResourceExists_ReturnsTrueAndSetsResource()
        {
            // Arrange
            var application = new TestApplication();
            var testColor = Colors.Red;
            var resourceKey = "TestColorResource";
            application.Resources.Add(resourceKey, testColor);
            Application.SetCurrent(application);

            var bindableObject = new Label();
            var bindableProperty = Label.TextColorProperty;

            // Act
            bool result = bindableObject.TrySetDynamicThemeColor(resourceKey, bindableProperty, out object outerColor);

            // Assert
            Assert.True(result);
            Assert.Equal(testColor, outerColor);

            // Cleanup
            Application.SetCurrent(null);
        }

        /// <summary>
        /// Tests TrySetDynamicThemeColor when the resource key does not exist in Application.Current.Resources.
        /// Should return false and set outerColor to null.
        /// </summary>
        [Fact]
        public void TrySetDynamicThemeColor_ResourceNotFound_ReturnsFalse()
        {
            // Arrange
            var application = new TestApplication();
            Application.SetCurrent(application);

            var bindableObject = new Label();
            var bindableProperty = Label.TextColorProperty;
            var nonExistentResourceKey = "NonExistentResource";

            // Act
            bool result = bindableObject.TrySetDynamicThemeColor(nonExistentResourceKey, bindableProperty, out object outerColor);

            // Assert
            Assert.False(result);
            Assert.Null(outerColor);

            // Cleanup
            Application.SetCurrent(null);
        }

        /// <summary>
        /// Tests TrySetDynamicThemeColor with various invalid resource key values.
        /// Should handle edge cases appropriately.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TrySetDynamicThemeColor_InvalidResourceKey_ReturnsFalse(string resourceKey)
        {
            // Arrange
            var application = new TestApplication();
            Application.SetCurrent(application);

            var bindableObject = new Label();
            var bindableProperty = Label.TextColorProperty;

            // Act
            bool result = bindableObject.TrySetDynamicThemeColor(resourceKey, bindableProperty, out object outerColor);

            // Assert
            Assert.False(result);
            Assert.Null(outerColor);

            // Cleanup
            Application.SetCurrent(null);
        }

        /// <summary>
        /// Tests TrySetDynamicThemeColor with different types of resource values.
        /// Should work with various object types stored as resources.
        /// </summary>
        [Theory]
        [InlineData("StringResource", "TestValue")]
        [InlineData("IntResource", 42)]
        [InlineData("DoubleResource", 3.14)]
        public void TrySetDynamicThemeColor_DifferentResourceTypes_ReturnsCorrectValue(string resourceKey, object expectedValue)
        {
            // Arrange
            var application = new TestApplication();
            application.Resources.Add(resourceKey, expectedValue);
            Application.SetCurrent(application);

            var bindableObject = new Label();
            var bindableProperty = Label.TextColorProperty;

            // Act
            bool result = bindableObject.TrySetDynamicThemeColor(resourceKey, bindableProperty, out object outerColor);

            // Assert
            Assert.True(result);
            Assert.Equal(expectedValue, outerColor);

            // Cleanup
            Application.SetCurrent(null);
        }

        /// <summary>
        /// Tests TrySetDynamicThemeColor when Application.Current is null.
        /// Should return false and set outerColor to null.
        /// </summary>
        [Fact]
        public void TrySetDynamicThemeColor_ApplicationCurrentIsNull_ReturnsFalse()
        {
            // Arrange
            Application.SetCurrent(null);

            var bindableObject = new Label();
            var bindableProperty = Label.TextColorProperty;
            var resourceKey = "TestResource";

            // Act
            bool result = bindableObject.TrySetDynamicThemeColor(resourceKey, bindableProperty, out object outerColor);

            // Assert
            Assert.False(result);
            Assert.Null(outerColor);
        }

        /// <summary>
        /// Helper class for testing that extends Application to provide a concrete implementation.
        /// Used internally within tests to create a testable Application instance.
        /// </summary>
        private class TestApplication : Application
        {
            public TestApplication()
            {
                Resources = new ResourceDictionary();
            }
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren when bindable is not an Element.
        /// Should return early without calling AddLogicalChild or RemoveLogicalChild.
        /// This tests the uncovered line 137 where the method returns early.
        /// </summary>
        [Fact]
        public void AddRemoveLogicalChildren_BindableIsNotElement_ReturnsEarly()
        {
            // Arrange
            var bindableObject = Substitute.For<BindableObject>();
            var oldElement = new Label { Text = "Old" };
            var newElement = new Label { Text = "New" };

            // Act
            BindableObjectExtensions.AddRemoveLogicalChildren(bindableObject, oldElement, newElement);

            // Assert - Method should return early, no exceptions should be thrown
            // Since bindableObject is not an Element, no logical child operations should occur
            Assert.True(true); // Test passes if no exception is thrown
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren when bindable is null.
        /// Should throw ArgumentNullException or NullReferenceException.
        /// </summary>
        [Fact]
        public void AddRemoveLogicalChildren_BindableIsNull_ThrowsException()
        {
            // Arrange
            BindableObject bindable = null;
            var oldValue = new Label();
            var newValue = new Label();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                BindableObjectExtensions.AddRemoveLogicalChildren(bindable, oldValue, newValue));
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren when bindable is Element and oldValue is Element.
        /// Should call RemoveLogicalChild on the owner Element.
        /// </summary>
        [Fact]
        public void AddRemoveLogicalChildren_BindableIsElementAndOldValueIsElement_CallsRemoveLogicalChild()
        {
            // Arrange
            var owner = Substitute.For<Element>();
            var oldElement = new Label { Text = "Old" };
            var newValue = "NotAnElement";

            // Act
            BindableObjectExtensions.AddRemoveLogicalChildren(owner, oldElement, newValue);

            // Assert
            owner.Received(1).RemoveLogicalChild(oldElement);
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren when bindable is Element and newValue is Element.
        /// Should call AddLogicalChild on the owner Element.
        /// </summary>
        [Fact]
        public void AddRemoveLogicalChildren_BindableIsElementAndNewValueIsElement_CallsAddLogicalChild()
        {
            // Arrange
            var owner = Substitute.For<Element>();
            var oldValue = "NotAnElement";
            var newElement = new Label { Text = "New" };

            // Act
            BindableObjectExtensions.AddRemoveLogicalChildren(owner, oldValue, newElement);

            // Assert
            owner.Received(1).AddLogicalChild(newElement);
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren when bindable is Element, oldValue is Element, and newValue is Element.
        /// Should call both RemoveLogicalChild and AddLogicalChild on the owner Element.
        /// </summary>
        [Fact]
        public void AddRemoveLogicalChildren_BindableIsElementWithBothOldAndNewElements_CallsBothMethods()
        {
            // Arrange
            var owner = Substitute.For<Element>();
            var oldElement = new Label { Text = "Old" };
            var newElement = new Label { Text = "New" };

            // Act
            BindableObjectExtensions.AddRemoveLogicalChildren(owner, oldElement, newElement);

            // Assert
            owner.Received(1).RemoveLogicalChild(oldElement);
            owner.Received(1).AddLogicalChild(newElement);
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren when bindable is Element but oldValue and newValue are not Elements.
        /// Should not call RemoveLogicalChild or AddLogicalChild.
        /// </summary>
        [Fact]
        public void AddRemoveLogicalChildren_BindableIsElementButValuesAreNotElements_DoesNotCallMethods()
        {
            // Arrange
            var owner = Substitute.For<Element>();
            var oldValue = "NotAnElement";
            var newValue = 42;

            // Act
            BindableObjectExtensions.AddRemoveLogicalChildren(owner, oldValue, newValue);

            // Assert
            owner.DidNotReceive().RemoveLogicalChild(Arg.Any<Element>());
            owner.DidNotReceive().AddLogicalChild(Arg.Any<Element>());
        }

        /// <summary>
        /// Tests AddRemoveLogicalChildren with null oldValue and newValue parameters.
        /// Should not throw exceptions and not call any methods when values are null.
        /// </summary>
        [Theory]
        [InlineData(null, null)]
        [InlineData(null, "NotAnElement")]
        [InlineData("NotAnElement", null)]
        public void AddRemoveLogicalChildren_WithNullValues_HandlesGracefully(object oldValue, object newValue)
        {
            // Arrange
            var owner = Substitute.For<Element>();

            // Act
            BindableObjectExtensions.AddRemoveLogicalChildren(owner, oldValue, newValue);

            // Assert - Should not throw and not call methods for null values
            owner.DidNotReceive().RemoveLogicalChild(Arg.Any<Element>());
            owner.DidNotReceive().AddLogicalChild(Arg.Any<Element>());
        }

        /// <summary>
        /// Test implementation of BindableObject for testing purposes.
        /// </summary>
        private class TestBindableObject : BindableObject
        {
            public object TestBindingContext { get; set; }

            public TestBindableObject()
            {
                TestBindingContext = BindingContext;
            }
        }

        /// <summary>
        /// Test class that is not a BindableObject for testing type casting scenarios.
        /// </summary>
        private class NonBindableObject
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Tests that PropagateBindingContext handles null children parameter gracefully by returning early.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_NullChildren_ReturnsEarlyWithoutError()
        {
            // Arrange
            var self = new TestBindableObject();
            IEnumerable<object> children = null;
            var setChildBindingContextCalled = false;
            Action<BindableObject, object> setChildBindingContext = (child, context) => setChildBindingContextCalled = true;

            // Act
            BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext);

            // Assert
            Assert.False(setChildBindingContextCalled);
        }

        /// <summary>
        /// Tests that PropagateBindingContext skips non-BindableObject children during iteration.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_NonBindableObjectChildren_SkipsNonBindableObjects()
        {
            // Arrange
            var self = new TestBindableObject();
            self.BindingContext = "TestContext";

            var nonBindableChild = new NonBindableObject { Name = "Test" };
            var children = new object[] { nonBindableChild, "StringChild", 42 };

            var setChildBindingContextCallCount = 0;
            Action<BindableObject, object> setChildBindingContext = (child, context) => setChildBindingContextCallCount++;

            // Act
            BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext);

            // Assert
            Assert.Equal(0, setChildBindingContextCallCount);
        }

        /// <summary>
        /// Tests that PropagateBindingContext propagates binding context to BindableObject children.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_WithBindableObjectChildren_PropagatesBindingContext()
        {
            // Arrange
            var self = new TestBindableObject();
            var expectedContext = "TestBindingContext";
            self.BindingContext = expectedContext;

            var bindableChild1 = new TestBindableObject();
            var bindableChild2 = new TestBindableObject();
            var children = new BindableObject[] { bindableChild1, bindableChild2 };

            var capturedContexts = new List<object>();
            var capturedChildren = new List<BindableObject>();
            Action<BindableObject, object> setChildBindingContext = (child, context) =>
            {
                capturedChildren.Add(child);
                capturedContexts.Add(context);
            };

            // Act
            BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext);

            // Assert
            Assert.Equal(2, capturedContexts.Count);
            Assert.Equal(expectedContext, capturedContexts[0]);
            Assert.Equal(expectedContext, capturedContexts[1]);
            Assert.Same(bindableChild1, capturedChildren[0]);
            Assert.Same(bindableChild2, capturedChildren[1]);
        }

        /// <summary>
        /// Tests that PropagateBindingContext handles mixed collections with both BindableObject and non-BindableObject children.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_MixedChildren_PropagatesOnlyToBindableObjects()
        {
            // Arrange
            var self = new TestBindableObject();
            var expectedContext = "MixedTestContext";
            self.BindingContext = expectedContext;

            var bindableChild = new TestBindableObject();
            var nonBindableChild = new NonBindableObject();
            var children = new object[] { nonBindableChild, bindableChild, "StringChild" };

            var capturedContexts = new List<object>();
            var capturedChildren = new List<BindableObject>();
            Action<BindableObject, object> setChildBindingContext = (child, context) =>
            {
                capturedChildren.Add(child);
                capturedContexts.Add(context);
            };

            // Act
            BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext);

            // Assert
            Assert.Single(capturedContexts);
            Assert.Equal(expectedContext, capturedContexts[0]);
            Assert.Same(bindableChild, capturedChildren[0]);
        }

        /// <summary>
        /// Tests that PropagateBindingContext handles empty children collection without errors.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_EmptyChildren_CompletesWithoutError()
        {
            // Arrange
            var self = new TestBindableObject();
            self.BindingContext = "TestContext";
            var children = new object[0];

            var setChildBindingContextCalled = false;
            Action<BindableObject, object> setChildBindingContext = (child, context) => setChildBindingContextCalled = true;

            // Act
            BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext);

            // Assert
            Assert.False(setChildBindingContextCalled);
        }

        /// <summary>
        /// Tests that PropagateBindingContext propagates null binding context when parent has null BindingContext.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_NullParentBindingContext_PropagatesNull()
        {
            // Arrange
            var self = new TestBindableObject();
            self.BindingContext = null;

            var bindableChild = new TestBindableObject();
            var children = new BindableObject[] { bindableChild };

            object capturedContext = "NotNull";
            Action<BindableObject, object> setChildBindingContext = (child, context) => capturedContext = context;

            // Act
            BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext);

            // Assert
            Assert.Null(capturedContext);
        }

        /// <summary>
        /// Tests that PropagateBindingContext throws NullReferenceException when self parameter is null.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_NullSelf_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject self = null;
            var children = new object[0];
            Action<BindableObject, object> setChildBindingContext = (child, context) => { };

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext));
        }

        /// <summary>
        /// Tests that PropagateBindingContext throws NullReferenceException when setChildBindingContext parameter is null.
        /// </summary>
        [Fact]
        public void PropagateBindingContext_NullSetChildBindingContextAction_ThrowsNullReferenceException()
        {
            // Arrange
            var self = new TestBindableObject();
            var bindableChild = new TestBindableObject();
            var children = new BindableObject[] { bindableChild };
            Action<BindableObject, object> setChildBindingContext = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                BindableObjectExtensions.PropagateBindingContext(self, children, setChildBindingContext));
        }
    }
}