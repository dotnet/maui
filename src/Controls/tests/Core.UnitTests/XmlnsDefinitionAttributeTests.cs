#nullable disable

using System;
using System.Diagnostics;

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests;

public sealed class XmlnsDefinitionAttributeTests
{
    /// <summary>
    /// Tests that the constructor correctly initializes properties with valid non-http target parameters.
    /// Input conditions: Valid non-null strings where target doesn't start with "http".
    /// Expected result: Properties are set correctly without exceptions.
    /// </summary>
    [Theory]
    [InlineData("http://example.com", "MyNamespace")]
    [InlineData("http://test.com", "")]
    [InlineData("http://test.com", " ")]
    [InlineData("http://test.com", "SomeVeryLongNamespaceValueThatShouldStillWork")]
    public void Constructor_ValidNonHttpTarget_SetsPropertiesCorrectly(string xmlNamespace, string target)
    {
        // Act
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Assert
        Assert.Equal(xmlNamespace, attribute.XmlNamespace);
        Assert.Equal(target, attribute.Target);
    }

    /// <summary>
    /// Tests that the constructor correctly initializes properties with valid http target and correct namespace.
    /// Input conditions: Target starts with "http" and xmlNamespace is the allowed aggregation namespace.
    /// Expected result: Properties are set correctly without exceptions.
    /// </summary>
    [Theory]
    [InlineData("http://schemas.microsoft.com/dotnet/maui/global", "http://example.com")]
    [InlineData("http://schemas.microsoft.com/dotnet/maui/global", "https://test.com")]
    [InlineData("http://schemas.microsoft.com/dotnet/maui/global", "http://")]
    public void Constructor_HttpTargetWithCorrectNamespace_SetsPropertiesCorrectly(string xmlNamespace, string target)
    {
        // Act
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Assert
        Assert.Equal(xmlNamespace, attribute.XmlNamespace);
        Assert.Equal(target, attribute.Target);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when target parameter is null.
    /// Input conditions: target is null, xmlNamespace is valid.
    /// Expected result: ArgumentNullException with parameter name "target".
    /// </summary>
    [Fact]
    public void Constructor_NullTarget_ThrowsArgumentNullException()
    {
        // Arrange
        string xmlNamespace = "http://example.com";
        string target = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new XmlnsDefinitionAttribute(xmlNamespace, target));
        Assert.Equal("target", exception.ParamName);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when xmlNamespace parameter is null.
    /// Input conditions: xmlNamespace is null, target is valid.
    /// Expected result: ArgumentNullException with parameter name "xmlNamespace".
    /// </summary>
    [Fact]
    public void Constructor_NullXmlNamespace_ThrowsArgumentNullException()
    {
        // Arrange
        string xmlNamespace = null;
        string target = "MyNamespace";

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new XmlnsDefinitionAttribute(xmlNamespace, target));
        Assert.Equal("xmlNamespace", exception.ParamName);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentNullException when both parameters are null.
    /// Input conditions: Both xmlNamespace and target are null.
    /// Expected result: ArgumentNullException (target is checked first).
    /// </summary>
    [Fact]
    public void Constructor_BothParametersNull_ThrowsArgumentNullException()
    {
        // Arrange
        string xmlNamespace = null;
        string target = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new XmlnsDefinitionAttribute(xmlNamespace, target));
        Assert.Equal("target", exception.ParamName);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentException when target is a forbidden XAML namespace.
    /// Input conditions: target is one of the forbidden XAML namespace values.
    /// Expected result: ArgumentException with parameter name "target" and specific message.
    /// </summary>
    [Theory]
    [InlineData("http://schemas.microsoft.com/winfx/2009/xaml")]
    [InlineData("http://schemas.microsoft.com/winfx/2006/xaml")]
    public void Constructor_ForbiddenTargetNamespace_ThrowsArgumentException(string forbiddenTarget)
    {
        // Arrange
        string xmlNamespace = "http://example.com";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new XmlnsDefinitionAttribute(xmlNamespace, forbiddenTarget));
        Assert.Equal("target", exception.ParamName);
        Assert.Contains($"Target cannot be {forbiddenTarget}. That namespace can't be aggregated", exception.Message);
    }

    /// <summary>
    /// Tests that the constructor throws ArgumentException when target starts with "http" but xmlNamespace is not the allowed aggregation namespace.
    /// Input conditions: target starts with "http" but xmlNamespace is not "http://schemas.microsoft.com/dotnet/maui/global".
    /// Expected result: ArgumentException with parameter name "target" and aggregation message.
    /// </summary>
    [Theory]
    [InlineData("http://example.com", "http://target.com")]
    [InlineData("", "http://target.com")]
    [InlineData("http://schemas.microsoft.com/dotnet/maui/GLOBAL", "http://target.com")]
    [InlineData("http://different.namespace", "https://target.com")]
    [InlineData("http://test.com", "HTTP://target.com")]
    public void Constructor_HttpTargetWithIncorrectNamespace_ThrowsArgumentException(string xmlNamespace, string target)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new XmlnsDefinitionAttribute(xmlNamespace, target));
        Assert.Equal("target", exception.ParamName);
        Assert.Contains("We only support xmlns aggregation in http://schemas.microsoft.com/dotnet/maui/global", exception.Message);
    }

    /// <summary>
    /// Tests that the constructor works with empty string parameters (as they are not null).
    /// Input conditions: Empty strings for both parameters.
    /// Expected result: Properties are set to empty strings without exceptions.
    /// </summary>
    [Fact]
    public void Constructor_EmptyStrings_SetsPropertiesCorrectly()
    {
        // Arrange
        string xmlNamespace = "";
        string target = "";

        // Act
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Assert
        Assert.Equal("", attribute.XmlNamespace);
        Assert.Equal("", attribute.Target);
    }

    /// <summary>
    /// Tests that the constructor is case-sensitive for "http" prefix check.
    /// Input conditions: target with uppercase "HTTP" prefix.
    /// Expected result: No exception thrown as "HTTP" != "http".
    /// </summary>
    [Fact]
    public void Constructor_CaseSensitiveHttpCheck_DoesNotThrow()
    {
        // Arrange
        string xmlNamespace = "http://example.com";
        string target = "HTTP://target.com";

        // Act
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Assert
        Assert.Equal(xmlNamespace, attribute.XmlNamespace);
        Assert.Equal(target, attribute.Target);
    }

    /// <summary>
    /// Tests that the constructor works with whitespace-only strings.
    /// Input conditions: Whitespace-only strings for parameters.
    /// Expected result: Properties are set without exceptions.
    /// </summary>
    [Theory]
    [InlineData("   ", "MyNamespace")]
    [InlineData("http://example.com", "   ")]
    [InlineData("\t", "\n")]
    public void Constructor_WhitespaceStrings_SetsPropertiesCorrectly(string xmlNamespace, string target)
    {
        // Act
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Assert
        Assert.Equal(xmlNamespace, attribute.XmlNamespace);
        Assert.Equal(target, attribute.Target);
    }

    /// <summary>
    /// Tests that the constructor works with strings containing special characters.
    /// Input conditions: Strings with various special characters.
    /// Expected result: Properties are set correctly without exceptions.
    /// </summary>
    [Theory]
    [InlineData("http://example.com/ñáéíóú", "namespace-with-special!@#$%^&*()")]
    [InlineData("http://测试.com", "命名空间")]
    [InlineData("http://example.com", "namespace\0with\nnull\tand\rcontrol")]
    public void Constructor_SpecialCharacters_SetsPropertiesCorrectly(string xmlNamespace, string target)
    {
        // Act
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Assert
        Assert.Equal(xmlNamespace, attribute.XmlNamespace);
        Assert.Equal(target, attribute.Target);
    }

    /// <summary>
    /// Tests that ClrNamespace property returns the same value as Target property for various valid string inputs.
    /// This verifies the obsolete ClrNamespace property correctly delegates to the Target property.
    /// </summary>
    /// <param name="xmlNamespace">The XML namespace to use in the constructor</param>
    /// <param name="target">The target namespace that should be returned by both Target and ClrNamespace properties</param>
    [Theory]
    [InlineData("http://schemas.microsoft.com/dotnet/2021/maui", "Microsoft.Maui.Controls")]
    [InlineData("http://example.com/test", "MyNamespace.Controls")]
    [InlineData("custom-namespace", "")]
    [InlineData("another-namespace", "A")]
    [InlineData("test-ns", "Very.Long.Namespace.With.Many.Dots.And.Numbers123.AndSpecialChars")]
    [InlineData("simple", "Simple")]
    [InlineData("whitespace-test", "Namespace With Spaces")]
    [InlineData("special-chars", "Namespace!@#$%^&*()")]
    [InlineData("unicode-test", "Namespace.测试.Тест")]
    public void ClrNamespace_ReturnsTargetValue_ForValidInputs(string xmlNamespace, string target)
    {
        // Arrange
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Act
        string clrNamespace = attribute.ClrNamespace;
        string targetValue = attribute.Target;

        // Assert
        Assert.Equal(targetValue, clrNamespace);
        Assert.Equal(target, clrNamespace);
    }

    /// <summary>
    /// Tests that ClrNamespace property returns the same value as Target property when using the special
    /// aggregation namespace. This ensures the obsolete property works correctly even with the special
    /// http aggregation scenario.
    /// </summary>
    /// <param name="target">The target namespace that should be returned by both Target and ClrNamespace properties</param>
    [Theory]
    [InlineData("http://schemas.microsoft.com/winfx/2009/xaml/presentation")]
    [InlineData("http://example.com/custom")]
    [InlineData("http://test.namespace")]
    [InlineData("https://secure.namespace")]
    public void ClrNamespace_ReturnsTargetValue_WithHttpAggregationNamespace(string target)
    {
        // Arrange
        const string aggregationNamespace = "http://schemas.microsoft.com/dotnet/maui/global";
        var attribute = new XmlnsDefinitionAttribute(aggregationNamespace, target);

        // Act
        string clrNamespace = attribute.ClrNamespace;
        string targetValue = attribute.Target;

        // Assert
        Assert.Equal(targetValue, clrNamespace);
        Assert.Equal(target, clrNamespace);
    }

    /// <summary>
    /// Tests that ClrNamespace property returns the same reference as Target property, ensuring
    /// they point to the exact same string object for efficiency.
    /// </summary>
    [Fact]
    public void ClrNamespace_ReturnsSameReference_AsTarget()
    {
        // Arrange
        const string xmlNamespace = "http://test.namespace";
        const string target = "TestNamespace.Controls";
        var attribute = new XmlnsDefinitionAttribute(xmlNamespace, target);

        // Act
        string clrNamespace = attribute.ClrNamespace;
        string targetValue = attribute.Target;

        // Assert
        Assert.Same(targetValue, clrNamespace);
    }
}