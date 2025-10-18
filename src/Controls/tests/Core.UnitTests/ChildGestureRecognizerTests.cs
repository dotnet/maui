#nullable disable

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Microsoft.Maui.Controls;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Unit tests for the ChildGestureRecognizer class.
    /// </summary>
    public sealed class ChildGestureRecognizerTests
    {
        /// <summary>
        /// Tests that the GestureRecognizer property returns null when not initialized.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenNotInitialized_ReturnsNull()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();

            // Act
            var result = childGestureRecognizer.GestureRecognizer;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that the GestureRecognizer property returns the value that was set.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenSet_ReturnsSetValue()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();

            // Act
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer;
            var result = childGestureRecognizer.GestureRecognizer;

            // Assert
            Assert.Same(mockGestureRecognizer, result);
        }

        /// <summary>
        /// Tests that setting the GestureRecognizer property to null stores null value.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenSetToNull_ReturnsNull()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer;

            // Act
            childGestureRecognizer.GestureRecognizer = null;
            var result = childGestureRecognizer.GestureRecognizer;

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that setting the GestureRecognizer property raises PropertyChanged event with correct property name.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenSet_RaisesPropertyChangedEvent()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();
            string raisedPropertyName = null;

            childGestureRecognizer.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

            // Act
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer;

            // Assert
            Assert.Equal("GestureRecognizer", raisedPropertyName);
        }

        /// <summary>
        /// Tests that setting the GestureRecognizer property to null raises PropertyChanged event.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenSetToNull_RaisesPropertyChangedEvent()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer;

            string raisedPropertyName = null;
            childGestureRecognizer.PropertyChanged += (sender, e) => raisedPropertyName = e.PropertyName;

            // Act
            childGestureRecognizer.GestureRecognizer = null;

            // Assert
            Assert.Equal("GestureRecognizer", raisedPropertyName);
        }

        /// <summary>
        /// Tests that setting the GestureRecognizer property multiple times with same value raises PropertyChanged event each time.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenSetMultipleTimesWithSameValue_RaisesPropertyChangedEventEachTime()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            var mockGestureRecognizer = Substitute.For<IGestureRecognizer>();
            var eventRaisedCount = 0;

            childGestureRecognizer.PropertyChanged += (sender, e) => eventRaisedCount++;

            // Act
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer;
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer;

            // Assert
            Assert.Equal(2, eventRaisedCount);
        }

        /// <summary>
        /// Tests that setting the GestureRecognizer property to different values updates the stored value correctly.
        /// </summary>
        [Fact]
        public void GestureRecognizer_WhenSetToDifferentValues_UpdatesValueCorrectly()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            var mockGestureRecognizer1 = Substitute.For<IGestureRecognizer>();
            var mockGestureRecognizer2 = Substitute.For<IGestureRecognizer>();

            // Act & Assert - First value
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer1;
            Assert.Same(mockGestureRecognizer1, childGestureRecognizer.GestureRecognizer);

            // Act & Assert - Second value
            childGestureRecognizer.GestureRecognizer = mockGestureRecognizer2;
            Assert.Same(mockGestureRecognizer2, childGestureRecognizer.GestureRecognizer);
        }

        /// <summary>
        /// Tests that OnPropertyChanged raises PropertyChanged event with correct property name when explicitly provided.
        /// Verifies that the method correctly passes the provided property name to PropertyChangedEventArgs.
        /// Expected result: PropertyChanged event is raised with the specified property name.
        /// </summary>
        [Theory]
        [InlineData("TestProperty")]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Property.With.Dots")]
        [InlineData("Property-With-Dashes")]
        [InlineData("Property_With_Underscores")]
        [InlineData("PropertyWithVeryLongNameThatExceedsNormalExpectationsAndCouldPotentiallyCauseIssuesWithMemoryOrStringHandling")]
        [InlineData("Property\nWith\nNewlines")]
        [InlineData("Property\tWith\tTabs")]
        [InlineData("PropertyWith@#$%SpecialCharacters")]
        public void OnPropertyChanged_WithExplicitPropertyName_RaisesPropertyChangedEventWithCorrectPropertyName(string propertyName)
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            string actualPropertyName = null;
            childGestureRecognizer.PropertyChanged += (sender, e) => actualPropertyName = e.PropertyName;

            // Act
            childGestureRecognizer.OnPropertyChanged(propertyName);

            // Assert
            Assert.Equal(propertyName, actualPropertyName);
        }

        /// <summary>
        /// Tests that OnPropertyChanged raises PropertyChanged event with null property name.
        /// Verifies that the method handles null property name without throwing exceptions.
        /// Expected result: PropertyChanged event is raised with null property name.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_WithNullPropertyName_RaisesPropertyChangedEventWithNullPropertyName()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            string actualPropertyName = "NotNull";
            childGestureRecognizer.PropertyChanged += (sender, e) => actualPropertyName = e.PropertyName;

            // Act
            childGestureRecognizer.OnPropertyChanged(null);

            // Assert
            Assert.Null(actualPropertyName);
        }

        /// <summary>
        /// Tests that OnPropertyChanged uses default empty string when no property name is provided.
        /// Verifies that the CallerMemberName attribute default value is used correctly.
        /// Expected result: PropertyChanged event is raised with empty string property name.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_WithoutPropertyName_RaisesPropertyChangedEventWithEmptyStringPropertyName()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            string actualPropertyName = null;
            childGestureRecognizer.PropertyChanged += (sender, e) => actualPropertyName = e.PropertyName;

            // Act
            childGestureRecognizer.OnPropertyChanged();

            // Assert
            Assert.Equal("", actualPropertyName);
        }

        /// <summary>
        /// Tests that OnPropertyChanged passes correct sender to PropertyChanged event.
        /// Verifies that the sender parameter in the event args is the ChildGestureRecognizer instance.
        /// Expected result: PropertyChanged event is raised with correct sender reference.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_RaisesPropertyChangedEventWithCorrectSender()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            object actualSender = null;
            childGestureRecognizer.PropertyChanged += (sender, e) => actualSender = sender;

            // Act
            childGestureRecognizer.OnPropertyChanged("TestProperty");

            // Assert
            Assert.Same(childGestureRecognizer, actualSender);
        }

        /// <summary>
        /// Tests that OnPropertyChanged does not throw exception when PropertyChanged event is null.
        /// Verifies that the method handles null event subscribers gracefully.
        /// Expected result: No exception is thrown.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_WithNullPropertyChangedEvent_DoesNotThrowException()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            // PropertyChanged event is null by default

            // Act & Assert
            var exception = Record.Exception(() => childGestureRecognizer.OnPropertyChanged("TestProperty"));
            Assert.Null(exception);
        }

        /// <summary>
        /// Tests that OnPropertyChanged invokes multiple PropertyChanged event subscribers.
        /// Verifies that all registered event handlers are called when the event is raised.
        /// Expected result: All event handlers are invoked with correct parameters.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_WithMultipleSubscribers_InvokesAllSubscribers()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            int invocationCount = 0;
            string firstActualPropertyName = null;
            string secondActualPropertyName = null;

            childGestureRecognizer.PropertyChanged += (sender, e) =>
            {
                invocationCount++;
                firstActualPropertyName = e.PropertyName;
            };

            childGestureRecognizer.PropertyChanged += (sender, e) =>
            {
                invocationCount++;
                secondActualPropertyName = e.PropertyName;
            };

            // Act
            childGestureRecognizer.OnPropertyChanged("TestProperty");

            // Assert
            Assert.Equal(2, invocationCount);
            Assert.Equal("TestProperty", firstActualPropertyName);
            Assert.Equal("TestProperty", secondActualPropertyName);
        }

        /// <summary>
        /// Tests that OnPropertyChanged creates new PropertyChangedEventArgs instance for each call.
        /// Verifies that the method doesn't reuse PropertyChangedEventArgs instances.
        /// Expected result: Different PropertyChangedEventArgs instances are created for each call.
        /// </summary>
        [Fact]
        public void OnPropertyChanged_CreatesNewPropertyChangedEventArgsForEachCall()
        {
            // Arrange
            var childGestureRecognizer = new ChildGestureRecognizer();
            PropertyChangedEventArgs firstEventArgs = null;
            PropertyChangedEventArgs secondEventArgs = null;

            childGestureRecognizer.PropertyChanged += (sender, e) => firstEventArgs = e;

            // Act
            childGestureRecognizer.OnPropertyChanged("FirstProperty");

            childGestureRecognizer.PropertyChanged -= (sender, e) => firstEventArgs = e;
            childGestureRecognizer.PropertyChanged += (sender, e) => secondEventArgs = e;

            childGestureRecognizer.OnPropertyChanged("SecondProperty");

            // Assert
            Assert.NotNull(firstEventArgs);
            Assert.NotNull(secondEventArgs);
            Assert.NotSame(firstEventArgs, secondEventArgs);
            Assert.Equal("FirstProperty", firstEventArgs.PropertyName);
            Assert.Equal("SecondProperty", secondEventArgs.PropertyName);
        }
    }
}