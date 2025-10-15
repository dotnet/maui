using System;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.SourceGen;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for the XamlProjectItemForCB constructor that takes projectItem and exception parameters.
/// </summary>
[TestFixture]
public partial class XamlProjectItemForCBTests
{
	/// <summary>
	/// Tests constructor with valid exception parameter and null projectItem.
	/// Should create instance with Exception property set and ProjectItem property null.
	/// </summary>
	[Test]
	public void Constructor_WithValidExceptionAndNullProjectItem_SetsPropertiesCorrectly()
	{
		// Arrange
		var exception = new InvalidOperationException("Test exception");
		ProjectItem? projectItem = null;

		// Act
		var result = new XamlProjectItemForCB(projectItem!, exception);

		// Assert
		Assert.IsNull(result.ProjectItem);
		Assert.AreSame(exception, result.Exception);
		Assert.IsNull(result.Root);
		Assert.IsNull(result.Nsmgr);
	}

	/// <summary>
	/// Tests constructor with null exception parameter and null projectItem.
	/// Should create instance with both Exception and ProjectItem properties null.
	/// </summary>
	[Test]
	public void Constructor_WithNullExceptionAndNullProjectItem_SetsPropertiesCorrectly()
	{
		// Arrange
		ProjectItem? projectItem = null;
		Exception? exception = null;

		// Act
		var result = new XamlProjectItemForCB(projectItem!, exception!);

		// Assert
		Assert.IsNull(result.ProjectItem);
		Assert.IsNull(result.Exception);
		Assert.IsNull(result.Root);
		Assert.IsNull(result.Nsmgr);
	}

	/// <summary>
	/// Tests constructor with different exception types.
	/// Should create instance with Exception property set to the provided exception regardless of type.
	/// </summary>
	[TestCase(typeof(ArgumentException))]
	[TestCase(typeof(InvalidOperationException))]
	[TestCase(typeof(NotSupportedException))]
	[TestCase(typeof(Exception))]
	public void Constructor_WithDifferentExceptionTypes_SetsExceptionPropertyCorrectly(Type exceptionType)
	{
		// Arrange
		var exception = (Exception)Activator.CreateInstance(exceptionType, "Test message")!;
		ProjectItem? projectItem = null;

		// Act
		var result = new XamlProjectItemForCB(projectItem!, exception);

		// Assert
		Assert.AreSame(exception, result.Exception);
		Assert.AreEqual(exceptionType, result.Exception!.GetType());
		Assert.IsNull(result.ProjectItem);
	}

	/// <summary>
	/// Tests constructor with ProjectItem parameter - PARTIAL TEST.
	/// This test demonstrates the pattern but requires proper ProjectItem instances.
	/// To complete this test, provide valid AdditionalText and AnalyzerConfigOptions instances
	/// which are typically available in the context of a source generator.
	/// </summary>
	[Test]
	public void Constructor_WithValidProjectItemAndException_SetsPropertiesCorrectly()
	{
		// Arrange
		var mockAdditionalText = new Moq.Mock<AdditionalText>();
		var mockOptions = new Moq.Mock<Microsoft.CodeAnalysis.Diagnostics.AnalyzerConfigOptions>();
		var projectItem = new ProjectItem(mockAdditionalText.Object, mockOptions.Object);

		var exception = new InvalidOperationException("Test exception");

		// Act
		var result = new XamlProjectItemForCB(projectItem, exception);

		// Assert
		Assert.AreSame(projectItem, result.ProjectItem);
		Assert.AreSame(exception, result.Exception);
		Assert.IsNull(result.Root);
		Assert.IsNull(result.Nsmgr);
	}

	/// <summary>
	/// Tests constructor with exception containing inner exceptions.
	/// Should preserve the complete exception hierarchy in the Exception property.
	/// </summary>
	[Test]
	public void Constructor_WithExceptionContainingInnerException_PreservesExceptionHierarchy()
	{
		// Arrange
		var innerException = new ArgumentException("Inner exception message");
		var outerException = new InvalidOperationException("Outer exception message", innerException);
		ProjectItem? projectItem = null;

		// Act
		var result = new XamlProjectItemForCB(projectItem!, outerException);

		// Assert
		Assert.AreSame(outerException, result.Exception);
		Assert.AreSame(innerException, result.Exception!.InnerException);
		Assert.AreEqual("Outer exception message", result.Exception.Message);
		Assert.AreEqual("Inner exception message", result.Exception.InnerException!.Message);
	}
}
