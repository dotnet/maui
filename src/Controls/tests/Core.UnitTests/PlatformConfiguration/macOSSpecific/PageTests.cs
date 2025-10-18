#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests.PlatformConfiguration.macOSSpecific
{
    public partial class PageTests
    {
        /// <summary>
        /// Tests that SetTabOrder extension method returns the same configuration instance when called with valid parameters.
        /// </summary>
        [Fact]
        public void SetTabOrder_ValidConfig_ReturnsSameInstance()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.macOS, Page>>();
            var page = Substitute.For<Page>();
            config.Element.Returns(page);
            var visualElements = new VisualElement[] { Substitute.For<VisualElement>() };

            // Act
            var result = config.SetTabOrder(visualElements);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetTabOrder extension method works correctly when called with null value array.
        /// </summary>
        [Fact]
        public void SetTabOrder_NullValueArray_ReturnsSameInstance()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.macOS, Page>>();
            var page = Substitute.For<Page>();
            config.Element.Returns(page);

            // Act
            var result = config.SetTabOrder(null);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetTabOrder extension method works correctly when called with empty VisualElement array.
        /// </summary>
        [Fact]
        public void SetTabOrder_EmptyArray_ReturnsSameInstance()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.macOS, Page>>();
            var page = Substitute.For<Page>();
            config.Element.Returns(page);
            var emptyArray = new VisualElement[0];

            // Act
            var result = config.SetTabOrder(emptyArray);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetTabOrder extension method works correctly when called with array containing null elements.
        /// </summary>
        [Fact]
        public void SetTabOrder_ArrayWithNullElements_ReturnsSameInstance()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.macOS, Page>>();
            var page = Substitute.For<Page>();
            config.Element.Returns(page);
            var arrayWithNulls = new VisualElement[] { null, Substitute.For<VisualElement>(), null };

            // Act
            var result = config.SetTabOrder(arrayWithNulls);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetTabOrder extension method works correctly when called with multiple VisualElement instances.
        /// </summary>
        [Fact]
        public void SetTabOrder_MultipleVisualElements_ReturnsSameInstance()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.macOS, Page>>();
            var page = Substitute.For<Page>();
            config.Element.Returns(page);
            var element1 = Substitute.For<VisualElement>();
            var element2 = Substitute.For<VisualElement>();
            var element3 = Substitute.For<VisualElement>();

            // Act
            var result = config.SetTabOrder(element1, element2, element3);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetTabOrder extension method accesses the Element property of the configuration.
        /// </summary>
        [Fact]
        public void SetTabOrder_ValidConfig_AccessesElementProperty()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.macOS, Page>>();
            var page = Substitute.For<Page>();
            config.Element.Returns(page);
            var visualElements = new VisualElement[] { Substitute.For<VisualElement>() };

            // Act
            config.SetTabOrder(visualElements);

            // Assert
            var _ = config.Received(1).Element;
        }

        /// <summary>
        /// Tests that GetTabOrder throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void GetTabOrder_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => Page.GetTabOrder(element));
        }

        /// <summary>
        /// Tests that GetTabOrder returns the VisualElement array from the element's TabOrderProperty.
        /// </summary>
        [Fact]
        public void GetTabOrder_ValidElementWithVisualElementArray_ReturnsExpectedArray()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedArray = new VisualElement[] { Substitute.For<VisualElement>(), Substitute.For<VisualElement>() };
            element.GetValue(Page.TabOrderProperty).Returns(expectedArray);

            // Act
            var result = Page.GetTabOrder(element);

            // Assert
            Assert.Same(expectedArray, result);
        }

        /// <summary>
        /// Tests that GetTabOrder returns null when the element's TabOrderProperty value is null.
        /// </summary>
        [Fact]
        public void GetTabOrder_ValidElementWithNullValue_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Page.TabOrderProperty).Returns(null);

            // Act
            var result = Page.GetTabOrder(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetTabOrder returns an empty array when the element's TabOrderProperty contains an empty array.
        /// </summary>
        [Fact]
        public void GetTabOrder_ValidElementWithEmptyArray_ReturnsEmptyArray()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var emptyArray = new VisualElement[0];
            element.GetValue(Page.TabOrderProperty).Returns(emptyArray);

            // Act
            var result = Page.GetTabOrder(element);

            // Assert
            Assert.Same(emptyArray, result);
            Assert.Empty(result);
        }

        /// <summary>
        /// Tests that GetTabOrder throws InvalidCastException when the element's TabOrderProperty contains an incompatible type.
        /// </summary>
        [Fact]
        public void GetTabOrder_ValidElementWithIncompatibleType_ThrowsInvalidCastException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var incompatibleValue = "not a VisualElement array";
            element.GetValue(Page.TabOrderProperty).Returns(incompatibleValue);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() => Page.GetTabOrder(element));
        }
    }
}