using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;


public class NamingHelpersTests
{
	const string code =
"""
using System;

namespace Test;

public class TestClass
{
    public class Nested
    {
    }
}

public class TestClass<T>
{
}

""";

	[Fact]
	public void CreateVariableName()
	{
		var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
		var context = new object();
		Assert.Equal("testClass", NamingHelpers.CreateUniqueVariableNameImpl(context, compilation.GetTypeByMetadataName("Test.TestClass")!));
		Assert.Equal("testClass1", NamingHelpers.CreateUniqueVariableNameImpl(context, compilation.GetTypeByMetadataName("Test.TestClass")!));

		Assert.Equal("testClass", NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.GetTypeByMetadataName("Test.TestClass`1")!));
		Assert.Equal("testClass", NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.GetTypeByMetadataName("Test.TestClass`1")!.Construct(compilation.GetTypeByMetadataName("Test.TestClass")!)));

		Assert.Equal("testClassArray", NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.CreateArrayTypeSymbol(compilation.GetTypeByMetadataName("Test.TestClass")!)!));

		Assert.Equal("nested", NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.GetTypeByMetadataName("Test.TestClass+Nested")!));
	}
}