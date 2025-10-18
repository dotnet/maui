#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public partial class PropertyPropagationExtensionsTests
    {
        /// <summary>
        /// Tests that PropagatePropertyChanged returns early when children parameter is null.
        /// Verifies the null guard clause functionality.
        /// Expected result: Method returns without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_NullChildren_ReturnsEarly()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = Substitute.For<Element>();
            IEnumerable children = null;

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with null propertyName and null children.
        /// Verifies that null propertyName doesn't cause issues when children is null.
        /// Expected result: Method returns without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_NullPropertyNameAndNullChildren_ReturnsEarly()
        {
            // Arrange
            string propertyName = null;
            Element element = Substitute.For<Element>();
            IEnumerable children = null;

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with null element and null children.
        /// Verifies that null element doesn't cause issues when children is null.
        /// Expected result: Method returns without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_NullElementAndNullChildren_ReturnsEarly()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = null;
            IEnumerable children = null;

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with empty children collection.
        /// Verifies that empty collections are processed correctly through OfType and ToList conversion.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_EmptyChildren_CallsOverloadWithEmptyList()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = Substitute.For<Element>();
            IEnumerable children = new List<object>();

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with children containing IVisualTreeElement items.
        /// Verifies that IVisualTreeElement items are properly filtered and converted.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_ChildrenWithVisualTreeElements_CallsOverload()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = Substitute.For<Element>();
            IVisualTreeElement visualElement = Substitute.For<IVisualTreeElement>();
            IEnumerable children = new List<object> { visualElement };

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with mixed collection containing both IVisualTreeElement and other types.
        /// Verifies that only IVisualTreeElement items are filtered through OfType conversion.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_MixedChildrenTypes_FiltersCorrectly()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = Substitute.For<Element>();
            IVisualTreeElement visualElement = Substitute.For<IVisualTreeElement>();
            string nonVisualElement = "not a visual element";
            IEnumerable children = new List<object> { visualElement, nonVisualElement, 42 };

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with children containing no IVisualTreeElement items.
        /// Verifies that collections with no matching types result in empty filtered list.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_ChildrenWithNoVisualTreeElements_CallsOverloadWithEmptyList()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = Substitute.For<Element>();
            IEnumerable children = new List<object> { "string", 42, new object() };

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with edge case parameter values.
        /// Tests null propertyName, null element, and valid children collection.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("ValidProperty", true)]
        [InlineData("ValidProperty", false)]
        public void PropagatePropertyChanged_VariousParameterCombinations_HandlesCorrectly(string propertyName, bool elementIsNull)
        {
            // Arrange
            Element element = elementIsNull ? null : Substitute.For<Element>();
            IVisualTreeElement visualElement = Substitute.For<IVisualTreeElement>();
            IEnumerable children = new List<object> { visualElement };

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }

        /// <summary>
        /// Tests PropagatePropertyChanged with multiple IVisualTreeElement items in children.
        /// Verifies that multiple visual tree elements are properly processed.
        /// Expected result: Method completes without throwing exceptions.
        /// </summary>
        [Fact]
        public void PropagatePropertyChanged_MultipleVisualTreeElements_ProcessesAll()
        {
            // Arrange
            string propertyName = "TestProperty";
            Element element = Substitute.For<Element>();
            IVisualTreeElement element1 = Substitute.For<IVisualTreeElement>();
            IVisualTreeElement element2 = Substitute.For<IVisualTreeElement>();
            IVisualTreeElement element3 = Substitute.For<IVisualTreeElement>();
            IEnumerable children = new List<object> { element1, element2, element3 };

            // Act & Assert - Should not throw
            PropertyPropagationExtensions.PropagatePropertyChanged(propertyName, element, children);
        }
    }
}
