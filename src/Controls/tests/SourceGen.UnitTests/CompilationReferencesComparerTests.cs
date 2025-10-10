using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the CompilationReferencesComparer class.
/// </summary>
[TestFixture]
public class CompilationReferencesComparerTests
{
    private CompilationReferencesComparer _comparer = null!;

    [SetUp]
    public void SetUp()
    {
        _comparer = new CompilationReferencesComparer();
    }

    /// <summary>
    /// Tests that GetHashCode throws ArgumentNullException when the compilation parameter is null.
    /// Verifies proper null parameter validation.
    /// Expected result: ArgumentNullException is thrown.
    /// </summary>
    [Test]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    [Category("ProductionBugSuspected")]
    public void GetHashCode_NullCompilation_ThrowsArgumentNullException()
    {
        // Arrange
        Compilation nullCompilation = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _comparer.GetHashCode(nullCompilation));
    }

}
