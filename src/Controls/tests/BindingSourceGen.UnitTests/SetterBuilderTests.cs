using System.Linq;
using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

public class SetterBuilderTests
{
    private static readonly TypeDescription NullableType = new TypeDescription("MyType", IsNullable: true);
    private static readonly TypeDescription NonNullableType = new TypeDescription("MyType", IsNullable: false);

    [Fact]
    public void GeneratesSetterWithoutAnyPatternMatchingForEmptyPath()
    {
        var setter = Setter.From(NullableType, []);

        Assert.Empty(setter.PatternMatchingExpressions);
        Assert.Equal("source = value;", setter.AssignmentStatement);
    }

    [Fact]
    public void GeneratesSetterWithSourceNotNullPatternMatchingForSignlePathStepWhenSourceTypeIsNullable()
    {
        var setter = Setter.From(NullableType, [new MemberAccess("A")]);

        Assert.Single(setter.PatternMatchingExpressions);
        Assert.Equal("source is {} p0", setter.PatternMatchingExpressions[0]);
        Assert.Equal("p0.A = value;", setter.AssignmentStatement);
    }

    [Fact]
    public void GeneratesSetterWithoutAnyPatternMatchingForSignlePathStepWhenSourceTypeIsNotNullable()
    {
        var setter = Setter.From(NonNullableType, [new MemberAccess("A")]);

        Assert.Empty(setter.PatternMatchingExpressions);
        Assert.Equal("source.A = value;", setter.AssignmentStatement);
    }

    [Fact]
    public void GeneratesSetterWithCorrectConditionalAccess()
    {
        var setter = Setter.From(NonNullableType, 
            [
                new MemberAccess("A"),
                new ConditionalAccess(new MemberAccess("B")),
                new ConditionalAccess(new MemberAccess("C")),
            ]);

        Assert.Equal(2, setter.PatternMatchingExpressions.Length);
        Assert.Equal("source.A is {} p0", setter.PatternMatchingExpressions[0]);
        Assert.Equal("p0.B is {} p1", setter.PatternMatchingExpressions[1]);
        Assert.Equal("p1.C = value;", setter.AssignmentStatement);
    }

    [Fact]
    public void GeneratesSetterWithPatternMatchingWithValueTypeCast1()
    {
        var setter = Setter.From(NonNullableType, 
            [
                new MemberAccess("A"),
                new Cast(new TypeDescription("X", IsValueType: false)),
                new ConditionalAccess(new MemberAccess("B")),
                new Cast(new TypeDescription("Y", IsValueType: true)),
                new ConditionalAccess(new MemberAccess("C")),
                new MemberAccess("D"),
            ]);

        Assert.Equal(2, setter.PatternMatchingExpressions.Length);
        Assert.Equal("source.A is X p0", setter.PatternMatchingExpressions[0]);
        Assert.Equal("p0.B is Y p1", setter.PatternMatchingExpressions[1]);
        Assert.Equal("p1.C.D = value;", setter.AssignmentStatement);
    }

    [Fact]
    public void GeneratesSetterWithPatternMatchingWithValueTypeCast2()
    {
        var setter = Setter.From(NonNullableType, 
            [
                new MemberAccess("A"),
                new Cast(new TypeDescription("X", IsValueType: false)),
                new ConditionalAccess(new MemberAccess("B")),
                new Cast(new TypeDescription("Y", IsValueType: true)),
                new ConditionalAccess(new MemberAccess("C")),
                new ConditionalAccess(new MemberAccess("D")),
            ]);

        Assert.Equal(3, setter.PatternMatchingExpressions.Length);
        Assert.Equal("source.A is X p0", setter.PatternMatchingExpressions[0]);
        Assert.Equal("p0.B is Y p1", setter.PatternMatchingExpressions[1]);
        Assert.Equal("p1.C is {} p2", setter.PatternMatchingExpressions[2]);
        Assert.Equal("p2.D = value;", setter.AssignmentStatement);
    }

    [Fact]
    public void GeneratesSetterWithPatternMatchingWithCastsAndConditionalAccess()
    {
        var setter = Setter.From(NonNullableType, 
            [
                new MemberAccess("A"),
                new Cast(TargetType: new TypeDescription("X", IsValueType: false, IsNullable: false, IsGenericParameter: false)),
                new ConditionalAccess(new MemberAccess("B")),
                new Cast(new TypeDescription("Y", IsValueType: false, IsNullable: false, IsGenericParameter: false)),
                new ConditionalAccess(new MemberAccess("C")),
                new Cast(new TypeDescription("Z", IsValueType: true, IsNullable: true, IsGenericParameter: false)),
                new ConditionalAccess(new MemberAccess("D")),
            ]);

        Assert.Equal(3, setter.PatternMatchingExpressions.Length);
        Assert.Equal("source.A is X p0", setter.PatternMatchingExpressions[0]);
        Assert.Equal("p0.B is Y p1", setter.PatternMatchingExpressions[1]);
        Assert.Equal("p1.C is Z p2", setter.PatternMatchingExpressions[2]);
        Assert.Equal("p2.D = value;", setter.AssignmentStatement);
    }
}
