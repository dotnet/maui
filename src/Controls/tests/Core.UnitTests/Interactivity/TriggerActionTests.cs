#nullable disable

#nullable disable
using System;

using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class TriggerActionTests
    {
        /// <summary>
        /// Tests that the TriggerAction{T} constructor properly initializes the AssociatedType property
        /// with the generic type parameter when T is a concrete BindableObject type.
        /// Expected result: AssociatedType should be set to the type of T.
        /// </summary>
        [Fact]
        public void Constructor_WithConcreteBindableObjectType_SetsAssociatedTypeCorrectly()
        {
            // Arrange & Act
            var triggerAction = new TestTriggerAction<View>();

            // Assert
            Assert.Equal(typeof(View), triggerAction.GetAssociatedType());
        }

        /// <summary>
        /// Tests that the TriggerAction{T} constructor properly initializes the AssociatedType property
        /// when T is another BindableObject-derived type.
        /// Expected result: AssociatedType should be set to the type of T.
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentBindableObjectType_SetsAssociatedTypeCorrectly()
        {
            // Arrange & Act
            var triggerAction = new TestTriggerAction<Label>();

            // Assert
            Assert.Equal(typeof(Label), triggerAction.GetAssociatedType());
        }

        /// <summary>
        /// Tests that the TriggerAction{T} constructor properly initializes the AssociatedType property
        /// when T is a Layout-derived type (which inherits from BindableObject).
        /// Expected result: AssociatedType should be set to the type of T.
        /// </summary>
        [Fact]
        public void Constructor_WithLayoutType_SetsAssociatedTypeCorrectly()
        {
            // Arrange & Act
            var triggerAction = new TestTriggerAction<StackLayout>();

            // Assert
            Assert.Equal(typeof(StackLayout), triggerAction.GetAssociatedType());
        }

        /// <summary>
        /// Helper test class that exposes the protected AssociatedType property for testing.
        /// Provides a concrete implementation of the abstract TriggerAction{T} class.
        /// </summary>
        private class TestTriggerAction<T> : TriggerAction<T> where T : BindableObject
        {
            /// <summary>
            /// Exposes the protected AssociatedType property for testing purposes.
            /// </summary>
            /// <returns>The AssociatedType property value.</returns>
            public Type GetAssociatedType()
            {
                return AssociatedType;
            }

            /// <summary>
            /// Required implementation of the abstract Invoke method.
            /// </summary>
            /// <param name="sender">The sender object of type T.</param>
            protected override void Invoke(T sender)
            {
                // Test implementation - no action needed
            }
        }

        /// <summary>
        /// Tests that the TriggerAction constructor throws ArgumentNullException when associatedType parameter is null.
        /// This test specifically targets the uncovered null check validation logic.
        /// </summary>
        [Fact]
        public void Constructor_NullAssociatedType_ThrowsArgumentNullException()
        {
            // Arrange & Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new TestTriggerAction(null));
            Assert.Equal("associatedType", exception.ParamName);
        }

        /// <summary>
        /// Tests that the TriggerAction constructor successfully initializes with a valid Type parameter
        /// and correctly sets the AssociatedType property.
        /// </summary>
        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        [InlineData(typeof(object))]
        [InlineData(typeof(TriggerAction))]
        public void Constructor_ValidAssociatedType_SetsAssociatedTypeProperty(Type associatedType)
        {
            // Arrange & Act
            var triggerAction = new TestTriggerAction(associatedType);

            // Assert
            Assert.Equal(associatedType, triggerAction.GetAssociatedType());
        }

        /// <summary>
        /// Helper test class that inherits from TriggerAction to enable testing of the internal constructor
        /// and protected members.
        /// </summary>
        private class TestTriggerAction : TriggerAction
        {
            public TestTriggerAction(Type associatedType) : base(associatedType)
            {
            }

            protected override void Invoke(object sender)
            {
                // Test implementation - not relevant for constructor testing
            }

            public Type GetAssociatedType()
            {
                return AssociatedType;
            }
        }
    }
}