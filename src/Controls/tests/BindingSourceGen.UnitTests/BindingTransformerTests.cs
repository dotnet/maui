using Microsoft.Maui.Controls.BindingSourceGen;
using Xunit;

namespace BindingSourceGen.UnitTests;

public class BindingTransformerTests
{
	[Fact]
	public void WrapMemberAccessInConditionalAccessWhenSourceTypeIsReferenceType()
	{
		var binding = new BindingInvocationDescription(
			InterceptableLocation: new InterceptableLocationRecord(1, "serializedData"),
			SimpleLocation: new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			SourceType: new TypeDescription("MyType", IsValueType: false),
			PropertyType: new TypeDescription("MyType2"),
			Path: new EquatableArray<IPathPart>([new MemberAccess("A")]),
			SetterOptions: new SetterOptions(IsWritable: true),
			NullableContextEnabled: false,
			MethodType: InterceptedMethodType.SetBinding);

		var transformer = new ReferenceTypesConditionalAccessTransformer();
		var transformedBinding = transformer.Transform(binding);

		var transformedPath = new EquatableArray<IPathPart>([new ConditionalAccess(new MemberAccess("A"))]);
		Assert.Equal(transformedPath, transformedBinding.Path);
	}

	[Fact]
	public void WrapMemberAccessInConditionalAccessWhePreviousPartTypeIsReferenceType()
	{
		var binding = new BindingInvocationDescription(
			InterceptableLocation: new InterceptableLocationRecord(1, "serializedData"),
			SimpleLocation: new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			SourceType: new TypeDescription("MyType", IsValueType: true),
			PropertyType: new TypeDescription("MyType2"),
			Path: new EquatableArray<IPathPart>(
				[
					new MemberAccess("A", IsValueType: false),
					new MemberAccess("B"),
				]),
			SetterOptions: new SetterOptions(IsWritable: true),
			NullableContextEnabled: false,
			MethodType: InterceptedMethodType.SetBinding);

		var transformer = new ReferenceTypesConditionalAccessTransformer();
		var transformedBinding = transformer.Transform(binding);

		var transformedPath = new EquatableArray<IPathPart>(
			[
				new MemberAccess("A"),
				new ConditionalAccess(new MemberAccess("B")),
			]);
		Assert.Equal(transformedPath, transformedBinding.Path);
	}

	[Fact]
	public void DoNotWrapMemberAccessInConditionalAccessWhePreviousPartTypeIsValueType()
	{
		var binding = new BindingInvocationDescription(
			InterceptableLocation: new InterceptableLocationRecord(1, "serializedData"),
			SimpleLocation: new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			SourceType: new TypeDescription("MyType", IsValueType: false),
			PropertyType: new TypeDescription("MyType2"),
			Path: new EquatableArray<IPathPart>(
				[
					new MemberAccess("A", IsValueType: true),
					new MemberAccess("B"),
				]),
			SetterOptions: new SetterOptions(IsWritable: true),
			NullableContextEnabled: false,
			MethodType: InterceptedMethodType.SetBinding);

		var transformer = new ReferenceTypesConditionalAccessTransformer();
		var transformedBinding = transformer.Transform(binding);

		var transformedPath = new EquatableArray<IPathPart>(
			[
				new ConditionalAccess(new MemberAccess("A", IsValueType: true)),
				new MemberAccess("B"),
			]);
		Assert.Equal(transformedPath, transformedBinding.Path);
	}

	[Fact]
	public void WrapAccessInConditionalAccessWhenAllPartsAreReferenceTypes()
	{
		var binding = new BindingInvocationDescription(
			InterceptableLocation: new InterceptableLocationRecord(1, "serializedData"),
			SimpleLocation: new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			SourceType: new TypeDescription("MyType"),
			PropertyType: new TypeDescription("MyType2"),
			Path: new EquatableArray<IPathPart>(
				[
					new MemberAccess("A"),
					new IndexAccess("Item", 0),
					new MemberAccess("B"),
				]),
			SetterOptions: new SetterOptions(IsWritable: true),
			NullableContextEnabled: false,
			MethodType: InterceptedMethodType.SetBinding);

		var transformer = new ReferenceTypesConditionalAccessTransformer();
		var transformedBinding = transformer.Transform(binding);

		var transformedPath = new EquatableArray<IPathPart>(
			[
				new ConditionalAccess(new MemberAccess("A")),
				new ConditionalAccess(new IndexAccess("Item", 0)),
				new ConditionalAccess(new MemberAccess("B")),
			]);
		Assert.Equal(transformedPath, transformedBinding.Path);
	}

	[Fact]
	public void DoNotWrapAccessInConditionalAccessWhenNoPartsAreReferenceTypes()
	{
		var binding = new BindingInvocationDescription(
			InterceptableLocation: new InterceptableLocationRecord(1, "serializedData"),
			SimpleLocation: new SimpleLocation(@"Path\To\Program.cs", 3, 7),
			SourceType: new TypeDescription("MyType", IsValueType: true),
			PropertyType: new TypeDescription("MyType2"),
			Path: new EquatableArray<IPathPart>(
				[
					new MemberAccess("A", IsValueType: true),
					new IndexAccess("Item", 0, IsValueType: true),
					new MemberAccess("B", IsValueType: true),
				]),
			SetterOptions: new SetterOptions(IsWritable: true),
			NullableContextEnabled: false,
			MethodType: InterceptedMethodType.SetBinding);

		var transformer = new ReferenceTypesConditionalAccessTransformer();
		var transformedBinding = transformer.Transform(binding);

		var transformedPath = new EquatableArray<IPathPart>(
			[
				new MemberAccess("A", IsValueType: true),
				new IndexAccess("Item", 0, IsValueType: true),
				new MemberAccess("B", IsValueType: true),
			]);
		Assert.Equal(transformedPath, transformedBinding.Path);
	}
}
