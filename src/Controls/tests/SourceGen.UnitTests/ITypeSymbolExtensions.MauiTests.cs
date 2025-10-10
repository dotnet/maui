#nullable disable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Xml;


using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;



/// <summary>
/// Unit tests for the GetBPTypeAndConverter method in ITypeSymbolExtensions.
/// </summary>
[TestFixture]
public partial class ITypeSymbolExtensionsTests
{
    /// <summary>
    /// Tests that GetBPTypeAndConverter returns null when field name does not end with "Property".
    /// This test targets the early return condition on line 34.
    /// </summary>
    [TestCase("TestField")]
    [TestCase("SomeProperty2")]
    [TestCase("PropertyTest")]
    [TestCase("Proper")]
    [TestCase("")]
    [TestCase("P")]
    [TestCase("Propert")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    [Category("auto-generated")]
    public void GetBPTypeAndConverter_FieldNameDoesNotEndWithProperty_ReturnsNull(string fieldName)
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();

        mockFieldSymbol.Setup(f => f.Name).Returns(fieldName);

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, mockContext.Object);

        // Assert
        Assert.IsNull(result);
    }


    /// <summary>
    /// Tests that GetBPTypeAndConverter handles field name edge cases correctly.
    /// This test verifies behavior with field names that are exactly "Property" or very short.
    /// </summary>
    [TestCase("Property")]
    [TestCase("AProperty")]
    [TestCase("XProperty")]
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetBPTypeAndConverter_EdgeCaseFieldNames_ProcessesCorrectly(string fieldName)
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockContainingType = new Mock<INamedTypeSymbol>();
        var mockProperties = new List<IPropertySymbol>();
        var mockMethods = new List<IMethodSymbol>();

        string expectedPropertyName = fieldName.Substring(0, fieldName.Length - 8);
        string expectedMethodName = $"Get{expectedPropertyName}";

        mockFieldSymbol.Setup(f => f.Name).Returns(fieldName);
        mockFieldSymbol.Setup(f => f.ContainingType).Returns(mockContainingType.Object);
        mockContainingType.Setup(t => t.GetAllProperties(expectedPropertyName, mockContext.Object)).Returns(mockProperties);
        mockContainingType.Setup(t => t.GetAllMethods(expectedMethodName, mockContext.Object)).Returns(mockMethods);

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, mockContext.Object);

        // Assert
        Assert.IsNull(result); // No getter found, should return null
    }

    /// <summary>
    /// Tests that GetBPTypeAndConverter handles case where static getter method doesn't meet criteria.
    /// This test verifies that non-static, non-public, or wrong parameter count methods are ignored.
    /// </summary>
    [TestCase(false, true, 1)] // Non-static
    [TestCase(true, false, 1)] // Non-public
    [TestCase(true, true, 0)]  // Wrong parameter count (0)
    [TestCase(true, true, 2)]  // Wrong parameter count (2)
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetBPTypeAndConverter_StaticGetterDoesNotMeetCriteria_ReturnsNull(bool isStatic, bool isPublic, int parameterCount)
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockContainingType = new Mock<INamedTypeSymbol>();
        var mockGetter = new Mock<IMethodSymbol>();
        var mockProperties = new List<IPropertySymbol>();
        var mockMethods = new List<IMethodSymbol> { mockGetter.Object };
        var parameters = new List<IParameterSymbol>();

        for (int i = 0; i < parameterCount; i++)
        {
            parameters.Add(new Mock<IParameterSymbol>().Object);
        }

        mockFieldSymbol.Setup(f => f.Name).Returns("TestProperty");
        mockFieldSymbol.Setup(f => f.ContainingType).Returns(mockContainingType.Object);
        mockContainingType.Setup(t => t.GetAllProperties("Test", mockContext.Object)).Returns(mockProperties);
        mockContainingType.Setup(t => t.GetAllMethods("GetTest", mockContext.Object)).Returns(mockMethods);

        mockGetter.Setup(g => g.IsStatic).Returns(isStatic);
        mockGetter.Setup(g => g.IsPublic()).Returns(isPublic);
        mockGetter.Setup(g => g.Parameters).Returns(ImmutableArray.CreateRange(parameters));

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, mockContext.Object);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetBPTypeAndConverter handles property without getter method correctly.
    /// This test verifies the scenario where property exists but has no GetMethod.
    /// </summary>
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetBPTypeAndConverter_PropertyWithoutGetter_ReturnsNull()
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockContainingType = new Mock<INamedTypeSymbol>();
        var mockProperty = new Mock<IPropertySymbol>();

        var mockProperties = new List<IPropertySymbol> { mockProperty.Object };
        var mockMethods = new List<IMethodSymbol>();

        mockFieldSymbol.Setup(f => f.Name).Returns("TestProperty");
        mockFieldSymbol.Setup(f => f.ContainingType).Returns(mockContainingType.Object);
        mockContainingType.Setup(t => t.GetAllProperties("Test", mockContext.Object)).Returns(mockProperties);
        mockContainingType.Setup(t => t.GetAllMethods("GetTest", mockContext.Object)).Returns(mockMethods);

        mockProperty.Setup(p => p.GetMethod).Returns((IMethodSymbol)null);

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, mockContext.Object);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetBPTypeAndConverter handles null context parameter correctly.
    /// This test verifies behavior when context parameter is null.
    /// </summary>
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetBPTypeAndConverter_NullContext_HandledCorrectly()
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();

        mockFieldSymbol.Setup(f => f.Name).Returns("TestField");

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, null);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetBPTypeAndConverter handles null field symbol correctly.
    /// This test verifies behavior when fieldSymbol parameter is null.
    /// </summary>
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetBPTypeAndConverter_NullFieldSymbol_ThrowsException()
    {
        // Arrange
        var mockContext = new Mock<SourceGenContext>();

        // Act & Assert
        Assert.Throws<NullReferenceException>(() =>
            ITypeSymbolExtensions.GetBPTypeAndConverter(null, mockContext.Object));
    }

    /// <summary>
    /// Tests that GetBPTypeAndConverter handles multiple static getter methods correctly.
    /// This test verifies that the first matching static getter method is used.
    /// </summary>
    [Category("auto-generated")]
    [Author("Code Testing Agent 0.4.133-alpha+a413c4336c")]
    public void GetBPTypeAndConverter_MultipleStaticGetters_UsesFirstMatch()
    {
        // Arrange
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockContainingType = new Mock<INamedTypeSymbol>();
        var mockGetter1 = new Mock<IMethodSymbol>();
        var mockGetter2 = new Mock<IMethodSymbol>();
        var mockReturnType1 = new Mock<ITypeSymbol>();
        var mockReturnType2 = new Mock<ITypeSymbol>();
        var mockParameter = new Mock<IParameterSymbol>();

        var mockProperties = new List<IPropertySymbol>();
        var mockMethods = new List<IMethodSymbol> { mockGetter1.Object, mockGetter2.Object };

        mockFieldSymbol.Setup(f => f.Name).Returns("TestProperty");
        mockFieldSymbol.Setup(f => f.ContainingType).Returns(mockContainingType.Object);
        mockContainingType.Setup(t => t.GetAllProperties("Test", mockContext.Object)).Returns(mockProperties);
        mockContainingType.Setup(t => t.GetAllMethods("GetTest", mockContext.Object)).Returns(mockMethods);

        // First getter meets criteria
        mockGetter1.Setup(g => g.IsStatic).Returns(true);
        mockGetter1.Setup(g => g.IsPublic()).Returns(true);
        mockGetter1.Setup(g => g.Parameters).Returns(ImmutableArray.Create(mockParameter.Object));
        mockGetter1.Setup(g => g.GetAttributes()).Returns(ImmutableArray<AttributeData>.Empty);
        mockGetter1.Setup(g => g.ReturnType).Returns(mockReturnType1.Object);

        // Second getter also meets criteria but should not be used
        mockGetter2.Setup(g => g.IsStatic).Returns(true);
        mockGetter2.Setup(g => g.IsPublic()).Returns(true);
        mockGetter2.Setup(g => g.Parameters).Returns(ImmutableArray.Create(mockParameter.Object));
        mockGetter2.Setup(g => g.GetAttributes()).Returns(ImmutableArray<AttributeData>.Empty);
        mockGetter2.Setup(g => g.ReturnType).Returns(mockReturnType2.Object);

        mockReturnType1.Setup(rt => rt.GetAttributes()).Returns(ImmutableArray<AttributeData>.Empty);
        mockReturnType2.Setup(rt => rt.GetAttributes()).Returns(ImmutableArray<AttributeData>.Empty);

        // Act
        var result = ITypeSymbolExtensions.GetBPTypeAndConverter(mockFieldSymbol.Object, mockContext.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(mockReturnType1.Object, result.Value.type);
        Assert.IsNull(result.Value.converter);
    }
}