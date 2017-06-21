using System;
using System.Linq;

using Mono.Cecil;

using Xamarin.Forms.Build.Tasks;

using NUnit.Framework;
using System.Collections.Generic;

namespace Xamarin.Forms.Xaml.XamlcUnitTests
{
	[TestFixture]
	public class PropertyDefinitionExtensionsTests
	{
		public class NonGenericClass
		{
			public object Property { get; set; }
		}

		public class GenericClass<T, U, V>
		{
			public object Property { get; set; }
			public T GenericT { get; set; }
			public U GenericU { get; set; }
			public V GenericV { get; set; }
			public IEnumerable<T> EnumerableT { get; set; }
			public KeyValuePair<V, U> KvpVU { get; set; }
		}

		ModuleDefinition module;

		[SetUp]
		public void SetUp ()
		{
			module = ModuleDefinition.CreateModule ("foo", ModuleKind.Dll);
		}

		[Test]
		public void ResolveGenericPropertyType ()
		{
			var type = module.ImportReference (typeof (GenericClass<bool, string, int>));
			TypeReference declaringTypeReference;
			var prop = type.GetProperty (fd => fd.Name == "Property", out declaringTypeReference);
			var propertyType = prop.ResolveGenericPropertyType (declaringTypeReference, module);
			Assert.AreEqual ("System.Object", propertyType.FullName);

			prop = type.GetProperty(fd => fd.Name == "GenericT", out declaringTypeReference);
			propertyType = prop.ResolveGenericPropertyType(declaringTypeReference, module);
			Assert.AreEqual("System.Boolean", propertyType.FullName);

			prop = type.GetProperty(fd => fd.Name == "GenericU", out declaringTypeReference);
			propertyType = prop.ResolveGenericPropertyType(declaringTypeReference, module);
			Assert.AreEqual("System.String", propertyType.FullName);

			prop = type.GetProperty(fd => fd.Name == "GenericV", out declaringTypeReference);
			propertyType = prop.ResolveGenericPropertyType(declaringTypeReference, module);
			Assert.AreEqual("System.Int32", propertyType.FullName);

		}
	}
}