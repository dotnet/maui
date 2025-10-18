#nullable disable

using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class BackgroundingEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes the BackgroundingEventArgs with a valid IPersistedState.
        /// Verifies that the State property is correctly assigned and the object inherits from EventArgs.
        /// </summary>
        [Fact]
        public void Constructor_WithValidState_SetsStateProperty()
        {
            // Arrange
            var mockState = Substitute.For<IPersistedState>();

            // Act
            var eventArgs = new BackgroundingEventArgs(mockState);

            // Assert
            Assert.Same(mockState, eventArgs.State);
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }

        /// <summary>
        /// Tests that the constructor accepts a null IPersistedState parameter.
        /// Verifies that the State property is set to null and the object is properly constructed.
        /// </summary>
        [Fact]
        public void Constructor_WithNullState_SetsStateToNull()
        {
            // Arrange
            IPersistedState nullState = null;

            // Act
            var eventArgs = new BackgroundingEventArgs(nullState);

            // Assert
            Assert.Null(eventArgs.State);
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }

        /// <summary>
        /// Tests that the State property can be modified after construction.
        /// Verifies that the State property has both getter and setter functionality.
        /// </summary>
        [Fact]
        public void Constructor_StateProperty_CanBeModifiedAfterConstruction()
        {
            // Arrange
            var initialState = Substitute.For<IPersistedState>();
            var newState = Substitute.For<IPersistedState>();
            var eventArgs = new BackgroundingEventArgs(initialState);

            // Act
            eventArgs.State = newState;

            // Assert
            Assert.Same(newState, eventArgs.State);
            Assert.NotSame(initialState, eventArgs.State);
        }

        /// <summary>
        /// Tests that the State property can be set to null after construction.
        /// Verifies that null assignment works correctly through the setter.
        /// </summary>
        [Fact]
        public void Constructor_StateProperty_CanBeSetToNull()
        {
            // Arrange
            var initialState = Substitute.For<IPersistedState>();
            var eventArgs = new BackgroundingEventArgs(initialState);

            // Act
            eventArgs.State = null;

            // Assert
            Assert.Null(eventArgs.State);
        }
    }
}
