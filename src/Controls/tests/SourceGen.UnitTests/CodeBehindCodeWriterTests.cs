using System;
using System.Collections.Generic;
using System.Xml;
using Microsoft.Maui.Controls.SourceGen;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the CodeBehindCodeWriter class.
/// </summary>
[TestFixture]
public partial class CodeBehindCodeWriterTests
{
    /// <summary>
    /// Tests GetWarningDisable method with null XmlDocument parameter.
    /// Should throw ArgumentNullException when xmlDoc is null.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    [Category("ProductionBugSuspected")]
    public void GetWarningDisable_NullXmlDocument_ThrowsArgumentNullException()
    {
        // Arrange
        XmlDocument? xmlDoc = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => CodeBehindCodeWriter.GetWarningDisable(xmlDoc!));
    }

    /// <summary>
    /// Tests GetWarningDisable method with empty XmlDocument.
    /// Should return empty string when no processing instructions exist.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_EmptyXmlDocument_ReturnsEmptyString()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with xaml-comp processing instruction containing warning-disable.
    /// Should extract and return the warning disable value.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_XamlCompWithWarningDisable_ReturnsWarningValue()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable=\"CS1234\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("CS1234", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with xaml-comp processing instruction using space separator.
    /// Should extract and return the warning disable value when separated by space.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_XamlCompWithSpaceSeparator_ReturnsWarningValue()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable \"CS1234\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("CS1234", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with multiple xaml-comp processing instructions.
    /// Should extract and return all warning disable values joined by comma and space.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_MultipleXamlCompInstructions_ReturnsJoinedWarnings()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable=\"CS1234\"?><?xaml-comp warning-disable=\"CS5678\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("CS1234, CS5678", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with xaml-comp processing instruction without warning-disable.
    /// Should return empty string when warning-disable is not present.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_XamlCompWithoutWarningDisable_ReturnsEmptyString()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp some-other-attribute=\"value\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with warning disable values containing quotes.
    /// Should trim both single and double quotes from the warning values.
    /// </summary>
    [TestCase("\"CS1234\"", "CS1234")]
    [TestCase("'CS1234'", "CS1234")]
    [TestCase("\"'CS1234'\"", "'CS1234'")]
    [TestCase("'\"CS1234\"'", "\"CS1234\"")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_WarningValueWithQuotes_TrimsQuotes(string input, string expected)
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml($"<?xml version=\"1.0\"?><?xaml-comp warning-disable={input}?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with processing instruction that would cause IndexOutOfRangeException.
    /// Should handle the case where warning-disable is the last element gracefully.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_WarningDisableAsLastElement_ThrowsIndexOutOfRangeException()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable?><root></root>");

        // Act & Assert
        Assert.Throws<IndexOutOfRangeException>(() => CodeBehindCodeWriter.GetWarningDisable(xmlDoc));
    }

    /// <summary>
    /// Tests GetWarningDisable method with complex processing instruction data.
    /// Should extract warning disable value from complex instruction with multiple attributes.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_ComplexProcessingInstruction_ExtractsCorrectValue()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp compile=\"true\" warning-disable=\"CS1234\" other=\"value\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("CS1234", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with mixed processing instructions.
    /// Should only process xaml-comp instructions and ignore others.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_MixedProcessingInstructions_OnlyProcessesXamlComp()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?other-instruction warning-disable=\"CS9999\"?><?xaml-comp warning-disable=\"CS1234\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("CS1234", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with empty warning disable value.
    /// Should handle empty values correctly.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_EmptyWarningDisableValue_HandlesEmptyValue()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable=\"\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with whitespace-only warning disable value.
    /// Should preserve whitespace in the warning value after trimming quotes.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    [Category("ProductionBugSuspected")]
    public void GetWarningDisable_WhitespaceWarningDisableValue_PreservesWhitespace()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable=\"  \"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("  ", result);
    }

    /// <summary>
    /// Tests GetWarningDisable method with multiple warning-disable entries in single instruction.
    /// Should only return the first warning-disable value found.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetWarningDisable_MultipleWarningDisableInSingleInstruction_ReturnsFirstValue()
    {
        // Arrange
        var xmlDoc = new XmlDocument();
        xmlDoc.LoadXml("<?xml version=\"1.0\"?><?xaml-comp warning-disable=\"CS1234\" warning-disable=\"CS5678\"?><root></root>");

        // Act
        var result = CodeBehindCodeWriter.GetWarningDisable(xmlDoc);

        // Assert
        Assert.AreEqual("CS1234", result);
    }
}
