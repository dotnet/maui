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
/// Unit tests for the CompilationExtensions class.
/// </summary>
[TestFixture]
public class CompilationExtensionsTests
{
	/// <summary>
	/// Tests GetAssembly behavior with various edge case assembly names.
	/// Input: Compilation with assemblies, various edge case assembly names.
	/// Expected: Returns appropriate results for each case.
	/// </summary>
	[TestCase("", ExpectedResult = null, TestName = "GetAssembly_EmptyAssemblyName_ReturnsNull")]
	[TestCase("   ", ExpectedResult = null, TestName = "GetAssembly_WhitespaceAssemblyName_ReturnsNull")]
	[TestCase("Assembly With Spaces", ExpectedResult = null, TestName = "GetAssembly_AssemblyNameWithSpaces_ReturnsNull")]
	[TestCase("Assembly.With.Dots", ExpectedResult = null, TestName = "GetAssembly_AssemblyNameWithDots_ReturnsNull")]
	[TestCase("Assembly-With-Dashes", ExpectedResult = null, TestName = "GetAssembly_AssemblyNameWithDashes_ReturnsNull")]
	[TestCase("Assembly_With_Underscores", ExpectedResult = null, TestName = "GetAssembly_AssemblyNameWithUnderscores_ReturnsNull")]
	public IAssemblySymbol? GetAssembly_EdgeCaseAssemblyNames_HandlesGracefully(string assemblyName)
	{
		// Arrange
		var mockCompilation = new Mock<Compilation>();
		var mockAssembly1 = new Mock<IAssemblySymbol>();
		var mockAssembly2 = new Mock<IAssemblySymbol>();
		var mockObjectType = new Mock<INamedTypeSymbol>();
		var mockSourceModule = new Mock<IModuleSymbol>();
		var mockIdentity1 = new Mock<AssemblyIdentity>("NormalAssembly");
		var mockIdentity2 = new Mock<AssemblyIdentity>("AnotherAssembly");

		mockAssembly1.Setup(a => a.Identity).Returns(mockIdentity1.Object);
		mockAssembly2.Setup(a => a.Identity).Returns(mockIdentity2.Object);

		mockCompilation.Setup(c => c.Assembly).Returns(mockAssembly1.Object);
		mockCompilation.Setup(c => c.ObjectType).Returns(mockObjectType.Object);
		mockObjectType.Setup(ot => ot.ContainingAssembly).Returns(mockAssembly2.Object);
		mockCompilation.Setup(c => c.SourceModule).Returns(mockSourceModule.Object);
		mockSourceModule.Setup(sm => sm.ReferencedAssemblySymbols).Returns(ImmutableArray<IAssemblySymbol>.Empty);

		// Act
		return mockCompilation.Object.GetAssembly(assemblyName);
	}

	/// <summary>
	/// Tests that GetAllAssemblies throws ArgumentNullException when compilation parameter is null.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void GetAllAssemblies_NullCompilation_ThrowsArgumentNullException()
	{
		// Arrange
		Compilation? compilation = null;

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => compilation!.GetAllAssemblies());
	}

	/// <summary>
	/// Tests that GetAllAssemblies handles empty referenced assemblies correctly.
	/// Expected result: Returns immutable array containing only compilation assembly and object type assembly.
	/// </summary>
	[Test]
	public void GetAllAssemblies_EmptyReferencedAssemblies_ReturnsCompilationAndObjectTypeAssemblies()
	{
		// Arrange
		// Note: Cannot mock Compilation directly as its properties are not virtual
		// Using CSharpCompilation.Create() to create a minimal compilation for testing
		var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
			"TestAssembly",
			references: new[] { Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location) });

		// Act
		var result = compilation.GetAllAssemblies();

		// Assert
		// The result should contain at least the compilation assembly and object type assembly
		Assert.That(result.Length, Is.GreaterThanOrEqualTo(2));
		Assert.That(result, Contains.Item(compilation.Assembly));
		Assert.That(result, Contains.Item(compilation.ObjectType.ContainingAssembly));
	}


	/// <summary>
	/// Tests GetAssembly method with null compilation parameter.
	/// Should throw ArgumentNullException when compilation is null.
	/// </summary>
	[Test]
	[Category("ProductionBugSuspected")]
	public void GetAssembly_NullCompilation_ThrowsArgumentNullException()
	{
		// Arrange
		Compilation? compilation = null;
		const string assemblyName = "TestAssembly";

		// Act & Assert
		Assert.Throws<ArgumentNullException>(() => compilation!.GetAssembly(assemblyName));
	}

	/// <summary>
	/// Tests GetAssembly method with null assembly name parameter.
	/// Input: Valid compilation, null assembly name.
	/// Expected: Returns null as no assembly will have null as its name.
	/// </summary>
	[Test]
	public void GetAssembly_NullAssemblyName_ReturnsNull()
	{
		// Arrange
		var mockCompilation = new Mock<Compilation>();
		var mockAssembly1 = new Mock<IAssemblySymbol>();
		var mockAssembly2 = new Mock<IAssemblySymbol>();
		var mockObjectType = new Mock<INamedTypeSymbol>();
		var mockSourceModule = new Mock<IModuleSymbol>();
		var mockIdentity1 = new Mock<AssemblyIdentity>("TestAssembly");
		var mockIdentity2 = new Mock<AssemblyIdentity>("ObjectAssembly");

		mockAssembly1.Setup(a => a.Identity).Returns(mockIdentity1.Object);
		mockAssembly2.Setup(a => a.Identity).Returns(mockIdentity2.Object);

		mockCompilation.Setup(c => c.Assembly).Returns(mockAssembly1.Object);
		mockCompilation.Setup(c => c.ObjectType).Returns(mockObjectType.Object);
		mockObjectType.Setup(ot => ot.ContainingAssembly).Returns(mockAssembly2.Object);
		mockCompilation.Setup(c => c.SourceModule).Returns(mockSourceModule.Object);
		mockSourceModule.Setup(sm => sm.ReferencedAssemblySymbols).Returns(ImmutableArray<IAssemblySymbol>.Empty);

		// Act
		var result = mockCompilation.Object.GetAssembly(null!);

		// Assert
		Assert.IsNull(result);
	}

}