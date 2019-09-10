using System;
using System.Linq;

using Mono.Cecil;

using Xamarin.Forms.Build.Tasks;

using NUnit.Framework;
using Xamarin.Forms.Xaml.UnitTests;

namespace Xamarin.Forms.XamlcUnitTests
{
	[TestFixture]
	public class FieldReferenceExtensionsTests
	{
		public class NonGenericClass {
			public object Field;
		}

		public class GenericClass<T1,T2> {
			public object NonGenericField;
			public T1 GenericField1;
			public T2 GenericField2;
		}

		public class Inheritor : GenericClass<string,double>
		{
		}

		ModuleDefinition module;

		[SetUp]
		public void SetUp ()
		{
			module = ModuleDefinition.CreateModule("foo", new ModuleParameters()
			{
				AssemblyResolver = new MockAssemblyResolver(),
				Kind = ModuleKind.Dll,
			});
		}

		[Test]
		public void ResolveGenericsOnNonGenericDoesNotThrow ()
		{
			var type = module.ImportReference (typeof (NonGenericClass));
			TypeReference declaringTypeReference;
			FieldDefinition field = type.GetField (fd => fd.Name == "Field", out declaringTypeReference);
			Assert.DoesNotThrow (() => field.ResolveGenericParameters (declaringTypeReference));
		}

		[Test]
		public void NonGenericFieldOnGenericType ()
		{
			var type = module.ImportReference (typeof (Inheritor));
			TypeReference declaringTypeReference;
			FieldDefinition field = type.GetField (fd => fd.Name == "NonGenericField", out declaringTypeReference);
			Assert.AreEqual ("NonGenericField", field.Name);
			Assert.AreEqual ("Xamarin.Forms.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2", field.DeclaringType.FullName);
			Assert.False (field.DeclaringType.IsGenericInstance);
			var genericField = field.ResolveGenericParameters (declaringTypeReference);
			Assert.AreEqual ("NonGenericField", genericField.Name);
			Assert.AreEqual ("Xamarin.Forms.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2<System.String,System.Double>", genericField.DeclaringType.FullName);
			Assert.True (genericField.DeclaringType.IsGenericInstance);
		}

		[Test]
		public void GenericFieldOnGenericType ()
		{
			var type = module.ImportReference (typeof (Inheritor));
			TypeReference declaringTypeReference;

			FieldDefinition field1 = type.GetField (fd => fd.Name == "GenericField1", out declaringTypeReference);
			Assert.AreEqual ("GenericField1", field1.Name);
			Assert.AreEqual ("Xamarin.Forms.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2", field1.DeclaringType.FullName);
			Assert.False (field1.DeclaringType.IsGenericInstance);
			Assert.True (field1.FieldType.IsGenericParameter);
			Assert.AreEqual ("T1", field1.FieldType.FullName);

			var genericField1 = field1.ResolveGenericParameters (declaringTypeReference);
			Assert.AreEqual ("GenericField1", genericField1.Name);
			Assert.AreEqual ("Xamarin.Forms.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2<System.String,System.Double>", genericField1.DeclaringType.FullName);
			Assert.True (genericField1.DeclaringType.IsGenericInstance);
			Assert.False (genericField1.FieldType.IsGenericParameter);
			Assert.AreEqual ("System.String", genericField1.FieldType.FullName);

			FieldDefinition field2 = type.GetField (fd => fd.Name == "GenericField2", out declaringTypeReference);
			Assert.AreEqual ("GenericField2", field2.Name);
			Assert.AreEqual ("Xamarin.Forms.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2", field2.DeclaringType.FullName);
			Assert.False (field2.DeclaringType.IsGenericInstance);
			Assert.True (field2.FieldType.IsGenericParameter);
			Assert.AreEqual ("T2", field2.FieldType.FullName);

			var genericField2 = field2.ResolveGenericParameters (declaringTypeReference);
			Assert.AreEqual ("GenericField2", genericField2.Name);
			Assert.AreEqual ("Xamarin.Forms.XamlcUnitTests.FieldReferenceExtensionsTests/GenericClass`2<System.String,System.Double>", genericField2.DeclaringType.FullName);
			Assert.True (genericField2.DeclaringType.IsGenericInstance);
			Assert.False (genericField2.FieldType.IsGenericParameter);
			Assert.AreEqual ("System.Double", genericField2.FieldType.FullName);
		}
	}
}