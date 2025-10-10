using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for XamlProjectItemForIC constructor that takes ProjectItem and Exception parameters.
/// </summary>
public partial class XamlProjectItemForICTests
{
    /// <summary>
    /// Tests XamlProjectItemForIC constructor with valid ProjectItem and Exception.
    /// Should create instance with properties correctly assigned.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Constructor_ValidProjectItemAndException_SetsPropertiesCorrectly()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var exception = new InvalidOperationException("Test exception");

        // Act
        var result = new XamlProjectItemForIC(projectItem, exception);

        // Assert
        Assert.AreEqual(projectItem, result.ProjectItem);
        Assert.AreEqual(exception, result.Exception);
        Assert.IsNull(result.Root);
    }

    /// <summary>
    /// Tests XamlProjectItemForIC constructor with null ProjectItem parameter.
    /// Should allow null assignment despite non-nullable annotation for testing runtime behavior.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Constructor_NullProjectItem_AssignsNullToProperty()
    {
        // Arrange
        ProjectItem? projectItem = null;
        var exception = new Exception("Test exception");

        // Act
        var result = new XamlProjectItemForIC(projectItem!, exception);

        // Assert
        Assert.IsNull(result.ProjectItem);
        Assert.AreEqual(exception, result.Exception);
    }

    /// <summary>
    /// Tests XamlProjectItemForIC constructor with null Exception parameter.
    /// Should allow null assignment despite non-nullable annotation for testing runtime behavior.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Constructor_NullException_AssignsNullToProperty()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        Exception? exception = null;

        // Act
        var result = new XamlProjectItemForIC(projectItem, exception!);

        // Assert
        Assert.AreEqual(projectItem, result.ProjectItem);
        Assert.IsNull(result.Exception);
    }

    /// <summary>
    /// Tests XamlProjectItemForIC constructor with both parameters null.
    /// Should allow null assignment for both properties for testing runtime behavior.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Constructor_BothParametersNull_AssignsNullToBothProperties()
    {
        // Arrange
        ProjectItem? projectItem = null;
        Exception? exception = null;

        // Act
        var result = new XamlProjectItemForIC(projectItem!, exception!);

        // Assert
        Assert.IsNull(result.ProjectItem);
        Assert.IsNull(result.Exception);
        Assert.IsNull(result.Root);
    }

    /// <summary>
    /// Tests XamlProjectItemForIC constructor with exception containing complex message.
    /// Should preserve exception details and message correctly.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Constructor_ExceptionWithComplexMessage_PreservesExceptionDetails()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        const string complexMessage = "Complex error message with special characters: !@#$%^&*()_+ and unicode: \u00E9\u00F1";
        var exception = new ArgumentException(complexMessage, "parameterName");
        const string expectedFullMessage = "Complex error message with special characters: !@#$%^&*()_+ and unicode: \u00E9\u00F1 (Parameter 'parameterName')";

        // Act
        var result = new XamlProjectItemForIC(projectItem, exception);

        // Assert
        Assert.AreEqual(projectItem, result.ProjectItem);
        Assert.AreEqual(exception, result.Exception);
        Assert.AreEqual(expectedFullMessage, result.Exception?.Message);
        Assert.IsInstanceOf<ArgumentException>(result.Exception);
    }

    /// <summary>
    /// Tests XamlProjectItemForIC constructor with nested exception.
    /// Should preserve inner exception information correctly.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void Constructor_ExceptionWithInnerException_PreservesInnerException()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var innerException = new ArgumentNullException("innerParam", "Inner exception message");
        var outerException = new InvalidOperationException("Outer exception message", innerException);

        // Act
        var result = new XamlProjectItemForIC(projectItem, outerException);

        // Assert
        Assert.AreEqual(projectItem, result.ProjectItem);
        Assert.AreEqual(outerException, result.Exception);
        Assert.AreEqual(innerException, result.Exception?.InnerException);
    }
}
