using System;
using System.Linq;

using Mono.Cecil;

using Xamarin.Forms.Build.Tasks;

using NUnit.Framework;

namespace Xamarin.Forms.Xaml.XamlcUnitTests
{
	[TestFixture]
	public class PropertyDefinitionExtensionsTests
	{
		public class NonGenericClass
		{
			public object Property { get; set; }
		}

		public class GenericClass<T>
		{
			public object Property { get; set; }
			public T GenericProperty { get; set; }
		}

		ModuleDefinition module;

		[SetUp]
		public void SetUp ()
		{
			module = ModuleDefinition.CreateModule ("foo", ModuleKind.Dll);
		}

//		[Test]
//		public void ResolveGenericsOnNonGenericPreserveAccessors ()
//		{
//			var type = module.Import (typeof (NonGenericClass));
//			TypeReference declaringTypeReference;
//			PropertyDefinition prop = type.GetProperty (fd => fd.Name == "Property", out declaringTypeReference);
//			Assert.AreEqual ("System.Object", prop.PropertyType.FullName);
//			Assert.AreEqual ("System.Void Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/NonGenericClass::set_Property(System.Object)", prop.SetMethod.FullName);
//			Assert.AreEqual ("System.Object Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/NonGenericClass::get_Property()", prop.GetMethod.FullName);
//			Assert.AreEqual ("Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/NonGenericClass", prop.DeclaringType.FullName);
//
//			prop.ResolveGenericParameters (declaringTypeReference);
//
//			Assert.AreEqual ("System.Object", prop.PropertyType.FullName);
//			Assert.AreEqual ("System.Void Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/NonGenericClass::set_Property(System.Object)", prop.SetMethod.FullName);
//			Assert.AreEqual ("System.Object Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/NonGenericClass::get_Property()", prop.GetMethod.FullName);
//			Assert.AreEqual ("Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/NonGenericClass", prop.DeclaringType.FullName);
//
//		}
//
//		[Test]
//		public void NonGenericPropertyOnGenericType ()
//		{
//			var type = module.Import (typeof (GenericClass<bool>));
//			TypeReference declaringTypeReference;
//			PropertyDefinition prop = type.GetProperty (fd => fd.Name == "Property", out declaringTypeReference);
//			Assert.AreEqual ("System.Object", prop.PropertyType.FullName);
//			Assert.AreEqual ("System.Void Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/GenericClass`1::set_Property(System.Object)", prop.SetMethod.FullName);
//			Assert.AreEqual ("System.Object Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/GenericClass`1::get_Property()", prop.GetMethod.FullName);
//			Assert.AreEqual ("Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/GenericClass`1", prop.DeclaringType.FullName);
//			Assert.False (prop.DeclaringType.IsGenericInstance);
//
//			prop.ResolveGenericParameters (declaringTypeReference);
//			Assert.AreEqual ("System.Object", prop.PropertyType.FullName);
//			Assert.AreEqual ("System.Void Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/GenericClass`1::set_Property(System.Object)", prop.SetMethod.FullName);
//			Assert.AreEqual ("System.Object Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/GenericClass`1::get_Property()", prop.GetMethod.FullName);
//			Assert.AreEqual ("Xamarin.Forms.Xaml.XamlcUnitTests.PropertyDefinitionExtensionsTests/GenericClass`1", prop.DeclaringType.FullName);
//			Assert.True (prop.DeclaringType.IsGenericInstance);
//
//		}
	}
}