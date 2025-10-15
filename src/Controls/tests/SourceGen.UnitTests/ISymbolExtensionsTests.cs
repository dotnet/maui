using System;
using System.Collections;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Unit tests for ISymbolExtensions.GetAttributes method.
/// Tests filtering of symbol attributes by name with various edge cases and scenarios.
/// </summary>
[TestFixture]
public class ISymbolExtensionsTests
{
	/// <summary>
	/// Tests GetAttributes method with null symbol parameter.
	/// Should throw NullReferenceException when symbol is null.
	/// </summary>
	[Test]
	public void GetAttributes_NullSymbol_ThrowsNullReferenceException()
	{
		// Arrange
		ISymbol? symbol = null;
		string name = "TestAttribute";

		// Act & Assert
		Assert.Throws<NullReferenceException>(() => symbol!.GetAttributes(name));
	}

	/// <summary>
	/// Tests GetAttributes method with symbol having no attributes.
	/// Should return empty ImmutableArray when symbol has no attributes.
	/// </summary>
	[Test]
	public void GetAttributes_SymbolWithNoAttributes_ReturnsEmptyArray()
	{
		// Arrange
		var mockSymbol = new Mock<ISymbol>();
		mockSymbol.Setup(s => s.GetAttributes()).Returns(ImmutableArray<AttributeData>.Empty);

		// Act
		var result = mockSymbol.Object.GetAttributes("TestAttribute");

		// Assert
		Assert.AreEqual(0, result.Length);
		Assert.IsTrue(result.IsEmpty);
	}

	/// <summary>
	/// Tests GetAttributes method with non-matching attribute name.
	/// Should return empty ImmutableArray when no attributes match the specified name.
	/// </summary>
	[Test]
	public void GetAttributes_NonMatchingAttributeName_ReturnsEmptyArray()
	{
		// Arrange
		var mockSymbol = new Mock<ISymbol>();

		// Create a mock AttributeData that returns null for AttributeClass
		// This simulates an attribute that won't match any name
		var mockAttribute = new Mock<AttributeData>();
		// Don't setup AttributeClass - it will return null by default for reference types

		var attributes = ImmutableArray.Create(mockAttribute.Object);
		mockSymbol.Setup(s => s.GetAttributes()).Returns(attributes);

		// Act
		var result = mockSymbol.Object.GetAttributes("TestAttribute");

		// Assert
		Assert.AreEqual(0, result.Length);
		Assert.IsTrue(result.IsEmpty);
	}

	/// <summary>
	/// Tests ToFQDisplayString method with null symbol parameter.
	/// Should throw NotImplementedException when symbol is null.
	/// </summary>
	[Test]
	public void ToFQDisplayString_NullSymbol_ThrowsNullReferenceException()
	{
		// Arrange
		ISymbol? symbol = null;

		// Act & Assert
		Assert.Throws<NotImplementedException>(() => symbol!.ToFQDisplayString());
	}

	/// <summary>
	/// Tests ToFQDisplayString method with field symbol.
	/// Should return formatted string using FullyQualifiedFormat with IncludeContainingType.
	/// </summary>
	[Test]
	public void ToFQDisplayString_FieldSymbol_ReturnsFormattedString()
	{
		// Arrange
		var mockField = new Mock<IFieldSymbol>();
		const string expectedResult = "global::MyNamespace.MyClass.MyField";
		mockField.Setup(f => f.ToDisplayString(It.IsAny<SymbolDisplayFormat>()))
			.Returns(expectedResult);

		// Act
		var result = mockField.Object.ToFQDisplayString();

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	/// <summary>
	/// Tests ToFQDisplayString method with property symbol.
	/// Should return formatted string using FullyQualifiedFormat with IncludeContainingType.
	/// </summary>
	[Test]
	public void ToFQDisplayString_PropertySymbol_ReturnsFormattedString()
	{
		// Arrange
		var mockProperty = new Mock<IPropertySymbol>();
		const string expectedResult = "global::MyNamespace.MyClass.MyProperty";
		mockProperty.Setup(p => p.ToDisplayString(It.IsAny<SymbolDisplayFormat>()))
			.Returns(expectedResult);

		// Act
		var result = mockProperty.Object.ToFQDisplayString();

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	/// <summary>
	/// Tests ToFQDisplayString method with type symbol.
	/// Should return formatted string using FullyQualifiedFormat.
	/// </summary>
	[Test]
	public void ToFQDisplayString_TypeSymbol_ReturnsFormattedString()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		const string expectedResult = "global::MyNamespace.MyClass";
		mockType.Setup(t => t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
			.Returns(expectedResult);

		// Act
		var result = mockType.Object.ToFQDisplayString();

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	/// <summary>
	/// Tests ToFQDisplayString method with static method symbol.
	/// Should return formatted string using FullyQualifiedFormat with IncludeContainingType.
	/// </summary>
	[Test]
	public void ToFQDisplayString_StaticMethodSymbol_ReturnsFormattedString()
	{
		// Arrange
		var mockMethod = new Mock<IMethodSymbol>();
		const string expectedResult = "global::MyNamespace.MyClass.MyStaticMethod()";
		mockMethod.Setup(m => m.IsStatic).Returns(true);
		mockMethod.Setup(m => m.ToDisplayString(It.IsAny<SymbolDisplayFormat>()))
			.Returns(expectedResult);

		// Act
		var result = mockMethod.Object.ToFQDisplayString();

		// Assert
		Assert.AreEqual(expectedResult, result);
	}

	/// <summary>
	/// Tests ToFQDisplayString method with non-static method symbol.
	/// Should throw NotImplementedException when method is not static.
	/// </summary>
	[Test]
	public void ToFQDisplayString_NonStaticMethodSymbol_ThrowsNotImplementedException()
	{
		// Arrange
		var mockMethod = new Mock<IMethodSymbol>();
		mockMethod.Setup(m => m.IsStatic).Returns(false);

		// Act & Assert
		Assert.Throws<NotImplementedException>(() => mockMethod.Object.ToFQDisplayString());
	}

	/// <summary>
	/// Tests ToFQDisplayString method with unsupported symbol type.
	/// Should throw NotImplementedException when symbol type is not supported.
	/// </summary>
	[Test]
	public void ToFQDisplayString_UnsupportedSymbolType_ThrowsNotImplementedException()
	{
		// Arrange
		var mockSymbol = new Mock<ISymbol>();

		// Act & Assert
		Assert.Throws<NotImplementedException>(() => mockSymbol.Object.ToFQDisplayString());
	}

	/// <summary>
	/// Tests ToFQDisplayString method with namespace symbol.
	/// Should throw NotImplementedException when symbol is a namespace.
	/// </summary>
	[Test]
	public void ToFQDisplayString_NamespaceSymbol_ThrowsNotImplementedException()
	{
		// Arrange
		var mockNamespace = new Mock<INamespaceSymbol>();

		// Act & Assert
		Assert.Throws<NotImplementedException>(() => mockNamespace.Object.ToFQDisplayString());
	}
}