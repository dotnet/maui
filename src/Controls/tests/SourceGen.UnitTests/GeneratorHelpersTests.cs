#nullable disable
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Xaml.Internals;
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
    /// Tests ComputeProjectItem when cancellation token is cancelled.
    /// Should return null when cancellation is requested.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeProjectItem_WithCancelledToken_ReturnsNull()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptionsProvider = new Mock<AnalyzerConfigOptionsProvider>();
        var tuple = (mockAdditionalText.Object, mockOptionsProvider.Object);
        var cancellationToken = new CancellationToken(true);

        // Act
        var result = GeneratorHelpers.ComputeProjectItem(tuple, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests ComputeProjectItem when project item Kind is "None".
    /// Should return null when the ProjectItem.Kind equals "None".
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeProjectItem_WithKindNone_ReturnsNull()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptionsProvider = new Mock<AnalyzerConfigOptionsProvider>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup options to return false for TryGetValue (which will make GetValueOrDefault return "None")
        mockOptions.Setup(x => x.TryGetValue("build_metadata.additionalfiles.GenKind", out It.Ref<string>.IsAny))
                  .Returns(false);

        mockOptionsProvider.Setup(x => x.GetOptions(mockAdditionalText.Object))
                          .Returns(mockOptions.Object);

        var tuple = (mockAdditionalText.Object, mockOptionsProvider.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeProjectItem(tuple, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests ComputeProjectItem when project item Kind is not "None".
    /// Should return the ProjectItem when Kind is any value other than "None".
    /// </summary>
    [TestCase("EmbeddedResource")]
    [TestCase("Compile")]
    [TestCase("Content")]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("CustomKind")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeProjectItem_WithKindNotNone_ReturnsProjectItem(string kindValue)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptionsProvider = new Mock<AnalyzerConfigOptionsProvider>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(x => x.GetValueOrDefault("build_metadata.additionalfiles.GenKind", "None"))
                  .Returns(kindValue);

        mockOptionsProvider.Setup(x => x.GetOptions(mockAdditionalText.Object))
                          .Returns(mockOptions.Object);

        var tuple = (mockAdditionalText.Object, mockOptionsProvider.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeProjectItem(tuple, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AdditionalText, Is.EqualTo(mockAdditionalText.Object));
        Assert.That(result.Options, Is.EqualTo(mockOptions.Object));
        Assert.That(result.Kind, Is.EqualTo(kindValue));
    }

    /// <summary>
    /// Tests ComputeProjectItem with non-cancelled token and default Kind behavior.
    /// Should return ProjectItem when cancellation is not requested and proper options are provided.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeProjectItem_WithNonCancelledTokenAndValidOptions_ReturnsProjectItem()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptionsProvider = new Mock<AnalyzerConfigOptionsProvider>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        mockOptions.Setup(x => x.TryGetValue("build_metadata.additionalfiles.GenKind", out It.Ref<string>.IsAny))
                  .Returns((string key, out string value) =>
                  {
                      value = "EmbeddedResource";
                      return true;
                  });

        mockOptionsProvider.Setup(x => x.GetOptions(mockAdditionalText.Object))
                          .Returns(mockOptions.Object);

        var tuple = (mockAdditionalText.Object, mockOptionsProvider.Object);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeProjectItem(tuple, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.AdditionalText, Is.EqualTo(mockAdditionalText.Object));
        Assert.That(result.Options, Is.EqualTo(mockOptions.Object));
        Assert.That(result.Kind, Is.EqualTo("EmbeddedResource"));
    }

    /// <summary>
    /// Tests GetAssemblyAttributes with null compilation parameter.
    /// Should throw appropriate exception when compilation is null.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetAssemblyAttributes_WithNullCompilation_ThrowsException()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            GeneratorHelpers.GetAssemblyAttributes(null!, cancellationToken));
    }

}




/// <summary>
/// Unit tests for the GeneratorHelpers.LoadXmlDocument method.
/// </summary>
[TestFixture]
public partial class GeneratorHelpersLoadXmlDocumentTests
{
    /// <summary>
    /// Tests LoadXmlDocument with empty XML string.
    /// Should throw XmlException when trying to parse empty XML content.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithEmptyText_ThrowsXmlException()
    {
        // Arrange
        var text = SourceText.From("");
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.Throws<XmlException>(() =>
            GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken));
    }

    /// <summary>
    /// Tests LoadXmlDocument with whitespace-only XML string.
    /// Should throw XmlException when trying to parse whitespace-only content.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithWhitespaceOnlyText_ThrowsXmlException()
    {
        // Arrange
        var text = SourceText.From("   \t\n\r   ");
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.Throws<XmlException>(() =>
            GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken));
    }

    /// <summary>
    /// Tests LoadXmlDocument with malformed XML.
    /// Should throw XmlException when XML is not well-formed.
    /// </summary>
    [TestCase("<root><unclosed>", Description = "Unclosed tag")]
    [TestCase("<root attr=\"unclosed>content</root>", Description = "Unclosed attribute")]
    [TestCase("<root><child></root>", Description = "Mismatched tags")]
    [TestCase("not xml at all", Description = "Non-XML content")]
    [TestCase("<", Description = "Incomplete XML")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithMalformedXml_ThrowsXmlException(string xmlContent)
    {
        // Arrange
        var text = SourceText.From(xmlContent);
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        Assert.Throws<XmlException>(() =>
            GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken));
    }

    /// <summary>
    /// Tests LoadXmlDocument with valid XML content and AllowImplicitXmlns false.
    /// Should successfully parse valid XML and return root node with namespace manager.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithValidXmlAndImplicitXmlnsFalse_ReturnsRootNodeAndNamespaceManager()
    {
        // Arrange
        var xmlContent = "<?xml version=\"1.0\"?><root xmlns=\"http://test.com\"><child>content</child></root>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = new AssemblyCaches([], [], [], [], false);
        var cancellationToken = CancellationToken.None;

        // Act
        var (root, nsmgr) = GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken);

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(nsmgr, Is.Not.Null);
        Assert.That(root!.LocalName, Is.EqualTo("root"));
        Assert.That(root.NamespaceURI, Is.EqualTo("http://test.com"));
    }

    /// <summary>
    /// Tests LoadXmlDocument with valid XML content and AllowImplicitXmlns true.
    /// Should successfully parse valid XML with fragment conformance level.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithValidXmlAndImplicitXmlnsTrue_ReturnsRootNodeAndNamespaceManager()
    {
        // Arrange
        var xmlContent = "<root xmlns=\"http://test.com\"><child>content</child></root>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = new AssemblyCaches([], [], [], [], true);
        var cancellationToken = CancellationToken.None;

        // Act
        var (root, nsmgr) = GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken);

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(nsmgr, Is.Not.Null);
        Assert.That(root!.LocalName, Is.EqualTo("root"));
        Assert.That(root.NamespaceURI, Is.EqualTo("http://test.com"));
    }

    /// <summary>
    /// Tests LoadXmlDocument with XML containing obsolete Xamarin Forms namespace URI.
    /// Should throw Exception with specific message about using MAUI URI instead.
    /// This test targets the "Not Covered" line 78 in the LoadXmlDocument method.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithObsoleteFormsUri_ThrowsExceptionWithMauiUriMessage()
    {
        // Arrange
        var xmlContent = "<?xml version=\"1.0\"?><ContentPage xmlns=\"http://xamarin.com/schemas/2014/forms\"><Label Text=\"Test\" /></ContentPage>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
            GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken));

        Assert.That(exception!.Message, Does.Contain("http://xamarin.com/schemas/2014/forms"));
        Assert.That(exception.Message, Does.Contain("not a valid namespace"));
        Assert.That(exception.Message, Does.Contain("http://schemas.microsoft.com/dotnet/2021/maui"));
    }

    /// <summary>
    /// Tests LoadXmlDocument with cancelled cancellation token.
    /// Should throw OperationCanceledException when cancellation is requested.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithCancelledToken_ThrowsOperationCanceledException()
    {
        // Arrange
        var xmlContent = "<?xml version=\"1.0\"?><root><child>content</child></root>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        Assert.Throws<OperationCanceledException>(() =>
            GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationTokenSource.Token));
    }

    /// <summary>
    /// Tests LoadXmlDocument with complex XML structure.
    /// Should successfully parse XML with multiple levels and attributes.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithComplexXmlStructure_ReturnsCorrectRootNode()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
             xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
             x:Class=""MyApp.MainPage"">
    <StackLayout>
        <Label Text=""Hello World!"" FontSize=""24"" />
        <Button Text=""Click Me"" Clicked=""OnButtonClicked"" />
    </StackLayout>
</ContentPage>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act
        var (root, nsmgr) = GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken);

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(nsmgr, Is.Not.Null);
        Assert.That(root!.LocalName, Is.EqualTo("ContentPage"));
        Assert.That(root.NamespaceURI, Is.EqualTo("http://schemas.microsoft.com/dotnet/2021/maui"));
    }

    /// <summary>
    /// Tests LoadXmlDocument with XML containing special characters and CDATA.
    /// Should successfully parse XML with special characters, escape sequences, and CDATA sections.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithSpecialCharactersAndCdata_ReturnsCorrectRootNode()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<root>
    <description>&lt;![CDATA[This contains &amp; special &lt; characters &gt;]]&gt;</description>
    <text>Content with &quot;quotes&quot; and &apos;apostrophes&apos;</text>
    <unicode>Unicode: αβγ 中文 🚀</unicode>
</root>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act
        var (root, nsmgr) = GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken);

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(nsmgr, Is.Not.Null);
        Assert.That(root!.LocalName, Is.EqualTo("root"));
        Assert.That(root.HasChildNodes, Is.True);
    }

    /// <summary>
    /// Tests LoadXmlDocument edge case with very large XML content.
    /// Should handle large XML documents without issues.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithLargeXmlContent_ReturnsCorrectRootNode()
    {
        // Arrange
        var xmlBuilder = new System.Text.StringBuilder();
        xmlBuilder.AppendLine("<?xml version=\"1.0\"?>");
        xmlBuilder.AppendLine("<root>");

        // Generate large content
        for (int i = 0; i < 1000; i++)
        {
            xmlBuilder.AppendLine($"    <item id=\"{i}\">Content {i}</item>");
        }

        xmlBuilder.AppendLine("</root>");

        var text = SourceText.From(xmlBuilder.ToString());
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act
        var (root, nsmgr) = GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken);

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(nsmgr, Is.Not.Null);
        Assert.That(root!.LocalName, Is.EqualTo("root"));
        Assert.That(root.HasChildNodes, Is.True);
        Assert.That(root.ChildNodes.Count, Is.EqualTo(1000));
    }

    /// <summary>
    /// Tests LoadXmlDocument with XML containing multiple namespace declarations.
    /// Should handle complex namespace scenarios correctly.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void LoadXmlDocument_WithMultipleNamespaces_ReturnsCorrectRootNodeAndNamespaceManager()
    {
        // Arrange
        var xmlContent = @"<?xml version=""1.0""?>
<root xmlns=""http://default.namespace.com""
      xmlns:ns1=""http://namespace1.com""
      xmlns:ns2=""http://namespace2.com""
      xmlns:xml=""http://www.w3.org/XML/1998/namespace"">
    <ns1:element1 />
    <ns2:element2 />
    <element3 />
</root>";
        var text = SourceText.From(xmlContent);
        var assemblyCaches = AssemblyCaches.Empty;
        var cancellationToken = CancellationToken.None;

        // Act
        var (root, nsmgr) = GeneratorHelpers.LoadXmlDocument(text, assemblyCaches, cancellationToken);

        // Assert
        Assert.That(root, Is.Not.Null);
        Assert.That(nsmgr, Is.Not.Null);
        Assert.That(root!.LocalName, Is.EqualTo("root"));
        Assert.That(root.NamespaceURI, Is.EqualTo("http://default.namespace.com"));

        // Check namespace manager contains the __f__ and __g__ prefixes for MAUI URIs
        Assert.That(nsmgr.LookupNamespace("__f__"), Is.EqualTo("http://schemas.microsoft.com/dotnet/2021/maui"));
        Assert.That(nsmgr.LookupNamespace("__g__"), Is.EqualTo("http://schemas.microsoft.com/dotnet/maui/global"));
    }
}



/// <summary>
/// Unit tests for the GeneratorHelpers.ComputeXamlProjectItemForIC method.
/// </summary>
[TestFixture]
public partial class GeneratorHelpersComputeXamlProjectItemForICTests
{
    /// <summary>
    /// Tests ComputeXamlProjectItemForIC when AdditionalText.GetText returns null.
    /// Should return null when SourceText is null, exercising the "Not Covered" line 47.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithNullSourceText_ReturnsNull()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();

        // Setup GetText to return null
        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Returns((SourceText)null);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC when ParseXaml throws an exception.
    /// Should return XamlProjectItemForIC with exception when malformed XAML is provided,
    /// exercising the "Not Covered" lines 53-55.
    /// </summary>
    [TestCase("<root><unclosed>", Description = "Unclosed XML tag")]
    [TestCase("not xml at all", Description = "Non-XML content")]
    [TestCase("<", Description = "Incomplete XML")]
    [TestCase("<root attr=\"unclosed>content</root>", Description = "Unclosed attribute")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithMalformedXaml_ReturnsXamlProjectItemWithException(string malformedXaml)
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var mockSourceText = new Mock<SourceText>();

        mockSourceText.Setup(x => x.ToString()).Returns(malformedXaml);
        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Returns(mockSourceText.Object);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Root, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with valid XAML content.
    /// Should return XamlProjectItemForIC with root node when well-formed XAML is provided.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithValidXaml_ReturnsXamlProjectItemWithRoot()
    {
        // Arrange
        var validXaml = "<ContentPage xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"><Label Text=\"Hello\" /></ContentPage>";
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var sourceText = SourceText.From(validXaml);

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Returns(sourceText);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Null);
        Assert.That(result.Root, Is.Not.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with cancelled cancellation token.
    /// Should handle cancellation appropriately when token is cancelled.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithCancelledToken_HandlesCancellation()
    {
        // Arrange
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var cancellationToken = new CancellationToken(true);

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Throws<OperationCanceledException>();

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);

        // Act & Assert
        Assert.That(() => GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken),
                   Throws.TypeOf<OperationCanceledException>());
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with empty XAML string.
    /// Should handle empty XAML content by throwing exception and returning XamlProjectItemForIC with exception.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithEmptyXaml_ReturnsXamlProjectItemWithException()
    {
        // Arrange
        var emptyXaml = "";
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var sourceText = SourceText.From(emptyXaml);

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Returns(sourceText);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Root, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with whitespace-only XAML string.
    /// Should handle whitespace-only XAML content by throwing exception and returning XamlProjectItemForIC with exception.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithWhitespaceOnlyXaml_ReturnsXamlProjectItemWithException()
    {
        // Arrange
        var whitespaceXaml = "   \t\n\r   ";
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var sourceText = SourceText.From(whitespaceXaml);

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Returns(sourceText);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Not.Null);
        Assert.That(result.Root, Is.Null);
    }

    /// <summary>
    /// Tests ComputeXamlProjectItemForIC with XAML containing special characters.
    /// Should handle XAML with special characters and escape sequences correctly.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void ComputeXamlProjectItemForIC_WithSpecialCharactersXaml_ReturnsXamlProjectItemWithRoot()
    {
        // Arrange
        var specialCharXaml = "<ContentPage xmlns=\"http://schemas.microsoft.com/dotnet/2021/maui\"><Label Text=\"Hello &amp; Welcome &lt;&gt; 'quotes' &quot;double&quot;\" /></ContentPage>";
        var mockAdditionalText = new Mock<AdditionalText>();
        var mockOptions = new Mock<AnalyzerConfigOptions>();
        var sourceText = SourceText.From(specialCharXaml);

        mockAdditionalText.Setup(x => x.GetText(It.IsAny<CancellationToken>()))
                         .Returns(sourceText);

        var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);
        var assemblyCaches = AssemblyCaches.Empty;
        var itemAndCaches = (projectItem, assemblyCaches);
        var cancellationToken = CancellationToken.None;

        // Act
        var result = GeneratorHelpers.ComputeXamlProjectItemForIC(itemAndCaches, cancellationToken);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ProjectItem, Is.EqualTo(projectItem));
        Assert.That(result.Exception, Is.Null);
        Assert.That(result.Root, Is.Not.Null);
    }
}



/// <summary>
/// Unit tests for the GeneratorHelpers.GetAssemblyAttributes method.
/// Tests focus on edge cases, error conditions, and the specific uncovered code paths.
/// </summary>
[TestFixture]
public partial class GeneratorHelpersGetAssemblyAttributesTests
{
}



/// <summary>
/// Unit tests for the GeneratorHelpers.ComputeXamlProjectItemForCB method.
/// </summary>
[TestFixture]
public partial class GeneratorHelpersComputeXamlProjectItemForCBTests
{
}
