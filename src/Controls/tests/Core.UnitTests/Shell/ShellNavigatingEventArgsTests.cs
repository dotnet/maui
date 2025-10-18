#nullable disable

#nullable disable
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ShellNavigatingEventArgsTests
    {
        /// <summary>
        /// Tests the constructor with valid parameters to ensure all properties are correctly assigned.
        /// </summary>
        [Fact]
        public void Constructor_WithValidParameters_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var current = new ShellNavigationState("//current");
            var target = new ShellNavigationState("//target");
            var source = ShellNavigationSource.Push;
            bool canCancel = true;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Equal(current, args.Current);
            Assert.Equal(target, args.Target);
            Assert.Equal(source, args.Source);
            Assert.Equal(canCancel, args.CanCancel);
            Assert.True(args.Animate);
        }

        /// <summary>
        /// Tests the constructor with canCancel set to false to ensure the property is correctly assigned.
        /// </summary>
        [Fact]
        public void Constructor_WithCanCancelFalse_SetsCanCancelPropertyToFalse()
        {
            // Arrange
            var current = new ShellNavigationState("//current");
            var target = new ShellNavigationState("//target");
            var source = ShellNavigationSource.Pop;
            bool canCancel = false;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.False(args.CanCancel);
        }

        /// <summary>
        /// Tests the constructor with null current parameter to verify behavior when nullable is disabled.
        /// </summary>
        [Fact]
        public void Constructor_WithNullCurrent_SetsCurrentToNull()
        {
            // Arrange
            ShellNavigationState current = null;
            var target = new ShellNavigationState("//target");
            var source = ShellNavigationSource.Unknown;
            bool canCancel = true;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Null(args.Current);
            Assert.Equal(target, args.Target);
            Assert.Equal(source, args.Source);
            Assert.Equal(canCancel, args.CanCancel);
        }

        /// <summary>
        /// Tests the constructor with null target parameter to verify behavior when nullable is disabled.
        /// </summary>
        [Fact]
        public void Constructor_WithNullTarget_SetsTargetToNull()
        {
            // Arrange
            var current = new ShellNavigationState("//current");
            ShellNavigationState target = null;
            var source = ShellNavigationSource.PopToRoot;
            bool canCancel = false;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Equal(current, args.Current);
            Assert.Null(args.Target);
            Assert.Equal(source, args.Source);
            Assert.Equal(canCancel, args.CanCancel);
        }

        /// <summary>
        /// Tests the constructor with both current and target set to null.
        /// </summary>
        [Fact]
        public void Constructor_WithBothCurrentAndTargetNull_SetsBothToNull()
        {
            // Arrange
            ShellNavigationState current = null;
            ShellNavigationState target = null;
            var source = ShellNavigationSource.Insert;
            bool canCancel = true;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Null(args.Current);
            Assert.Null(args.Target);
            Assert.Equal(source, args.Source);
            Assert.Equal(canCancel, args.CanCancel);
        }

        /// <summary>
        /// Tests the constructor with different ShellNavigationSource enum values to ensure proper assignment.
        /// </summary>
        [Theory]
        [InlineData(ShellNavigationSource.Unknown)]
        [InlineData(ShellNavigationSource.Push)]
        [InlineData(ShellNavigationSource.Pop)]
        [InlineData(ShellNavigationSource.PopToRoot)]
        [InlineData(ShellNavigationSource.Insert)]
        [InlineData(ShellNavigationSource.Remove)]
        [InlineData(ShellNavigationSource.ShellItemChanged)]
        [InlineData(ShellNavigationSource.ShellSectionChanged)]
        [InlineData(ShellNavigationSource.ShellContentChanged)]
        public void Constructor_WithDifferentNavigationSources_SetsSourceCorrectly(ShellNavigationSource source)
        {
            // Arrange
            var current = new ShellNavigationState("//current");
            var target = new ShellNavigationState("//target");
            bool canCancel = true;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Equal(source, args.Source);
        }

        /// <summary>
        /// Tests the constructor with an invalid enum value to verify behavior with out-of-range enum values.
        /// </summary>
        [Fact]
        public void Constructor_WithInvalidEnumValue_AcceptsValue()
        {
            // Arrange
            var current = new ShellNavigationState("//current");
            var target = new ShellNavigationState("//target");
            var source = (ShellNavigationSource)999; // Invalid enum value
            bool canCancel = true;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Equal(source, args.Source);
        }

        /// <summary>
        /// Tests that the constructor always sets Animate property to true regardless of other parameters.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Constructor_WithDifferentCanCancelValues_AlwaysSetsAnimateToTrue(bool canCancel)
        {
            // Arrange
            var current = new ShellNavigationState("//current");
            var target = new ShellNavigationState("//target");
            var source = ShellNavigationSource.Push;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.True(args.Animate);
        }

        /// <summary>
        /// Tests the constructor with ShellNavigationState created from different sources (string, Uri).
        /// </summary>
        [Fact]
        public void Constructor_WithDifferentNavigationStateTypes_SetsPropertiesCorrectly()
        {
            // Arrange
            var currentFromString = new ShellNavigationState("//current/page");
            var targetFromUri = new ShellNavigationState(new Uri("//target/page", UriKind.Relative));
            var source = ShellNavigationSource.ShellItemChanged;
            bool canCancel = true;

            // Act
            var args = new ShellNavigatingEventArgs(currentFromString, targetFromUri, source, canCancel);

            // Assert
            Assert.Equal(currentFromString, args.Current);
            Assert.Equal(targetFromUri, args.Target);
            Assert.Equal(source, args.Source);
            Assert.Equal(canCancel, args.CanCancel);
        }

        /// <summary>
        /// Tests the constructor with empty ShellNavigationState instances.
        /// </summary>
        [Fact]
        public void Constructor_WithEmptyNavigationStates_SetsPropertiesCorrectly()
        {
            // Arrange
            var current = new ShellNavigationState();
            var target = new ShellNavigationState();
            var source = ShellNavigationSource.Remove;
            bool canCancel = false;

            // Act
            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Assert
            Assert.Equal(current, args.Current);
            Assert.Equal(target, args.Target);
            Assert.Equal(source, args.Source);
            Assert.Equal(canCancel, args.CanCancel);
        }

        /// <summary>
        /// Tests that GetDeferral throws InvalidOperationException when deferral has already been completed.
        /// This test covers the scenario where _deferralCompleted is true.
        /// Expected result: InvalidOperationException with specific message.
        /// </summary>
        [Fact]
        public void GetDeferral_WhenDeferralAlreadyCompleted_ThrowsInvalidOperationException()
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var source = ShellNavigationSource.Push;
            var canCancel = true;

            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Get a deferral and complete it to set _deferralCompleted to true
            var deferral = args.GetDeferral();
            deferral.Complete();

            // Wait for the async DecrementDeferral to complete
            // This is necessary because DecrementDeferral is async void and sets _deferralCompleted
            Task.Delay(10).Wait();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => args.GetDeferral());
            Assert.Equal("Deferral has already been completed", exception.Message);
        }

        /// <summary>
        /// Tests that GetDeferral returns null when CanCancel is false.
        /// This test covers the scenario where navigation cannot be cancelled.
        /// Expected result: null return value.
        /// </summary>
        [Fact]
        public void GetDeferral_WhenCanCancelIsFalse_ReturnsNull()
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var source = ShellNavigationSource.Push;
            var canCancel = false;

            var args = new ShellNavigatingEventArgs(current, target, source, canCancel);

            // Act
            var result = args.GetDeferral();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetDeferral returns null when CanCancel is false, using different navigation source.
        /// This test covers boundary conditions for the CanCancel check with various enum values.
        /// Expected result: null return value.
        /// </summary>
        [Theory]
        [InlineData(ShellNavigationSource.Push)]
        [InlineData(ShellNavigationSource.Pop)]
        [InlineData(ShellNavigationSource.PopToRoot)]
        [InlineData(ShellNavigationSource.Insert)]
        [InlineData(ShellNavigationSource.Remove)]
        [InlineData(ShellNavigationSource.ShellSectionChanged)]
        [InlineData(ShellNavigationSource.ShellItemChanged)]
        [InlineData(ShellNavigationSource.ShellContentChanged)]
        public void GetDeferral_WhenCanCancelIsFalseWithVariousNavigationSources_ReturnsNull(ShellNavigationSource navigationSource)
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var canCancel = false;

            var args = new ShellNavigatingEventArgs(current, target, navigationSource, canCancel);

            // Act
            var result = args.GetDeferral();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that Cancel method returns true and sets Cancelled to true when CanCancel is true.
        /// Input: CanCancel = true
        /// Expected: Cancel() returns true and Cancelled property becomes true
        /// </summary>
        [Fact]
        public void Cancel_WhenCanCancelIsTrue_SetsCancelledToTrueAndReturnsTrue()
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var source = ShellNavigationSource.Push;
            var eventArgs = new ShellNavigatingEventArgs(current, target, source, canCancel: true);

            // Act
            var result = eventArgs.Cancel();

            // Assert
            Assert.True(result);
            Assert.True(eventArgs.Cancelled);
        }

        /// <summary>
        /// Tests that Cancel method returns false and does not set Cancelled when CanCancel is false.
        /// Input: CanCancel = false
        /// Expected: Cancel() returns false and Cancelled property remains false
        /// </summary>
        [Fact]
        public void Cancel_WhenCanCancelIsFalse_DoesNotSetCancelledAndReturnsFalse()
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var source = ShellNavigationSource.Push;
            var eventArgs = new ShellNavigatingEventArgs(current, target, source, canCancel: false);

            // Act
            var result = eventArgs.Cancel();

            // Assert
            Assert.False(result);
            Assert.False(eventArgs.Cancelled);
        }

        /// <summary>
        /// Tests that multiple calls to Cancel when CanCancel is true continue to return true.
        /// Input: CanCancel = true, multiple Cancel() calls
        /// Expected: All calls return true and Cancelled remains true
        /// </summary>
        [Fact]
        public void Cancel_WhenCalledMultipleTimesWithCanCancelTrue_ContinuesToReturnTrue()
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var source = ShellNavigationSource.Push;
            var eventArgs = new ShellNavigatingEventArgs(current, target, source, canCancel: true);

            // Act
            var firstResult = eventArgs.Cancel();
            var secondResult = eventArgs.Cancel();

            // Assert
            Assert.True(firstResult);
            Assert.True(secondResult);
            Assert.True(eventArgs.Cancelled);
        }

        /// <summary>
        /// Tests that multiple calls to Cancel when CanCancel is false continue to return false.
        /// Input: CanCancel = false, multiple Cancel() calls
        /// Expected: All calls return false and Cancelled remains false
        /// </summary>
        [Fact]
        public void Cancel_WhenCalledMultipleTimesWithCanCancelFalse_ContinuesToReturnFalse()
        {
            // Arrange
            var current = Substitute.For<ShellNavigationState>();
            var target = Substitute.For<ShellNavigationState>();
            var source = ShellNavigationSource.Push;
            var eventArgs = new ShellNavigatingEventArgs(current, target, source, canCancel: false);

            // Act
            var firstResult = eventArgs.Cancel();
            var secondResult = eventArgs.Cancel();

            // Assert
            Assert.False(firstResult);
            Assert.False(secondResult);
            Assert.False(eventArgs.Cancelled);
        }
    }
}