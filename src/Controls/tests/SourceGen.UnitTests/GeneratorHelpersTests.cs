using System;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the GeneratorHelpers.ComputeXamlProjectItemForIC method.
/// </summary>
[TestFixture]
public partial class GeneratorHelpersTests
{
    /// <summary>
    /// Tests ComputeXamlProjectItemForIC when projectItem is null.
    /// Should return null as the projectItem?.AdditionalText access will be null.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void ComputeXamlProjectItemForIC_WithNullProjectItem_ReturnsNull()
    {
        // Arrange
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = ((ProjectItem?)null, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC when AdditionalText.GetText returns null.
    /// Should return null when the source text cannot be retrieved.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void ComputeXamlProjectItemForIC_WithGetTextReturningNull_ReturnsNull()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>())).Returns((SourceText?)null);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = ((ProjectItem?)projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC when ParseXaml throws an exception.
    /// Should return XamlProjectItemForIC with the exception when XAML parsing fails.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithParseXamlException_ReturnsXamlProjectItemWithException()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup invalid XAML that will cause ParseXaml to throw
        var invalidXaml = "<?xml version=\"1.0\"?><InvalidXml><UnclosedTag></InvalidXml>";
        var sourceText = SourceText.From(invalidXaml);
        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>())).Returns(sourceText);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = ((ProjectItem?)projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Root, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with valid XAML content.
    /// Should return XamlProjectItemForIC with successfully parsed root node.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithValidXaml_ReturnsSuccessfulResult()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup valid XAML content
        var validXaml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<ContentPage xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"></ContentPage>";
        var sourceText = SourceText.From(validXaml);
        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>())).Returns(sourceText);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = ((ProjectItem?)projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with cancellation token being cancelled.
    /// Should respect the cancellation token and potentially throw OperationCanceledException.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void ComputeXamlProjectItemForIC_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        var cancellationToken = cancellationTokenSource.Token;

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
            .Throws<OperationCanceledException>();

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = ((ProjectItem?)projectItem, assemblyCaches);

        // Act & Assert
        Assert.Throws<OperationCanceledException>(() =>
            GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken));
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with edge case XAML content that could cause parsing issues.
    /// </summary>
    [TestCase("", Description = "Empty string")]
    [TestCase("   ", Description = "Whitespace only")]
    [TestCase("Not XML at all", Description = "Non-XML content")]
    [TestCase("<", Description = "Incomplete XML")]
    [TestCase("<?xml version=\"1.0\"?><root><child></root>", Description = "Malformed XML with unclosed tag")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void ComputeXamlProjectItemForIC_WithEdgeCaseXamlContent_HandlesGracefully(string xamlContent)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var mockSourceText = new Mock<SourceText>();

        mockSourceText.Setup(x => x.ToString()).Returns(xamlContent);
        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>())).Returns(mockSourceText.Object);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = ((ProjectItem?)projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ProjectItem, Is.EqualTo(projectItem));
        // For edge cases, we expect either successful parsing (Root not null) or exception handling (Exception not null)
        Assert.That(result.Root != null || result.Exception != null, Is.True);
    }
}
