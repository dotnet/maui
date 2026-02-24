using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;


namespace BindingSourceGen.UnitTests;


public class BindingRepresentationGenTests
{
	[Fact]
	public void GenerateSimpleBinding()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (string s) => s.Length);
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("string"),
				new TypeDescription("int", IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Length", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWithNestedProperties()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button b) => b.Text?.Length);
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Microsoft.Maui.Controls.Button"),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Text"),
					new ConditionalAccess(new MemberAccess("Length", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWithNullableReferenceElementInPathWhenNullableEnabled()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Button?.Text?.Length);

        class Foo
        {
            public Button? Button { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Button"),
					new ConditionalAccess(new MemberAccess("Text")),
					new ConditionalAccess(new MemberAccess("Length", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);

	}

	[Fact]
	public void GenerateBindingWithNullableReferenceSourceWhenNullableEnabled()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button? b) => b?.Text?.Length);
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Microsoft.Maui.Controls.Button", IsNullable: true),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new ConditionalAccess(new MemberAccess("Text")),
					new ConditionalAccess(new MemberAccess("Length", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWithNullableValueTypeWhenNullableEnabled()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value);

        class Foo
        {
            public int? Value { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Value", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: true, AcceptsNullValue: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWithNullableSourceReferenceAndNullableReferenceElementInPathWhenNullableEnabled()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Button? b) => b?.Text?.Length);
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Microsoft.Maui.Controls.Button", IsNullable: true),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new ConditionalAccess(new MemberAccess("Text")),
					new ConditionalAccess(new MemberAccess("Length", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWithNullablePropertyReferenceWhenNullableEnabled()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value);

        class Foo
        {
            public string? Value { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("string", IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Value"),
				]),
				SetterOptions: new(IsWritable: true, AcceptsNullValue: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWithNullableReferenceTypesWhenNullableDisabledAndConditionalAccessOperator()
	{
		var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f?.Bar?.Length);

        class Foo
        {
            public string Bar { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 4, 7),
				new TypeDescription("global::Foo", IsNullable: true),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new ConditionalAccess(new MemberAccess("Bar")),
					new ConditionalAccess(new MemberAccess("Length", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: false,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenNullableDisabledAndPropertyNonNullableValueType()
	{
		var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Bar.Length);

        class Foo
        {
            public Bar Bar { get; set; }
        }

        class Bar
        {
            public int Length { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 4, 7),
				new TypeDescription("global::Foo", IsNullable: true),
				new TypeDescription("int", IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Bar"),
					new MemberAccess("Length", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: false,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenNullableDisabledAndPropertyNullableValueType()
	{
		var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Bar.Length);

        class Foo
        {
            public Bar Bar { get; set; }
        }

        class Bar
        {
            public int? Length { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 4, 7),
				new TypeDescription("global::Foo", IsNullable: true),
				new TypeDescription("int", IsNullable: true, IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Bar"),
					new MemberAccess("Length", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: true, AcceptsNullValue: true),
				NullableContextEnabled: false,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenNullableDisabledAndPropertyReferenceType()
	{
		var source = """
        using Microsoft.Maui.Controls;
        #nullable disable
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Bar.Length);

        class Foo
        {
            public Bar Bar { get; set; }
        }

        class Bar
        {
            public CustomLength Length { get; set; }
        }

        class CustomLength 
        {

        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 4, 7),
				new TypeDescription("global::Foo", IsNullable: true),
				new TypeDescription("global::CustomLength", IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Bar"),
					new MemberAccess("Length"),
				]),
				SetterOptions: new(IsWritable: true, AcceptsNullValue: true),
				NullableContextEnabled: false,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenBindingContainsIntegerIndexing()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Items[0].Length);

        class Foo
        {
            public string[] Items { get; set; } = { "Item1" };
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Items"),
					new IndexAccess("Item", 0),
					new MemberAccess("Length", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsStringIndexing()
	{
		var source = """
        using Microsoft.Maui.Controls;
        using System.Collections.Generic;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Items["key"].Length);

        class Foo
        {
            public Dictionary<string, string> Items { get; set; } = new();
        }
        """;
		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 4, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Items"),
					new IndexAccess("Item", "key"),
					new MemberAccess("Length", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: false),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsCustomIndexerWithIndexerNameAttribute()
	{
		var source = """
        using Microsoft.Maui.Controls;
        using System.Runtime.CompilerServices;

        var label = new Label();
        var foo = new Foo();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f["key"].Length);

        class Foo
        {   [IndexerName("CustomIndexer")]
            public string this[string key] => key;
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 6, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("int", IsValueType: true),
			new EquatableArray<IPathPart>([
				new IndexAccess("CustomIndexer", "key"),
				new MemberAccess("Length", IsValueType: true),
			]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsNullableIndexer()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f["key"]?.Length);

        class Foo
        {
            public string? this[string key] => key;
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
				new IndexAccess("Item", "key"),
				new ConditionalAccess(new MemberAccess("Length", IsValueType: true)), // TODO: Improve naming so this looks right
            ]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsConditionallyAccessedIndexer()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.bar?["key"].Length);

        class Foo
        {
            public Bar? bar { get; set; }
        }

        class Bar
        {
            public string this[string key] => key;
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("int", IsValueType: true, IsNullable: true),
			new EquatableArray<IPathPart>([
				new MemberAccess("bar"),
				new ConditionalAccess(new IndexAccess("Item", "key")),
				new MemberAccess("Length", IsValueType: true),
			]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsComplexCombinedIndexers()
	{
		var source = """
            using Microsoft.Maui.Controls;
            using System.Runtime.CompilerServices;
            using MyNamespace;

            var label = new Label();
            label.SetBinding(Label.TextProperty, static (MySourceClass s) => (s[12]?["Abc"][0]));

            namespace MyNamespace
            {
                public class MySourceClass
                {
                    public B this[int index] => new B();
                }

                public class B
                {
                    [IndexerName("Indexer")]
                    public MyPropertyClass[] this[string index] => [];
                }

                public class MyPropertyClass
                {

                }

            }
            """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 6, 7),
			new TypeDescription("global::MyNamespace.MySourceClass"),
			new TypeDescription("global::MyNamespace.MyPropertyClass", IsNullable: true),
				new EquatableArray<IPathPart>([
				new IndexAccess("Item", 12),
				new ConditionalAccess(new IndexAccess("Indexer", "Abc")),
				new IndexAccess("Item", 0),
			]),
			SetterOptions: new(IsWritable: true),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsCustomIndexerWithDefaultMemberAttribute()
	{
		var source = """
        using Microsoft.Maui.Controls;
        using System.Text;

        var label = new Label();
        var foo = new Foo();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.s[0]);

        class Foo
        { 
            public StringBuilder s {get; set;} = new();
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 6, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("char", IsValueType: true),
				new EquatableArray<IPathPart>([
				new MemberAccess("s"),
				new IndexAccess("Chars", 0, IsValueType: true),
			]),
			SetterOptions: new(IsWritable: true),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsCustomIndexerWithoutAttributes()
	{
		var source = """
        using Microsoft.Maui.Controls;

        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f["key"].Length);

        class Foo
        { 
            public string this[string key] => key;
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 4, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("int", IsValueType: true),
			new EquatableArray<IPathPart>([
				new IndexAccess("Item", "key"),
				new MemberAccess("Length", IsValueType: true),
			]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsSimpleReferenceTypeCast()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value as string);

        class Foo
        {
            public object Value { get; set; } = "Value";
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("string", IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Value"),
					new Cast(new TypeDescription("string")),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsSimpleReferenceTypeExplicitCast()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (string)f.Value);

        class Foo
        {
            public object Value { get; set; } = "Value";
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("string"),
				new EquatableArray<IPathPart>([
					new MemberAccess("Value"),
					new Cast(new TypeDescription("string")),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsMemberAccessOfCastReferenceType()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (f.C as C)?.X);

        public class Foo
        {
            public object C { get; set; } = new C();
        }

        class C
        {
            public int X { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true, IsNullable: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("C"),
					new Cast(new TypeDescription("global::C")),
					new ConditionalAccess(new MemberAccess("X", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsMemberAccessOfExplicitCastReferenceType()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => ((C)f.C).X);

        public class Foo
        {
            public object C { get; set; } = new C();
        }

        class C
        {
            public int X { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("C"),
					new Cast(new TypeDescription("global::C")),
					new MemberAccess("X", IsValueType: true),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}


	[Theory]
	[InlineData("static (Foo f) => (f.C as C)?.X")]
	[InlineData("static (Foo f) => ((C?)f.C)?.X")]
	public void GenerateBindingWhenGetterContainsMemberAccessOfCastNullableReferenceType(string bindingLambda)
	{
		var source = $$"""
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, {{bindingLambda}});

        public class Foo
        {
            public object? C { get; set; }
        }

        class C
        {
            public int X { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsNullable: true, IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("C"),
					new Cast(new TypeDescription("global::C")),
					new ConditionalAccess(new MemberAccess("X", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Theory]
	[InlineData("static (Foo f) => f.Value as int?")]
	[InlineData("static (Foo f) => (int?)f.Value")]
	public void GenerateBindingWhenGetterContainsSimpleValueTypeCast(string bindingLambda)
	{
		var source = $$"""
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, {{bindingLambda}});

        class Foo
        {
            public int Value { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsNullable: true, IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Value", IsValueType: true),
					new Cast(new TypeDescription("int", IsNullable: true, IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);


		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GenerateBindingWhenGetterContainsSimpleValueTypeExplicitCast()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => (int)f.Value);

        class Foo
        {
            public int Value { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("Value", IsValueType: true),
					new Cast(new TypeDescription("int", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);


		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Theory]
	[InlineData("static (Foo f) => (f.C as C?)?.X")]
	[InlineData("static (Foo f) => ((C?)f.C)?.X")]
	public void GenerateBindingWhenGetterContainsMemberAccessOfCastNullableValueType(string bindingLambda)
	{
		var source = $$"""
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, {{bindingLambda}});

        public class Foo
        {
            public object? C { get; set; }
        }

        struct C
        {
            public int X { get; set; }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
				new SimpleLocation(@"Path\To\Program.cs", 3, 7),
				new TypeDescription("global::Foo"),
				new TypeDescription("int", IsNullable: true, IsValueType: true),
				new EquatableArray<IPathPart>([
					new MemberAccess("C"),
					new Cast(new TypeDescription("global::C", IsNullable: true, IsValueType: true)),
					new ConditionalAccess(new MemberAccess("X", IsValueType: true)),
				]),
				SetterOptions: new(IsWritable: true),
				NullableContextEnabled: true,
				MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void SetsIsWritableFalseWhenPropertyComesFromImmutableCollection()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.S[0]);

        class Foo
        {
            public string S { get; set; } = "Value";
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("char", IsValueType: true),
			new EquatableArray<IPathPart>([
				new MemberAccess("S"),
				new IndexAccess("Chars", 0, IsValueType: true),
			]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void SetsIsWritableTrueWhenPropertyComesFromMutableCollection()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f.S[0]);

        class Foo
        {
            public char[] S { get; set; } = { 'A' };
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("char", IsValueType: true),
			new EquatableArray<IPathPart>([
				new MemberAccess("S"),
				new IndexAccess("Item", 0, IsValueType: true),
			]),
			SetterOptions: new(IsWritable: true),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void SetsIsWritableFalseWhenCustomIndexerHasNoSetter()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f["key"]);

        class Foo
        {
            public string this[string key] => key;
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("string"),
			new EquatableArray<IPathPart>([
				new IndexAccess("Item", "key"),
			]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void SetsIsWritableTrueWhenCustomIndexerHasSetter()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo f) => f["key"]);

        class Foo
        {
            public string this[string key] { get => key; set {} }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo"),
			new TypeDescription("string"),
			new EquatableArray<IPathPart>([
				new IndexAccess("Item", "key"),
			]),
			SetterOptions: new(IsWritable: true),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void SetsIsWritableWhenElementAccessIsConditional()
	{
		var source = """
        using Microsoft.Maui.Controls;
        var label = new Label();
        label.SetBinding(Label.RotationProperty, static (Foo? f) => f?[0]);

        class Foo
        {
            public int this[int key] => key;
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			new TypeDescription("global::Foo", IsNullable: true),
			new TypeDescription("int", IsValueType: true, IsNullable: true),
			new EquatableArray<IPathPart>([
				new ConditionalAccess(new IndexAccess("Item", 0, IsValueType: true)),
			]),
			SetterOptions: new(IsWritable: false),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GeneratesInaccessibleMemberAccessWhenUsingPrivateFields()
	{
		var source = """
        using Microsoft.Maui.Controls;

        var foo = new Foo();
        foo.Bar();

        class Foo
        {
            private int _value = 0;
            public void Bar()
            {
                var label = new Label();
                label.SetBinding(Label.RotationProperty, static (Foo f) => f._value);
            }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 12, 15),
			new TypeDescription("global::Foo"),
			new TypeDescription("int", IsValueType: true),
			new EquatableArray<IPathPart>([
				new MemberAccess(
					"_value",
					IsValueType: true,
					ContainingType: new TypeDescription("global::Foo"),
					MemberType: new TypeDescription("int", IsValueType: true),
					Kind: AccessorKind.Field,
					IsGetterInaccessible: true,
					IsSetterInaccessible: true
				)
			]),
			SetterOptions: new(IsWritable: true),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}

	[Fact]
	public void GeneratesInaccessibleMemberAccessWhenUsingPrivateProperties()
	{
		var source = """
        using Microsoft.Maui.Controls;

        var foo = new Foo();
        foo.Bar();

        class Foo
        {
            private int Value { get; set; } = 0;
            public void Bar()
            {
                var label = new Label();
                label.SetBinding(Label.RotationProperty, static (Foo f) => f.Value);
            }
        }
        """;

		var codeGeneratorResult = SourceGenHelpers.Run(source);
		var expectedBinding = new BindingInvocationDescription(
				new InterceptableLocationRecord(1, "serializedData"),
			new SimpleLocation(@"Path\To\Program.cs", 12, 15),
			new TypeDescription("global::Foo"),
			new TypeDescription("int", IsValueType: true),
			new EquatableArray<IPathPart>([
				new MemberAccess(
					"Value",
					IsValueType: true,
					ContainingType: new TypeDescription("global::Foo"),
					MemberType: new TypeDescription("int", IsValueType: true),
					Kind: AccessorKind.Property,
					IsGetterInaccessible: true,
					IsSetterInaccessible: true
				)
			]),
			SetterOptions: new(IsWritable: true),
			NullableContextEnabled: true,
			MethodType: InterceptedMethodType.SetBinding);

		AssertExtensions.BindingsAreEqual(expectedBinding, codeGeneratorResult);
	}
}
