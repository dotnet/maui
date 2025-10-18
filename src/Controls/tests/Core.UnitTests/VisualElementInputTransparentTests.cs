#nullable disable

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    public class VisualElementInputTransparentTests
    {
        // this is both for color diff and cols
        const bool truee = true;

        static (Layout Root, Layout Nested, VisualElement Child) CreateViews(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans)
        {
            Layout root;
            Layout nested;
            VisualElement child;

            root = new Grid
            {
                InputTransparent = rootTrans,
                CascadeInputTransparent = rootCascade,
                Children =
                 {
                      (nested = new Grid
                      {
                          InputTransparent = nestedTrans,
                          CascadeInputTransparent = nestedCascade,
                          Children =
                          {
                              (child = new Button
                              {
                                  InputTransparent = trans
                              })
                          }
                      })
                 }
            };

            return (root, nested, child);
        }

        static void AssertState(bool rootTrans, bool rootCascade, bool nestedTrans, bool nestedCascade, bool trans, Layout nested, VisualElement child)
        {
            var (finalNestedTrans, finalTrans) = States[(rootTrans, rootCascade, nestedTrans, nestedCascade, trans)];

            if (finalNestedTrans)
                Assert.True(nested.InputTransparent, "Nested layout was not input transparent when it should have been.");
            else
                Assert.False(nested.InputTransparent, "Nested layout was input transparent when it should not have been.");

            if (finalTrans)
                Assert.True(child.InputTransparent, "Child element was not input transparent when it should have been.");
            else
                Assert.False(child.InputTransparent, "Child element was input transparent when it should not have been.");
        }

        static void AssertState(bool layoutTrans, bool layoutCascade, bool trans, Layout layout, VisualElement child)
        {
            var (finalLayoutTrans, finalTrans) = States[(false, false, layoutTrans, layoutCascade, trans)];

            if (finalLayoutTrans)
                Assert.True(layout.InputTransparent, "Layout was not input transparent when it should have been.");
            else
                Assert.False(layout.InputTransparent, "Layout was input transparent when it should not have been.");

            if (finalTrans)
                Assert.True(child.InputTransparent, "Child element was not input transparent when it should have been.");
            else
                Assert.False(child.InputTransparent, "Child element was input transparent when it should not have been.");
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void InputTransparencyOnLayoutDoesNotOverrideNestedLayoutWhenNotCascadeInputTransparentButCascadeOnNested(bool parent, bool nested)
        {
            var element = new Button();
            Grid nestedLayout;
            var layout = new Grid
            {
                InputTransparent = parent,
                CascadeInputTransparent = false,
                Children =
                 {
                     new Grid
                     {
                         (nestedLayout = new Grid
                         {
                             InputTransparent = nested,
                             CascadeInputTransparent = true, // default
 							Children =
                             {
                                 new Grid { element }
                             }
                         })
                     }
                 }
            };

            AssertState(parent, false, nested, true, false, nestedLayout, element);

            layout.InputTransparent = !parent;

            AssertState(!parent, false, nested, true, false, nestedLayout, element);

            layout.InputTransparent = parent;

            AssertState(parent, false, nested, true, false, nestedLayout, element);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public void InputTransparencyOnLayoutDoesNotOverrideNestedLayoutWhenNotCascadeInputTransparent(bool parent, bool nested)
        {
            var element = new Button();
            Grid nestedLayout;
            var layout = new Grid
            {
                InputTransparent = parent,
                CascadeInputTransparent = false,
                Children =
                 {
                     new Grid
                     {
                         (nestedLayout = new Grid
                         {
                             InputTransparent = nested,
                             CascadeInputTransparent = false,
                             Children =
                             {
                                 new Grid { element }
                             }
                         })
                     }
                 }
            };

            AssertState(parent, false, nested, false, false, nestedLayout, element);

            layout.InputTransparent = !parent;

            AssertState(!parent, false, nested, false, false, nestedLayout, element);

            layout.InputTransparent = parent;

            AssertState(parent, false, nested, false, false, nestedLayout, element);
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, true, false)]
        [InlineData(true, false, true)]
        [InlineData(true, false, false)]
        [InlineData(false, true, true)]
        [InlineData(false, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, false, false)]
        public void InputTransparencyOnLayoutOverridesNestedLayout(bool parent, bool nested, bool cascadeNested)
        {
            var element = new Button();
            Grid nestedLayout;
            var layout = new Grid
            {
                InputTransparent = parent,
                CascadeInputTransparent = true, // default
                Children =
                 {
                     new Grid
                     {
                         (nestedLayout = new Grid
                         {
                             InputTransparent = nested,
                             CascadeInputTransparent = cascadeNested,
                             Children =
                             {
                                 new Grid { element }
                             }
                         })
                     }
                 }
            };

            AssertState(parent, true, nested, cascadeNested, false, nestedLayout, element);

            layout.InputTransparent = !parent;

            AssertState(!parent, true, nested, cascadeNested, false, nestedLayout, element);

            layout.InputTransparent = parent;

            AssertState(parent, true, nested, cascadeNested, false, nestedLayout, element);
        }
    }

    /// <summary>
    /// Tests for the GetNextFocusRightView extension method in TizenSpecific.VisualElement
    /// </summary>
    public partial class TizenSpecificVisualElementTests
    {
        /// <summary>
        /// Tests that GetNextFocusRightView returns the expected View when config and element are valid and GetValue returns a View.
        /// Input: Valid config with Element that returns a View from GetValue.
        /// Expected: The same View instance that was returned by GetValue.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ValidConfigWithView_ReturnsView()
        {
            // Arrange
            var expectedView = new View();
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(TizenSpecific.VisualElement.NextFocusRightViewProperty).Returns(expectedView);

            // Act
            var result = TizenSpecific.VisualElement.GetNextFocusRightView(mockConfig);

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusRightView returns null when config and element are valid but GetValue returns null.
        /// Input: Valid config with Element that returns null from GetValue.
        /// Expected: null.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ValidConfigWithNullView_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(TizenSpecific.VisualElement.NextFocusRightViewProperty).Returns((View)null);

            // Act
            var result = TizenSpecific.VisualElement.GetNextFocusRightView(mockConfig);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusRightView throws ArgumentNullException when config parameter is null.
        /// Input: null config parameter.
        /// Expected: ArgumentNullException when trying to access config.Element.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TizenSpecific.VisualElement.GetNextFocusRightView(nullConfig));
        }

        /// <summary>
        /// Tests that GetNextFocusRightView throws NullReferenceException when config.Element is null.
        /// Input: Valid config but with null Element property.
        /// Expected: NullReferenceException when trying to call GetValue on null element.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TizenSpecific.VisualElement.GetNextFocusRightView(mockConfig));
        }

        /// <summary>
        /// Tests that GetNextFocusRightView properly delegates to the static GetNextFocusRightView method.
        /// Input: Valid config with Element.
        /// Expected: The Element's GetValue method is called with NextFocusRightViewProperty.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ValidConfig_CallsGetValueOnElement()
        {
            // Arrange
            var expectedView = new View();
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();

            mockConfig.Element.Returns(mockElement);
            mockElement.GetValue(TizenSpecific.VisualElement.NextFocusRightViewProperty).Returns(expectedView);

            // Act
            var result = TizenSpecific.VisualElement.GetNextFocusRightView(mockConfig);

            // Assert
            mockElement.Received(1).GetValue(TizenSpecific.VisualElement.NextFocusRightViewProperty);
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusRightView throws an exception when passed a null element.
        /// Verifies proper parameter validation for null input.
        /// Expected to throw ArgumentNullException or NullReferenceException.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_NullElement_ThrowsException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusRightView(element));
        }

        /// <summary>
        /// Tests that GetNextFocusRightView returns null when no value is set for the NextFocusRightView property.
        /// Verifies default behavior when the property has not been explicitly set.
        /// Expected to return null as the default value for View type.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ElementWithNoValueSet_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusRightViewProperty)
                .Returns((View)null);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusRightView(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusRightView returns the correct View when a value is set for the NextFocusRightView property.
        /// Verifies that the method correctly retrieves and returns the stored View instance.
        /// Expected to return the same View instance that was set.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ElementWithValidViewSet_ReturnsView()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedView = new Button();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusRightViewProperty)
                .Returns(expectedView);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusRightView(element);

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusRightView calls GetValue with the correct BindableProperty.
        /// Verifies that the method uses the NextFocusRightViewProperty to retrieve the value.
        /// Expected to call GetValue exactly once with the NextFocusRightViewProperty parameter.
        /// </summary>
        [Fact]
        public void GetNextFocusRightView_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var view = new Label();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusRightViewProperty)
                .Returns(view);

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusRightView(element);

            // Assert
            element.Received(1).GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusRightViewProperty);
        }

        /// <summary>
        /// Tests that SetNextFocusRightView successfully sets the property value when provided with valid element and view.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_ValidElementAndView_SetsPropertyValue()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockView = Substitute.For<View>();

            // Act
            VisualElement.SetNextFocusRightView(mockElement, mockView);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusRightViewProperty, mockView);
        }

        /// <summary>
        /// Tests that SetNextFocusRightView successfully sets the property value to null when provided with valid element and null view.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_ValidElementAndNullView_SetsPropertyValueToNull()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();

            // Act
            VisualElement.SetNextFocusRightView(mockElement, null);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusRightViewProperty, null);
        }

        /// <summary>
        /// Tests that SetNextFocusRightView throws NullReferenceException when element is null.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockView = Substitute.For<View>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.SetNextFocusRightView(null, mockView));
        }

        /// <summary>
        /// Tests that SetNextFocusRightView throws NullReferenceException when both element and view are null.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_NullElementAndNullView_ThrowsNullReferenceException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.SetNextFocusRightView(null, null));
        }

        /// <summary>
        /// Tests that SetToolTip calls the underlying SetToolTip method and returns the config object for fluent API chaining.
        /// Input: Valid config with mocked element and normal string value.
        /// Expected result: Underlying SetToolTip is called with correct parameters and same config is returned.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidConfigAndValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<VisualElement>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement>>();
            mockConfig.Element.Returns(mockElement);
            var tooltipValue = "Test tooltip";

            // Act
            var result = mockConfig.SetToolTip(tooltipValue);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.ToolTipProperty, tooltipValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolTip handles null string values correctly.
        /// Input: Valid config with mocked element and null string value.
        /// Expected result: Underlying SetToolTip is called with null value and config is returned.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidConfigAndNullValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<VisualElement>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.SetToolTip(null);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.ToolTipProperty, null);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolTip handles empty string values correctly.
        /// Input: Valid config with mocked element and empty string value.
        /// Expected result: Underlying SetToolTip is called with empty string and config is returned.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidConfigAndEmptyValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<VisualElement>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement>>();
            mockConfig.Element.Returns(mockElement);
            var emptyValue = string.Empty;

            // Act
            var result = mockConfig.SetToolTip(emptyValue);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.ToolTipProperty, emptyValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolTip handles whitespace-only string values correctly.
        /// Input: Valid config with mocked element and whitespace-only string value.
        /// Expected result: Underlying SetToolTip is called with whitespace string and config is returned.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidConfigAndWhitespaceValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<VisualElement>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement>>();
            mockConfig.Element.Returns(mockElement);
            var whitespaceValue = "   \t\n   ";

            // Act
            var result = mockConfig.SetToolTip(whitespaceValue);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.ToolTipProperty, whitespaceValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolTip handles long string values correctly.
        /// Input: Valid config with mocked element and very long string value.
        /// Expected result: Underlying SetToolTip is called with long string and config is returned.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidConfigAndLongValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<VisualElement>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement>>();
            mockConfig.Element.Returns(mockElement);
            var longValue = new string('A', 10000);

            // Act
            var result = mockConfig.SetToolTip(longValue);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.ToolTipProperty, longValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolTip handles strings with special characters correctly.
        /// Input: Valid config with mocked element and string containing special characters.
        /// Expected result: Underlying SetToolTip is called with special character string and config is returned.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidConfigAndSpecialCharacterValue_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<VisualElement>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement>>();
            mockConfig.Element.Returns(mockElement);
            var specialCharValue = "Tooltip with special chars: !@#$%^&*()_+{}|:<>?[]\\;'\",./ and unicode: 🚀✨";

            // Act
            var result = mockConfig.SetToolTip(specialCharValue);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.ToolTipProperty, specialCharValue);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetToolTip throws NullReferenceException when config is null.
        /// Input: Null config parameter.
        /// Expected result: NullReferenceException is thrown when trying to access config.Element.
        /// </summary>
        [Fact]
        public void SetToolTip_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, VisualElement> nullConfig = null;
            var tooltipValue = "Test tooltip";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => nullConfig.SetToolTip(tooltipValue));
        }

        /// <summary>
        /// Tests that GetToolTip extension method returns the tooltip value from the underlying element
        /// when a valid configuration with a valid element is provided.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidConfigWithElement_ReturnsToolTipValue()
        {
            // Arrange
            const string expectedToolTip = "Test tooltip";
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(expectedToolTip);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            string result = config.GetToolTip();

            // Assert
            Assert.Equal(expectedToolTip, result);
        }

        /// <summary>
        /// Tests that GetToolTip extension method returns null when the underlying element
        /// has no tooltip value set (default value).
        /// </summary>
        [Fact]
        public void GetToolTip_ValidConfigWithElementHavingNullToolTip_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns((string)null);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            string result = config.GetToolTip();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetToolTip extension method returns empty string when the underlying element
        /// has an empty tooltip value set.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidConfigWithElementHavingEmptyToolTip_ReturnsEmptyString()
        {
            // Arrange
            const string expectedToolTip = "";
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(expectedToolTip);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            string result = config.GetToolTip();

            // Assert
            Assert.Equal(expectedToolTip, result);
        }

        /// <summary>
        /// Tests that GetToolTip extension method throws ArgumentNullException
        /// when a null configuration parameter is provided.
        /// </summary>
        [Fact]
        public void GetToolTip_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.GetToolTip());
        }

        /// <summary>
        /// Tests that GetToolTip extension method throws NullReferenceException
        /// when the configuration has a null Element property.
        /// </summary>
        [Fact]
        public void GetToolTip_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.GetToolTip());
        }

        /// <summary>
        /// Tests that GetToolTip extension method handles special characters in tooltip values correctly
        /// when the underlying element has a tooltip with special characters.
        /// </summary>
        [Theory]
        [InlineData("Tooltip with unicode: 🎉")]
        [InlineData("Tooltip\nwith\nnewlines")]
        [InlineData("Tooltip\twith\ttabs")]
        [InlineData("Tooltip with \"quotes\" and 'apostrophes'")]
        [InlineData("Tooltip with <xml> & special chars")]
        public void GetToolTip_ValidConfigWithSpecialCharacterToolTip_ReturnsCorrectValue(string expectedToolTip)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(expectedToolTip);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            string result = config.GetToolTip();

            // Assert
            Assert.Equal(expectedToolTip, result);
        }

        /// <summary>
        /// Tests that GetToolTip extension method handles very long tooltip strings correctly
        /// when the underlying element has a very long tooltip value.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidConfigWithVeryLongToolTip_ReturnsCompleteValue()
        {
            // Arrange
            string expectedToolTip = new string('A', 10000); // Very long string
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(expectedToolTip);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            string result = config.GetToolTip();

            // Assert
            Assert.Equal(expectedToolTip, result);
            Assert.Equal(10000, result.Length);
        }
    }


    public partial class VisualElementTests
    {
        /// <summary>
        /// Tests that GetNextFocusForwardView throws ArgumentNullException when element parameter is null.
        /// Input: null element parameter.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusForwardView(element));
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView returns null when the property value is null.
        /// Input: valid BindableObject with null NextFocusForwardView property value.
        /// Expected result: null is returned.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_ElementWithNullPropertyValue_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty)
                .Returns(null);

            // Act
            var result = PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusForwardView(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView returns the correct View when property has a valid View value.
        /// Input: valid BindableObject with valid View set in NextFocusForwardView property.
        /// Expected result: the View object is returned.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_ElementWithValidView_ReturnsView()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var expectedView = Substitute.For<View>();
            element.GetValue(PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty)
                .Returns(expectedView);

            // Act
            var result = PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusForwardView(element);

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView properly calls GetValue with the correct property.
        /// Input: valid BindableObject.
        /// Expected result: GetValue is called with NextFocusForwardViewProperty.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_ValidElement_CallsGetValueWithCorrectProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var mockView = Substitute.For<View>();
            element.GetValue(Arg.Any<BindableProperty>()).Returns(mockView);

            // Act
            PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusForwardView(element);

            // Assert
            element.Received(1).GetValue(PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty);
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView throws InvalidCastException when property contains non-View object.
        /// Input: BindableObject with non-View object in NextFocusForwardView property.
        /// Expected result: InvalidCastException is thrown during cast.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_ElementWithNonViewValue_ThrowsInvalidCastException()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var nonViewObject = new object();
            element.GetValue(PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty)
                .Returns(nonViewObject);

            // Act & Assert
            Assert.Throws<InvalidCastException>(() =>
                PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusForwardView(element));
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView returns default value when property returns default View.
        /// Input: BindableObject with default View value.
        /// Expected result: the default View is returned.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_ElementWithDefaultValue_ReturnsDefaultView()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty)
                .Returns(default(View));

            // Act
            var result = PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusForwardView(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that MoveFocusUp calls SetNextFocusDirection with the correct element and Up direction.
        /// Verifies that the method properly delegates to SetNextFocusDirection with FocusDirection.Up constant.
        /// </summary>
        [Fact]
        public void MoveFocusUp_ValidConfig_CallsSetNextFocusDirectionWithUp()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.MoveFocusUp(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, "Up");
        }

        /// <summary>
        /// Tests that MoveFocusUp returns the same configuration instance for fluent API chaining.
        /// Verifies the method enables fluent method chaining by returning the input configuration.
        /// </summary>
        [Fact]
        public void MoveFocusUp_ValidConfig_ReturnsSameConfigInstance()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.MoveFocusUp(mockConfig);

            // Assert
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that MoveFocusUp throws NullReferenceException when config parameter is null.
        /// Verifies proper error handling for null input parameter.
        /// </summary>
        [Fact]
        public void MoveFocusUp_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement> nullConfig = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.MoveFocusUp(nullConfig));
        }

        /// <summary>
        /// Tests that MoveFocusUp throws NullReferenceException when config.Element is null.
        /// Verifies proper error handling when the configuration's Element property returns null.
        /// </summary>
        [Fact]
        public void MoveFocusUp_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.MoveFocusUp(mockConfig));
        }

        /// <summary>
        /// Tests that MoveFocusUp uses the exact FocusDirection.Up constant value.
        /// Verifies that the method passes the correct string constant to SetNextFocusDirection.
        /// </summary>
        [Fact]
        public void MoveFocusUp_ValidConfig_UsesCorrectFocusDirectionConstant()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            VisualElement.MoveFocusUp(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(Arg.Any<BindableProperty>(), FocusDirection.Up);
        }

        /// <summary>
        /// Tests that MoveFocusUp uses the correct NextFocusDirectionProperty.
        /// Verifies that the method calls SetValue with the specific bindable property.
        /// </summary>
        [Fact]
        public void MoveFocusUp_ValidConfig_UsesCorrectBindableProperty()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            VisualElement.MoveFocusUp(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, Arg.Any<string>());
        }

        /// <summary>
        /// Tests that GetToolTip returns the correct value when the tooltip property is set on the element.
        /// Input: Valid BindableObject with tooltip set to a regular string.
        /// Expected: Returns the exact string value that was set.
        /// </summary>
        [Theory]
        [InlineData("Test tooltip")]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   \t\n   ")]
        [InlineData("Multi\nLine\rTooltip")]
        [InlineData("Special chars: !@#$%^&*()[]{}|\\:;\"'<>?,.")]
        [InlineData("Unicode: 你好世界🌍")]
        public void GetToolTip_ValidElementWithTooltipSet_ReturnsTooltipValue(string tooltipValue)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(tooltipValue);

            // Act
            string result = TizenSpecific.VisualElement.GetToolTip(element);

            // Assert
            Assert.Equal(tooltipValue, result);
        }

        /// <summary>
        /// Tests that GetToolTip returns null when the tooltip property is not set (default value).
        /// Input: Valid BindableObject with tooltip property returning null.
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidElementWithTooltipNotSet_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns((string)null);

            // Act
            string result = TizenSpecific.VisualElement.GetToolTip(element);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetToolTip throws ArgumentNullException when element parameter is null.
        /// Input: null element parameter.
        /// Expected: Throws NullReferenceException.
        /// </summary>
        [Fact]
        public void GetToolTip_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => TizenSpecific.VisualElement.GetToolTip(element));
        }

        /// <summary>
        /// Tests that GetToolTip handles very long tooltip strings correctly.
        /// Input: Valid BindableObject with very long tooltip string.
        /// Expected: Returns the complete long string value.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidElementWithVeryLongTooltip_ReturnsCompleteString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string longTooltip = new string('A', 10000);
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(longTooltip);

            // Act
            string result = TizenSpecific.VisualElement.GetToolTip(element);

            // Assert
            Assert.Equal(longTooltip, result);
            Assert.Equal(10000, result.Length);
        }

        /// <summary>
        /// Tests that GetToolTip correctly calls GetValue with the ToolTipProperty.
        /// Input: Valid BindableObject mock.
        /// Expected: GetValue is called exactly once with ToolTipProperty parameter.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidElement_CallsGetValueWithToolTipProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string expectedValue = "Test";
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(expectedValue);

            // Act
            TizenSpecific.VisualElement.GetToolTip(element);

            // Assert
            element.Received(1).GetValue(TizenSpecific.VisualElement.ToolTipProperty);
        }

        /// <summary>
        /// Tests that GetToolTip handles string values with null characters correctly.
        /// Input: Valid BindableObject with tooltip containing null characters.
        /// Expected: Returns the string including null characters.
        /// </summary>
        [Fact]
        public void GetToolTip_ValidElementWithNullCharactersInTooltip_ReturnsStringWithNullChars()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string tooltipWithNulls = "Before\0After\0End";
            element.GetValue(TizenSpecific.VisualElement.ToolTipProperty).Returns(tooltipWithNulls);

            // Act
            string result = TizenSpecific.VisualElement.GetToolTip(element);

            // Assert
            Assert.Equal(tooltipWithNulls, result);
            Assert.Contains('\0', result);
        }

        /// <summary>
        /// Tests that SetToolTip throws ArgumentNullException when element parameter is null.
        /// Input: null element parameter and valid string value.
        /// Expected: ArgumentNullException should be thrown.
        /// </summary>
        [Fact]
        public void SetToolTip_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            string value = "test tooltip";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.SetToolTip(element, value));
        }

        /// <summary>
        /// Tests that SetToolTip correctly calls SetValue with ToolTipProperty and null value.
        /// Input: valid BindableObject and null string value.
        /// Expected: SetValue should be called with ToolTipProperty and null value.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidElementNullValue_CallsSetValueWithNullValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = null;

            // Act
            VisualElement.SetToolTip(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.ToolTipProperty, null);
        }

        /// <summary>
        /// Tests that SetToolTip correctly calls SetValue with ToolTipProperty and empty string value.
        /// Input: valid BindableObject and empty string value.
        /// Expected: SetValue should be called with ToolTipProperty and empty string.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidElementEmptyString_CallsSetValueWithEmptyString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = "";

            // Act
            VisualElement.SetToolTip(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.ToolTipProperty, "");
        }

        /// <summary>
        /// Tests that SetToolTip correctly calls SetValue with ToolTipProperty and whitespace string value.
        /// Input: valid BindableObject and whitespace-only string value.
        /// Expected: SetValue should be called with ToolTipProperty and whitespace string.
        /// </summary>
        [Fact]
        public void SetToolTip_ValidElementWhitespaceString_CallsSetValueWithWhitespaceString()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = "   \t\n  ";

            // Act
            VisualElement.SetToolTip(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.ToolTipProperty, "   \t\n  ");
        }

        /// <summary>
        /// Tests that SetToolTip correctly calls SetValue with various string values.
        /// Input: valid BindableObject and different string values including normal, long, and special character strings.
        /// Expected: SetValue should be called with ToolTipProperty and the exact provided value.
        /// </summary>
        [Theory]
        [InlineData("normal tooltip")]
        [InlineData("Tooltip with special characters: !@#$%^&*()")]
        [InlineData("Tooltip with unicode: 你好世界")]
        [InlineData("Very long tooltip text that contains multiple sentences and various punctuation marks to test the handling of longer strings in the tooltip functionality.")]
        [InlineData("Tooltip\nwith\nnewlines")]
        [InlineData("Tooltip\twith\ttabs")]
        public void SetToolTip_ValidElementVariousStringValues_CallsSetValueWithCorrectValue(string value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            VisualElement.SetToolTip(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.ToolTipProperty, value);
        }

        /// <summary>
        /// Tests that SetFocusAllowed correctly sets the focus allowed value to true on a valid BindableObject.
        /// Input: Valid BindableObject and true value.
        /// Expected: SetValue is called with IsFocusAllowedProperty and true.
        /// </summary>
        [Fact]
        public void SetFocusAllowed_ValidElementAndTrueValue_SetsValueCorrectly()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            bool value = true;

            // Act
            VisualElement.SetFocusAllowed(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.IsFocusAllowedProperty, value);
        }

        /// <summary>
        /// Tests that SetFocusAllowed correctly sets the focus allowed value to false on a valid BindableObject.
        /// Input: Valid BindableObject and false value.
        /// Expected: SetValue is called with IsFocusAllowedProperty and false.
        /// </summary>
        [Fact]
        public void SetFocusAllowed_ValidElementAndFalseValue_SetsValueCorrectly()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            bool value = false;

            // Act
            VisualElement.SetFocusAllowed(mockElement, value);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.IsFocusAllowedProperty, value);
        }

        /// <summary>
        /// Tests that SetFocusAllowed throws ArgumentNullException when element parameter is null.
        /// Input: Null BindableObject and any bool value.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetFocusAllowed_NullElement_ThrowsArgumentNullException(bool value)
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.SetFocusAllowed(element, value));
        }

        /// <summary>
        /// Tests that GetNextFocusUpView returns the correct view when a valid configuration with a set next focus up view is provided.
        /// Validates the method properly delegates to the underlying GetNextFocusUpView method and returns the expected view.
        /// </summary>
        [Fact]
        public void GetNextFocusUpView_ValidConfigWithSetView_ReturnsView()
        {
            // Arrange
            var expectedView = new Button();
            var element = new Button();
            PlatformConfiguration.TizenSpecific.VisualElement.SetNextFocusUpView(element, expectedView);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetNextFocusUpView();

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusUpView returns null when a valid configuration with no next focus up view set is provided.
        /// Validates the method properly handles the case where no next focus up view has been configured.
        /// </summary>
        [Fact]
        public void GetNextFocusUpView_ValidConfigWithNoView_ReturnsNull()
        {
            // Arrange
            var element = new Button();

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetNextFocusUpView();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusUpView throws NullReferenceException when a null configuration is provided.
        /// Validates the method properly handles null input by throwing an appropriate exception.
        /// </summary>
        [Fact]
        public void GetNextFocusUpView_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement> config = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.GetNextFocusUpView());
        }

        /// <summary>
        /// Tests that GetNextFocusUpView throws NullReferenceException when configuration has a null element.
        /// Validates the method properly handles the case where the configuration's Element property is null.
        /// </summary>
        [Fact]
        public void GetNextFocusUpView_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.GetNextFocusUpView());
        }

        /// <summary>
        /// Tests that GetNextFocusUpView properly handles when the same view is set and retrieved multiple times.
        /// Validates the method consistently returns the same view reference across multiple calls.
        /// </summary>
        [Fact]
        public void GetNextFocusUpView_MultipleCallsWithSameView_ReturnsSameReference()
        {
            // Arrange
            var expectedView = new Button();
            var element = new Button();
            PlatformConfiguration.TizenSpecific.VisualElement.SetNextFocusUpView(element, expectedView);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result1 = config.GetNextFocusUpView();
            var result2 = config.GetNextFocusUpView();

            // Assert
            Assert.Same(expectedView, result1);
            Assert.Same(expectedView, result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView throws NullReferenceException when element parameter is null.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var value = Substitute.For<View>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.SetNextFocusForwardView(element, value));
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView works correctly when value parameter is null.
        /// This should clear the forward focus view by setting it to null.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_NullValue_SetsPropertyToNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            View value = null;

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.SetNextFocusForwardView(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty,
                null);
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView correctly sets the forward focus view property with valid inputs.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_ValidInputs_SetsProperty()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = Substitute.For<View>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.SetNextFocusForwardView(element, value);

            // Assert
            element.Received(1).SetValue(
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty,
                value);
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView calls SetValue exactly once with the correct parameters.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_ValidInputs_CallsSetValueOnce()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = Substitute.For<View>();

            // Act
            Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.SetNextFocusForwardView(element, value);

            // Assert
            element.Received(1).SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
        }

        /// <summary>
        /// Tests that SetNextFocusRightView extension method sets the property on the element and returns the config object when called with valid parameters.
        /// Input: Valid IPlatformElementConfiguration with VisualElement and a View.
        /// Expected: Property is set on the element and the same config object is returned.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_ValidConfigAndView_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var visualElement = new VisualElement();
            var view = new View();
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, VisualElement>>();
            config.Element.Returns(visualElement);

            // Act
            var result = config.SetNextFocusRightView(view);

            // Assert
            var actualView = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusRightView(visualElement);
            Assert.Same(view, actualView);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetNextFocusRightView extension method accepts null view and sets the property to null on the element.
        /// Input: Valid IPlatformElementConfiguration with VisualElement and null view.
        /// Expected: Property is set to null on the element and the same config object is returned.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_ValidConfigAndNullView_SetsPropertyAndReturnsConfig()
        {
            // Arrange
            var visualElement = new VisualElement();
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, VisualElement>>();
            config.Element.Returns(visualElement);

            // Act
            var result = config.SetNextFocusRightView(null);

            // Assert
            var actualView = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusRightView(visualElement);
            Assert.Null(actualView);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetNextFocusRightView extension method throws NullReferenceException when config parameter is null.
        /// Input: Null IPlatformElementConfiguration.
        /// Expected: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetNextFocusRightView_NullConfig_ThrowsNullReferenceException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, VisualElement> config = null;
            var view = new View();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.SetNextFocusRightView(view));
        }

        /// <summary>
        /// Tests that SetNextFocusBackView calls the underlying method and returns the config for fluent chaining when given valid parameters.
        /// Input: Valid config with mock element and valid View value.
        /// Expected: Method executes successfully, underlying SetNextFocusBackView is called, and same config instance is returned.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_ValidConfigAndValidView_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.VisualElement>();
            var testView = new Button();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.SetNextFocusBackView(mockConfig, testView);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusBackViewProperty, testView);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetNextFocusBackView accepts null view value and returns the config for fluent chaining.
        /// Input: Valid config with mock element and null View value.
        /// Expected: Method executes successfully, underlying SetNextFocusBackView is called with null, and same config instance is returned.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_ValidConfigAndNullView_CallsUnderlyingMethodAndReturnsConfig()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            var mockElement = Substitute.For<Microsoft.Maui.Controls.VisualElement>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.SetNextFocusBackView(mockConfig, null);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusBackViewProperty, null);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that SetNextFocusBackView throws ArgumentNullException when config parameter is null.
        /// Input: Null config parameter and valid View value.
        /// Expected: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement> nullConfig = null;
            var testView = new Button();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.SetNextFocusBackView(nullConfig, testView));
        }

        /// <summary>
        /// Tests that SetNextFocusBackView throws NullReferenceException when config.Element is null.
        /// Input: Valid config with null Element property and valid View value.
        /// Expected: NullReferenceException is thrown when accessing config.Element.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);
            var testView = new Button();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.SetNextFocusBackView(mockConfig, testView));
        }

        /// <summary>
        /// Tests that GetNextFocusDirection throws NullReferenceException when element parameter is null.
        /// Validates null parameter handling and ensures proper exception behavior.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void GetNextFocusDirection_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusDirection(element));
        }

        /// <summary>
        /// Tests that GetNextFocusDirection returns the string value obtained from GetValue method.
        /// Validates the method correctly retrieves and casts the property value to string.
        /// Expected result: The string value returned from GetValue is returned.
        /// </summary>
        [Fact]
        public void GetNextFocusDirection_ValidElement_ReturnsValueFromGetValue()
        {
            // Arrange
            const string expectedValue = "TestDirection";
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusDirectionProperty)
                .Returns(expectedValue);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusDirection(mockElement);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetNextFocusDirection returns null when GetValue returns null.
        /// Validates proper null handling and string casting behavior.
        /// Expected result: Null is returned without throwing an exception.
        /// </summary>
        [Fact]
        public void GetNextFocusDirection_ValidElementReturnsNull_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusDirectionProperty)
                .Returns((object)null);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusDirection(mockElement);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusDirection returns empty string when GetValue returns empty string.
        /// Validates edge case behavior with empty string values.
        /// Expected result: Empty string is returned.
        /// </summary>
        [Fact]
        public void GetNextFocusDirection_ValidElementReturnsEmptyString_ReturnsEmptyString()
        {
            // Arrange
            const string expectedValue = "";
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusDirectionProperty)
                .Returns(expectedValue);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusDirection(mockElement);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that GetNextFocusDirection returns whitespace string when GetValue returns whitespace.
        /// Validates edge case behavior with whitespace-only string values.
        /// Expected result: Whitespace string is returned unchanged.
        /// </summary>
        [Fact]
        public void GetNextFocusDirection_ValidElementReturnsWhitespace_ReturnsWhitespace()
        {
            // Arrange
            const string expectedValue = "   ";
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusDirectionProperty)
                .Returns(expectedValue);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusDirection(mockElement);

            // Assert
            Assert.Equal(expectedValue, result);
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView calls the static method with correct parameters and returns the config.
        /// Verifies the fluent API behavior with a valid View parameter.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_ValidConfigAndView_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            var element = Substitute.For<Microsoft.Maui.Controls.VisualElement>();
            var view = new View();

            config.Element.Returns(element);

            // Act
            var result = config.SetNextFocusForwardView(view);

            // Assert
            element.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty, view);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView calls the static method with null value and returns the config.
        /// Verifies the fluent API behavior when setting a null View parameter.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_ValidConfigAndNullView_CallsStaticMethodAndReturnsConfig()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            var element = Substitute.For<Microsoft.Maui.Controls.VisualElement>();

            config.Element.Returns(element);

            // Act
            var result = config.SetNextFocusForwardView(null);

            // Assert
            element.Received(1).SetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusForwardViewProperty, null);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetNextFocusForwardView returns the exact same config instance passed as parameter.
        /// Verifies the fluent API contract for method chaining.
        /// </summary>
        [Fact]
        public void SetNextFocusForwardView_ValidConfig_ReturnsSameConfigInstance()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            var element = Substitute.For<Microsoft.Maui.Controls.VisualElement>();
            var view = new View();

            config.Element.Returns(element);

            // Act
            var result = config.SetNextFocusForwardView(view);

            // Assert
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that SetNextFocusDirection throws ArgumentNullException when element parameter is null.
        /// Verifies proper null parameter validation for the element parameter.
        /// Should throw ArgumentNullException when attempting to call SetValue on null element.
        /// </summary>
        [Fact]
        public void SetNextFocusDirection_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject element = null;
            string value = "test";

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.SetNextFocusDirection(element, value));
        }

        /// <summary>
        /// Tests that SetNextFocusDirection works correctly with null value parameter.
        /// Verifies that null string values are properly handled and passed to SetValue.
        /// Should call SetValue with NextFocusDirectionProperty and null value.
        /// </summary>
        [Fact]
        public void SetNextFocusDirection_ValidElementNullValue_CallsSetValueWithNullValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = null;

            // Act
            VisualElement.SetNextFocusDirection(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, null);
        }

        /// <summary>
        /// Tests that SetNextFocusDirection works correctly with empty string value.
        /// Verifies that empty string values are properly handled and passed to SetValue.
        /// Should call SetValue with NextFocusDirectionProperty and empty string value.
        /// </summary>
        [Fact]
        public void SetNextFocusDirection_ValidElementEmptyValue_CallsSetValueWithEmptyValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = "";

            // Act
            VisualElement.SetNextFocusDirection(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, "");
        }

        /// <summary>
        /// Tests that SetNextFocusDirection works correctly with whitespace string value.
        /// Verifies that whitespace-only string values are properly handled and passed to SetValue.
        /// Should call SetValue with NextFocusDirectionProperty and whitespace string value.
        /// </summary>
        [Fact]
        public void SetNextFocusDirection_ValidElementWhitespaceValue_CallsSetValueWithWhitespaceValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = "   ";

            // Act
            VisualElement.SetNextFocusDirection(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, "   ");
        }

        /// <summary>
        /// Tests that SetNextFocusDirection works correctly with valid string value.
        /// Verifies that valid string values are properly handled and passed to SetValue.
        /// Should call SetValue with NextFocusDirectionProperty and the provided string value.
        /// </summary>
        [Theory]
        [InlineData("up")]
        [InlineData("down")]
        [InlineData("left")]
        [InlineData("right")]
        [InlineData("forward")]
        [InlineData("back")]
        [InlineData("none")]
        [InlineData("SomeRandomValue")]
        [InlineData("123")]
        [InlineData("special!@#$%characters")]
        public void SetNextFocusDirection_ValidElementValidValue_CallsSetValueWithCorrectParameters(string value)
        {
            // Arrange
            var element = Substitute.For<BindableObject>();

            // Act
            VisualElement.SetNextFocusDirection(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, value);
        }

        /// <summary>
        /// Tests that SetNextFocusDirection calls SetValue exactly once.
        /// Verifies that the method makes a single call to SetValue with proper parameters.
        /// Should call SetValue once with NextFocusDirectionProperty and the provided value.
        /// </summary>
        [Fact]
        public void SetNextFocusDirection_ValidParameters_CallsSetValueExactlyOnce()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            string value = "testValue";

            // Act
            VisualElement.SetNextFocusDirection(element, value);

            // Assert
            element.Received(1).SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>());
            element.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, value);
        }

        /// <summary>
        /// Tests that MoveFocusRight throws ArgumentNullException when config parameter is null.
        /// Validates that the method properly handles null input and throws the expected exception.
        /// </summary>
        [Fact]
        public void MoveFocusRight_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.MoveFocusRight());
        }

        /// <summary>
        /// Tests that MoveFocusRight throws NullReferenceException when config.Element is null.
        /// Validates that the method fails appropriately when the Element property returns null.
        /// </summary>
        [Fact]
        public void MoveFocusRight_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => config.MoveFocusRight());
        }

        /// <summary>
        /// Tests that MoveFocusRight sets the focus direction to Right and returns the same config.
        /// Validates that the method correctly calls SetValue with NextFocusDirectionProperty and "Right",
        /// and returns the original config object for fluent API chaining.
        /// </summary>
        [Fact]
        public void MoveFocusRight_ValidConfig_SetsDirectionRightAndReturnsConfig()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = config.MoveFocusRight();

            // Assert
            element.Received(1).SetValue(TizenSpecific.VisualElement.NextFocusDirectionProperty, FocusDirection.Right);
            Assert.Same(config, result);
        }

        /// <summary>
        /// Tests that MoveFocusRight correctly uses the FocusDirection.Right constant value.
        /// Validates that the method passes the correct string value ("Right") to SetNextFocusDirection.
        /// </summary>
        [Fact]
        public void MoveFocusRight_ValidConfig_UsesCorrectFocusDirectionValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            config.MoveFocusRight();

            // Assert
            element.Received(1).SetValue(TizenSpecific.VisualElement.NextFocusDirectionProperty, "Right");
        }

        /// <summary>
        /// Tests that MoveFocusBack sets the focus direction to Back and returns the config object.
        /// Input: Valid platform configuration object.
        /// Expected: SetNextFocusDirection called with FocusDirection.Back, same config returned.
        /// </summary>
        [Fact]
        public void MoveFocusBack_ValidConfig_SetsFocusDirectionToBackAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.MoveFocusBack(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, FocusDirection.Back);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that MoveFocusBack throws when config parameter is null.
        /// Input: Null config parameter.
        /// Expected: ArgumentNullException or NullReferenceException thrown.
        /// </summary>
        [Fact]
        public void MoveFocusBack_NullConfig_ThrowsException()
        {
            // Arrange
            IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement> config = null;

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => VisualElement.MoveFocusBack(config));
        }

        /// <summary>
        /// Tests that MoveFocusBack throws when config.Element is null.
        /// Input: Config with null Element property.
        /// Expected: ArgumentNullException or NullReferenceException thrown.
        /// </summary>
        [Fact]
        public void MoveFocusBack_ConfigWithNullElement_ThrowsException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => VisualElement.MoveFocusBack(mockConfig));
        }

        /// <summary>
        /// Tests that MoveFocusBack uses the exact FocusDirection.Back constant value.
        /// Input: Valid platform configuration object.
        /// Expected: SetValue called with exact string "Back".
        /// </summary>
        [Fact]
        public void MoveFocusBack_ValidConfig_UsesCorrectFocusDirectionConstant()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            VisualElement.MoveFocusBack(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, "Back");
        }

        /// <summary>
        /// Tests that MoveFocusBack supports fluent method chaining by returning the same config object.
        /// Input: Valid platform configuration object.
        /// Expected: Exact same object reference returned for chaining.
        /// </summary>
        [Fact]
        public void MoveFocusBack_ValidConfig_SupportsFluentChaining()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result1 = VisualElement.MoveFocusBack(mockConfig);
            var result2 = result1.MoveFocusBack();

            // Assert
            Assert.Same(mockConfig, result1);
            Assert.Same(mockConfig, result2);
            Assert.Same(result1, result2);
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView returns the expected View when the underlying property has a value.
        /// Tests the extension method with a valid configuration and verifies it delegates to the static method correctly.
        /// Expected to return the View that was previously set on the element.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_WithValidConfigurationAndSetView_ReturnsExpectedView()
        {
            // Arrange
            var expectedView = new Button { Text = "Test Button" };
            var element = new Microsoft.Maui.Controls.VisualElement();
            TizenSpecific.VisualElement.SetNextFocusForwardView(element, expectedView);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetNextFocusForwardView();

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView returns null when the underlying property has not been set.
        /// Tests the extension method with a configuration that has no focus view set.
        /// Expected to return null as the default value.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_WithValidConfigurationAndNoViewSet_ReturnsNull()
        {
            // Arrange
            var element = new Microsoft.Maui.Controls.VisualElement();
            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetNextFocusForwardView();

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusForwardView properly accesses the Element property from the configuration.
        /// Tests that the extension method correctly delegates to the underlying static method.
        /// Expected to call the Element property exactly once and return the result from the static method.
        /// </summary>
        [Fact]
        public void GetNextFocusForwardView_WithMockedConfiguration_AccessesElementPropertyCorrectly()
        {
            // Arrange
            var expectedView = new Button { Text = "Focus View" };
            var element = new Microsoft.Maui.Controls.VisualElement();
            TizenSpecific.VisualElement.SetNextFocusForwardView(element, expectedView);

            var config = Substitute.For<IPlatformElementConfiguration<PlatformConfiguration.Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = config.GetNextFocusForwardView();

            // Assert
            config.Received(1).Element;
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView returns the expected View when a valid BindableObject element is provided with a set NextFocusBackView property.
        /// Input conditions: Valid BindableObject with NextFocusBackView property set to a View instance.
        /// Expected result: Returns the View that was set on the property.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ValidElementWithView_ReturnsView()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var expectedView = Substitute.For<View>();
            mockElement.GetValue(VisualElement.NextFocusBackViewProperty).Returns(expectedView);

            // Act
            var result = VisualElement.GetNextFocusBackView(mockElement);

            // Assert
            Assert.Equal(expectedView, result);
            mockElement.Received(1).GetValue(VisualElement.NextFocusBackViewProperty);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView returns null when a valid BindableObject element is provided with NextFocusBackView property set to null.
        /// Input conditions: Valid BindableObject with NextFocusBackView property set to null.
        /// Expected result: Returns null.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ValidElementWithNullView_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(VisualElement.NextFocusBackViewProperty).Returns((View)null);

            // Act
            var result = VisualElement.GetNextFocusBackView(mockElement);

            // Assert
            Assert.Null(result);
            mockElement.Received(1).GetValue(VisualElement.NextFocusBackViewProperty);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView returns the default value when a valid BindableObject element is provided with NextFocusBackView property unset.
        /// Input conditions: Valid BindableObject with NextFocusBackView property returning default value.
        /// Expected result: Returns null (the default value for View type).
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ValidElementWithDefaultValue_ReturnsNull()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            mockElement.GetValue(VisualElement.NextFocusBackViewProperty).Returns(default(View));

            // Act
            var result = VisualElement.GetNextFocusBackView(mockElement);

            // Assert
            Assert.Null(result);
            mockElement.Received(1).GetValue(VisualElement.NextFocusBackViewProperty);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView throws ArgumentNullException when null element parameter is provided.
        /// Input conditions: Null BindableObject element parameter.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_NullElement_ThrowsArgumentNullException()
        {
            // Arrange
            BindableObject nullElement = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.GetNextFocusBackView(nullElement));
        }

        /// <summary>
        /// Tests that GetNextFocusBackView correctly casts the returned object to View type.
        /// Input conditions: Valid BindableObject with GetValue returning an object that can be cast to View.
        /// Expected result: Returns the object cast to View type.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ValidElement_CastsReturnValueToView()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockView = Substitute.For<View>();
            object returnValue = mockView;
            mockElement.GetValue(VisualElement.NextFocusBackViewProperty).Returns(returnValue);

            // Act
            var result = VisualElement.GetNextFocusBackView(mockElement);

            // Assert
            Assert.Same(mockView, result);
            Assert.IsType<View>(result);
            mockElement.Received(1).GetValue(VisualElement.NextFocusBackViewProperty);
        }

        /// <summary>
        /// Tests that SetNextFocusBackView throws NullReferenceException when element parameter is null.
        /// Verifies that the method does not handle null element parameter gracefully.
        /// Expected result: NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_NullElement_ThrowsNullReferenceException()
        {
            // Arrange
            BindableObject element = null;
            var value = Substitute.For<View>();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                VisualElement.SetNextFocusBackView(element, value));
        }

        /// <summary>
        /// Tests that SetNextFocusBackView successfully calls SetValue when element is valid and value is null.
        /// Verifies that null values are accepted for the View parameter.
        /// Expected result: SetValue is called with NextFocusBackViewProperty and null value.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_ValidElementAndNullValue_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            View value = null;

            // Act
            VisualElement.SetNextFocusBackView(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.NextFocusBackViewProperty, null);
        }

        /// <summary>
        /// Tests that SetNextFocusBackView successfully calls SetValue when both element and value are valid.
        /// Verifies the normal operation path with valid parameters.
        /// Expected result: SetValue is called with NextFocusBackViewProperty and the provided view.
        /// </summary>
        [Fact]
        public void SetNextFocusBackView_ValidElementAndValue_CallsSetValue()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            var value = Substitute.For<View>();

            // Act
            VisualElement.SetNextFocusBackView(element, value);

            // Assert
            element.Received(1).SetValue(VisualElement.NextFocusBackViewProperty, value);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView extension method returns the view when config has valid element with focus view set.
        /// Input: Valid platform configuration with element that has NextFocusBackView set to a specific view.
        /// Expected: Returns the view that was set on the element.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ValidConfigWithFocusViewSet_ReturnsExpectedView()
        {
            // Arrange
            var expectedView = Substitute.For<View>();
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusBackViewProperty)
                .Returns(expectedView);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusBackView(config);

            // Assert
            Assert.Same(expectedView, result);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView extension method returns null when config has element with no focus view set.
        /// Input: Valid platform configuration with element that has no NextFocusBackView set (returns null).
        /// Expected: Returns null.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ValidConfigWithNoFocusViewSet_ReturnsNull()
        {
            // Arrange
            var element = Substitute.For<BindableObject>();
            element.GetValue(Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.NextFocusBackViewProperty)
                .Returns(null);

            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns(element);

            // Act
            var result = Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusBackView(config);

            // Assert
            Assert.Null(result);
        }

        /// <summary>
        /// Tests that GetNextFocusBackView extension method throws ArgumentNullException when config parameter is null.
        /// Input: Null platform configuration parameter.
        /// Expected: Throws ArgumentNullException.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusBackView(config));
        }

        /// <summary>
        /// Tests that GetNextFocusBackView extension method throws NullReferenceException when config has null element.
        /// Input: Valid platform configuration with null Element property.
        /// Expected: Throws NullReferenceException when trying to access the null element.
        /// </summary>
        [Fact]
        public void GetNextFocusBackView_ConfigWithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var config = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            config.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.GetNextFocusBackView(config));
        }
    }


    public class TizenSpecificVisualElementSetStyleTests
    {
        /// <summary>
        /// Tests that SetStyle throws ArgumentNullException when config parameter is null.
        /// Verifies that accessing config.Element on null config throws the expected exception.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void SetStyle_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement> config = null;
            string value = "test-style";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => config.SetStyle(value));
        }

        /// <summary>
        /// Tests that SetStyle calls SetValue on the underlying element and returns the config object.
        /// Verifies that the extension method properly delegates to the static SetStyle method and maintains fluent interface.
        /// Expected result: SetValue is called with StyleProperty and the provided value, and the same config is returned.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("normal-style")]
        [InlineData("very-long-style-name-with-many-characters-that-exceeds-typical-length-boundaries-to-test-edge-cases")]
        [InlineData("style-with-special-chars-!@#$%^&*()")]
        public void SetStyle_ValidConfig_CallsSetValueAndReturnsConfig(string value)
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = mockConfig.SetStyle(value);

            // Assert
            mockElement.Received(1).SetValue(TizenSpecific.VisualElement.StyleProperty, value);
            Assert.Same(mockConfig, result);
        }
    }


    /// <summary>
    /// Tests for Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.VisualElement.MoveFocusLeft method.
    /// </summary>
    public class TizenSpecificVisualElementMoveFocusLeftTests
    {
        /// <summary>
        /// Tests that MoveFocusLeft with a valid configuration calls SetNextFocusDirection with the Left direction
        /// and returns the same configuration object for fluent chaining.
        /// </summary>
        [Fact]
        public void MoveFocusLeft_WithValidConfig_CallsSetNextFocusDirectionWithLeftDirectionAndReturnsSameConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.MoveFocusLeft(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, FocusDirection.Left);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that MoveFocusLeft throws NullReferenceException when the config parameter is null,
        /// as it attempts to access config.Element without null checking.
        /// </summary>
        [Fact]
        public void MoveFocusLeft_WithNullConfig_ThrowsNullReferenceException()
        {
            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.MoveFocusLeft(null));
        }

        /// <summary>
        /// Tests that MoveFocusLeft throws NullReferenceException when config.Element is null,
        /// as SetNextFocusDirection attempts to call SetValue on a null element.
        /// </summary>
        [Fact]
        public void MoveFocusLeft_WithNullElement_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((Microsoft.Maui.Controls.VisualElement)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => VisualElement.MoveFocusLeft(mockConfig));
        }

        /// <summary>
        /// Tests that MoveFocusLeft correctly passes the Left direction constant to SetNextFocusDirection,
        /// verifying the specific string value used for the left focus direction.
        /// </summary>
        [Fact]
        public void MoveFocusLeft_WithValidConfig_PassesCorrectLeftDirectionValue()
        {
            // Arrange  
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            VisualElement.MoveFocusLeft(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, "Left");
        }

        /// <summary>
        /// Tests that MoveFocusLeft maintains the fluent interface pattern by returning the exact same
        /// configuration object that was passed in, enabling method chaining.
        /// </summary>
        [Fact]
        public void MoveFocusLeft_WithValidConfig_ReturnsExactSameConfigReference()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.MoveFocusLeft(mockConfig);

            // Assert
            Assert.True(ReferenceEquals(mockConfig, result));
        }
    }


    public class VisualElementTizenFocusTests
    {
        /// <summary>
        /// Tests that MoveFocusForward throws ArgumentNullException when config parameter is null.
        /// This verifies proper null parameter validation.
        /// Expected result: ArgumentNullException is thrown.
        /// </summary>
        [Fact]
        public void MoveFocusForward_NullConfig_ThrowsArgumentNullException()
        {
            // Arrange
            IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement> config = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.MoveFocusForward(config));
        }

        /// <summary>
        /// Tests that MoveFocusForward sets the focus direction to Forward and returns the config for fluent chaining.
        /// This verifies the main functionality of setting focus direction and fluent interface support.
        /// Expected result: SetNextFocusDirection is called with Forward direction and the same config is returned.
        /// </summary>
        [Fact]
        public void MoveFocusForward_ValidConfig_SetsFocusDirectionAndReturnsConfig()
        {
            // Arrange
            var mockElement = Substitute.For<BindableObject>();
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns(mockElement);

            // Act
            var result = VisualElement.MoveFocusForward(mockConfig);

            // Assert
            mockElement.Received(1).SetValue(VisualElement.NextFocusDirectionProperty, FocusDirection.Forward);
            Assert.Same(mockConfig, result);
        }

        /// <summary>
        /// Tests that MoveFocusForward handles null Element property gracefully by passing null to SetNextFocusDirection.
        /// This verifies behavior when the config's Element property is null.
        /// Expected result: ArgumentNullException is thrown when trying to call SetValue on null element.
        /// </summary>
        [Fact]
        public void MoveFocusForward_ConfigWithNullElement_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConfig = Substitute.For<IPlatformElementConfiguration<Tizen, Microsoft.Maui.Controls.VisualElement>>();
            mockConfig.Element.Returns((BindableObject)null);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => VisualElement.MoveFocusForward(mockConfig));
        }
    }
}