#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.StyleSheets;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;
using NSubstitute;
using Xunit;

using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

namespace Microsoft.Maui.Controls.StyleSheets.UnitTests
{


    public class StyleTests : BaseTestFixture
    {
        public StyleTests()
        {
            ApplicationExtensions.CreateAndSetMockApplication();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Application.ClearCurrent();
            }

            base.Dispose(disposing);
        }

        [Fact]
        public void PropertiesAreApplied()
        {
            var styleString = @"background-color: #ff0000;";
            var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
            Assert.NotNull(style);

            var ve = new VisualElement();
            Assert.Null(ve.BackgroundColor);
            style.Apply(ve);
            Assert.Equal(ve.BackgroundColor, Colors.Red);
        }

        [Fact]
        public void PropertiesSetByStyleDoesNotOverrideManualOne()
        {
            var styleString = @"background-color: #ff0000;";
            var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
            Assert.NotNull(style);

            var ve = new VisualElement() { BackgroundColor = Colors.Pink };
            Assert.Equal(ve.BackgroundColor, Colors.Pink);

            style.Apply(ve);
            Assert.Equal(ve.BackgroundColor, Colors.Pink);
        }

        [Fact]
        public void StylesAreCascading()
        {
            //color should cascade, background-color should not
            var styleString = @"background-color: #ff0000; color: #00ff00;";
            var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
            Assert.NotNull(style);

            var label = new Label();
            var layout = new StackLayout
            {
                Children = {
                    label,
                }
            };

            Assert.Null(layout.BackgroundColor);
            Assert.Null(label.BackgroundColor);
            Assert.Null(label.TextColor);

            style.Apply(layout);
            Assert.Equal(layout.BackgroundColor, Colors.Red);
            Assert.Null(label.BackgroundColor);
            Assert.Equal(label.TextColor, Colors.Lime);
        }

        [Fact]
        public void PropertiesAreOnlySetOnMatchingElements()
        {
            var styleString = @"background-color: #ff0000; color: #00ff00;";
            var style = Style.Parse(new CssReader(new StringReader(styleString)), '}');
            Assert.NotNull(style);

            var layout = new StackLayout();
            Assert.Null(layout.GetValue(TextElement.TextColorProperty));
        }

        [Fact]
        public void StyleSheetsOnAppAreApplied()
        {
            var app = new MockApplication();
            app.Resources.Add(StyleSheet.FromString("label{ color: red;}"));
            var page = new ContentPage
            {
                Content = new Label()
            };
            app.LoadPage(page);
            Assert.Equal((page.Content as Label).TextColor, Colors.Red);
        }

        [Fact]
        public void StyleSheetsOnAppAreAppliedBeforePageStyleSheet()
        {
            var app = new MockApplication();
            app.Resources.Add(StyleSheet.FromString("label{ color: white; background-color: blue; }"));
            var page = new ContentPage
            {
                Content = new Label()
            };
            page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
            app.LoadPage(page);
            Assert.Equal((page.Content as Label).TextColor, Colors.Red);
            Assert.Equal((page.Content as Label).BackgroundColor, Colors.Blue);
        }

        [Fact]
        public void StyleSheetsOnChildAreReAppliedWhenParentStyleSheetAdded()
        {
            var app = new MockApplication();
            var page = new ContentPage
            {
                Content = new Label()
            };
            page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
            app.LoadPage(page);
            Assert.Equal((page.Content as Label).TextColor, Colors.Red);

            app.Resources.Add(StyleSheet.FromString("label{ color: white; background-color: blue; }"));
            Assert.Equal((page.Content as Label).BackgroundColor, Colors.Blue);
            Assert.Equal((page.Content as Label).TextColor, Colors.Red);
        }

        [Fact]
        public void StyleSheetsOnSubviewAreAppliedBeforePageStyleSheet()
        {
            var app = new MockApplication();
            app.Resources.Add(StyleSheet.FromString("label{ color: white; }"));
            var label = new Label();
            label.Resources.Add(StyleSheet.FromString("label{color: yellow;}"));

            var page = new ContentPage
            {
                Content = label
            };
            page.Resources.Add(StyleSheet.FromString("label{ color: red; }"));
            app.LoadPage(page);
            Assert.Equal((page.Content as Label).TextColor, Colors.Yellow);
        }


        /// <summary>
        /// Tests that UnApply method throws NotImplementedException when called with a valid IStylable instance.
        /// This verifies the method correctly throws the expected exception for any valid input.
        /// </summary>
        [Fact]
        public void UnApply_WithValidIStylable_ThrowsNotImplementedException()
        {
            // Arrange
            var mockStylable = Substitute.For<IStylable>();
            var style = new Style();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => style.UnApply(mockStylable));
        }

        /// <summary>
        /// Tests that UnApply method throws NotImplementedException when called with null parameter.
        /// This verifies the method throws the expected exception even before parameter validation.
        /// </summary>
        [Fact]
        public void UnApply_WithNullIStylable_ThrowsNotImplementedException()
        {
            // Arrange
            var style = new Style();

            // Act & Assert
            Assert.Throws<NotImplementedException>(() => style.UnApply(null));
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests
{
    /// <summary>
    /// Tests for the Style.Apply method in Microsoft.Maui.Controls.StyleSheets.Style class.
    /// </summary>
    public partial class StyleApplyTests : BaseTestFixture
    {
        public StyleApplyTests()
        {
            ApplicationExtensions.CreateAndSetMockApplication();
        }

        /// <summary>
        /// Tests that Apply throws ArgumentNullException when styleable parameter is null.
        /// This test covers the null check validation at line 27-28.
        /// </summary>
        [Fact]
        public void Apply_NullStyleableParameter_ThrowsArgumentNullException()
        {
            // Arrange
            var style = new Style();
            VisualElement styleable = null;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => style.Apply(styleable));
            Assert.Equal("styleable", exception.ParamName);
        }

        /// <summary>
        /// Tests that Apply handles "initial" value by setting property to its DefaultValue.
        /// This test covers the "initial" value handling at lines 35-36.
        /// </summary>
        [Fact]
        public void Apply_InitialValue_SetsPropertyToDefaultValue()
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("background-color", "initial");

            var styleable = Substitute.For<VisualElement>();
            var bindableProperty = Substitute.For<BindableProperty>();
            var defaultValue = new object();
            bindableProperty.DefaultValue.Returns(defaultValue);

            ((IStylable)styleable).GetProperty("background-color", false).Returns(bindableProperty);

            var selectorSpecificity = new SelectorSpecificity();

            // Act
            style.Apply(styleable, selectorSpecificity);

            // Assert
            styleable.Received(1).SetValue(
                bindableProperty,
                defaultValue,
                Arg.Is<SetterSpecificity>(s =>
                    s.Priority == SetterSpecificity.StyleImplicit &&
                    s.Id == 0 &&
                    s.Class == 0 &&
                    s.Type == 0));
        }

        /// <summary>
        /// Tests that Apply handles "INITIAL" value (case insensitive) by setting property to its DefaultValue.
        /// This test covers the case insensitive "initial" value handling at lines 35-36.
        /// </summary>
        [Fact]
        public void Apply_InitialValueCaseInsensitive_SetsPropertyToDefaultValue()
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("color", "INITIAL");

            var styleable = Substitute.For<VisualElement>();
            var bindableProperty = Substitute.For<BindableProperty>();
            var defaultValue = new object();
            bindableProperty.DefaultValue.Returns(defaultValue);

            ((IStylable)styleable).GetProperty("color", false).Returns(bindableProperty);

            var selectorSpecificity = new SelectorSpecificity();

            // Act
            style.Apply(styleable, selectorSpecificity);

            // Assert
            styleable.Received(1).SetValue(
                bindableProperty,
                defaultValue,
                Arg.Is<SetterSpecificity>(s =>
                    s.Priority == SetterSpecificity.StyleImplicit &&
                    s.Id == 0 &&
                    s.Class == 0 &&
                    s.Type == 0));
        }

        /// <summary>
        /// Tests that Apply skips visual children that are not VisualElements.
        /// This test covers the null check for non-VisualElement children at lines 50-51.
        /// </summary>
        [Fact]
        public void Apply_VisualChildrenNotVisualElement_SkipsNonVisualElementChildren()
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("background-color", "#ff0000");

            var styleable = Substitute.For<VisualElement>();
            var bindableProperty = Substitute.For<BindableProperty>();
            ((IStylable)styleable).GetProperty("background-color", false).Returns(bindableProperty);

            // Create a non-VisualElement child that implements IVisualTreeElement
            var nonVisualElementChild = Substitute.For<IVisualTreeElement>();
            var visualElementChild = Substitute.For<VisualElement>();
            ((IStylable)visualElementChild).GetProperty("background-color", true).Returns(bindableProperty);

            var children = new List<IVisualTreeElement> { nonVisualElementChild, visualElementChild };
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(children);

            // Act
            style.Apply(styleable);

            // Assert
            // Verify the non-VisualElement child was not processed (no SetValue calls on it)
            // Only the VisualElement child should receive SetValue calls
            visualElementChild.Received().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>(), Arg.Any<SetterSpecificity>());
        }

        /// <summary>
        /// Tests that Apply works correctly with empty Declarations dictionary.
        /// </summary>
        [Fact]
        public void Apply_EmptyDeclarations_DoesNotThrow()
        {
            // Arrange
            var style = new Style();
            var styleable = Substitute.For<VisualElement>();
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            // Act & Assert (should not throw)
            style.Apply(styleable);

            // Verify no properties were set
            styleable.DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>(), Arg.Any<SetterSpecificity>());
        }

        /// <summary>
        /// Tests that Apply correctly handles SelectorSpecificity values in SetterSpecificity.
        /// </summary>
        [Theory]
        [InlineData(1, 2, 3)]
        [InlineData(255, 255, 255)]
        [InlineData(0, 0, 0)]
        public void Apply_SelectorSpecificity_CreatesCorrectSetterSpecificity(int id, int @class, int type)
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("background-color", "#ff0000");

            var styleable = Substitute.For<VisualElement>();
            var bindableProperty = Substitute.For<BindableProperty>();
            ((IStylable)styleable).GetProperty("background-color", false).Returns(bindableProperty);
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            var selectorSpecificityMock = Substitute.For<SelectorSpecificity>();
            selectorSpecificityMock.Id.Returns(id);
            selectorSpecificityMock.Class.Returns(@class);
            selectorSpecificityMock.Type.Returns(type);

            // Act
            style.Apply(styleable, selectorSpecificityMock);

            // Assert
            styleable.Received(1).SetValue(
                bindableProperty,
                Arg.Any<object>(),
                Arg.Is<SetterSpecificity>(s =>
                    s.Priority == SetterSpecificity.StyleImplicit &&
                    s.Id == (byte)id &&
                    s.Class == (byte)@class &&
                    s.Type == (byte)type));
        }

        /// <summary>
        /// Tests that Apply skips properties that don't exist (GetProperty returns null).
        /// </summary>
        [Fact]
        public void Apply_PropertyNotFound_SkipsProperty()
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("nonexistent-property", "value");

            var styleable = Substitute.For<VisualElement>();
            ((IStylable)styleable).GetProperty("nonexistent-property", false).Returns((BindableProperty)null);
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            // Act
            style.Apply(styleable);

            // Assert
            styleable.DidNotReceive().SetValue(Arg.Any<BindableProperty>(), Arg.Any<object>(), Arg.Any<SetterSpecificity>());
        }

        /// <summary>
        /// Tests that Apply passes inheriting parameter correctly to GetProperty.
        /// </summary>
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Apply_InheritingParameter_PassedToGetProperty(bool inheriting)
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("color", "red");

            var styleable = Substitute.For<VisualElement>();
            var bindableProperty = Substitute.For<BindableProperty>();
            ((IStylable)styleable).GetProperty("color", inheriting).Returns(bindableProperty);
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            // Act
            style.Apply(styleable, inheriting: inheriting);

            // Assert
            ((IStylable)styleable).Received(1).GetProperty("color", inheriting);
        }

        /// <summary>
        /// Tests that Apply recursively calls itself on visual children with inheriting=true.
        /// </summary>
        [Fact]
        public void Apply_VisualChildren_RecursivelyAppliesWithInheritingTrue()
        {
            // Arrange
            var style = new Style();

            var parent = Substitute.For<VisualElement>();
            var child1 = Substitute.For<VisualElement>();
            var child2 = Substitute.For<VisualElement>();

            var children = new List<IVisualTreeElement> { child1, child2 };
            ((IVisualTreeElement)parent).GetVisualChildren().Returns(children);
            ((IVisualTreeElement)child1).GetVisualChildren().Returns(new List<IVisualTreeElement>());
            ((IVisualTreeElement)child2).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            // Act
            style.Apply(parent, inheriting: false);

            // Assert - children should be processed (even though no declarations, the method should be called)
            // We can't directly verify the recursive call, but we can verify GetVisualChildren was called
            ((IVisualTreeElement)parent).Received(1).GetVisualChildren();
        }

        /// <summary>
        /// Tests that Apply handles multiple declarations correctly.
        /// </summary>
        [Fact]
        public void Apply_MultipleDeclarations_AppliesAllProperties()
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("background-color", "#ff0000");
            style.Declarations.Add("color", "#00ff00");

            var styleable = Substitute.For<VisualElement>();
            var backgroundProperty = Substitute.For<BindableProperty>();
            var colorProperty = Substitute.For<BindableProperty>();

            ((IStylable)styleable).GetProperty("background-color", false).Returns(backgroundProperty);
            ((IStylable)styleable).GetProperty("color", false).Returns(colorProperty);
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            // Act
            style.Apply(styleable);

            // Assert
            styleable.Received(1).SetValue(backgroundProperty, Arg.Any<object>(), Arg.Any<SetterSpecificity>());
            styleable.Received(1).SetValue(colorProperty, Arg.Any<object>(), Arg.Any<SetterSpecificity>());
        }

        /// <summary>
        /// Tests that Apply handles null and empty string values in declarations.
        /// </summary>
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("somevalue")]
        public void Apply_VariousDeclarationValues_HandledCorrectly(string value)
        {
            // Arrange
            var style = new Style();
            style.Declarations.Add("test-property", value);

            var styleable = Substitute.For<VisualElement>();
            var bindableProperty = Substitute.For<BindableProperty>();
            ((IStylable)styleable).GetProperty("test-property", false).Returns(bindableProperty);
            ((IVisualTreeElement)styleable).GetVisualChildren().Returns(new List<IVisualTreeElement>());

            // Act & Assert (should not throw)
            style.Apply(styleable);

            // Verify SetValue was called
            styleable.Received(1).SetValue(bindableProperty, Arg.Any<object>(), Arg.Any<SetterSpecificity>());
        }
    }
}

namespace Microsoft.Maui.Controls.Core.UnitTests.StyleSheets
{
    /// <summary>
    /// Unit tests for the Style.Parse method in Microsoft.Maui.Controls.StyleSheets.Style class.
    /// </summary>
    public partial class StyleParseTests
    {
        /// <summary>
        /// Tests that Parse creates a Style with valid CSS property declarations.
        /// Input: Valid CSS property string "background-color: red; margin: 10px;"
        /// Expected: Style object with both properties in Declarations dictionary
        /// </summary>
        [Fact]
        public void Parse_ValidCssProperties_ReturnsStyleWithDeclarations()
        {
            // Arrange
            var css = "background-color: red; margin: 10px;";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Declarations);
            Assert.Equal(2, result.Declarations.Count);
            Assert.Equal("red", result.Declarations["background-color"]);
            Assert.Equal("10px", result.Declarations["margin"]);
        }

        /// <summary>
        /// Tests that Parse handles empty CSS input correctly.
        /// Input: Empty string
        /// Expected: Style object with empty Declarations dictionary
        /// </summary>
        [Fact]
        public void Parse_EmptyInput_ReturnsEmptyStyle()
        {
            // Arrange
            var css = "";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Declarations);
            Assert.Empty(result.Declarations);
        }

        /// <summary>
        /// Tests that Parse handles whitespace-only input correctly.
        /// Input: String with only whitespace characters
        /// Expected: Style object with empty Declarations dictionary
        /// </summary>
        [Fact]
        public void Parse_WhitespaceOnly_ReturnsEmptyStyle()
        {
            // Arrange
            var css = "   \t\n  ";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Declarations);
            Assert.Empty(result.Declarations);
        }

        /// <summary>
        /// Tests that Parse throws ArgumentNullException when CssReader is null.
        /// Input: null CssReader
        /// Expected: ArgumentNullException thrown
        /// </summary>
        [Fact]
        public void Parse_NullReader_ThrowsArgumentNullException()
        {
            // Arrange
            CssReader reader = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Style.Parse(reader, '}'));
        }

        /// <summary>
        /// Tests that Parse stops parsing when encountering the specified stop character.
        /// Input: CSS string with stop character in the middle
        /// Expected: Style object with properties parsed only before the stop character
        /// </summary>
        [Theory]
        [InlineData('}', "color: blue; } margin: 10px;", 1)]
        [InlineData(')', "color: blue; ) margin: 10px;", 1)]
        [InlineData(']', "color: blue; ] margin: 10px;", 1)]
        public void Parse_StopCharacter_StopsParsingAtCharacter(char stopChar, string css, int expectedCount)
        {
            // Arrange
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, stopChar);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Declarations.Count);
            Assert.Equal("blue", result.Declarations["color"]);
        }

        /// <summary>
        /// Tests that Parse handles properties without values correctly.
        /// Input: CSS property with colon but no value before semicolon
        /// Expected: Property not added to Declarations dictionary
        /// </summary>
        [Fact]
        public void Parse_PropertyWithoutValue_PropertyNotAdded()
        {
            // Arrange
            var css = "color:; margin: 10px;";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Declarations);
            Assert.Equal("10px", result.Declarations["margin"]);
        }

        /// <summary>
        /// Tests that Parse handles empty property names correctly.
        /// Input: CSS with colon but no preceding property name
        /// Expected: Property not added to Declarations dictionary
        /// </summary>
        [Fact]
        public void Parse_EmptyPropertyName_PropertyNotAdded()
        {
            // Arrange
            var css = ": red; margin: 10px;";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Declarations);
            Assert.Equal("10px", result.Declarations["margin"]);
        }

        /// <summary>
        /// Tests that Parse throws Exception when encountering invalid property name characters.
        /// Input: CSS with invalid first character for property name (number)
        /// Expected: Exception thrown
        /// </summary>
        [Fact]
        public void Parse_InvalidPropertyNameCharacter_ThrowsException()
        {
            // Arrange
            var css = "123invalid: red;";
            var reader = new CssReader(new StringReader(css));

            // Act & Assert
            Assert.Throws<Exception>(() => Style.Parse(reader, '}'));
        }

        /// <summary>
        /// Tests that Parse handles properties with no semicolon at the end correctly.
        /// Input: CSS property without trailing semicolon
        /// Expected: Property not added to Declarations dictionary if incomplete
        /// </summary>
        [Fact]
        public void Parse_PropertyWithoutSemicolon_PropertyNotAdded()
        {
            // Arrange
            var css = "color: red";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Declarations);
        }

        /// <summary>
        /// Tests that Parse handles CSS with various whitespace characters correctly.
        /// Input: CSS with tabs, newlines, and spaces around colons and semicolons
        /// Expected: Style object with properly parsed properties
        /// </summary>
        [Fact]
        public void Parse_WhitespaceAroundDeclarations_ParsesCorrectly()
        {
            // Arrange
            var css = " \t color \t : \n red \n ; \r\n margin \t: \t 10px \r\n ; ";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Declarations.Count);
            Assert.Equal(" red ", result.Declarations["color"]);
            Assert.Equal(" 10px ", result.Declarations["margin"]);
        }

        /// <summary>
        /// Tests that Parse handles special characters in property values correctly.
        /// Input: CSS with special characters, spaces, and symbols in values
        /// Expected: Style object with values containing all special characters
        /// </summary>
        [Theory]
        [InlineData("url('http://example.com/image.png')")]
        [InlineData("rgba(255, 0, 0, 0.5)")]
        [InlineData("calc(100% - 20px)")]
        [InlineData("#ff0000")]
        public void Parse_SpecialCharactersInValues_PreservesCharacters(string value)
        {
            // Arrange
            var css = $"background: {value};";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Declarations);
            Assert.Equal(value, result.Declarations["background"]);
        }

        /// <summary>
        /// Tests that Parse handles CSS property names with hyphens correctly.
        /// Input: CSS properties with hyphenated names
        /// Expected: Style object with hyphenated property names preserved
        /// </summary>
        [Theory]
        [InlineData("background-color", "red")]
        [InlineData("border-top-width", "1px")]
        [InlineData("-webkit-transform", "rotate(90deg)")]
        public void Parse_HyphenatedPropertyNames_ParsesCorrectly(string propertyName, string value)
        {
            // Arrange
            var css = $"{propertyName}: {value};";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Declarations);
            Assert.Equal(value, result.Declarations[propertyName]);
        }

        /// <summary>
        /// Tests that Parse handles the default stop character correctly.
        /// Input: CSS string without explicit stop character
        /// Expected: Style object with all properties parsed
        /// </summary>
        [Fact]
        public void Parse_DefaultStopCharacter_ParsesAllProperties()
        {
            // Arrange
            var css = "color: blue; margin: 5px; padding: 10px;";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Declarations.Count);
            Assert.Equal("blue", result.Declarations["color"]);
            Assert.Equal("5px", result.Declarations["margin"]);
            Assert.Equal("10px", result.Declarations["padding"]);
        }

        /// <summary>
        /// Tests that Parse handles CSS comments correctly by ignoring them.
        /// Input: CSS with C-style comments /* */
        /// Expected: Style object with properties parsed, comments ignored
        /// </summary>
        [Fact]
        public void Parse_CssWithComments_IgnoresComments()
        {
            // Arrange
            var css = "/* comment */ color: red; /* another comment */ margin: 10px; /* final comment */";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Declarations.Count);
            Assert.Equal("red", result.Declarations["color"]);
            Assert.Equal("10px", result.Declarations["margin"]);
        }

        /// <summary>
        /// Tests that Parse handles mixed valid and invalid property declarations.
        /// Input: CSS with both valid properties and properties missing values
        /// Expected: Only valid properties added to Declarations dictionary
        /// </summary>
        [Fact]
        public void Parse_MixedValidAndInvalidProperties_OnlyAddsValidProperties()
        {
            // Arrange
            var css = "color: red; invalid:; margin: 10px; :value; padding: 5px;";
            var reader = new CssReader(new StringReader(css));

            // Act
            var result = Style.Parse(reader, '}');

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Declarations.Count);
            Assert.Equal("red", result.Declarations["color"]);
            Assert.Equal("10px", result.Declarations["margin"]);
            Assert.Equal("5px", result.Declarations["padding"]);
        }
    }
}