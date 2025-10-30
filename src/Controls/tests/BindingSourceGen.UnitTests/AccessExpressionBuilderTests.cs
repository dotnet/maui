using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

public class AccessExpressionBuilderTests
{
	[Fact]
	public void CorrectlyFormatsSimpleCast()
	{
		var generatedCode = Build("source",
			[
				new MemberAccess("A"),
				new Cast(new TypeDescription("X", IsNullable: false, IsGenericParameter: false, IsValueType: false)),
				new ConditionalAccess(new MemberAccess("B")),
			]);

		Assert.Equal("(source.A as X)?.B", generatedCode);
	}

	[Fact]
	public void CorrectlyFormatsSimpleCastOfNonNullableValueTypes()
	{
		var generatedCode = Build("source",
			[
				new MemberAccess("A"),
				new Cast(new TypeDescription("X", IsNullable: false, IsGenericParameter: false, IsValueType: true)),
				new ConditionalAccess(new MemberAccess("B")),
			]);

		Assert.Equal("(source.A as X?)?.B", generatedCode);
	}

	[Fact]
	public void CorrectlyFormatsSimpleCastOfNullableValueTypes()
	{
		var generatedCode = Build("source",
			[
				new MemberAccess("A"),
				new Cast(new TypeDescription("X", IsNullable: true, IsGenericParameter: false, IsValueType: true)),
				new ConditionalAccess(new MemberAccess("B")),
			]);

		Assert.Equal("(source.A as X?)?.B", generatedCode);
	}

	[Fact]
	public void CorrectlyFormatsInaccessibleFieldMemberAccess()
	{
		var generatedCode = Build("source",
			[
				new InaccessibleMemberAccess(new TypeDescription("X"), new TypeDescription("Z"), AccessorKind.Field, "Y")
			]);

		Assert.Equal("GetUnsafeField_Y(source)", generatedCode);
	}

	[Fact]
	public void CorrectlyFormatsInaccessiblePropertyMemberAccess()
	{
		var generatedCode = Build("source",
			[
				new InaccessibleMemberAccess(new TypeDescription("X"), new TypeDescription("Z"), AccessorKind.Property, "Y")
			]);

		Assert.Equal("GetUnsafeProperty_Y(source)", generatedCode);
	}

	private static string Build(string initialExpression, IPathPart[] path)
	{
		string expression = initialExpression;

		foreach (var part in path)
		{
			expression = AccessExpressionBuilder.ExtendExpression(expression, part);
		}

		return expression;
	}
}
