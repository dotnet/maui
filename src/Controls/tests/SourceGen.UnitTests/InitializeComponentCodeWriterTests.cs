using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;


/// <summary>
/// Unit tests for InitializeComponentCodeWriter class.
/// </summary>
[TestFixture]
public partial class InitializeComponentCodeWriterTests
{
    /// <summary>
    /// Tests GeneratedCodeAttribute property returns correctly formatted attribute string.
    /// Should include assembly full name and version in the generated code attribute.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GeneratedCodeAttribute_ReturnsFormattedAttributeString()
    {
        // Act
        var result = InitializeComponentCodeWriter.GeneratedCodeAttribute;

        // Assert
        Assert.That(result, Does.StartWith("[global::System.CodeDom.Compiler.GeneratedCodeAttribute("));
        Assert.That(result, Does.Contain("Microsoft.Maui.Controls.SourceGen"));
        Assert.That(result, Does.EndWith("\")]"));
    }

}
