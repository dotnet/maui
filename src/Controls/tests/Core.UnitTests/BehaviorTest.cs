#nullable disable

using System;


using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    class MockBehavior<T> : Behavior<T> where T : BindableObject
    {
        public static int AttachCount { get; set; }
        public bool attached;
        public bool detached;

        protected override void OnAttachedTo(BindableObject bindable)
        {
            base.OnAttachedTo(bindable);
            attached = true;
            AttachCount++;
            AssociatedObject = bindable;
        }

        protected override void OnDetachingFrom(BindableObject bindable)
        {
            AttachCount--;
            detached = true;
            base.OnDetachingFrom(bindable);
            AssociatedObject = null;
        }

        public BindableObject AssociatedObject { get; set; }
    }



    public class BehaviorTest : BaseTestFixture
    {
        [Fact]
        public void AttachAndDetach()
        {
            var behavior = new MockBehavior<MockBindable>();
            var bindable = new MockBindable();

            Assert.False(behavior.attached);
            Assert.False(behavior.detached);
            Assert.Null(behavior.AssociatedObject);

            ((IAttachedObject)behavior).AttachTo(bindable);

            Assert.True(behavior.attached);
            Assert.False(behavior.detached);
            Assert.Same(bindable, behavior.AssociatedObject);

            ((IAttachedObject)behavior).DetachFrom(bindable);

            Assert.True(behavior.attached);
            Assert.True(behavior.detached);
            Assert.Null(behavior.AssociatedObject);
        }

        [Fact]
        public void AttachToTypeCompatibleWithTargetType()
        {
            var behavior = new MockBehavior<MockBindable>();
            var bindable = new View();

            Assert.Throws<InvalidOperationException>(() => ((IAttachedObject)behavior).AttachTo(bindable));
        }

        [Fact]
        public void BehaviorsInCollectionAreAttachedWhenCollectionIsAttached()
        {
            var behavior = new MockBehavior<MockBindable>();
            var collection = new AttachedCollection<Behavior>();
            var bindable = new MockBindable();
            collection.Add(behavior);
            Assert.Null(behavior.AssociatedObject);

            ((IAttachedObject)collection).AttachTo(bindable);
            Assert.Same(bindable, behavior.AssociatedObject);

            ((IAttachedObject)collection).DetachFrom(bindable);
            Assert.Null(behavior.AssociatedObject);
        }

        [Fact]
        public void BehaviorsAddedToAttachedCollectionAreAttached()
        {
            var behavior = new MockBehavior<MockBindable>();
            var collection = new AttachedCollection<Behavior>();
            var bindable = new MockBindable();
            ((IAttachedObject)collection).AttachTo(bindable);
            Assert.Null(behavior.AssociatedObject);

            collection.Add(behavior);
            Assert.Same(bindable, behavior.AssociatedObject);

            collection.Remove(behavior);
            Assert.Null(behavior.AssociatedObject);
        }

        [Fact]
        public void TestBehaviorsAttachedDP()
        {
            var behavior = new MockBehavior<MockBindable>();
            var bindable = new MockBindable();
            var collection = bindable.Behaviors;
            Assert.Null(behavior.AssociatedObject);

            collection.Add(behavior);
            Assert.Same(bindable, behavior.AssociatedObject);

            collection.Remove(behavior);
            Assert.Null(behavior.AssociatedObject);
        }
    }

    public partial class BehaviorTests
    {
        /// <summary>
        /// Tests that the Behavior{T} parameterless constructor successfully creates an instance
        /// and sets the AssociatedType property to typeof(T) for BindableObject.
        /// </summary>
        [Fact]
        public void Constructor_WithBindableObjectType_SetsAssociatedTypeCorrectly()
        {
            // Arrange & Act
            var behavior = new TestBehavior<BindableObject>();

            // Assert
            Assert.Equal(typeof(BindableObject), behavior.GetAssociatedType());
        }

        /// <summary>
        /// Tests that the Behavior{T} parameterless constructor successfully creates an instance
        /// and sets the AssociatedType property to typeof(T) for View type.
        /// </summary>
        [Fact]
        public void Constructor_WithViewType_SetsAssociatedTypeCorrectly()
        {
            // Arrange & Act
            var behavior = new TestBehavior<View>();

            // Assert
            Assert.Equal(typeof(View), behavior.GetAssociatedType());
        }

        /// <summary>
        /// Tests that the Behavior{T} parameterless constructor successfully creates an instance
        /// and sets the AssociatedType property to typeof(T) for Element type.
        /// </summary>
        [Fact]
        public void Constructor_WithElementType_SetsAssociatedTypeCorrectly()
        {
            // Arrange & Act
            var behavior = new TestBehavior<Element>();

            // Assert
            Assert.Equal(typeof(Element), behavior.GetAssociatedType());
        }

        /// <summary>
        /// Tests that the Behavior{T} parameterless constructor can be called multiple times
        /// and each instance has the correct AssociatedType.
        /// </summary>
        [Fact]
        public void Constructor_MultipleInstances_EachHasCorrectAssociatedType()
        {
            // Arrange & Act
            var behavior1 = new TestBehavior<BindableObject>();
            var behavior2 = new TestBehavior<View>();
            var behavior3 = new TestBehavior<Element>();

            // Assert
            Assert.Equal(typeof(BindableObject), behavior1.GetAssociatedType());
            Assert.Equal(typeof(View), behavior2.GetAssociatedType());
            Assert.Equal(typeof(Element), behavior3.GetAssociatedType());
        }

        private class TestBehavior<T> : Behavior<T> where T : BindableObject
        {
            public Type GetAssociatedType() => AssociatedType;
        }

        /// <summary>
        /// Tests that the parameterless protected constructor properly initializes the Behavior
        /// by calling the internal constructor with typeof(BindableObject) as the associated type.
        /// Verifies that AssociatedType is set correctly and no exceptions are thrown.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessProtected_SetsAssociatedTypeToBindableObject()
        {
            // Arrange & Act
            var behavior = new TestBehavior();

            // Assert
            Assert.Equal(typeof(BindableObject), behavior.GetAssociatedType());
        }

        /// <summary>
        /// Tests that the parameterless protected constructor can be called without throwing exceptions.
        /// Verifies that the constructor chain properly handles the initialization flow.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessProtected_DoesNotThrowException()
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => new TestBehavior());
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that multiple instances created using the parameterless constructor
        /// all have the same AssociatedType value, ensuring consistent initialization.
        /// </summary>
        [Fact]
        public void Constructor_ParameterlessProtected_MultipleInstances_ConsistentAssociatedType()
        {
            // Arrange & Act
            var behavior1 = new TestBehavior();
            var behavior2 = new TestBehavior();

            // Assert
            Assert.Equal(behavior1.GetAssociatedType(), behavior2.GetAssociatedType());
            Assert.Equal(typeof(BindableObject), behavior1.GetAssociatedType());
            Assert.Equal(typeof(BindableObject), behavior2.GetAssociatedType());
        }

        /// <summary>
        /// Concrete test implementation of Behavior to test the protected constructor.
        /// Provides access to the protected AssociatedType property for testing purposes.
        /// </summary>
        private class TestBehavior : Behavior
        {
            /// <summary>
            /// Exposes the protected AssociatedType property for testing.
            /// </summary>
            /// <returns>The AssociatedType value set by the constructor.</returns>
            public Type GetAssociatedType() => AssociatedType;
        }
    }
}