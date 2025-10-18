using System;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class ElementChangedEventArgsTests
    {
        /// <summary>
        /// Tests the ElementChangedEventArgs constructor with both oldElement and newElement as null.
        /// Verifies that null values are properly assigned to both OldElement and NewElement properties.
        /// </summary>
        [Fact]
        public void Constructor_BothElementsNull_PropertiesSetToNull()
        {
            // Arrange & Act
            var eventArgs = new ElementChangedEventArgs<Button>(null, null);

            // Assert
            Assert.Null(eventArgs.OldElement);
            Assert.Null(eventArgs.NewElement);
        }

        /// <summary>
        /// Tests the ElementChangedEventArgs constructor with oldElement as null and newElement as non-null.
        /// Verifies that the properties are correctly assigned with null for old and valid element for new.
        /// </summary>
        [Fact]
        public void Constructor_OldElementNullNewElementNotNull_PropertiesSetCorrectly()
        {
            // Arrange
            var newButton = new Button();

            // Act
            var eventArgs = new ElementChangedEventArgs<Button>(null, newButton);

            // Assert
            Assert.Null(eventArgs.OldElement);
            Assert.Same(newButton, eventArgs.NewElement);
        }

        /// <summary>
        /// Tests the ElementChangedEventArgs constructor with oldElement as non-null and newElement as null.
        /// Verifies that the properties are correctly assigned with valid element for old and null for new.
        /// </summary>
        [Fact]
        public void Constructor_OldElementNotNullNewElementNull_PropertiesSetCorrectly()
        {
            // Arrange
            var oldButton = new Button();

            // Act
            var eventArgs = new ElementChangedEventArgs<Button>(oldButton, null);

            // Assert
            Assert.Same(oldButton, eventArgs.OldElement);
            Assert.Null(eventArgs.NewElement);
        }

        /// <summary>
        /// Tests the ElementChangedEventArgs constructor with both oldElement and newElement as non-null different instances.
        /// Verifies that both properties are correctly assigned to their respective element instances.
        /// </summary>
        [Fact]
        public void Constructor_BothElementsNotNull_PropertiesSetCorrectly()
        {
            // Arrange
            var oldButton = new Button();
            var newButton = new Button();

            // Act
            var eventArgs = new ElementChangedEventArgs<Button>(oldButton, newButton);

            // Assert
            Assert.Same(oldButton, eventArgs.OldElement);
            Assert.Same(newButton, eventArgs.NewElement);
        }

        /// <summary>
        /// Tests the ElementChangedEventArgs constructor with the same element instance for both parameters.
        /// Verifies that both properties reference the same instance when identical elements are provided.
        /// </summary>
        [Fact]
        public void Constructor_SameElementForBothParameters_PropertiesReferenceSameInstance()
        {
            // Arrange
            var button = new Button();

            // Act
            var eventArgs = new ElementChangedEventArgs<Button>(button, button);

            // Assert
            Assert.Same(button, eventArgs.OldElement);
            Assert.Same(button, eventArgs.NewElement);
            Assert.Same(eventArgs.OldElement, eventArgs.NewElement);
        }

        /// <summary>
        /// Tests the ElementChangedEventArgs constructor with different Element types to verify generic constraint.
        /// Verifies that the constructor works correctly with various Element-derived types.
        /// </summary>
        [Theory]
        [InlineData(typeof(Button))]
        [InlineData(typeof(Label))]
        [InlineData(typeof(Entry))]
        public void Constructor_DifferentElementTypes_PropertiesSetCorrectly(Type elementType)
        {
            // Arrange
            var oldElement = (Element)Activator.CreateInstance(elementType);
            var newElement = (Element)Activator.CreateInstance(elementType);

            // Act & Assert - Using reflection to call generic constructor
            var eventArgsType = typeof(ElementChangedEventArgs<>).MakeGenericType(elementType);
            var eventArgs = Activator.CreateInstance(eventArgsType, oldElement, newElement);

            var oldElementProperty = eventArgsType.GetProperty("OldElement");
            var newElementProperty = eventArgsType.GetProperty("NewElement");

            Assert.Same(oldElement, oldElementProperty.GetValue(eventArgs));
            Assert.Same(newElement, newElementProperty.GetValue(eventArgs));
        }

        /// <summary>
        /// Tests that ElementChangedEventArgs inherits from EventArgs.
        /// Verifies the inheritance hierarchy is correctly established.
        /// </summary>
        [Fact]
        public void Constructor_InheritsFromEventArgs_IsEventArgsInstance()
        {
            // Arrange & Act
            var eventArgs = new ElementChangedEventArgs<Button>(null, null);

            // Assert
            Assert.IsAssignableFrom<EventArgs>(eventArgs);
        }
    }

    public class VisualElementChangedEventArgsTests
    {
        /// <summary>
        /// Tests that the constructor properly initializes the event args when both oldElement and newElement are null.
        /// This verifies that the constructor can handle null values for both parameters.
        /// Expected result: The constructor should complete successfully and inherited properties should be null.
        /// </summary>
        [Fact]
        public void Constructor_BothElementsNull_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var eventArgs = new VisualElementChangedEventArgs(null, null);

            // Assert
            Assert.Null(eventArgs.OldElement);
            Assert.Null(eventArgs.NewElement);
        }

        /// <summary>
        /// Tests that the constructor properly initializes the event args when oldElement is null and newElement is provided.
        /// This verifies the common scenario of adding a new element where no previous element existed.
        /// Expected result: OldElement should be null and NewElement should contain the provided element.
        /// </summary>
        [Fact]
        public void Constructor_OldElementNullNewElementProvided_SetsPropertiesCorrectly()
        {
            // Arrange
            var newElement = Substitute.For<VisualElement>();

            // Act
            var eventArgs = new VisualElementChangedEventArgs(null, newElement);

            // Assert
            Assert.Null(eventArgs.OldElement);
            Assert.Same(newElement, eventArgs.NewElement);
        }

        /// <summary>
        /// Tests that the constructor properly initializes the event args when oldElement is provided and newElement is null.
        /// This verifies the scenario of removing an element where no replacement is provided.
        /// Expected result: OldElement should contain the provided element and NewElement should be null.
        /// </summary>
        [Fact]
        public void Constructor_OldElementProvidedNewElementNull_SetsPropertiesCorrectly()
        {
            // Arrange
            var oldElement = Substitute.For<VisualElement>();

            // Act
            var eventArgs = new VisualElementChangedEventArgs(oldElement, null);

            // Assert
            Assert.Same(oldElement, eventArgs.OldElement);
            Assert.Null(eventArgs.NewElement);
        }

        /// <summary>
        /// Tests that the constructor properly initializes the event args when both oldElement and newElement are provided.
        /// This verifies the scenario of replacing one element with another.
        /// Expected result: Both OldElement and NewElement should contain their respective provided elements.
        /// </summary>
        [Fact]
        public void Constructor_BothElementsProvided_SetsPropertiesCorrectly()
        {
            // Arrange
            var oldElement = Substitute.For<VisualElement>();
            var newElement = Substitute.For<VisualElement>();

            // Act
            var eventArgs = new VisualElementChangedEventArgs(oldElement, newElement);

            // Assert
            Assert.Same(oldElement, eventArgs.OldElement);
            Assert.Same(newElement, eventArgs.NewElement);
        }

        /// <summary>
        /// Tests that the constructor properly handles the edge case where the same element instance is provided for both parameters.
        /// This verifies that the constructor can handle identical references without issues.
        /// Expected result: Both OldElement and NewElement should reference the same instance.
        /// </summary>
        [Fact]
        public void Constructor_SameElementForBothParameters_SetsPropertiesCorrectly()
        {
            // Arrange
            var element = Substitute.For<VisualElement>();

            // Act
            var eventArgs = new VisualElementChangedEventArgs(element, element);

            // Assert
            Assert.Same(element, eventArgs.OldElement);
            Assert.Same(element, eventArgs.NewElement);
            Assert.Same(eventArgs.OldElement, eventArgs.NewElement);
        }
    }
}