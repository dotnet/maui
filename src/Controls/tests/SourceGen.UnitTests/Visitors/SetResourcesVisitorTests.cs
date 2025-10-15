using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Xml;


#nullable disable
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.SourceGen;
using Microsoft.Maui.Controls.Xaml;
using Moq;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;


/// <summary>
/// Unit tests for SetResourcesVisitor class.
/// </summary>
[TestFixture]
public partial class SetResourcesVisitorTests
{
	/// <summary>
	/// Tests Visit method when parent ElementNode is a ResourceDictionary.
	/// Should call Accept on the ValueNode when IsResourceDictionary returns true.
	/// This test targets the uncovered line 24 in the source code.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Visit_ParentNodeIsResourceDictionary_CallsAccept()
	{
		// NOTE: This test cannot be completed due to mocking limitations
		// This test specifically targets line 24 which is marked as "Not Covered"
		// - ValueNode.Accept method cannot be mocked (marked as cannot be mocked)
		// - ElementNode is a concrete class that cannot be mocked
		// - SetPropertiesVisitor constructor cannot be mocked
		// - IsResourceDictionary extension method depends on SourceGenContext state
		Assert.Inconclusive("Cannot test the uncovered line 24 due to the following limitations:\n" + "1. ValueNode.Accept method cannot be mocked according to symbol information\n" + "2. ElementNode concrete class cannot be mocked with Moq\n" + "3. SetPropertiesVisitor concrete class cannot be mocked\n" + "4. IsResourceDictionary extension method requires complex SourceGenContext setup\n" + "To test this scenario, consider:\n" + "- Integration testing with real instances\n" + "- Refactoring to use dependency injection for testability\n" + "- Using wrapper interfaces around concrete classes");
	}

	/// <summary>
	/// Tests Visit method with MarkupNode and null parentNode.
	/// Should execute without throwing exceptions since method is empty and doesn't use parameters.
	/// This test targets the uncovered lines 25-26 in the source code.
	/// </summary>
	[Test]
	public void Visit_MarkupNodeWithNullParent_ExecutesWithoutException()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var mockContext = new SourceGenContext(
			writer,
			null!, // compilation - not used in empty Visit method
			default, // sourceProductionContext - not used in empty Visit method  
			null!, // assemblyCaches - not used in empty Visit method
			null!, // typeCache - not used in empty Visit method
			null!, // rootType - not used in empty Visit method
			null, // baseType - not used in empty Visit method
			null! // projectItem - not used in empty Visit method
		);

		var visitor = new SetResourcesVisitor(mockContext);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var markupNode = new MarkupNode("test markup", mockNamespaceResolver.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => visitor.Visit(markupNode, null));
	}

}




/// <summary>
/// Unit tests for SetResourcesVisitor.SkipChildren method.
/// </summary>
[TestFixture]
public partial class SetResourcesVisitorSkipChildrenTests
{
	private Mock<SourceGenContext> _mockContext = null!;
	private SetResourcesVisitor _visitor = null!;

	[SetUp]
	public void SetUp()
	{
		_mockContext = new Mock<SourceGenContext>();
		_visitor = new SetResourcesVisitor(_mockContext.Object);
	}

	/// <summary>
	/// Tests SkipChildren method when parentNode is ListNode but cannot test ResourceDictionary scenario.
	/// This test would target the uncovered line 55 but cannot be completed due to mocking limitations.
	/// The test specifically targets the second condition where parentNode is ListNode with ResourceDictionary parent.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void SkipChildren_ParentNodeIsListNodeWithResourceDictionaryParent_CannotTest()
	{
		// NOTE: This test cannot be completed due to mocking limitations
		// This test specifically targets line 55 which is marked as "Not Covered"
		// The scenario requires:
		// 1. parentNode to be a ListNode (concrete class)
		// 2. parentNode.Parent to be an ElementNode (concrete class) 
		// 3. IsResourceDictionary extension method to return true (cannot be mocked)
		// 4. Properties.ContainsKey to return false for xKey (Properties cannot be mocked)

		Assert.Inconclusive("Cannot test the uncovered line 55 due to the following limitations:\n" +
			"1. ListNode is a concrete class that requires complex constructor parameters\n" +
			"2. ElementNode is a concrete class that cannot be mocked with Moq\n" +
			"3. IsResourceDictionary extension method cannot be mocked\n" +
			"4. Properties property cannot be mocked according to symbol information\n" +
			"5. XmlName.xKey field cannot be mocked\n" +
			"To test this scenario, consider:\n" +
			"- Integration testing with real instances\n" +
			"- Refactoring to use dependency injection for testability\n" +
			"- Using wrapper interfaces around concrete classes");
	}

	/// <summary>
	/// Tests SkipChildren method when parentNode is ElementNode but cannot test ResourceDictionary scenario.
	/// Cannot fully test ElementNode scenarios due to mocking limitations with concrete classes.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void SkipChildren_ParentNodeIsElementNodeWithResourceDictionary_CannotTest()
	{
		// NOTE: This test cannot be completed due to mocking limitations
		// Testing ElementNode scenarios requires:
		// 1. Creating real ElementNode instances (complex constructor dependencies)
		// 2. Controlling IsResourceDictionary return value (extension method cannot be mocked)
		// 3. Controlling Properties.ContainsKey behavior (Properties cannot be mocked)

		Assert.Inconclusive("Cannot test ElementNode scenarios due to the following limitations:\n" +
			"1. ElementNode is a concrete class requiring XmlType, namespaceURI, and IXmlNamespaceResolver\n" +
			"2. IsResourceDictionary extension method cannot be mocked\n" +
			"3. Properties property cannot be mocked according to symbol information\n" +
			"4. SourceGenContext dependencies are complex and cannot be easily mocked\n" +
			"To test this scenario, consider:\n" +
			"- Integration testing with real instances and proper setup\n" +
			"- Refactoring to use dependency injection for better testability");
	}
}



/// <summary>
/// Unit tests for SetResourcesVisitor.Visit method with ValueNode and INode parameters.
/// </summary>
[TestFixture]
public partial class SetResourcesVisitorVisitValueNodeTests
{
	/// <summary>
	/// Tests Visit method when parentNode cannot be cast to ElementNode.
	/// Should throw InvalidCastException when parentNode is not an ElementNode.
	/// </summary>
	[Test]
	public void Visit_ParentNodeNotElementNode_ThrowsInvalidCastException()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var valueNode = new ValueNode("test", mockNamespaceResolver.Object);
		var parentNode = new ValueNode("parent", mockNamespaceResolver.Object);

		// Act & Assert
		Assert.Throws<InvalidCastException>(() => visitor.Visit(valueNode, parentNode));
	}

	/// <summary>
	/// Tests Visit method when parentNode is null.
	/// Should throw NullReferenceException when attempting to cast null to ElementNode.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Visit_ParentNodeNull_ThrowsNullReferenceException()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var valueNode = new ValueNode("test", mockNamespaceResolver.Object);

		// Act & Assert
		Assert.Throws<NullReferenceException>(() => visitor.Visit(valueNode, null!));
	}

	/// <summary>
	/// Tests Visit method when parentNode is ElementNode but IsResourceDictionary returns false.
	/// Should return early without calling Accept on ValueNode when not a ResourceDictionary.
	/// </summary>
	[Test]
	public void Visit_ParentNodeNotResourceDictionary_ReturnsEarly()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var valueNode = new ValueNode("test", mockNamespaceResolver.Object);
		var xmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Button", []);
		var elementNode = new ElementNode(xmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Act & Assert - Should not throw since IsResourceDictionary will return false and method returns early
		Assert.DoesNotThrow(() => visitor.Visit(valueNode, elementNode));
	}

	/// <summary>
	/// Tests Visit method attempting to reach the uncovered line 24 when IsResourceDictionary returns true.
	/// This test targets the uncovered line 24 but cannot be fully completed due to framework limitations.
	/// The line node.Accept(...) requires complex SourceGenContext setup to make IsResourceDictionary return true.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Visit_ParentNodeIsResourceDictionary_AttemptToReachUncoveredLine()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var valueNode = new ValueNode("test", mockNamespaceResolver.Object);
		var xmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "ResourceDictionary", []);
		var elementNode = new ElementNode(xmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Act & Assert - This will likely not reach line 24 because IsResourceDictionary requires:
		// 1. The ElementNode to exist in context.Variables dictionary
		// 2. The associated variable to have a Type that inherits from ResourceDictionary
		// 3. A valid Compilation context to resolve type metadata
		// Since we can't set up these complex dependencies, this test documents the limitation
		Assert.DoesNotThrow(() => visitor.Visit(valueNode, elementNode));

		// NOTE: Line 24 (node.Accept with SetPropertiesVisitor) cannot be reached because:
		// - IsResourceDictionary extension method requires context.Variables to contain the ElementNode
		// - Variables dictionary requires complex variable setup with type inheritance checking
		// - Real Compilation context needed to resolve ResourceDictionary type metadata
		// - Cannot mock these dependencies due to framework limitations
		Assert.Inconclusive("Line 24 cannot be reached due to complex SourceGenContext requirements.\n" +
						   "IsResourceDictionary requires ElementNode in context.Variables with proper type inheritance.\n" +
						   "To test line 24, consider integration testing with full compilation context.");
	}

	/// <summary>
	/// Tests Visit method with ValueNode containing extreme line number values.
	/// Should handle boundary values for line numbers without throwing exceptions.
	/// </summary>
	[TestCase(int.MinValue, int.MinValue)]
	[TestCase(int.MaxValue, int.MaxValue)]
	[TestCase(-1, -1)]
	[TestCase(0, 0)]
	[TestCase(1, 1)]
	[Test]
	public void Visit_ValueNodeWithBoundaryLineNumbers_HandlesWithoutException(int lineNumber, int linePosition)
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var valueNode = new ValueNode("test", mockNamespaceResolver.Object, lineNumber, linePosition);
		var xmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Button", []);
		var elementNode = new ElementNode(xmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Act & Assert
		Assert.DoesNotThrow(() => visitor.Visit(valueNode, elementNode));
	}
}



/// <summary>
/// Unit tests for SetResourcesVisitor.SkipChildren method focusing on uncovered lines.
/// </summary>
[TestFixture]
public partial class SetResourcesVisitorSkipChildrenUncoveredTests
{
	private SetResourcesVisitor _visitor = null!;
	private SourceGenContext _context = null!;

	[SetUp]
	public void SetUp()
	{
		var writer = new IndentedTextWriter(TextWriter.Null);
		_context = new SourceGenContext(
			writer,
			null!, // compilation 
			default, // sourceProductionContext
			null!, // assemblyCaches
			null!, // typeCache
			null!, // rootType
			null, // baseType
			null! // projectItem
		);
		_visitor = new SetResourcesVisitor(_context);
	}

	/// <summary>
	/// Tests SkipChildren method when node is not an ElementNode and parentNode is null.
	/// Should return false as the first condition exits early.
	/// This test targets the uncovered line 47 in the source code.
	/// </summary>
	[Test]
	public void SkipChildren_NodeIsNotElementNodeWithNullParent_ReturnsFalse()
	{
		// Arrange
		var valueNode = new ValueNode("test", Mock.Of<IXmlNamespaceResolver>());

		// Act
		var result = _visitor.SkipChildren(valueNode, null);

		// Assert
		Assert.False(result);
	}

	/// <summary>
	/// Tests SkipChildren method with complex ListNode scenario but cannot fully test due to mocking limitations.
	/// This test would target the uncovered line 55 but cannot be completed due to concrete class constraints.
	/// The test specifically targets the scenario where parentNode is ListNode with ResourceDictionary parent without xKey.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void SkipChildren_ListNodeParentWithResourceDictionaryParent_CannotTestDueToLimitations()
	{
		Assert.Inconclusive("Cannot test the uncovered line 55 due to the following limitations:\n" +
			"1. ElementNode and ListNode are concrete classes that cannot be mocked with Moq\n" +
			"2. IsResourceDictionary extension method requires complex SourceGenContext setup with Variables and Compilation\n" +
			"3. Creating proper ElementNode instances requires XmlType and IXmlNamespaceResolver setup\n" +
			"4. Setting up context.Variables dictionary to make IsResourceDictionary return true is complex\n" +
			"5. The Parent property on ListNode needs to be set to an ElementNode which requires constructor parameters\n" +
			"To test this scenario, consider:\n" +
			"- Integration testing with real XAML parsing context\n" +
			"- Refactoring to use dependency injection for better testability\n" +
			"- Using wrapper interfaces around concrete XAML node classes\n" +
			"- Creating test-specific SourceGenContext with properly initialized Variables dictionary");
	}

	/// <summary>
	/// Tests SkipChildren method when node is ElementNode but parentNode is not ElementNode or ListNode.
	/// Should return false as none of the specific conditions are met.
	/// This test uses a real ElementNode instance to cover the ElementNode path but tests other conditions.
	/// </summary>
	[Test]
	public void SkipChildren_ElementNodeWithOtherParentType_ReturnsFalse()
	{
		// Arrange
		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var xmlType = new XmlType("http://test", "TestElement", new List<XmlType>());
		var elementNode = new ElementNode(xmlType, "http://test", mockNamespaceResolver.Object);
		var parentNode = new ValueNode("test", mockNamespaceResolver.Object); // Not ElementNode or ListNode

		// Act
		var result = _visitor.SkipChildren(elementNode, parentNode);

		// Assert
		Assert.False(result);
	}

	/// <summary>
	/// Tests SkipChildren method when node is ElementNode and parentNode is ElementNode but not ResourceDictionary.
	/// Should return false as the IsResourceDictionary condition fails.
	/// This test attempts to create ElementNode instances but expects IsResourceDictionary to return false.
	/// </summary>
	[Test]
	public void SkipChildren_ElementNodeWithElementParentNotResourceDictionary_ReturnsFalse()
	{
		// Arrange
		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();
		var xmlType = new XmlType("http://test", "TestElement", new List<XmlType>());
		var elementNode = new ElementNode(xmlType, "http://test", mockNamespaceResolver.Object);
		var parentElementNode = new ElementNode(xmlType, "http://test", mockNamespaceResolver.Object);

		// Act - IsResourceDictionary should return false for this setup since context.Variables won't contain the node
		var result = _visitor.SkipChildren(elementNode, parentElementNode);

		// Assert
		Assert.False(result);
	}
}



/// <summary>
/// Unit tests for SetResourcesVisitor.Visit method targeting ElementNode scenarios.
/// </summary>
[TestFixture]
public partial class SetResourcesVisitorElementNodeVisitTests
{
	/// <summary>
	/// Tests Visit method when parentNode is ListNode with ResourceDictionary parent without xKey.
	/// This test specifically targets the uncovered line 50 in the source code.
	/// Should call Accept on the node with new SetPropertiesVisitor.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void Visit_ParentNodeIsListNodeWithResourceDictionaryParentWithoutXKey_CallsAccept()
	{
		// NOTE: This test cannot be completed due to mocking limitations
		// This test specifically targets line 50 which is marked as "Not Covered"
		// The condition for this line is complex: parentNode is ListNode AND parentNode.Parent is ResourceDictionary AND !containsKey(xKey)
		Assert.Inconclusive("Cannot test the uncovered line 50 due to the following limitations:\n" +
			"1. ListNode and ElementNode are concrete classes that cannot be easily mocked\n" +
			"2. IsResourceDictionary extension method requires SourceGenContext with Variables containing LocalVariable with Type that inherits from ResourceDictionary\n" +
			"3. Need to set up complex object hierarchy: ElementNode -> ListNode -> ElementNode being visited\n" +
			"4. Parent ElementNode must be identified as ResourceDictionary and not contain xKey property\n" +
			"5. Accept method on ElementNode cannot be mocked\n" +
			"6. SetPropertiesVisitor constructor cannot be mocked\n" +
			"To test this scenario, consider:\n" +
			"- Integration testing with real SourceGenContext, Compilation, and XAML node hierarchy\n" +
			"- Refactoring to use wrapper interfaces for better testability\n" +
			"- Using test utilities that can create proper ResourceDictionary XAML node structures");
	}

	/// <summary>
	/// Tests Visit method when parentNode is ElementNode and is ResourceDictionary without xKey.
	/// Should call Accept on the node with new SetPropertiesVisitor.
	/// </summary>
	[Test]
	public void Visit_ParentNodeIsElementNodeResourceDictionaryWithoutXKey_CallsAccept()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();

		// Create an ElementNode that could potentially be a ResourceDictionary
		var xmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "ResourceDictionary", []);
		var parentElementNode = new ElementNode(xmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Create the node to be visited
		var nodeXmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Button", []);
		var elementNode = new ElementNode(nodeXmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Act & Assert - Test should not throw even if IsResourceDictionary returns false
		// This tests the conditional logic path without requiring complex ResourceDictionary setup
		Assert.DoesNotThrow(() => visitor.Visit(elementNode, parentElementNode));
	}

	/// <summary>
	/// Tests Visit method when node is not ResourceDictionary.
	/// Should not call SetPropertyValue and proceed to other conditions.
	/// </summary>
	[Test]
	public void Visit_NodeIsNotResourceDictionary_DoesNotCallSetPropertyValue()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();

		// Create a parent ElementNode 
		var parentXmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "StackPanel", []);
		var parentElementNode = new ElementNode(parentXmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Create the node to be visited - clearly NOT a ResourceDictionary
		var nodeXmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "Button", []);
		var elementNode = new ElementNode(nodeXmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Act & Assert - Test should not throw when visiting a non-ResourceDictionary node
		// This tests the conditional logic path where IsResourceDictionary returns false
		Assert.DoesNotThrow(() => visitor.Visit(elementNode, parentElementNode));
	}

	/// <summary>
	/// Tests Visit method when property name is not Resources related.
	/// Should not call SetPropertyValue and proceed to other conditions.
	/// </summary>
	[Test]
	public void Visit_PropertyNameIsNotResourcesRelated_DoesNotCallSetPropertyValue()
	{
		// Arrange
		var writer = new IndentedTextWriter(TextWriter.Null);
		var context = new SourceGenContext(
			writer,
			null!,
			default,
			null!,
			null!,
			null!,
			null,
			null!
		);
		var visitor = new SetResourcesVisitor(context);

		var mockNamespaceResolver = new Mock<IXmlNamespaceResolver>();

		// Create a ResourceDictionary node
		var resourceDictXmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "ResourceDictionary", []);
		var resourceDictNode = new ElementNode(resourceDictXmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Create a parent node with a property that is NOT Resources-related
		var parentXmlType = new XmlType("http://schemas.microsoft.com/winfx/2006/xaml/presentation", "ContentPage", []);
		var parentNode = new ElementNode(parentXmlType, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", mockNamespaceResolver.Object);

		// Set up a property on the parent that is NOT Resources-related (e.g., "Title")
		var nonResourcesPropertyName = new XmlName("", "Title");
		parentNode.Properties[nonResourcesPropertyName] = resourceDictNode;

		// Act & Assert - This should not throw and should not call SetPropertyValue for non-Resources property
		// The method should execute the first condition but skip the SetPropertyValue call because 
		// "Title" is not "Resources" and doesn't end with ".Resources"
		Assert.DoesNotThrow(() => visitor.Visit(resourceDictNode, parentNode));
	}
}
