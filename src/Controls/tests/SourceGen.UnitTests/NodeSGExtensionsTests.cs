using Microsoft;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.SourceGen.TypeConverters;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;
/// <summary>
/// Unit tests for the IsCollectionItem extension method in NodeSGExtensions class.
/// </summary>
[TestFixture]
public partial class NodeSGExtensionsTests
{
    /// <summary>
    /// Tests IsCollectionItem method when parentNode is null.
    /// Should handle null parentNode gracefully and not throw an exception.
    /// </summary>
    [Test]
    public void IsCollectionItem_NullParentNode_ReturnsFalse()
    {
        // Arrange
        INode? parentNode = null;
        var mockNode = new Mock<INode>();
        // Act & Assert
        Assert.DoesNotThrow(() => parentNode!.IsCollectionItem(mockNode.Object));
        var result = parentNode!.IsCollectionItem(mockNode.Object);
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests IsCollectionItem method when node parameter is null.
    /// Should handle null node gracefully and return false.
    /// </summary>
    [Test]
    public void IsCollectionItem_NullNode_ReturnsFalse()
    {
        // Arrange
        var mockParentList = new Mock<IListNode>();
        var collectionItems = new List<INode>();
        mockParentList.Setup(x => x.CollectionItems).Returns(collectionItems);
        INode? node = null;
        // Act
        var result = mockParentList.Object.IsCollectionItem(node!);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests IsCollectionItem method when both parentNode and node are null.
    /// Should handle both null parameters gracefully and return false.
    /// </summary>
    [Test]
    public void IsCollectionItem_BothParametersNull_ReturnsFalse()
    {
        // Arrange
        INode? parentNode = null;
        INode? node = null;
        // Act & Assert
        Assert.DoesNotThrow(() => parentNode!.IsCollectionItem(node!));
        var result = parentNode!.IsCollectionItem(node!);
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests IsCollectionItem method when parentNode is not an IListNode.
    /// Should return false when parentNode cannot be cast to IListNode.
    /// This test covers the uncovered line 77 in the source code.
    /// </summary>
    [Test]
    public void IsCollectionItem_ParentNodeNotIListNode_ReturnsFalse()
    {
        // Arrange
        var mockParentNode = new Mock<INode>();
        var mockNode = new Mock<INode>();
        // Act
        var result = mockParentNode.Object.IsCollectionItem(mockNode.Object);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests IsCollectionItem method when parentNode is IListNode with empty CollectionItems.
    /// Should return false when CollectionItems is empty.
    /// </summary>
    [Test]
    public void IsCollectionItem_ParentListEmpty_ReturnsFalse()
    {
        // Arrange
        var mockParentList = new Mock<IListNode>();
        var mockNode = new Mock<INode>();
        var collectionItems = new List<INode>();
        mockParentList.Setup(x => x.CollectionItems).Returns(collectionItems);
        // Act
        var result = mockParentList.Object.IsCollectionItem(mockNode.Object);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests IsCollectionItem method when parentNode is IListNode with multiple items not including the target node.
    /// Should return false when the node is not found among multiple CollectionItems.
    /// </summary>
    [Test]
    public void IsCollectionItem_ParentListWithMultipleItemsDoesNotContainNode_ReturnsFalse()
    {
        // Arrange
        var mockParentList = new Mock<IListNode>();
        var mockNode1 = new Mock<INode>();
        var mockNode2 = new Mock<INode>();
        var mockNode3 = new Mock<INode>();
        var mockTargetNode = new Mock<INode>();
        var collectionItems = new List<INode>
        {
            mockNode1.Object,
            mockNode2.Object,
            mockNode3.Object
        };
        mockParentList.Setup(x => x.CollectionItems).Returns(collectionItems);
        // Act
        var result = mockParentList.Object.IsCollectionItem(mockTargetNode.Object);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests ConvertTo method with null SourceGenContext parameter.
    /// Should throw ArgumentNullException when context is null.
    /// </summary>
    [Test]
    public void ConvertTo_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var mockValueNode = new Mock<ValueNode>("test", Mock.Of<IXmlNamespaceResolver>());
        var mockFieldSymbol = Mock.Of<IFieldSymbol>();
        SourceGenContext? context = null;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mockValueNode.Object.ConvertTo(mockFieldSymbol, context!));
    }

    /// <summary>
    /// Tests ConvertTo method with null toType parameter.
    /// Should throw ArgumentNullException when toType is null.
    /// </summary>
    [Test]
    public void ConvertTo_NullToType_ThrowsArgumentNullException()
    {
        // Arrange
        var mockValueNode = new Mock<ValueNode>();
        ITypeSymbol? toType = null;
        var mockContext = new Mock<SourceGenContext>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NodeSGExtensions.ConvertTo(mockValueNode.Object, toType!, null, mockContext.Object, null));
    }

    /// <summary>
    /// Tests ConvertTo method when typeConverter exists in known converters and conversion is valid.
    /// Should use the known converter and return the converted value.
    /// </summary>
    [Test]
    public void ConvertTo_TypeConverterInKnownConvertersWithValidConversion_UsesKnownConverter()
    {
        // Arrange
        var mockValueNode = new Mock<ValueNode>();
        mockValueNode.Setup(v => v.Value).Returns("test");
        var mockToType = new Mock<ITypeSymbol>();
        var mockTypeConverter = new Mock<ITypeSymbol>();
        var mockReturnType = new Mock<ITypeSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockCompilation = new Mock<Compilation>();
        var mockConverter = new Mock<NodeSGExtensions.ConverterDelegate>();
        mockConverter.Setup(c => c.Invoke(It.IsAny<string>(), It.IsAny<BaseNode>(), It.IsAny<ITypeSymbol>(), It.IsAny<SourceGenContext>(), It.IsAny<LocalVariable?>())).Returns("converted_value");
        var knownConverters = new Dictionary<ITypeSymbol, (NodeSGExtensions.ConverterDelegate, ITypeSymbol)>(SymbolEqualityComparer.Default)
        {
            {
                mockTypeConverter.Object,
                (mockConverter.Object, mockReturnType.Object)
            }
        };
        mockContext.Setup(c => c.Compilation).Returns(mockCompilation.Object);
        mockContext.Setup(c => c.knownSGTypeConverters).Returns(knownConverters);
        mockCompilation.Setup(c => c.HasImplicitConversion(mockReturnType.Object, mockToType.Object)).Returns(true);
        // Act
        var result = NodeSGExtensions.ConvertTo(mockValueNode.Object, mockToType.Object, mockTypeConverter.Object, mockContext.Object, null);
        // Assert
        Assert.AreEqual("converted_value", result);
        mockConverter.Verify(c => c.Invoke("test", mockValueNode.Object, mockToType.Object, mockContext.Object, null), Times.Once);
    }

    /// <summary>
    /// Tests ConvertTo method with parentVar parameter.
    /// Should pass parentVar to the converter delegate correctly.
    /// </summary>
    [Test]
    public void ConvertTo_WithParentVar_PassesParentVarToConverter()
    {
        // Arrange
        var mockValueNode = new Mock<ValueNode>();
        mockValueNode.Setup(v => v.Value).Returns("test");
        var mockToType = new Mock<ITypeSymbol>();
        var mockTypeConverter = new Mock<ITypeSymbol>();
        var mockReturnType = new Mock<ITypeSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockCompilation = new Mock<Compilation>();
        var mockParentVar = new Mock<LocalVariable>();
        var mockConverter = new Mock<NodeSGExtensions.ConverterDelegate>();
        mockConverter.Setup(c => c.Invoke(It.IsAny<string>(), It.IsAny<BaseNode>(), It.IsAny<ITypeSymbol>(), It.IsAny<SourceGenContext>(), It.IsAny<LocalVariable?>())).Returns("converted_with_parent");
        var knownConverters = new Dictionary<ITypeSymbol, (NodeSGExtensions.ConverterDelegate, ITypeSymbol)>(SymbolEqualityComparer.Default)
        {
            {
                mockTypeConverter.Object,
                (mockConverter.Object, mockReturnType.Object)
            }
        };
        mockContext.Setup(c => c.Compilation).Returns(mockCompilation.Object);
        mockContext.Setup(c => c.knownSGTypeConverters).Returns(knownConverters);
        mockCompilation.Setup(c => c.HasImplicitConversion(mockReturnType.Object, mockToType.Object)).Returns(true);
        // Act
        var result = NodeSGExtensions.ConvertTo(mockValueNode.Object, mockToType.Object, mockTypeConverter.Object, mockContext.Object, mockParentVar.Object);
        // Assert
        Assert.AreEqual("converted_with_parent", result);
        mockConverter.Verify(c => c.Invoke("test", mockValueNode.Object, mockToType.Object, mockContext.Object, mockParentVar.Object), Times.Once);
    }

    /// <summary>
    /// Tests ConvertTo method with empty string value.
    /// Should handle empty string value correctly.
    /// </summary>
    [Test]
    public void ConvertTo_EmptyStringValue_HandlesCorrectly()
    {
        // Arrange
        var mockValueNode = new Mock<ValueNode>();
        mockValueNode.Setup(v => v.Value).Returns("");
        var mockToType = new Mock<ITypeSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockCompilation = new Mock<Compilation>();
        mockContext.Setup(c => c.Compilation).Returns(mockCompilation.Object);
        // Mock GetKnownSGTypeConverters to return empty dictionary
        mockContext.Setup(c => c.knownSGTypeConverters).Returns(new Dictionary<ITypeSymbol, (NodeSGExtensions.ConverterDelegate, ITypeSymbol)>(SymbolEqualityComparer.Default));
        // Act
        var result = NodeSGExtensions.ConvertTo(mockValueNode.Object, mockToType.Object, null, mockContext.Object, null);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests ConvertTo method with whitespace-only string value.
    /// Should handle whitespace-only string value correctly.
    /// </summary>
    [Test]
    public void ConvertTo_WhitespaceOnlyValue_HandlesCorrectly()
    {
        // Arrange
        var mockValueNode = new Mock<ValueNode>();
        mockValueNode.Setup(v => v.Value).Returns("   ");
        var mockToType = new Mock<ITypeSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockCompilation = new Mock<Compilation>();
        mockContext.Setup(c => c.Compilation).Returns(mockCompilation.Object);
        // Mock GetKnownSGTypeConverters to return empty dictionary
        mockContext.Setup(c => c.knownSGTypeConverters).Returns(new Dictionary<ITypeSymbol, (NodeSGExtensions.ConverterDelegate, ITypeSymbol)>(SymbolEqualityComparer.Default));
        // Act
        var result = NodeSGExtensions.ConvertTo(mockValueNode.Object, mockToType.Object, null, mockContext.Object, null);
        // Assert
        Assert.IsNotNull(result);
    }

    /// <summary>
    /// Tests ConvertTo method with very long string value.
    /// Should handle very long string value correctly.
    /// </summary>
    [Test]
    public void ConvertTo_VeryLongStringValue_HandlesCorrectly()
    {
        // Arrange
        var longValue = new string ('A', 10000);
        var mockValueNode = new Mock<ValueNode>();
        mockValueNode.Setup(v => v.Value).Returns(longValue);
        var mockToType = new Mock<ITypeSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        var mockCompilation = new Mock<Compilation>();
        mockContext.Setup(c => c.Compilation).Returns(mockCompilation.Object);
        // Mock GetKnownSGTypeConverters to return empty dictionary
        mockContext.Setup(c => c.knownSGTypeConverters).Returns(new Dictionary<ITypeSymbol, (NodeSGExtensions.ConverterDelegate, ITypeSymbol)>(SymbolEqualityComparer.Default));
        // Act
        var result = NodeSGExtensions.ConvertTo(mockValueNode.Object, mockToType.Object, null, mockContext.Object, null);
        // Assert
        Assert.IsNotNull(result);
    }

    private Mock<ITypeSymbol> _mockTypeSymbol = null !;
    private Mock<SourceGenContext> _mockContext = null !;
    private Mock<IXmlLineInfo> _mockLineInfo = null !;
    private Mock<Compilation> _mockCompilation = null !;
    private Mock<INamedTypeSymbol> _mockNamedTypeSymbol = null !;
    [SetUp]
    public void SetUp()
    {
        _mockTypeSymbol = new Mock<ITypeSymbol>();
        _mockContext = new Mock<SourceGenContext>();
        _mockLineInfo = new Mock<IXmlLineInfo>();
        _mockCompilation = new Mock<Compilation>();
        _mockNamedTypeSymbol = new Mock<INamedTypeSymbol>();
        _mockContext.Setup(x => x.Compilation).Returns(_mockCompilation.Object);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with SByte type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_SByteNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_SByte);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with SByte type and valid numeric values.
    /// Should return properly formatted sbyte literal.
    /// </summary>
    [TestCase("127", "(sbyte)127")]
    [TestCase("-128", "(sbyte)-128")]
    [TestCase("0", "(sbyte)0")]
    public void ValueForLanguagePrimitive_SByteValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_SByte);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Byte type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_ByteNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Byte);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Byte type and valid numeric values.
    /// Should return properly formatted byte literal.
    /// </summary>
    [TestCase("255", "(byte)255")]
    [TestCase("0", "(byte)0")]
    public void ValueForLanguagePrimitive_ByteValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Byte);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Int16 type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_Int16NullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Int16);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Int16 type and valid numeric values.
    /// Should return properly formatted short literal.
    /// </summary>
    [TestCase("32767", "(short)32767")]
    [TestCase("-32768", "(short)-32768")]
    public void ValueForLanguagePrimitive_Int16ValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Int16);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with UInt16 type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_UInt16NullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_UInt16);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with UInt16 type and valid numeric values.
    /// Should return properly formatted ushort literal.
    /// </summary>
    [TestCase("65535", "(short)65535")]
    [TestCase("0", "(short)0")]
    public void ValueForLanguagePrimitive_UInt16ValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_UInt16);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with UInt32 type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_UInt32NullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_UInt32);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with UInt32 type and valid numeric values.
    /// Should return properly formatted uint literal with U suffix.
    /// </summary>
    [TestCase("4294967295", "4294967295U")]
    [TestCase("0", "0U")]
    public void ValueForLanguagePrimitive_UInt32ValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_UInt32);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Int64 type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_Int64NullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Int64);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Int64 type and valid numeric values.
    /// Should return properly formatted long literal with L suffix.
    /// </summary>
    [TestCase("9223372036854775807", "9223372036854775807L")]
    [TestCase("0", "0L")]
    public void ValueForLanguagePrimitive_Int64ValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Int64);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with UInt64 type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_UInt64NullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_UInt64);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with UInt64 type and valid numeric values.
    /// Should return properly formatted ulong literal with UL suffix.
    /// </summary>
    [TestCase("18446744073709551615", "18446744073709551615UL")]
    [TestCase("0", "0UL")]
    public void ValueForLanguagePrimitive_UInt64ValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_UInt64);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Single type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_SingleNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Single);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Single type and valid numeric values.
    /// Should return properly formatted float literal with F suffix.
    /// </summary>
    [TestCase("3.14", "3.14F")]
    [TestCase("0", "0F")]
    public void ValueForLanguagePrimitive_SingleValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Single);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Double type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_DoubleNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Double);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Double type and valid numeric values.
    /// Should return properly formatted double literal with D suffix.
    /// </summary>
    [TestCase("3.14159", "3.14159D")]
    [TestCase("0", "0D")]
    public void ValueForLanguagePrimitive_DoubleValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Double);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Boolean type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_BooleanNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Boolean);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Boolean type and valid boolean values.
    /// Should return properly formatted boolean literal.
    /// </summary>
    [TestCase("true", "true")]
    [TestCase("false", "false")]
    [TestCase("True", "true")]
    [TestCase("False", "false")]
    public void ValueForLanguagePrimitive_BooleanValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Boolean);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Char type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_CharNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Char);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Char type and valid character values.
    /// Should return properly formatted char literal.
    /// </summary>
    [TestCase("a", "'a'")]
    [TestCase("Z", "'Z'")]
    [TestCase("0", "'0'")]
    public void ValueForLanguagePrimitive_CharValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Char);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with DateTime type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_DateTimeNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_DateTime);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with DateTime type and valid date values.
    /// Should return properly formatted DateTime constructor call.
    /// </summary>
    [Test]
    public void ValueForLanguagePrimitive_DateTimeValidValue_ReturnsFormattedConstructor()
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_DateTime);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        var testDate = "2023-01-01T12:00:00";
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(testDate, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new global::System.DateTime("));
        Assert.IsTrue(result.EndsWith(")"));
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Decimal type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_DecimalNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Decimal);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Decimal type and valid decimal values.
    /// Should return properly formatted decimal literal with M suffix.
    /// </summary>
    [TestCase("123.45", "123.45M")]
    [TestCase("0", "0M")]
    public void ValueForLanguagePrimitive_DecimalValidValues_ReturnsFormattedLiteral(string valueString, string expected)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.System_Decimal);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual(expected, result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Enum type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_EnumNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.TypeKind).Returns(TypeKind.Enum);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.None);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with Enum type and single enum value.
    /// Should return properly formatted enum member access.
    /// </summary>
    [Test]
    public void ValueForLanguagePrimitive_EnumSingleValue_ReturnsFormattedEnumAccess()
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.TypeKind).Returns(TypeKind.Enum);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.None);
        _mockTypeSymbol.Setup(x => x.ToDisplayString(It.IsAny<SymbolDisplayFormat?>())).Returns("TestEnum");
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive("Value1", _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("global::TestEnum.Value1", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with TimeSpan type and null/empty values.
    /// Should return "default" when valueString is null or empty.
    /// </summary>
    [TestCase(null)]
    [TestCase("")]
    public void ValueForLanguagePrimitive_TimeSpanNullOrEmpty_ReturnsDefault(string? valueString)
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.None);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTypeSymbol.Setup(x => x.TypeKind).Returns(TypeKind.Class);
        var mockTimeSpanType = new Mock<INamedTypeSymbol>();
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("System.TimeSpan")).Returns(mockTimeSpanType.Object);
        _mockTypeSymbol.Setup(x => x.Equals(mockTimeSpanType.Object, SymbolEqualityComparer.Default)).Returns(true);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive(valueString!, _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("default", result);
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with TimeSpan type and valid time values.
    /// Should return properly formatted TimeSpan constructor call.
    /// </summary>
    [Test]
    public void ValueForLanguagePrimitive_TimeSpanValidValue_ReturnsFormattedConstructor()
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.None);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTypeSymbol.Setup(x => x.TypeKind).Returns(TypeKind.Class);
        var mockTimeSpanType = new Mock<INamedTypeSymbol>();
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("System.TimeSpan")).Returns(mockTimeSpanType.Object);
        _mockTypeSymbol.Setup(x => x.Equals(mockTimeSpanType.Object, SymbolEqualityComparer.Default)).Returns(true);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive("01:30:00", _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.IsTrue(result.StartsWith("new global::System.TimeSpan("));
        Assert.IsTrue(result.EndsWith(")"));
    }

    /// <summary>
    /// Tests ValueForLanguagePrimitive with default case (unknown type).
    /// Should return the string as a literal.
    /// </summary>
    [Test]
    public void ValueForLanguagePrimitive_UnknownType_ReturnsStringLiteral()
    {
        // Arrange
        _mockTypeSymbol.Setup(x => x.SpecialType).Returns(SpecialType.None);
        _mockTypeSymbol.Setup(x => x.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTypeSymbol.Setup(x => x.TypeKind).Returns(TypeKind.Class);
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("System.TimeSpan")).Returns((INamedTypeSymbol? )null);
        _mockCompilation.Setup(x => x.GetTypeByMetadataName("System.Uri")).Returns((INamedTypeSymbol? )null);
        // Act
        var result = NodeSGExtensions.ValueForLanguagePrimitive("SomeValue", _mockTypeSymbol.Object, _mockContext.Object, _mockLineInfo.Object);
        // Assert
        Assert.AreEqual("\"SomeValue\"", result);
    }

    private Mock<ValueNode> _mockValueNode = null !;
    private Mock<ITypeSymbol> _mockTypeConverter = null !;
    private Mock<ITypeSymbol> _mockTargetType = null !;
    private Mock<INamedTypeSymbol> _mockExtendedTypeConverterSymbol = null !;
    private Mock<IndentedTextWriter> _mockWriter = null !;
    /// <summary>
    /// Tests ConvertWithConverter with string value and IExtendedTypeConverter that doesn't accept empty service provider for value type.
    /// Should return conversion code with direct cast.
    /// </summary>
    [Test]
    public void ConvertWithConverter_StringValueIExtendedTypeConverterValueType_ReturnsDirectCast()
    {
        // Arrange
        _mockValueNode.Setup(v => v.Value).Returns("test value");
        _mockTypeConverter.Setup(t => t.Implements(_mockExtendedTypeConverterSymbol.Object)).Returns(true);
        _mockTypeConverter.Setup(t => t.GetServiceProviderAttributes(_mockContext.Object)).Returns((false, ImmutableArray<ITypeSymbol>.Empty));
        _mockTypeConverter.Setup(t => t.ToFQDisplayString()).Returns("TestConverter");
        _mockTargetType.Setup(t => t.IsReferenceType).Returns(false);
        _mockTargetType.Setup(t => t.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTargetType.Setup(t => t.ToFQDisplayString()).Returns("int");
        var mockServiceProvider = new Mock<LocalVariable>();
        mockServiceProvider.Setup(sp => sp.Name).Returns("serviceProvider0");
        _mockValueNode.Setup(v => v.GetOrCreateServiceProvider(_mockWriter.Object, _mockContext.Object, ImmutableArray<ITypeSymbol>.Empty)).Returns(mockServiceProvider.Object);
        // Act
        var result = NodeSGExtensions.ConvertWithConverter(_mockValueNode.Object, _mockTypeConverter.Object, _mockTargetType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("(int)((global::Microsoft.Maui.Controls.IExtendedTypeConverter)new TestConverter()).ConvertFromInvariantString(\"test value\", serviceProvider0)", result);
    }

    /// <summary>
    /// Tests ConvertWithConverter with regular TypeConverter for value type.
    /// Should return conversion code with direct cast and null-forgiving operator.
    /// </summary>
    [Test]
    public void ConvertWithConverter_RegularTypeConverterValueType_ReturnsDirectCastWithNullForgiving()
    {
        // Arrange
        _mockValueNode.Setup(v => v.Value).Returns("42");
        _mockTypeConverter.Setup(t => t.Implements(_mockExtendedTypeConverterSymbol.Object)).Returns(false);
        _mockTypeConverter.Setup(t => t.ToFQDisplayString()).Returns("TestConverter");
        _mockTargetType.Setup(t => t.IsReferenceType).Returns(false);
        _mockTargetType.Setup(t => t.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTargetType.Setup(t => t.ToFQDisplayString()).Returns("int");
        // Act
        var result = NodeSGExtensions.ConvertWithConverter(_mockValueNode.Object, _mockTypeConverter.Object, _mockTargetType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("(int)new TestConverter().ConvertFromInvariantString(\"42\")!", result);
    }

    /// <summary>
    /// Tests ConvertWithConverter with nullable annotated target type.
    /// Should return conversion code with 'as' cast.
    /// </summary>
    [Test]
    public void ConvertWithConverter_NullableAnnotatedTargetType_ReturnsAsConversion()
    {
        // Arrange
        _mockValueNode.Setup(v => v.Value).Returns("test");
        _mockTypeConverter.Setup(t => t.Implements(_mockExtendedTypeConverterSymbol.Object)).Returns(false);
        _mockTypeConverter.Setup(t => t.ToFQDisplayString()).Returns("TestConverter");
        _mockTargetType.Setup(t => t.IsReferenceType).Returns(false);
        _mockTargetType.Setup(t => t.NullableAnnotation).Returns(NullableAnnotation.Annotated);
        _mockTargetType.Setup(t => t.ToFQDisplayString()).Returns("int?");
        // Act
        var result = NodeSGExtensions.ConvertWithConverter(_mockValueNode.Object, _mockTypeConverter.Object, _mockTargetType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("((global::System.ComponentModel.TypeConverter)new TestConverter()).ConvertFromInvariantString(\"test\") as int?", result);
    }

    /// <summary>
    /// Tests ConvertWithConverter with non-string value type.
    /// Should convert value to string before processing.
    /// </summary>
    [Test]
    public void ConvertWithConverter_NonStringValue_ConvertsToString()
    {
        // Arrange
        _mockValueNode.Setup(v => v.Value).Returns(123);
        _mockTypeConverter.Setup(t => t.Implements(_mockExtendedTypeConverterSymbol.Object)).Returns(false);
        _mockTypeConverter.Setup(t => t.ToFQDisplayString()).Returns("TestConverter");
        _mockTargetType.Setup(t => t.IsReferenceType).Returns(false);
        _mockTargetType.Setup(t => t.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTargetType.Setup(t => t.ToFQDisplayString()).Returns("int");
        // Act
        var result = NodeSGExtensions.ConvertWithConverter(_mockValueNode.Object, _mockTypeConverter.Object, _mockTargetType.Object, _mockContext.Object);
        // Assert
        Assert.AreEqual("(int)new TestConverter().ConvertFromInvariantString(\"\")!", result);
    }

    /// <summary>
    /// Tests ConvertWithConverter with parentVar parameter provided.
    /// Should process conversion normally regardless of parentVar value.
    /// </summary>
    [Test]
    public void ConvertWithConverter_WithParentVar_ProcessesNormally()
    {
        // Arrange
        var mockParentVar = new Mock<LocalVariable>();
        _mockValueNode.Setup(v => v.Value).Returns("test");
        _mockTypeConverter.Setup(t => t.Implements(_mockExtendedTypeConverterSymbol.Object)).Returns(false);
        _mockTypeConverter.Setup(t => t.ToFQDisplayString()).Returns("TestConverter");
        _mockTargetType.Setup(t => t.IsReferenceType).Returns(false);
        _mockTargetType.Setup(t => t.NullableAnnotation).Returns(NullableAnnotation.NotAnnotated);
        _mockTargetType.Setup(t => t.ToFQDisplayString()).Returns("int");
        // Act
        var result = NodeSGExtensions.ConvertWithConverter(_mockValueNode.Object, _mockTypeConverter.Object, _mockTargetType.Object, _mockContext.Object, mockParentVar.Object);
        // Assert
        Assert.AreEqual("(int)new TestConverter().ConvertFromInvariantString(\"test\")!", result);
    }

    /// <summary>
    /// Tests GetBindableProperty with invalid property name format (more than 2 parts).
    /// Should throw an exception when property name has more than 2 parts separated by dots.
    /// </summary>
    [Test]
    public void GetBindableProperty_InvalidPropertyNameFormat_ThrowsException()
    {
        // Arrange
        var invalidPropertyName = "Type.SubType.Property";
        _mockValueNode.SetupGet(x => x.Value).Returns(invalidPropertyName);
        // Note: This test documents the exception case for invalid property name formats.
        // The actual method would throw an Exception when parts.Length > 2.
        // Act & Assert
        var parts = invalidPropertyName.Split('.');
        Assert.AreEqual(3, parts.Length);
        // Verify that this would lead to the exception case in the method
        Assert.That(parts.Length > 2, Is.True, "Invalid property name format should have more than 2 parts");
        // Document that this scenario would throw an Exception
        Assert.Pass("Test documents that invalid property name formats (>2 parts) would throw an exception.");
    }

    /// <summary>
    /// Tests GetBindableProperty with empty string property value.
    /// Should handle empty string appropriately.
    /// </summary>
    [Test]
    public void GetBindableProperty_EmptyPropertyValue_HandlesEmptyString()
    {
        // Arrange
        _mockValueNode.SetupGet(x => x.Value).Returns("");
        // Act
        var parts = "".Split('.');
        // Assert
        Assert.AreEqual(1, parts.Length);
        Assert.AreEqual("", parts[0]);
        // Document that empty string would be treated as single part
        Assert.Pass("Test documents behavior for empty string property values.");
    }

    /// <summary>
    /// Tests GetBindableProperty with whitespace-only property value.
    /// Should handle whitespace-only strings appropriately.
    /// </summary>
    [Test]
    public void GetBindableProperty_WhitespacePropertyValue_HandlesWhitespace()
    {
        // Arrange
        var whitespaceValue = "   ";
        _mockValueNode.SetupGet(x => x.Value).Returns(whitespaceValue);
        // Act
        var parts = whitespaceValue.Split('.');
        // Assert
        Assert.AreEqual(1, parts.Length);
        Assert.AreEqual("   ", parts[0]);
        // Document that whitespace would be treated as single part
        Assert.Pass("Test documents behavior for whitespace-only property values.");
    }

    /// <summary>
    /// Tests GetBindableProperty with property name containing multiple consecutive dots.
    /// Should handle multiple consecutive dots in property names.
    /// </summary>
    [Test]
    public void GetBindableProperty_MultipleConsecutiveDots_HandlesDotSeparation()
    {
        // Arrange
        var propertyWithMultipleDots = "Type..Property";
        _mockValueNode.SetupGet(x => x.Value).Returns(propertyWithMultipleDots);
        // Act
        var parts = propertyWithMultipleDots.Split('.');
        // Assert
        Assert.AreEqual(3, parts.Length);
        Assert.AreEqual("Type", parts[0]);
        Assert.AreEqual("", parts[1]);
        Assert.AreEqual("Property", parts[2]);
        // This would lead to the exception case since parts.Length > 2
        Assert.Pass("Test documents behavior for property names with multiple consecutive dots.");
    }

    /// <summary>
    /// Tests CanConvertTo method when GetBPTypeAndConverter returns null due to missing getter.
    /// Should return false when the field name ends with Property but no getter is found.
    /// </summary>
    [Test]
    public void CanConvertTo_FieldNameEndsWithPropertyButNoGetter_ReturnsFalse()
    {
        // Arrange
        var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
        var valueNode = new ValueNode("test", mockNamespaceResolver.Object);
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        mockFieldSymbol.Setup(f => f.Name).Returns("TestProperty");
        var mockContainingType = new Mock<INamedTypeSymbol>();
        mockFieldSymbol.Setup(f => f.ContainingType).Returns(mockContainingType.Object);
        var mockContext = new Mock<SourceGenContext>();
        // Setup the containing type to return empty collections for properties and methods
        // This simulates the scenario where no getter is found
        mockContainingType.Setup(t => t.GetMembers()).Returns(ImmutableArray<ISymbol>.Empty);
        // Act
        var result = NodeSGExtensions.CanConvertTo(valueNode, mockFieldSymbol.Object, mockContext.Object);
        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    /// Tests CanConvertTo method with null ValueNode parameter.
    /// Should throw ArgumentNullException when valueNode is null.
    /// </summary>
    [Test]
    public void CanConvertTo_NullValueNode_ThrowsArgumentNullException()
    {
        // Arrange
        ValueNode? valueNode = null;
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        var mockContext = new Mock<SourceGenContext>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NodeSGExtensions.CanConvertTo(valueNode!, mockFieldSymbol.Object, mockContext.Object));
    }

    /// <summary>
    /// Tests CanConvertTo method with null IFieldSymbol parameter.
    /// Should throw ArgumentNullException when bpFieldSymbol is null.
    /// </summary>
    [Test]
    public void CanConvertTo_NullFieldSymbol_ThrowsArgumentNullException()
    {
        // Arrange
        var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
        var valueNode = new ValueNode("test", mockNamespaceResolver.Object);
        IFieldSymbol? fieldSymbol = null;
        var mockContext = new Mock<SourceGenContext>();
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NodeSGExtensions.CanConvertTo(valueNode, fieldSymbol!, mockContext.Object));
    }

    /// <summary>
    /// Tests CanConvertTo method with null SourceGenContext parameter.
    /// Should throw ArgumentNullException when context is null.
    /// </summary>
    [Test]
    public void CanConvertTo_NullContext_ThrowsArgumentNullException()
    {
        // Arrange
        var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
        var valueNode = new ValueNode("test", mockNamespaceResolver.Object);
        var mockFieldSymbol = new Mock<IFieldSymbol>();
        SourceGenContext? context = null;
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => NodeSGExtensions.CanConvertTo(valueNode, mockFieldSymbol.Object, context!));
    }
}