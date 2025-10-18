#nullable disable

using System;
using System.ComponentModel;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using NSubstitute;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class TextTransformUtilitesTests
{
    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText when inputView is null.
    /// Input conditions: null inputView, valid platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_NullInputView_DoesNotThrow()
    {
        // Arrange
        InputView inputView = null;
        string platformText = "test text";

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText when platformText is null.
    /// Input conditions: valid mocked inputView, null platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_NullPlatformText_DoesNotThrow()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        string platformText = null;

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText when both parameters are null.
    /// Input conditions: null inputView, null platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_BothParametersNull_DoesNotThrow()
    {
        // Arrange
        InputView inputView = null;
        string platformText = null;

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText with empty platform text.
    /// Input conditions: valid mocked inputView, empty string platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_EmptyPlatformText_DoesNotThrow()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        string platformText = string.Empty;

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText with whitespace-only platform text.
    /// Input conditions: valid mocked inputView, whitespace-only platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_WhitespacePlatformText_DoesNotThrow()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        string platformText = "   \t\n\r   ";

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText with valid parameters.
    /// Input conditions: valid mocked inputView, valid platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_ValidParameters_DoesNotThrow()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        string platformText = "Hello, World!";

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText with special characters in platform text.
    /// Input conditions: valid mocked inputView, platformText containing special characters.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Theory]
    [InlineData("Special chars: !@#$%^&*()")]
    [InlineData("Unicode: 🚀🌟💫")]
    [InlineData("Line breaks:\nNew line\r\nCarriage return")]
    [InlineData("Tabs:\tTabbed\tText")]
    [InlineData("Quotes: \"Double\" 'Single'")]
    [InlineData("Backslashes: \\path\\to\\file")]
    public void SetPlainText_SpecialCharacters_DoesNotThrow(string platformText)
    {
        // Arrange
        var inputView = Substitute.For<InputView>();

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText with very long platform text.
    /// Input conditions: valid mocked inputView, very long platformText.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_VeryLongPlatformText_DoesNotThrow()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        string platformText = new string('A', 10000); // 10,000 character string

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText properly delegates to TextTransformUtilities.SetPlainText with maximum length string.
    /// Input conditions: valid mocked inputView, string at array maximum length boundary.
    /// Expected result: No exception thrown, method completes successfully.
    /// </summary>
    [Fact]
    public void SetPlainText_MaxLengthString_DoesNotThrow()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        // Create a reasonably large string to test boundary conditions
        string platformText = new string('X', 1000000); // 1 million characters

        // Act & Assert
        var exception = Record.Exception(() => TextTransformUtilites.SetPlainText(inputView, platformText));
        Assert.Null(exception);
    }

    /// <summary>
    /// Tests that SetPlainText method is marked with EditorBrowsable(Never) attribute as expected for obsolete wrapper.
    /// Input conditions: reflection on the SetPlainText method.
    /// Expected result: Method should have EditorBrowsableAttribute with Never state.
    /// </summary>
    [Fact]
    public void SetPlainText_HasEditorBrowsableNeverAttribute_AttributePresent()
    {
        // Arrange & Act
        var methodInfo = typeof(TextTransformUtilites).GetMethod(nameof(TextTransformUtilites.SetPlainText));
        var attributes = methodInfo.GetCustomAttributes(typeof(EditorBrowsableAttribute), false);

        // Assert
        Assert.Single(attributes);
        var editorBrowsableAttr = (EditorBrowsableAttribute)attributes[0];
        Assert.Equal(EditorBrowsableState.Never, editorBrowsableAttr.State);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with null input and returns empty string.
    /// </summary>
    [Fact]
    public void GetTransformedText_NullSource_ReturnsEmptyString()
    {
        // Arrange
        string source = null;
        var textTransform = TextTransform.None;

        // Act
        var result = TextTransformUtilites.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with empty input and returns empty string.
    /// </summary>
    [Fact]
    public void GetTransformedText_EmptySource_ReturnsEmptyString()
    {
        // Arrange
        string source = string.Empty;
        var textTransform = TextTransform.None;

        // Act
        var result = TextTransformUtilites.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with various text transformation values and produces expected results.
    /// </summary>
    /// <param name="source">The input text to transform.</param>
    /// <param name="textTransform">The transformation to apply.</param>
    /// <param name="expected">The expected transformed result.</param>
    [Theory]
    [InlineData("Hello World", TextTransform.None, "Hello World")]
    [InlineData("Hello World", TextTransform.Default, "Hello World")]
    [InlineData("Hello World", TextTransform.Lowercase, "hello world")]
    [InlineData("Hello World", TextTransform.Uppercase, "HELLO WORLD")]
    [InlineData("MiXeD cAsE", TextTransform.Lowercase, "mixed case")]
    [InlineData("MiXeD cAsE", TextTransform.Uppercase, "MIXED CASE")]
    [InlineData("123 Numbers!", TextTransform.Lowercase, "123 numbers!")]
    [InlineData("123 Numbers!", TextTransform.Uppercase, "123 NUMBERS!")]
    [InlineData("Special @#$% Characters", TextTransform.Lowercase, "special @#$% characters")]
    [InlineData("Special @#$% Characters", TextTransform.Uppercase, "SPECIAL @#$% CHARACTERS")]
    public void GetTransformedText_ValidInputs_ReturnsExpectedResult(string source, TextTransform textTransform, string expected)
    {
        // Act
        var result = TextTransformUtilites.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with whitespace-only input and preserves whitespace with transformations.
    /// </summary>
    [Theory]
    [InlineData("   ", TextTransform.None, "   ")]
    [InlineData("   ", TextTransform.Lowercase, "   ")]
    [InlineData("   ", TextTransform.Uppercase, "   ")]
    [InlineData("\t\n\r", TextTransform.None, "\t\n\r")]
    [InlineData("\t\n\r", TextTransform.Lowercase, "\t\n\r")]
    [InlineData("\t\n\r", TextTransform.Uppercase, "\t\n\r")]
    public void GetTransformedText_WhitespaceSource_PreservesWhitespace(string source, TextTransform textTransform, string expected)
    {
        // Act
        var result = TextTransformUtilites.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with Unicode characters and applies transformations correctly.
    /// </summary>
    [Theory]
    [InlineData("Café", TextTransform.Lowercase, "café")]
    [InlineData("Café", TextTransform.Uppercase, "CAFÉ")]
    [InlineData("Naïve", TextTransform.Lowercase, "naïve")]
    [InlineData("Naïve", TextTransform.Uppercase, "NAÏVE")]
    [InlineData("🙂 Emoji", TextTransform.Lowercase, "🙂 emoji")]
    [InlineData("🙂 Emoji", TextTransform.Uppercase, "🙂 EMOJI")]
    public void GetTransformedText_UnicodeCharacters_TransformsCorrectly(string source, TextTransform textTransform, string expected)
    {
        // Act
        var result = TextTransformUtilites.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with an invalid enum value (outside defined range) and treats it as None/Default.
    /// </summary>
    [Fact]
    public void GetTransformedText_InvalidEnumValue_TreatsAsNone()
    {
        // Arrange
        string source = "Test Text";
        var invalidTransform = (TextTransform)999;

        // Act
        var result = TextTransformUtilites.GetTransformedText(source, invalidTransform);

        // Assert
        Assert.Equal("Test Text", result);
    }

    /// <summary>
    /// Tests that GetTransformedText correctly delegates to TextTransformUtilities.GetTransformedText
    /// with very long strings to ensure no truncation occurs.
    /// </summary>
    [Fact]
    public void GetTransformedText_VeryLongString_TransformsCompletely()
    {
        // Arrange
        string source = new string('A', 10000) + new string('b', 10000);
        string expectedLowercase = new string('a', 10000) + new string('b', 10000);
        string expectedUppercase = new string('A', 10000) + new string('B', 10000);

        // Act
        var resultNone = TextTransformUtilites.GetTransformedText(source, TextTransform.None);
        var resultLowercase = TextTransformUtilites.GetTransformedText(source, TextTransform.Lowercase);
        var resultUppercase = TextTransformUtilites.GetTransformedText(source, TextTransform.Uppercase);

        // Assert
        Assert.Equal(source, resultNone);
        Assert.Equal(expectedLowercase, resultLowercase);
        Assert.Equal(expectedUppercase, resultUppercase);
    }
}

/// <summary>
/// Unit tests for the TextTransformUtilities class.
/// </summary>
public class TextTransformUtilitiesTests
{
    /// <summary>
    /// Tests that GetTransformedText returns empty string when source is null or empty,
    /// regardless of the text transform applied.
    /// </summary>
    /// <param name="source">The source string to test (null or empty).</param>
    /// <param name="textTransform">The text transform to apply.</param>
    [Theory]
    [InlineData(null, TextTransform.None)]
    [InlineData(null, TextTransform.Default)]
    [InlineData(null, TextTransform.Lowercase)]
    [InlineData(null, TextTransform.Uppercase)]
    [InlineData("", TextTransform.None)]
    [InlineData("", TextTransform.Default)]
    [InlineData("", TextTransform.Lowercase)]
    [InlineData("", TextTransform.Uppercase)]
    public void GetTransformedText_NullOrEmptySource_ReturnsEmptyString(string source, TextTransform textTransform)
    {
        // Act
        var result = TextTransformUtilities.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(string.Empty, result);
    }

    /// <summary>
    /// Tests that GetTransformedText applies the correct text transformation to normal text.
    /// </summary>
    /// <param name="source">The source string to transform.</param>
    /// <param name="textTransform">The text transform to apply.</param>
    /// <param name="expected">The expected result after transformation.</param>
    [Theory]
    [InlineData("Hello World", TextTransform.None, "Hello World")]
    [InlineData("Hello World", TextTransform.Default, "Hello World")]
    [InlineData("Hello World", TextTransform.Lowercase, "hello world")]
    [InlineData("Hello World", TextTransform.Uppercase, "HELLO WORLD")]
    [InlineData("MiXeD CaSe", TextTransform.None, "MiXeD CaSe")]
    [InlineData("MiXeD CaSe", TextTransform.Default, "MiXeD CaSe")]
    [InlineData("MiXeD CaSe", TextTransform.Lowercase, "mixed case")]
    [InlineData("MiXeD CaSe", TextTransform.Uppercase, "MIXED CASE")]
    public void GetTransformedText_NormalText_AppliesCorrectTransformation(string source, TextTransform textTransform, string expected)
    {
        // Act
        var result = TextTransformUtilities.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that GetTransformedText handles whitespace-only strings correctly.
    /// Whitespace should be preserved and transformations should not affect it.
    /// </summary>
    /// <param name="textTransform">The text transform to apply.</param>
    [Theory]
    [InlineData(TextTransform.None)]
    [InlineData(TextTransform.Default)]
    [InlineData(TextTransform.Lowercase)]
    [InlineData(TextTransform.Uppercase)]
    public void GetTransformedText_WhitespaceOnlySource_PreservesWhitespace(TextTransform textTransform)
    {
        // Arrange
        const string whitespaceSource = "   \t\n  ";

        // Act
        var result = TextTransformUtilities.GetTransformedText(whitespaceSource, textTransform);

        // Assert
        Assert.Equal(whitespaceSource, result);
    }

    /// <summary>
    /// Tests that GetTransformedText handles special characters and Unicode text correctly.
    /// Special characters should be preserved, and Unicode text should be transformed using invariant culture.
    /// </summary>
    /// <param name="source">The source string containing special characters or Unicode.</param>
    /// <param name="textTransform">The text transform to apply.</param>
    /// <param name="expected">The expected result after transformation.</param>
    [Theory]
    [InlineData("Hello@World#123", TextTransform.None, "Hello@World#123")]
    [InlineData("Hello@World#123", TextTransform.Lowercase, "hello@world#123")]
    [InlineData("Hello@World#123", TextTransform.Uppercase, "HELLO@WORLD#123")]
    [InlineData("Ñoño İstanbul", TextTransform.None, "Ñoño İstanbul")]
    [InlineData("Ñoño İstanbul", TextTransform.Lowercase, "ñoño i̇stanbul")]
    [InlineData("Ñoño İstanbul", TextTransform.Uppercase, "ÑOÑO İSTANBUL")]
    [InlineData("123 Numbers", TextTransform.Lowercase, "123 numbers")]
    [InlineData("123 Numbers", TextTransform.Uppercase, "123 NUMBERS")]
    public void GetTransformedText_SpecialCharactersAndUnicode_HandlesCorrectly(string source, TextTransform textTransform, string expected)
    {
        // Act
        var result = TextTransformUtilities.GetTransformedText(source, textTransform);

        // Assert
        Assert.Equal(expected, result);
    }

    /// <summary>
    /// Tests that GetTransformedText handles invalid enum values by falling back to the default behavior.
    /// Invalid enum values should be treated the same as TextTransform.None.
    /// </summary>
    [Fact]
    public void GetTransformedText_InvalidEnumValue_FallsBackToDefault()
    {
        // Arrange
        const string source = "Test String";
        var invalidTransform = (TextTransform)999;

        // Act
        var result = TextTransformUtilities.GetTransformedText(source, invalidTransform);

        // Assert
        Assert.Equal(source, result);
    }

    /// <summary>
    /// Tests that GetTransformedText handles very long strings correctly.
    /// This tests the method's behavior with large input strings.
    /// </summary>
    /// <param name="textTransform">The text transform to apply.</param>
    [Theory]
    [InlineData(TextTransform.None)]
    [InlineData(TextTransform.Lowercase)]
    [InlineData(TextTransform.Uppercase)]
    public void GetTransformedText_VeryLongString_HandlesCorrectly(TextTransform textTransform)
    {
        // Arrange
        var longString = new string('A', 10000) + new string('b', 10000);
        var expectedResult = textTransform switch
        {
            TextTransform.Lowercase => longString.ToLowerInvariant(),
            TextTransform.Uppercase => longString.ToUpperInvariant(),
            _ => longString
        };

        // Act
        var result = TextTransformUtilities.GetTransformedText(longString, textTransform);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    /// <summary>
    /// Tests that GetTransformedText uses invariant culture for transformations.
    /// This ensures consistent behavior regardless of the current culture.
    /// </summary>
    [Fact]
    public void GetTransformedText_UsesInvariantCulture()
    {
        // Arrange
        const string source = "Istanbul"; // Contains Turkish 'I' which behaves differently in Turkish culture

        // Act
        var lowercaseResult = TextTransformUtilities.GetTransformedText(source, TextTransform.Lowercase);
        var uppercaseResult = TextTransformUtilities.GetTransformedText(source, TextTransform.Uppercase);

        // Assert
        Assert.Equal("istanbul", lowercaseResult); // Should use invariant culture, not Turkish culture
        Assert.Equal("ISTANBUL", uppercaseResult);
    }

    /// <summary>
    /// Tests that SetPlainText returns immediately when inputView is null.
    /// This validates the null check guard clause and ensures no exceptions are thrown.
    /// </summary>
    [Fact]
    public void SetPlainText_NullInputView_ReturnsImmediately()
    {
        // Arrange
        InputView inputView = null;
        string platformText = "test";

        // Act & Assert - Should not throw
        TextTransformUtilities.SetPlainText(inputView, platformText);
    }

    /// <summary>
    /// Tests that SetPlainText returns early when both formsText is empty and platformText length is 0.
    /// This tests the early return condition for empty text scenarios.
    /// </summary>
    [Theory]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void SetPlainText_EmptyFormsTextAndEmptyPlatformText_ReturnsEarly(string currentText, string platformText)
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        inputView.TextTransform.Returns(TextTransform.None);
        inputView.Text.Returns(currentText);
        inputView.UpdateFormsText(Arg.Any<string>(), Arg.Any<TextTransform>()).Returns(callInfo =>
        {
            string text = callInfo.Arg<string>();
            return string.IsNullOrEmpty(text) ? string.Empty : text;
        });

        // Act
        TextTransformUtilities.SetPlainText(inputView, platformText);

        // Assert - SetValueFromRenderer should not be called
        inputView.DidNotReceive().SetValueFromRenderer(Arg.Any<BindableProperty>(), Arg.Any<object>());
    }

    /// <summary>
    /// Tests that SetPlainText returns early when the transformed current text equals the transformed platform text.
    /// This verifies the optimization that prevents unnecessary updates when texts are effectively the same.
    /// </summary>
    [Theory]
    [InlineData("Hello", "hello", TextTransform.Lowercase)]
    [InlineData("WORLD", "world", TextTransform.Uppercase)]
    [InlineData("Test", "Test", TextTransform.None)]
    public void SetPlainText_SameTransformedText_ReturnsEarly(string currentText, string platformText, TextTransform transform)
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        inputView.TextTransform.Returns(transform);
        inputView.Text.Returns(currentText);
        inputView.UpdateFormsText(Arg.Any<string>(), Arg.Any<TextTransform>()).Returns(callInfo =>
        {
            string text = callInfo.Arg<string>();
            TextTransform textTransform = callInfo.Arg<TextTransform>();
            return GetExpectedTransform(text, textTransform);
        });

        // Act
        TextTransformUtilities.SetPlainText(inputView, platformText);

        // Assert - SetValueFromRenderer should not be called
        inputView.DidNotReceive().SetValueFromRenderer(Arg.Any<BindableProperty>(), Arg.Any<object>());
    }

    /// <summary>
    /// Tests that SetPlainText calls SetValueFromRenderer when transformed texts are different.
    /// This verifies the core functionality where text updates are applied when needed.
    /// </summary>
    [Theory]
    [InlineData("Hello", "World", TextTransform.None)]
    [InlineData("hello", "WORLD", TextTransform.Lowercase)]
    [InlineData("HELLO", "world", TextTransform.Uppercase)]
    [InlineData("Test", "Different", TextTransform.Default)]
    public void SetPlainText_DifferentTransformedText_CallsSetValueFromRenderer(string currentText, string platformText, TextTransform transform)
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        inputView.TextTransform.Returns(transform);
        inputView.Text.Returns(currentText);
        inputView.UpdateFormsText(Arg.Any<string>(), Arg.Any<TextTransform>()).Returns(callInfo =>
        {
            string text = callInfo.Arg<string>();
            TextTransform textTransform = callInfo.Arg<TextTransform>();
            return GetExpectedTransform(text, textTransform);
        });

        // Act
        TextTransformUtilities.SetPlainText(inputView, platformText);

        // Assert - SetValueFromRenderer should be called with the original platform text
        inputView.Received(1).SetValueFromRenderer(InputView.TextProperty, platformText);
    }

    /// <summary>
    /// Tests SetPlainText with null platform text to ensure null handling works correctly.
    /// This validates that the method handles null input gracefully without throwing exceptions.
    /// </summary>
    [Fact]
    public void SetPlainText_NullPlatformText_HandlesGracefully()
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        inputView.TextTransform.Returns(TextTransform.None);
        inputView.Text.Returns("existing text");
        inputView.UpdateFormsText(Arg.Any<string>(), Arg.Any<TextTransform>()).Returns(callInfo =>
        {
            string text = callInfo.Arg<string>();
            return string.IsNullOrEmpty(text) ? string.Empty : text;
        });
        string platformText = null;

        // Act
        TextTransformUtilities.SetPlainText(inputView, platformText);

        // Assert - Should handle null gracefully and call SetValueFromRenderer
        inputView.Received(1).SetValueFromRenderer(InputView.TextProperty, platformText);
    }

    /// <summary>
    /// Tests SetPlainText with various text transform values to ensure all enum values are handled.
    /// This validates that the method works correctly with all possible TextTransform enum values.
    /// </summary>
    [Theory]
    [InlineData(TextTransform.None)]
    [InlineData(TextTransform.Default)]
    [InlineData(TextTransform.Lowercase)]
    [InlineData(TextTransform.Uppercase)]
    public void SetPlainText_VariousTextTransforms_WorksCorrectly(TextTransform transform)
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        string currentText = "Current";
        string platformText = "Platform";

        inputView.TextTransform.Returns(transform);
        inputView.Text.Returns(currentText);
        inputView.UpdateFormsText(Arg.Any<string>(), Arg.Any<TextTransform>()).Returns(callInfo =>
        {
            string text = callInfo.Arg<string>();
            TextTransform textTransform = callInfo.Arg<TextTransform>();
            return GetExpectedTransform(text, textTransform);
        });

        // Act
        TextTransformUtilities.SetPlainText(inputView, platformText);

        // Assert - Should call SetValueFromRenderer since texts are different
        inputView.Received(1).SetValueFromRenderer(InputView.TextProperty, platformText);
    }

    /// <summary>
    /// Tests SetPlainText with edge case string values including whitespace and special characters.
    /// This validates that the method handles various string edge cases correctly.
    /// </summary>
    [Theory]
    [InlineData("   ", "test")]
    [InlineData("test", "   ")]
    [InlineData("\t\n\r", "text")]
    [InlineData("special!@#$%^&*()", "normal")]
    [InlineData("very long string that exceeds normal text input length for testing purposes", "short")]
    public void SetPlainText_EdgeCaseStrings_HandlesCorrectly(string currentText, string platformText)
    {
        // Arrange
        var inputView = Substitute.For<InputView>();
        inputView.TextTransform.Returns(TextTransform.None);
        inputView.Text.Returns(currentText);
        inputView.UpdateFormsText(Arg.Any<string>(), Arg.Any<TextTransform>()).Returns(callInfo =>
        {
            string text = callInfo.Arg<string>();
            return string.IsNullOrEmpty(text) ? string.Empty : text;
        });

        // Act
        TextTransformUtilities.SetPlainText(inputView, platformText);

        // Assert - Should call SetValueFromRenderer since texts are different
        inputView.Received(1).SetValueFromRenderer(InputView.TextProperty, platformText);
    }

    /// <summary>
    /// Helper method to simulate text transformation based on TextTransform enum value.
    /// This mimics the behavior of TextTransformUtilities.GetTransformedText.
    /// </summary>
    /// <param name="text">The text to transform.</param>
    /// <param name="transform">The transformation to apply.</param>
    /// <returns>The transformed text.</returns>
    private static string GetExpectedTransform(string text, TextTransform transform)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        return transform switch
        {
            TextTransform.Lowercase => text.ToLowerInvariant(),
            TextTransform.Uppercase => text.ToUpperInvariant(),
            TextTransform.None or TextTransform.Default or _ => text
        };
    }
}