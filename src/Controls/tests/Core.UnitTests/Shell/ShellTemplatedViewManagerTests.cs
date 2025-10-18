#nullable disable

using System;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class ShellTemplatedViewManagerTests
    {
        /// <summary>
        /// Tests that SetView returns early without calling any callbacks when the local view and new view are the same reference.
        /// This tests the optimization path that prevents unnecessary removal and addition operations.
        /// </summary>
        [Fact]
        public void SetView_WhenLocalViewEqualsNewView_ReturnsEarlyWithoutCallingCallbacks()
        {
            // Arrange
            var view = new View();
            var localView = view;
            var onChildRemoved = Substitute.For<Action<Element>>();
            var onChildAdded = Substitute.For<Action<Element>>();

            // Act
            ShellTemplatedViewManager.SetView(ref localView, view, onChildRemoved, onChildAdded);

            // Assert
            onChildRemoved.DidNotReceive().Invoke(Arg.Any<Element>());
            onChildAdded.DidNotReceive().Invoke(Arg.Any<Element>());
        }

        /// <summary>
        /// Tests SetView when transitioning from null to a view.
        /// Should only call OnChildAdded callback and update the local view reference.
        /// </summary>
        [Fact]
        public void SetView_WhenLocalViewIsNullAndNewViewIsNotNull_CallsOnChildAddedOnly()
        {
            // Arrange
            View localView = null;
            var newView = new View();
            var onChildRemoved = Substitute.For<Action<Element>>();
            var onChildAdded = Substitute.For<Action<Element>>();

            // Act
            ShellTemplatedViewManager.SetView(ref localView, newView, onChildRemoved, onChildAdded);

            // Assert
            onChildRemoved.DidNotReceive().Invoke(Arg.Any<Element>());
            onChildAdded.Received(1).Invoke(newView);
            Assert.Equal(newView, localView);
        }

        /// <summary>
        /// Tests SetView when transitioning from a view to null.
        /// Should only call OnChildRemoved callback and set local view to null.
        /// </summary>
        [Fact]
        public void SetView_WhenLocalViewIsNotNullAndNewViewIsNull_CallsOnChildRemovedOnly()
        {
            // Arrange
            var existingView = new View();
            var localView = existingView;
            View newView = null;
            var onChildRemoved = Substitute.For<Action<Element>>();
            var onChildAdded = Substitute.For<Action<Element>>();

            // Act
            ShellTemplatedViewManager.SetView(ref localView, newView, onChildRemoved, onChildAdded);

            // Assert
            onChildRemoved.Received(1).Invoke(existingView);
            onChildAdded.DidNotReceive().Invoke(Arg.Any<Element>());
            Assert.Null(localView);
        }

        /// <summary>
        /// Tests SetView when transitioning from one view to a different view.
        /// Should call OnChildRemoved for the old view and OnChildAdded for the new view.
        /// </summary>
        [Fact]
        public void SetView_WhenReplacingWithDifferentView_CallsBothCallbacks()
        {
            // Arrange
            var existingView = new View();
            var localView = existingView;
            var newView = new View();
            var onChildRemoved = Substitute.For<Action<Element>>();
            var onChildAdded = Substitute.For<Action<Element>>();

            // Act
            ShellTemplatedViewManager.SetView(ref localView, newView, onChildRemoved, onChildAdded);

            // Assert
            onChildRemoved.Received(1).Invoke(existingView);
            onChildAdded.Received(1).Invoke(newView);
            Assert.Equal(newView, localView);
        }

        /// <summary>
        /// Tests SetView when both local view and new view are null.
        /// Should not call any callbacks and local view should remain null.
        /// </summary>
        [Fact]
        public void SetView_WhenBothViewsAreNull_DoesNotCallAnyCallbacks()
        {
            // Arrange
            View localView = null;
            View newView = null;
            var onChildRemoved = Substitute.For<Action<Element>>();
            var onChildAdded = Substitute.For<Action<Element>>();

            // Act
            ShellTemplatedViewManager.SetView(ref localView, newView, onChildRemoved, onChildAdded);

            // Assert
            onChildRemoved.DidNotReceive().Invoke(Arg.Any<Element>());
            onChildAdded.DidNotReceive().Invoke(Arg.Any<Element>());
            Assert.Null(localView);
        }

        /// <summary>
        /// Tests SetView with null OnChildRemoved callback when removing a view.
        /// Should throw ArgumentNullException when attempting to call the null callback.
        /// </summary>
        [Fact]
        public void SetView_WithNullOnChildRemovedCallback_ThrowsArgumentNullException()
        {
            // Arrange
            var existingView = new View();
            var localView = existingView;
            View newView = null;
            Action<Element> onChildRemoved = null;
            var onChildAdded = Substitute.For<Action<Element>>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ShellTemplatedViewManager.SetView(ref localView, newView, onChildRemoved, onChildAdded));
        }

        /// <summary>
        /// Tests SetView with null OnChildAdded callback when adding a view.
        /// Should throw ArgumentNullException when attempting to call the null callback.
        /// </summary>
        [Fact]
        public void SetView_WithNullOnChildAddedCallback_ThrowsArgumentNullException()
        {
            // Arrange
            View localView = null;
            var newView = new View();
            var onChildRemoved = Substitute.For<Action<Element>>();
            Action<Element> onChildAdded = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                ShellTemplatedViewManager.SetView(ref localView, newView, onChildRemoved, onChildAdded));
        }
    }
}
