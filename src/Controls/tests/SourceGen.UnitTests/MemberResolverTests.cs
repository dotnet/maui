using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

public class MemberResolverTests
{
	// Helper to create a type symbol from C# code
	private static ITypeSymbol? GetTypeSymbol(string typeName, string code)
	{
		var compilation = CSharpCompilation.Create("TestAssembly",
			new[] { CSharpSyntaxTree.ParseText(code) },
			new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) },
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		var type = compilation.GetTypeByMetadataName(typeName);
		return type;
	}

	private static ITypeSymbol GetPageType() => GetTypeSymbol("TestApp.MainPage", 
@"namespace TestApp {
	public class MainPage {
		public string Title { get; set; }
		public string LocalOnly { get; set; }
		public string SharedName { get; set; }
	}
}")!;

	private static ITypeSymbol GetViewModelType() => GetTypeSymbol("TestApp.ViewModel",
@"namespace TestApp {
	public class ViewModel {
		public string Name { get; set; }
		public string DataTypeOnly { get; set; }
		public string SharedName { get; set; }
		public User User { get; set; }
	}
	public class User {
		public string DisplayName { get; set; }
	}
}")!;

	[Fact]
	public void Resolve_ThisPrefix_ReturnsForcedThis()
	{
		var result = MemberResolver.Resolve("this.Title", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.ForcedThis, result.Location);
		Assert.Equal("Title", result.Expression);
		Assert.Equal("Title", result.RootIdentifier);
		Assert.True(result.IsLocal);
		Assert.False(result.IsBinding);
	}

	[Fact]
	public void Resolve_DotPrefix_ReturnsForcedDataType()
	{
		var result = MemberResolver.Resolve(".Name", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.ForcedDataType, result.Location);
		Assert.Equal("Name", result.Expression);
		Assert.Equal("Name", result.RootIdentifier);
		Assert.True(result.IsBinding);
		Assert.False(result.IsLocal);
	}

	[Fact]
	public void Resolve_BindingContextPrefix_ReturnsForcedDataType()
	{
		var result = MemberResolver.Resolve("BindingContext.Name", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.ForcedDataType, result.Location);
		Assert.Equal("Name", result.Expression);
		Assert.Equal("Name", result.RootIdentifier);
		Assert.True(result.IsBinding);
	}

	[Fact]
	public void Resolve_OnlyOnThis_ReturnsThis()
	{
		var result = MemberResolver.Resolve("LocalOnly", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.This, result.Location);
		Assert.Equal("LocalOnly", result.Expression);
		Assert.True(result.IsLocal);
	}

	[Fact]
	public void Resolve_OnlyOnDataType_ReturnsDataType()
	{
		var result = MemberResolver.Resolve("DataTypeOnly", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.DataType, result.Location);
		Assert.Equal("DataTypeOnly", result.Expression);
		Assert.True(result.IsBinding);
	}

	[Fact]
	public void Resolve_OnBoth_ReturnsBoth()
	{
		var result = MemberResolver.Resolve("SharedName", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.Both, result.Location);
		Assert.True(result.IsAmbiguous);
	}

	[Fact]
	public void Resolve_OnNeither_ReturnsNeither()
	{
		var result = MemberResolver.Resolve("NonExistent", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.Neither, result.Location);
		Assert.True(result.IsNotFound);
	}

	[Fact]
	public void Resolve_NestedProperty_ResolvesRoot()
	{
		var result = MemberResolver.Resolve("User.DisplayName", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.DataType, result.Location);
		Assert.Equal("User.DisplayName", result.Expression);
		Assert.Equal("User", result.RootIdentifier);
	}

	[Fact]
	public void Resolve_WithMethodCall_ResolvesRoot()
	{
		var result = MemberResolver.Resolve("User.ToString()", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.DataType, result.Location);
		Assert.Equal("User", result.RootIdentifier);
	}

	[Fact]
	public void Resolve_NullDataType_OnlyChecksThis()
	{
		var result = MemberResolver.Resolve("Title", GetPageType(), null);
		
		Assert.Equal(MemberLocation.This, result.Location);
	}

	[Fact]
	public void Resolve_NullThisType_OnlyChecksDataType()
	{
		var result = MemberResolver.Resolve("Name", null, GetViewModelType());
		
		Assert.Equal(MemberLocation.DataType, result.Location);
	}

	[Fact]
	public void Resolve_BothNull_ReturnsNeither()
	{
		var result = MemberResolver.Resolve("Anything", null, null);
		
		Assert.Equal(MemberLocation.Neither, result.Location);
	}

	[Fact]
	public void Resolve_ExpressionWithOperators_ResolvesFirstIdentifier()
	{
		var result = MemberResolver.Resolve("User.Age > 18", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.DataType, result.Location);
		Assert.Equal("User", result.RootIdentifier);
	}

	[Fact]
	public void Resolve_DotPrefixWithNestedPath_Works()
	{
		var result = MemberResolver.Resolve(".User.DisplayName", GetPageType(), GetViewModelType());
		
		Assert.Equal(MemberLocation.ForcedDataType, result.Location);
		Assert.Equal("User.DisplayName", result.Expression);
		Assert.Equal("User", result.RootIdentifier);
	}
}
