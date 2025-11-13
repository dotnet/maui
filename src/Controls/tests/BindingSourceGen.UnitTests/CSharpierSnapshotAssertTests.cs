using Xunit;

namespace BindingSourceGen.UnitTests;

/// <summary>
/// Tests for CSharpierSnapshotAssert to demonstrate its capabilities and limitations.
/// </summary>
public class CSharpierSnapshotAssertTests
{
	[Fact]
	public void Equal_IgnoresAllFormattingDifferences()
	{
		// Different indentation styles
		var expected = """
            namespace Test
            {
                class MyClass
                {
                    public void Method()
                    {
                        var x = 1;
                    }
                }
            }
            """;

		var actual = """
		namespace Test {
		class MyClass {
		public void Method() {
		var x = 1; } } }
		""";

		// Should pass - CSharpier formats both to the same style
		CSharpierSnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_IgnoresLineBreakDifferences()
	{
		var expected = """
            public class MyClass
            {
                public int Property1 { get; set; }
                public int Property2 { get; set; }
                public int Property3 { get; set; }
            }
            """;

		var actual = "public class MyClass { public int Property1 { get; set; } public int Property2 { get; set; } public int Property3 { get; set; } }";

		// Should pass - CSharpier normalizes line breaks
		CSharpierSnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_IgnoresSpacingAroundOperators()
	{
		var expected = "var x = 1 + 2 * 3;";
		var actual = "var x=1+2*3;";

		// Should pass - CSharpier normalizes spacing
		CSharpierSnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_FailsOnActualCodeDifferences()
	{
		var expected = """
            public class MyClass
            {
                public int Property { get; set; }
            }
            """;

		var actual = """
            public class MyClass
            {
                public string Property { get; set; }
            }
            """;

		// Should fail - different property type (int vs string)
		Assert.Throws<Xunit.Sdk.EqualException>(() =>
			CSharpierSnapshotAssert.Equal(expected, actual)
		);
	}

	[Fact]
	public void Equal_ComparesComplexGeneratedCode()
	{
		var expected = """
            namespace Microsoft.Maui.Controls.Generated
            {
                internal static partial class GeneratedBindingInterceptors
                {
                    public static void SetBinding(
                        this BindableObject bindableObject,
                        BindableProperty bindableProperty,
                        Func<TSource, TProperty> getter,
                        BindingMode mode = BindingMode.Default)
                    {
                        var binding = new TypedBinding<TSource, TProperty>(
                            getter: source => (getter(source), true),
                            setter: null,
                            handlers: new Tuple<Func<TSource, object?>, string>[]
                            {
                                new(static source => source, "Property")
                            });
                        bindableObject.SetBinding(bindableProperty, binding);
                    }
                }
            }
            """;

		var actual = """
		namespace Microsoft.Maui.Controls.Generated{internal static partial class GeneratedBindingInterceptors{public static void SetBinding(this BindableObject bindableObject,BindableProperty bindableProperty,Func<TSource,TProperty> getter,BindingMode mode=BindingMode.Default){var binding=new TypedBinding<TSource,TProperty>(getter:source=>(getter(source),true),setter:null,handlers:new Tuple<Func<TSource,object?>,string>[]{new(static source=>source,"Property")});bindableObject.SetBinding(bindableProperty,binding);}}}
		""";

		// Should pass - same code, just different formatting
		CSharpierSnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_HandlesIncompleteCode()
	{
		// CSharpier might not be able to format incomplete code
		// In that case, it should still allow comparison of the original
		var expected = "public class MyClass {";
		var actual = "public class MyClass {";

		// Should pass - same incomplete code
		CSharpierSnapshotAssert.Equal(expected, actual);
	}

	[Fact]
	public void Equal_HandlesCodeWithSyntaxErrors()
	{
		var expected = "public class MyClass { invalid syntax here }";
		var actual = "public class MyClass { invalid syntax here }";

		// Should pass - same invalid code
		CSharpierSnapshotAssert.Equal(expected, actual);
	}
}
