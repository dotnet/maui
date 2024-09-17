using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen;
using NUnit.Framework;

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

    [Test]
    public void CreateVariableName()
    {
        var compilation = SourceGeneratorDriver.CreateMauiCompilation();
		compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(code));
        var context = new object();
        Assert.That(NamingHelpers.CreateUniqueVariableNameImpl(context, compilation.GetTypeByMetadataName("Test.TestClass")!), Is.EqualTo("testClass"));
        Assert.That(NamingHelpers.CreateUniqueVariableNameImpl(context, compilation.GetTypeByMetadataName("Test.TestClass")!), Is.EqualTo("testClass1"));

        Assert.That(NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.GetTypeByMetadataName("Test.TestClass`1")!), Is.EqualTo("testClass"));
        Assert.That(NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.GetTypeByMetadataName("Test.TestClass`1")!.Construct(compilation.GetTypeByMetadataName("Test.TestClass")!)), Is.EqualTo("testClass"));

        Assert.That(NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.CreateArrayTypeSymbol(compilation.GetTypeByMetadataName("Test.TestClass")!)!), Is.EqualTo("testClassArray"));

        Assert.That(NamingHelpers.CreateUniqueVariableNameImpl(new object(), compilation.GetTypeByMetadataName("Test.TestClass+Nested")!), Is.EqualTo("nested"));
    }
}