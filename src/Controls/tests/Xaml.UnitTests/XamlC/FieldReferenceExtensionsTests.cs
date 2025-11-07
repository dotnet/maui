using System;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml.UnitTests;
using Mono.Cecil;
using Xunit;

namespace Microsoft.Maui.Controls.XamlcUnitTests
{

	public class FieldReferenceExtensionsTests
	{
		public class NonGenericClass
		{
			public object Field;
		}

		public class GenericClass<T1, T2>
		{
			public object NonGenericField;
			public T1 GenericField1;
			public T2 GenericField2;
		}

		public class Inheritor : GenericClass<string, double>
		{
		}

		ModuleDefinition module;

		public FieldReferenceExtensionsTests()
		{
			module = ModuleDefinition.CreateModule("foo", new ModuleParameters()
			{
				AssemblyResolver = new MockAssemblyResolver(),
				Kind = ModuleKind.Dll,
			});
		}

		public void Dispose()
		{
			module?.Dispose();
		}

		[Fact]
		public void ResolveGenericsOnNonGenericDoesNotThrow()
		{
			var type = module.ImportReference(typeof(NonGenericClass));
			TypeReference declaringTypeReference;
			FieldDefinition field = type.GetField(new XamlCache(), fd => fd.Name == "Field", out declaringTypeReference);
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => field.ResolveGenericParameters(declaringTypeReference));
		}

		[Fact]
		public void NonGenericFieldOnGenericType()
		{
			var type = module.ImportReference(typeof(Inheritor));
			TypeReference declaringTypeReference;
			FieldDefinition field = type.GetField(new XamlCache(), fd => fd.Name == "NonGenericField", out declaringTypeReference);
			Assert.Equal("NonGenericField", field.Name);
			Assert.Equal("Microsoft.Maui.Controls.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2", field.DeclaringType.FullName);
			Assert.False(field.DeclaringType.IsGenericInstance);
			var genericField = field.ResolveGenericParameters(declaringTypeReference);
			Assert.Equal("NonGenericField", genericField.Name);
			Assert.Equal("Microsoft.Maui.Controls.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2<System.String,System.Double>", genericField.DeclaringType.FullName);
			Assert.True(genericField.DeclaringType.IsGenericInstance);
		}

		[Fact]
		public void GenericFieldOnGenericType()
		{
			var cache = new XamlCache();
			var type = module.ImportReference(typeof(Inheritor));
			TypeReference declaringTypeReference;

			FieldDefinition field1 = type.GetField(cache, fd => fd.Name == "GenericField1", out declaringTypeReference);
			Assert.Equal("GenericField1", field1.Name);
			Assert.Equal("Microsoft.Maui.Controls.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2", field1.DeclaringType.FullName);
			Assert.False(field1.DeclaringType.IsGenericInstance);
			Assert.True(field1.FieldType.IsGenericParameter);
			Assert.Equal("T1", field1.FieldType.FullName);

			var genericField1 = field1.ResolveGenericParameters(declaringTypeReference);
			Assert.Equal("GenericField1", genericField1.Name);
			Assert.Equal("Microsoft.Maui.Controls.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2<System.String,System.Double>", genericField1.DeclaringType.FullName);
			Assert.True(genericField1.DeclaringType.IsGenericInstance);
			Assert.False(genericField1.FieldType.IsGenericParameter);
			Assert.Equal("System.String", genericField1.FieldType.FullName);

			FieldDefinition field2 = type.GetField(cache, fd => fd.Name == "GenericField2", out declaringTypeReference);
			Assert.Equal("GenericField2", field2.Name);
			Assert.Equal("Microsoft.Maui.Controls.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2", field2.DeclaringType.FullName);
			Assert.False(field2.DeclaringType.IsGenericInstance);
			Assert.True(field2.FieldType.IsGenericParameter);
			Assert.Equal("T2", field2.FieldType.FullName);

			var genericField2 = field2.ResolveGenericParameters(declaringTypeReference);
			Assert.Equal("GenericField2", genericField2.Name);
			Assert.Equal("Microsoft.Maui.Controls.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2<System.String,System.Double>", genericField2.DeclaringType.FullName);
			Assert.True(genericField2.DeclaringType.IsGenericInstance);
			Assert.False(genericField2.FieldType.IsGenericParameter);
			Assert.Equal("System.Double", genericField2.FieldType.FullName);
		}
	}
}
