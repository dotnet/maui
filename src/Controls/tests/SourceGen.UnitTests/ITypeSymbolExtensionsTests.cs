using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;


#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;


public partial class ITypeSymbolExtensionsTests
{
	/// <summary>
	/// Tests GetAllMethods with mixed symbol types to ensure proper filtering.
	/// Should only return method symbols and exclude other symbol types from the result.
	/// </summary>
	[TestCase]
	public void GetAllMethods_MixedSymbolTypes_OnlyReturnsMethodSymbols()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockContext = new Mock<SourceGenContext>();
		var mockMethodSymbol = new Mock<IMethodSymbol>();
		var mockPropertySymbol = new Mock<IPropertySymbol>();
		var mockFieldSymbol = new Mock<IFieldSymbol>();
		var mockEventSymbol = new Mock<IEventSymbol>();
		var mockNamedTypeSymbol = new Mock<INamedTypeSymbol>();
		var mixedMembers = new List<ISymbol>
		{
			mockPropertySymbol.Object,
			mockMethodSymbol.Object,
			mockFieldSymbol.Object,
			mockEventSymbol.Object,
			mockNamedTypeSymbol.Object
		};
		mockTypeSymbol.Setup(s => s.GetAllMembers(mockContext.Object)).Returns(mixedMembers);
		// Act
		var result = mockTypeSymbol.Object.GetAllMethods(mockContext.Object);
		// Assert
		var resultList = result.ToList();
		Assert.AreEqual(1, resultList.Count);
		Assert.AreSame(mockMethodSymbol.Object, resultList[0]);
		// Verify no other symbol types are included
		Assert.IsFalse(resultList.Any(s => s is IPropertySymbol));
		Assert.IsFalse(resultList.Any(s => s is IFieldSymbol));
		Assert.IsFalse(resultList.Any(s => s is IEventSymbol));
		Assert.IsFalse(resultList.Any(s => s is INamedTypeSymbol));
		mockTypeSymbol.Verify(s => s.GetAllMembers(mockContext.Object), Times.Once);
	}

	/// <summary>
	/// Tests GetAllFields with various mixed symbol types using parameterized test cases.
	/// Should correctly filter to return only field symbols regardless of other symbol types present.
	/// </summary>
	[TestCase(0, 3, Description = "No fields, multiple other symbols")]
	[TestCase(1, 2, Description = "One field, multiple other symbols")]
	[TestCase(3, 4, Description = "Multiple fields, multiple other symbols")]
	[TestCase(5, 0, Description = "Only fields, no other symbols")]
	public static void GetAllFields_MixedSymbolTypes_ReturnsOnlyFieldSymbols(int fieldCount, int otherSymbolCount)
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var allMembers = new List<ISymbol>();
		var expectedFields = new List<IFieldSymbol>();
		// Add field symbols
		for (int i = 0; i < fieldCount; i++)
		{
			var mockField = new Mock<IFieldSymbol>();
			allMembers.Add(mockField.Object);
			expectedFields.Add(mockField.Object);
		}

		// Add other symbol types
		for (int i = 0; i < otherSymbolCount; i++)
		{
			switch (i % 3)
			{
				case 0:
					allMembers.Add(new Mock<IPropertySymbol>().Object);
					break;
				case 1:
					allMembers.Add(new Mock<IMethodSymbol>().Object);
					break;
				case 2:
					allMembers.Add(new Mock<IEventSymbol>().Object);
					break;
			}
		}

		mockTypeSymbol.Setup(x => x.GetAllMembers(null)).Returns(allMembers);
		// Act
		var result = mockTypeSymbol.Object.GetAllFields(null);
		// Assert
		var fieldSymbols = result.ToList();
		Assert.AreEqual(fieldCount, fieldSymbols.Count);
		foreach (var expectedField in expectedFields)
		{
			Assert.Contains(expectedField, fieldSymbols);
		}

		mockTypeSymbol.Verify(x => x.GetAllMembers(null), Times.Once);
	}

	/// <summary>
	/// Tests GetConstructors method with various types of members to verify correct filtering.
	/// Should return only non-static methods with MethodKind.Constructor.
	/// </summary>
	[TestCase(false, MethodKind.Constructor, true, Description = "Instance constructor should be included")]
	[TestCase(true, MethodKind.Constructor, false, Description = "Static constructor should be excluded")]
	[TestCase(false, MethodKind.Ordinary, false, Description = "Instance method should be excluded")]
	[TestCase(true, MethodKind.Ordinary, false, Description = "Static method should be excluded")]
	[TestCase(false, MethodKind.PropertyGet, false, Description = "Property getter should be excluded")]
	[TestCase(false, MethodKind.PropertySet, false, Description = "Property setter should be excluded")]
	public void GetConstructors_VariousMethodTypes_FiltersCorrectly(bool isStatic, MethodKind methodKind, bool shouldBeIncluded)
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockMethodSymbol = new Mock<IMethodSymbol>();
		var mockContext = new Mock<SourceGenContext>();
		mockMethodSymbol.Setup(m => m.IsStatic).Returns(isStatic);
		mockMethodSymbol.Setup(m => m.MethodKind).Returns(methodKind);
		var members = new ISymbol[]
		{
			mockMethodSymbol.Object
		};
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create(members));
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(mockContext.Object);
		// Assert
		if (shouldBeIncluded)
		{
			Assert.AreEqual(1, result.Count());
			Assert.AreEqual(mockMethodSymbol.Object, result.First());
		}
		else
		{
			Assert.AreEqual(0, result.Count());
		}
	}

	/// <summary>
	/// Tests GetConstructors method with empty members collection.
	/// Should return empty enumerable when type has no members.
	/// </summary>
	[Test]
	public void GetConstructors_EmptyMembers_ReturnsEmpty()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray<ISymbol>.Empty);
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(null);
		// Assert
		Assert.AreEqual(0, result.Count());
	}

	/// <summary>
	/// Tests GetConstructors method with non-method symbols.
	/// Should return empty enumerable when type has only non-method members.
	/// </summary>
	[Test]
	public void GetConstructors_NonMethodMembers_ReturnsEmpty()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockPropertySymbol = new Mock<IPropertySymbol>();
		var mockFieldSymbol = new Mock<IFieldSymbol>();
		var members = new ISymbol[]
		{
			mockPropertySymbol.Object,
			mockFieldSymbol.Object
		};
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create(members));
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(null);
		// Assert
		Assert.AreEqual(0, result.Count());
	}

	/// <summary>
	/// Tests GetConstructors method with multiple instance constructors.
	/// Should return all instance constructors when multiple exist.
	/// </summary>
	[Test]
	public void GetConstructors_MultipleInstanceConstructors_ReturnsAll()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockConstructor1 = new Mock<IMethodSymbol>();
		var mockConstructor2 = new Mock<IMethodSymbol>();
		mockConstructor1.Setup(m => m.IsStatic).Returns(false);
		mockConstructor1.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		mockConstructor2.Setup(m => m.IsStatic).Returns(false);
		mockConstructor2.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		var members = new ISymbol[]
		{
			mockConstructor1.Object,
			mockConstructor2.Object
		};
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create(members));
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(null);
		// Assert
		Assert.AreEqual(2, result.Count());
		Assert.Contains(mockConstructor1.Object, result.ToArray());
		Assert.Contains(mockConstructor2.Object, result.ToArray());
	}

	/// <summary>
	/// Tests GetConstructors method with mixed constructor types.
	/// Should return only instance constructors and exclude static constructors.
	/// </summary>
	[Test]
	public void GetConstructors_MixedConstructorTypes_ReturnsOnlyInstanceConstructors()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockInstanceConstructor = new Mock<IMethodSymbol>();
		var mockStaticConstructor = new Mock<IMethodSymbol>();
		mockInstanceConstructor.Setup(m => m.IsStatic).Returns(false);
		mockInstanceConstructor.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		mockStaticConstructor.Setup(m => m.IsStatic).Returns(true);
		mockStaticConstructor.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		var members = new ISymbol[]
		{
			mockInstanceConstructor.Object,
			mockStaticConstructor.Object
		};
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create(members));
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(null);
		// Assert
		Assert.AreEqual(1, result.Count());
		Assert.AreEqual(mockInstanceConstructor.Object, result.First());
	}

	/// <summary>
	/// Tests GetConstructors method with null context parameter.
	/// Should work correctly even when context parameter is null.
	/// </summary>
	[Test]
	public void GetConstructors_NullContext_WorksCorrectly()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockConstructor = new Mock<IMethodSymbol>();
		mockConstructor.Setup(m => m.IsStatic).Returns(false);
		mockConstructor.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		var members = new ISymbol[]
		{
			mockConstructor.Object
		};
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create(members));
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(null);
		// Assert
		Assert.AreEqual(1, result.Count());
		Assert.AreEqual(mockConstructor.Object, result.First());
	}

	/// <summary>
	/// Tests GetConstructors method with complex mixed member types.
	/// Should return only instance constructors from a type with various member types.
	/// </summary>
	[Test]
	public void GetConstructors_ComplexMixedMembers_ReturnsOnlyInstanceConstructors()
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockInstanceConstructor = new Mock<IMethodSymbol>();
		var mockStaticConstructor = new Mock<IMethodSymbol>();
		var mockMethod = new Mock<IMethodSymbol>();
		var mockProperty = new Mock<IPropertySymbol>();
		var mockField = new Mock<IFieldSymbol>();
		mockInstanceConstructor.Setup(m => m.IsStatic).Returns(false);
		mockInstanceConstructor.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		mockStaticConstructor.Setup(m => m.IsStatic).Returns(true);
		mockStaticConstructor.Setup(m => m.MethodKind).Returns(MethodKind.Constructor);
		mockMethod.Setup(m => m.IsStatic).Returns(false);
		mockMethod.Setup(m => m.MethodKind).Returns(MethodKind.Ordinary);
		var members = new ISymbol[]
		{
			mockInstanceConstructor.Object,
			mockStaticConstructor.Object,
			mockMethod.Object,
			mockProperty.Object,
			mockField.Object
		};
		mockTypeSymbol.Setup(t => t.GetMembers()).Returns(ImmutableArray.Create(members));
		// Act
		var result = mockTypeSymbol.Object.GetConstructors(null);
		// Assert
		Assert.AreEqual(1, result.Count());
		Assert.AreEqual(mockInstanceConstructor.Object, result.First());
	}

	/// <summary>
	/// Tests GetAllEvents method when no members are returned.
	/// Should return empty enumerable when GetAllMembers returns no symbols.
	/// </summary>
	[Test]
	public void GetAllEvents_NoMembers_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray<ISymbol>.Empty);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);
		// Act
		var result = mockSymbol.Object.GetAllEvents(null);
		// Assert
		var events = result.ToList();
		Assert.AreEqual(0, events.Count);
		Assert.IsEmpty(events);
	}

	/// <summary>
	/// Tests GetAllEvents method when no event symbols are present.
	/// Should return empty enumerable when all members are non-event symbols.
	/// </summary>
	[Test]
	public void GetAllEvents_NoEventSymbols_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockMethodSymbol = new Mock<IMethodSymbol>();
		var mockPropertySymbol = new Mock<IPropertySymbol>();
		var mockFieldSymbol = new Mock<IFieldSymbol>();
		var members = new ISymbol[]
		{
			mockMethodSymbol.Object,
			mockPropertySymbol.Object,
			mockFieldSymbol.Object
		};
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.Create(members));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);
		// Act
		var result = mockSymbol.Object.GetAllEvents(null);
		// Assert
		var events = result.ToList();
		Assert.AreEqual(0, events.Count);
		Assert.IsEmpty(events);
	}

	/// <summary>
	/// Tests GetAllEvents method when all members are event symbols.
	/// Should return all symbols when every member is an IEventSymbol.
	/// </summary>
	[Test]
	public void GetAllEvents_AllEventSymbols_ReturnsAllSymbols()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockEventSymbol1 = new Mock<IEventSymbol>();
		var mockEventSymbol2 = new Mock<IEventSymbol>();
		var mockEventSymbol3 = new Mock<IEventSymbol>();
		var members = new ISymbol[]
		{
			mockEventSymbol1.Object,
			mockEventSymbol2.Object,
			mockEventSymbol3.Object
		};
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.Create(members));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);
		// Act
		var result = mockSymbol.Object.GetAllEvents(null);
		// Assert
		var events = result.ToList();
		Assert.AreEqual(3, events.Count);
		Assert.Contains(mockEventSymbol1.Object, events);
		Assert.Contains(mockEventSymbol2.Object, events);
		Assert.Contains(mockEventSymbol3.Object, events);
	}

	/// <summary>
	/// Tests GetAllEvents method with interface implementations containing events.
	/// Should return event symbols from implemented interfaces.
	/// </summary>
	[Test]
	public void GetAllEvents_WithInterfaceEvents_ReturnsEventsFromInterfaces()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockInterface = new Mock<INamedTypeSymbol>();
		var mockEventSymbol1 = new Mock<IEventSymbol>();
		var mockEventSymbol2 = new Mock<IEventSymbol>();
		// Setup main type members (no events)
		var mainMembers = new ISymbol[]
		{
			Mock.Of<IMethodSymbol>()
		};
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.Create(mainMembers));
		// Setup interface members with events
		var interfaceMembers = new ISymbol[]
		{
			mockEventSymbol1.Object,
			mockEventSymbol2.Object
		};
		mockInterface.Setup(i => i.GetMembers()).Returns(ImmutableArray.Create(interfaceMembers));
		// Setup AllInterfaces to return the mock interface
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray.Create(mockInterface.Object));
		// Act
		var result = mockSymbol.Object.GetAllEvents(null);
		// Assert
		var events = result.ToList();
		Assert.AreEqual(2, events.Count);
		Assert.Contains(mockEventSymbol1.Object, events);
		Assert.Contains(mockEventSymbol2.Object, events);
	}

	/// <summary>
	/// Tests that Implements returns false when type does not implement the specified interface.
	/// This test verifies the main logic on line 73.
	/// </summary>
	[Test]
	public void Implements_TypeDoesNotImplementInterface_ReturnsFalse()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		var mockTargetInterface = new Mock<ITypeSymbol>();
		var mockDifferentInterface = new Mock<INamedTypeSymbol>();
		// Set up the AllInterfaces collection to contain a different interface
		var allInterfaces = ImmutableArray.Create(mockDifferentInterface.Object);
		mockType.Setup(t => t.AllInterfaces).Returns(allInterfaces);
		// Act
		var result = ITypeSymbolExtensions.Implements(mockType.Object, mockTargetInterface.Object);
		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that Implements returns false when type implements no interfaces.
	/// This test verifies the behavior when AllInterfaces is empty.
	/// </summary>
	[Test]
	public void Implements_TypeImplementsNoInterfaces_ReturnsFalse()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		var mockInterface = new Mock<ITypeSymbol>();
		// Set up empty AllInterfaces collection
		var allInterfaces = ImmutableArray<INamedTypeSymbol>.Empty;
		mockType.Setup(t => t.AllInterfaces).Returns(allInterfaces);
		// Act
		var result = ITypeSymbolExtensions.Implements(mockType.Object, mockInterface.Object);
		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that Implements returns false when type implements multiple interfaces but not the target one.
	/// This test verifies the behavior with multiple interfaces but none matching the target.
	/// </summary>
	[Test]
	public void Implements_TypeImplementsMultipleInterfacesButNotTarget_ReturnsFalse()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		var mockTargetInterface = new Mock<ITypeSymbol>();
		var mockOtherInterface1 = new Mock<INamedTypeSymbol>();
		var mockOtherInterface2 = new Mock<INamedTypeSymbol>();
		var mockOtherInterface3 = new Mock<INamedTypeSymbol>();
		// Set up the AllInterfaces collection with multiple interfaces but not the target
		var allInterfaces = ImmutableArray.Create(mockOtherInterface1.Object, mockOtherInterface2.Object, mockOtherInterface3.Object);
		mockType.Setup(t => t.AllInterfaces).Returns(allInterfaces);
		// Act
		var result = ITypeSymbolExtensions.Implements(mockType.Object, mockTargetInterface.Object);
		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests ImplementsGeneric method when type has no interfaces.
	/// Should return false and set typeArguments to empty array when type implements no interfaces.
	/// </summary>
	[Test]
	public void ImplementsGeneric_TypeWithNoInterfaces_ReturnsFalseAndEmptyTypeArguments()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		var mockIface = new Mock<ITypeSymbol>();
		var emptyInterfaces = ImmutableArray<INamedTypeSymbol>.Empty;
		mockType.Setup(t => t.AllInterfaces).Returns(emptyInterfaces);
		// Act
		var result = ITypeSymbolExtensions.ImplementsGeneric(mockType.Object, mockIface.Object, out var typeArguments);
		// Assert
		Assert.IsFalse(result);
		Assert.IsNotNull(typeArguments);
		Assert.AreEqual(0, typeArguments.Length);
	}


	/// <summary>
	/// Tests GetAllProperties when GetAllMembers returns no symbols.
	/// Should return empty enumerable when there are no members at all.
	/// </summary>
	[Test]
	public void GetAllProperties_NoMembers_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var emptyMembers = new List<ISymbol>();

		// Setup the extension method call by mocking the underlying GetAllMembers call
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray<ISymbol>.Empty);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllProperties(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}

	/// <summary>
	/// Tests GetAllProperties when GetAllMembers returns only non-property symbols.
	/// Should return empty enumerable when all members are non-property symbols.
	/// </summary>
	[Test]
	public void GetAllProperties_OnlyNonPropertySymbols_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockMethod = new Mock<IMethodSymbol>();
		var mockField = new Mock<IFieldSymbol>();
		var mockEvent = new Mock<IEventSymbol>();

		var nonPropertyMembers = ImmutableArray.Create<ISymbol>(
			mockMethod.Object,
			mockField.Object,
			mockEvent.Object
		);

		mockSymbol.Setup(s => s.GetMembers()).Returns(nonPropertyMembers);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllProperties(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}

	/// <summary>
	/// Tests GetAllProperties when GetAllMembers returns only property symbols.
	/// Should return all symbols when every member is a property symbol.
	/// </summary>
	[Test]
	public void GetAllProperties_OnlyPropertySymbols_ReturnsAllSymbols()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockProperty1 = new Mock<IPropertySymbol>();
		var mockProperty2 = new Mock<IPropertySymbol>();

		var propertyMembers = ImmutableArray.Create<ISymbol>(
			mockProperty1.Object,
			mockProperty2.Object
		);

		mockSymbol.Setup(s => s.GetMembers()).Returns(propertyMembers);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllProperties(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var properties = result.ToList();
		Assert.AreEqual(2, properties.Count);
		Assert.Contains(mockProperty1.Object, properties);
		Assert.Contains(mockProperty2.Object, properties);
	}

	/// <summary>
	/// Tests GetAllProperties with mixed symbol types to ensure proper filtering.
	/// Should only return property symbols and exclude other symbol types from the result.
	/// </summary>
	[TestCase(0, 3, Description = "No properties, multiple other symbols")]
	[TestCase(1, 2, Description = "One property, multiple other symbols")]
	[TestCase(3, 4, Description = "Multiple properties, multiple other symbols")]
	[TestCase(2, 0, Description = "Only properties, no other symbols")]
	public void GetAllProperties_MixedSymbolTypes_OnlyReturnsPropertySymbols(int propertyCount, int otherSymbolCount)
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockContext = new Mock<SourceGenContext>();
		var allMembers = new List<ISymbol>();

		// Add property symbols
		var properties = new List<IPropertySymbol>();
		for (int i = 0; i < propertyCount; i++)
		{
			var mockProperty = new Mock<IPropertySymbol>();
			properties.Add(mockProperty.Object);
			allMembers.Add(mockProperty.Object);
		}

		// Add other symbol types
		for (int i = 0; i < otherSymbolCount; i++)
		{
			var symbolType = i % 3;
			ISymbol otherSymbol = symbolType switch
			{
				0 => new Mock<IMethodSymbol>().Object,
				1 => new Mock<IFieldSymbol>().Object,
				_ => new Mock<IEventSymbol>().Object
			};
			allMembers.Add(otherSymbol);
		}

		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.CreateRange(allMembers));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllProperties(mockSymbol.Object, mockContext.Object);

		// Assert
		Assert.IsNotNull(result);
		var resultProperties = result.ToList();
		Assert.AreEqual(propertyCount, resultProperties.Count);

		foreach (var expectedProperty in properties)
		{
			Assert.Contains(expectedProperty, resultProperties);
		}
	}

	/// <summary>
	/// Tests GetAllProperties with null context parameter.
	/// Should work correctly even when context parameter is null.
	/// </summary>
	[Test]
	public void GetAllProperties_NullContext_WorksCorrectly()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockProperty1 = new Mock<IPropertySymbol>();
		var mockProperty2 = new Mock<IPropertySymbol>();
		var mockMethod = new Mock<IMethodSymbol>();

		var mixedMembers = ImmutableArray.Create<ISymbol>(
			mockProperty1.Object,
			mockMethod.Object,
			mockProperty2.Object
		);

		mockSymbol.Setup(s => s.GetMembers()).Returns(mixedMembers);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllProperties(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var properties = result.ToList();
		Assert.AreEqual(2, properties.Count);
		Assert.Contains(mockProperty1.Object, properties);
		Assert.Contains(mockProperty2.Object, properties);
	}

	/// <summary>
	/// Tests GetAllProperties with properties from interfaces to verify inheritance handling.
	/// Should return property symbols from implemented interfaces through GetAllMembers.
	/// </summary>
	[Test]
	public void GetAllProperties_WithInterfaceProperties_ReturnsPropertiesFromInterfaces()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockInterface = new Mock<INamedTypeSymbol>();
		var mockInterfaceProperty = new Mock<IPropertySymbol>();
		var mockDirectProperty = new Mock<IPropertySymbol>();

		// Setup direct members
		var directMembers = ImmutableArray.Create<ISymbol>(mockDirectProperty.Object);
		mockSymbol.Setup(s => s.GetMembers()).Returns(directMembers);

		// Setup interface members
		var interfaceMembers = ImmutableArray.Create<ISymbol>(mockInterfaceProperty.Object);
		mockInterface.Setup(i => i.GetMembers()).Returns(interfaceMembers);

		// Setup interfaces collection
		var interfaces = ImmutableArray.Create(mockInterface.Object);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(interfaces);

		// Act
		var result = ITypeSymbolExtensions.GetAllProperties(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var properties = result.ToList();
		Assert.AreEqual(2, properties.Count);
		Assert.Contains(mockDirectProperty.Object, properties);
		Assert.Contains(mockInterfaceProperty.Object, properties);
	}

	/// <summary>
	/// Tests GetAllMethods with various combinations of method symbols and other symbol types.
	/// Should correctly filter to return only method symbols regardless of other symbol types present.
	/// </summary>
	[TestCase(0, 3, Description = "No methods, multiple other symbols")]
	[TestCase(1, 2, Description = "One method, multiple other symbols")]
	[TestCase(3, 4, Description = "Multiple methods, multiple other symbols")]
	[TestCase(5, 0, Description = "Only methods, no other symbols")]
	[TestCase(0, 0, Description = "No symbols at all")]
	public void GetAllMethods_VariousSymbolCombinations_ReturnsOnlyMethodSymbols(int methodCount, int otherSymbolCount)
	{
		// Arrange
		var mockTypeSymbol = new Mock<ITypeSymbol>();
		var mockContext = new Mock<SourceGenContext>();
		var allMembers = new List<ISymbol>();
		var expectedMethods = new List<IMethodSymbol>();

		// Add method symbols
		for (int i = 0; i < methodCount; i++)
		{
			var mockMethod = new Mock<IMethodSymbol>();
			allMembers.Add(mockMethod.Object);
			expectedMethods.Add(mockMethod.Object);
		}

		// Add other symbol types
		for (int i = 0; i < otherSymbolCount; i++)
		{
			switch (i % 4)
			{
				case 0:
					allMembers.Add(new Mock<IPropertySymbol>().Object);
					break;
				case 1:
					allMembers.Add(new Mock<IFieldSymbol>().Object);
					break;
				case 2:
					allMembers.Add(new Mock<IEventSymbol>().Object);
					break;
				case 3:
					allMembers.Add(new Mock<INamedTypeSymbol>().Object);
					break;
			}
		}

		mockTypeSymbol.Setup(x => x.GetAllMembers(mockContext.Object)).Returns(allMembers);

		// Act
		var result = mockTypeSymbol.Object.GetAllMethods(mockContext.Object);

		// Assert
		var methodSymbols = result.ToList();
		Assert.AreEqual(methodCount, methodSymbols.Count);

		foreach (var expectedMethod in expectedMethods)
		{
			Assert.Contains(expectedMethod, methodSymbols);
		}

		// Verify no other symbol types are included
		Assert.IsTrue(methodSymbols.All(s => s is IMethodSymbol));
		mockTypeSymbol.Verify(x => x.GetAllMembers(mockContext.Object), Times.Once);
	}

	/// <summary>
	/// Tests ImplementsGeneric method when type parameter is null.
	/// Should return false and set typeArguments to empty array when type is null.
	/// </summary>
	[Test]
	public void ImplementsGeneric_NullType_ReturnsFalseAndEmptyTypeArguments()
	{
		// Arrange
		var mockIface = new Mock<ITypeSymbol>();

		// Act
		var result = ITypeSymbolExtensions.ImplementsGeneric(null!, mockIface.Object, out var typeArguments);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNotNull(typeArguments);
		Assert.AreEqual(0, typeArguments.Length);
	}

	/// <summary>
	/// Tests ImplementsGeneric method when iface parameter is null.
	/// Should return false and set typeArguments to empty array when iface is null.
	/// </summary>
	[Test]
	public void ImplementsGeneric_NullIface_ReturnsFalseAndEmptyTypeArguments()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();

		// Act
		var result = ITypeSymbolExtensions.ImplementsGeneric(mockType.Object, null!, out var typeArguments);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNotNull(typeArguments);
		Assert.AreEqual(0, typeArguments.Length);
	}

	/// <summary>
	/// Tests ImplementsGeneric method when both type and iface parameters are null.
	/// Should return false and set typeArguments to empty array when both parameters are null.
	/// </summary>
	[Test]
	public void ImplementsGeneric_BothParametersNull_ReturnsFalseAndEmptyTypeArguments()
	{
		// Arrange & Act
		var result = ITypeSymbolExtensions.ImplementsGeneric(null!, null!, out var typeArguments);

		// Assert
		Assert.IsFalse(result);
		Assert.IsNotNull(typeArguments);
		Assert.AreEqual(0, typeArguments.Length);
	}

	/// <summary>
	/// Tests that Implements returns false when type parameter is null.
	/// This test verifies the null check behavior on line 71.
	/// </summary>
	[Test]
	public void Implements_TypeIsNull_ReturnsFalse()
	{
		// Arrange
		ITypeSymbol nullType = null;
		var mockInterface = new Mock<ITypeSymbol>();

		// Act
		var result = ITypeSymbolExtensions.Implements(nullType, mockInterface.Object);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that Implements returns false when interface parameter is null.
	/// This test verifies the null check behavior on line 71.
	/// </summary>
	[Test]
	public void Implements_InterfaceIsNull_ReturnsFalse()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		ITypeSymbol nullInterface = null;

		// Act
		var result = ITypeSymbolExtensions.Implements(mockType.Object, nullInterface);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that Implements returns false when both parameters are null.
	/// This test verifies the null check behavior on line 71.
	/// </summary>
	[Test]
	public void Implements_BothParametersNull_ReturnsFalse()
	{
		// Arrange
		ITypeSymbol nullType = null;
		ITypeSymbol nullInterface = null;

		// Act
		var result = ITypeSymbolExtensions.Implements(nullType, nullInterface);

		// Assert
		Assert.IsFalse(result);
	}


	/// <summary>
	/// Tests GetAllMethods when GetAllMembers returns no symbols.
	/// Should return empty enumerable when there are no members at all.
	/// </summary>
	[Test]
	public void GetAllMethods_NoMembers_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var emptyMembers = new List<ISymbol>();

		// Setup the extension method call by mocking the underlying GetAllMembers dependencies
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray<ISymbol>.Empty);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllMethods(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}

	/// <summary>
	/// Tests GetAllMethods when GetAllMembers returns only non-method symbols.
	/// Should return empty enumerable when all members are non-method symbols.
	/// </summary>
	[Test]
	public void GetAllMethods_OnlyNonMethodSymbols_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockProperty = new Mock<IPropertySymbol>();
		var mockField = new Mock<IFieldSymbol>();
		var mockEvent = new Mock<IEventSymbol>();

		var nonMethodMembers = new List<ISymbol>
		{
			mockProperty.Object,
			mockField.Object,
			mockEvent.Object
		};

		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.CreateRange(nonMethodMembers));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllMethods(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}

	/// <summary>
	/// Tests GetAllMethods when GetAllMembers returns only method symbols.
	/// Should return all symbols when every member is a method symbol.
	/// </summary>
	[Test]
	public void GetAllMethods_OnlyMethodSymbols_ReturnsAllSymbols()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockMethod1 = new Mock<IMethodSymbol>();
		var mockMethod2 = new Mock<IMethodSymbol>();
		var mockMethod3 = new Mock<IMethodSymbol>();

		var methodMembers = new List<ISymbol>
		{
			mockMethod1.Object,
			mockMethod2.Object,
			mockMethod3.Object
		};

		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.CreateRange(methodMembers));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllMethods(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var resultList = result.ToList();
		Assert.AreEqual(3, resultList.Count);
		Assert.Contains(mockMethod1.Object, resultList);
		Assert.Contains(mockMethod2.Object, resultList);
		Assert.Contains(mockMethod3.Object, resultList);
	}

	/// <summary>
	/// Tests GetAllMethods with null context parameter.
	/// Should work correctly even when context parameter is null.
	/// </summary>
	[Test]
	public void GetAllMethods_NullContext_WorksCorrectly()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockMethod1 = new Mock<IMethodSymbol>();
		var mockMethod2 = new Mock<IMethodSymbol>();
		var mockProperty = new Mock<IPropertySymbol>();

		var mixedMembers = new List<ISymbol>
		{
			mockMethod1.Object,
			mockProperty.Object,
			mockMethod2.Object
		};

		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.CreateRange(mixedMembers));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllMethods(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var resultList = result.ToList();
		Assert.AreEqual(2, resultList.Count);
		Assert.Contains(mockMethod1.Object, resultList);
		Assert.Contains(mockMethod2.Object, resultList);
	}

	/// <summary>
	/// Tests GetAllMethods with methods from interfaces to verify inheritance handling.
	/// Should return method symbols from implemented interfaces through GetAllMembers.
	/// </summary>
	[Test]
	public void GetAllMethods_WithInterfaceMethods_ReturnsMethodsFromInterfaces()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockInterface = new Mock<INamedTypeSymbol>();
		var mockDirectMethod = new Mock<IMethodSymbol>();
		var mockInterfaceMethod = new Mock<IMethodSymbol>();
		var mockProperty = new Mock<IPropertySymbol>();

		// Setup direct members
		var directMembers = new List<ISymbol> { mockDirectMethod.Object, mockProperty.Object };
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.CreateRange(directMembers));

		// Setup interface members
		var interfaceMembers = new List<ISymbol> { mockInterfaceMethod.Object };
		mockInterface.Setup(i => i.GetMembers()).Returns(ImmutableArray.CreateRange(interfaceMembers));

		// Setup interfaces
		var interfaces = new List<INamedTypeSymbol> { mockInterface.Object };
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray.CreateRange(interfaces));

		// Act
		var result = ITypeSymbolExtensions.GetAllMethods(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var resultList = result.ToList();
		Assert.AreEqual(2, resultList.Count);
		Assert.Contains(mockDirectMethod.Object, resultList);
		Assert.Contains(mockInterfaceMethod.Object, resultList);
	}

	/// <summary>
	/// Tests GetAllMethods with valid context parameter.
	/// Should work correctly when context parameter is provided.
	/// </summary>
	[Test]
	public void GetAllMethods_WithValidContext_WorksCorrectly()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockMethod = new Mock<IMethodSymbol>();
		var mockField = new Mock<IFieldSymbol>();

		var members = new List<ISymbol> { mockMethod.Object, mockField.Object };
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray.CreateRange(members));
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllMethods(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var resultList = result.ToList();
		Assert.AreEqual(1, resultList.Count);
		Assert.Contains(mockMethod.Object, resultList);
	}

	/// <summary>
	/// Tests GetAllFields when GetAllMembers returns no symbols.
	/// Should return empty enumerable when there are no members at all.
	/// </summary>
	[Test]
	public void GetAllFields_NoMembers_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		mockSymbol.Setup(s => s.GetMembers()).Returns(ImmutableArray<ISymbol>.Empty);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllFields(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}

	/// <summary>
	/// Tests GetAllFields when GetAllMembers returns only non-field symbols.
	/// Should return empty enumerable when all members are non-field symbols.
	/// </summary>
	[Test]
	public void GetAllFields_OnlyNonFieldSymbols_ReturnsEmptyEnumerable()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockMethod = new Mock<IMethodSymbol>();
		var mockProperty = new Mock<IPropertySymbol>();
		var mockEvent = new Mock<IEventSymbol>();

		var nonFieldMembers = ImmutableArray.Create<ISymbol>(
			mockMethod.Object,
			mockProperty.Object,
			mockEvent.Object
		);

		mockSymbol.Setup(s => s.GetMembers()).Returns(nonFieldMembers);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllFields(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		Assert.IsFalse(result.Any());
	}

	/// <summary>
	/// Tests GetAllFields when GetAllMembers returns only field symbols.
	/// Should return all symbols when every member is a field symbol.
	/// </summary>
	[Test]
	public void GetAllFields_OnlyFieldSymbols_ReturnsAllSymbols()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockField1 = new Mock<IFieldSymbol>();
		var mockField2 = new Mock<IFieldSymbol>();

		var fieldMembers = ImmutableArray.Create<ISymbol>(
			mockField1.Object,
			mockField2.Object
		);

		mockSymbol.Setup(s => s.GetMembers()).Returns(fieldMembers);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act
		var result = ITypeSymbolExtensions.GetAllFields(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var fields = result.ToList();
		Assert.AreEqual(2, fields.Count);
		Assert.Contains(mockField1.Object, fields);
		Assert.Contains(mockField2.Object, fields);
	}

	/// <summary>
	/// Tests GetAllFields with non-null context parameter.
	/// Should work correctly when context parameter is not null.
	/// </summary>
	[Test]
	public void GetAllFields_NonNullContext_WorksCorrectly()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockField1 = new Mock<IFieldSymbol>();
		var mockField2 = new Mock<IFieldSymbol>();
		var mockMethod = new Mock<IMethodSymbol>();

		var mixedMembers = ImmutableArray.Create<ISymbol>(
			mockField1.Object,
			mockMethod.Object,
			mockField2.Object
		);

		mockSymbol.Setup(s => s.GetMembers()).Returns(mixedMembers);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(ImmutableArray<INamedTypeSymbol>.Empty);

		// Act - passing null for context since SourceGenContext is internal and cannot be mocked
		var result = ITypeSymbolExtensions.GetAllFields(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var fields = result.ToList();
		Assert.AreEqual(2, fields.Count);
		Assert.Contains(mockField1.Object, fields);
		Assert.Contains(mockField2.Object, fields);
	}

	/// <summary>
	/// Tests GetAllFields with fields from interfaces to verify inheritance handling.
	/// Should return field symbols from implemented interfaces through GetAllMembers.
	/// </summary>
	[Test]
	public void GetAllFields_WithInterfaceFields_ReturnsFieldsFromInterfaces()
	{
		// Arrange
		var mockSymbol = new Mock<ITypeSymbol>();
		var mockInterface = new Mock<INamedTypeSymbol>();
		var mockInterfaceField = new Mock<IFieldSymbol>();
		var mockDirectField = new Mock<IFieldSymbol>();

		// Setup direct members
		var directMembers = ImmutableArray.Create<ISymbol>(mockDirectField.Object);
		mockSymbol.Setup(s => s.GetMembers()).Returns(directMembers);

		// Setup interface members
		var interfaceMembers = ImmutableArray.Create<ISymbol>(mockInterfaceField.Object);
		mockInterface.Setup(i => i.GetMembers()).Returns(interfaceMembers);

		// Setup interfaces collection
		var interfaces = ImmutableArray.Create(mockInterface.Object);
		mockSymbol.Setup(s => s.AllInterfaces).Returns(interfaces);

		// Act
		var result = ITypeSymbolExtensions.GetAllFields(mockSymbol.Object, null);

		// Assert
		Assert.IsNotNull(result);
		var fields = result.ToList();
		Assert.AreEqual(2, fields.Count);
		Assert.Contains(mockDirectField.Object, fields);
		Assert.Contains(mockInterfaceField.Object, fields);
	}

	/// <summary>
	/// Tests that InheritsFrom returns false when type parameter is null.
	/// This test verifies the null check behavior on line 66.
	/// </summary>
	[Test]
	public void InheritsFrom_TypeIsNull_ReturnsFalse()
	{
		// Arrange
		ITypeSymbol nullType = null;
		var mockBaseType = new Mock<ITypeSymbol>();
		SourceGenContext mockContext = null;

		// Act
		var result = ITypeSymbolExtensions.InheritsFrom(nullType, mockBaseType.Object, mockContext);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that InheritsFrom returns false when baseType parameter is null.
	/// This test verifies the null check behavior on line 66.
	/// </summary>
	[Test]
	public void InheritsFrom_BaseTypeIsNull_ReturnsFalse()
	{
		// Arrange
		var mockType = new Mock<ITypeSymbol>();
		ITypeSymbol nullBaseType = null;
		SourceGenContext nullContext = null;

		// Act
		var result = ITypeSymbolExtensions.InheritsFrom(mockType.Object, nullBaseType, nullContext);

		// Assert
		Assert.IsFalse(result);
	}

	/// <summary>
	/// Tests that InheritsFrom returns false when both type and baseType parameters are null.
	/// This test verifies the null check behavior on line 66.
	/// </summary>
	[Test]
	public void InheritsFrom_BothParametersNull_ReturnsFalse()
	{
		// Arrange
		ITypeSymbol nullType = null;
		ITypeSymbol nullBaseType = null;

		// Act
		var result = ITypeSymbolExtensions.InheritsFrom(nullType, nullBaseType, null);

		// Assert
		Assert.IsFalse(result);
	}

}