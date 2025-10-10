using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for the CreateValuesVisitor class.
/// </summary>
[TestFixture]
public partial class CreateValuesVisitorTests
{
    /// <summary>
    /// Tests that IsResourceDictionary throws NullReferenceException when node parameter is null.
    /// This test verifies proper null parameter handling for the delegation to the extension method.
    /// Expected result: NullReferenceException should be thrown.
    /// </summary>
    [Test]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void IsResourceDictionary_NullNode_ThrowsNullReferenceException()
    {
        // Arrange
        var visitor = new CreateValuesVisitor(null!);

        // Act & Assert
        Assert.Throws<NullReferenceException>(() => visitor.IsResourceDictionary(null!));
    }

}
