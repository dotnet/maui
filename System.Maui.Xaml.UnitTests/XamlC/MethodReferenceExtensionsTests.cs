using System.Linq;

using Mono.Cecil;

using System.Maui.Build.Tasks;

using NUnit.Framework;
using System.Collections.Generic;
using System.Maui.Xaml.UnitTests;

namespace System.Maui.XamlcUnitTests
{
	[TestFixture]
	public class MethodReferenceExtensionsTests
	{
		ModuleDefinition module;

		abstract class TestClass<T>
		{
			public abstract T UnresolvedGenericReturnType ();
			public abstract void CustmAttributeParameterMethod ([Parameter("Parameter")] int parameter);
			public abstract void UnresolvedGenericInstanceTypeMethod (TestClass<T> unresolved);
		}

		[SetUp]
		public void SetUp()
		{
			module = ModuleDefinition.CreateModule("foo", new ModuleParameters()
			{
				AssemblyResolver = new MockAssemblyResolver(),
				Kind = ModuleKind.Dll,
			});
		}

		[TearDown]
		public void TearDown()
		{
			module?.Dispose();
		}

		[Test]
		public void ResolveRowDefAdder ()
		{
			var propertyType = module.ImportReference(typeof (RowDefinitionCollection));
			var adderT = propertyType.GetMethods ((md, tr) => md.Name == "Add", module).Single ();
			var adder = adderT.Item1;
			var ptype = adderT.Item2;
			Assert.AreEqual ("System.Void System.Maui.DefinitionCollection`1::Add(T)", adder.FullName);
			Assert.AreEqual ("System.Maui.DefinitionCollection`1<System.Maui.RowDefinition>", ptype.FullName);
			var adderRef = module.ImportReference (adder);
			Assert.AreEqual ("System.Void System.Maui.DefinitionCollection`1::Add(T)", adderRef.FullName);
			adderRef = adderRef.ResolveGenericParameters (ptype, module);
			Assert.AreEqual ("System.Void System.Maui.DefinitionCollection`1<System.Maui.RowDefinition>::Add(T)", adderRef.FullName);
		}

		[Test]
		public void GenericGetter ()
		{
			TypeReference declaringTypeReference;
			var type = module.ImportReference (typeof (StackLayout));
			var property = type.GetProperty (pd => pd.Name == "Children", out declaringTypeReference);
			Assert.AreEqual ("System.Collections.Generic.IList`1<T> System.Maui.Layout`1::Children()", property.FullName);
			Assert.AreEqual ("System.Maui.Layout`1<System.Maui.View>", declaringTypeReference.FullName);
			var propertyGetter = property.GetMethod;
			Assert.AreEqual ("System.Collections.Generic.IList`1<T> System.Maui.Layout`1::get_Children()", propertyGetter.FullName);
			var propertyGetterRef = module.ImportReference (propertyGetter);
			Assert.AreEqual ("System.Collections.Generic.IList`1<T> System.Maui.Layout`1::get_Children()", propertyGetterRef.FullName);

			propertyGetterRef = module.ImportReference (propertyGetterRef.ResolveGenericParameters (declaringTypeReference, module));
			Assert.AreEqual ("System.Collections.Generic.IList`1<T> System.Maui.Layout`1<System.Maui.View>::get_Children()", propertyGetterRef.FullName);
			var returnType = propertyGetterRef.ReturnType.ResolveGenericParameters (declaringTypeReference);
			Assert.AreEqual ("System.Collections.Generic.IList`1<System.Maui.View>", returnType.FullName);
		}

		[Test]
		public void GetterWithGenericReturnType ()
		{
			TypeReference declaringTypeReference;
			var type = module.ImportReference (typeof (Style));
			var property = type.GetProperty (pd => pd.Name == "Setters", out declaringTypeReference);
			Assert.AreEqual ("System.Collections.Generic.IList`1<System.Maui.Setter> System.Maui.Style::Setters()", property.FullName);
			Assert.AreEqual ("System.Maui.Style", declaringTypeReference.FullName);
			var propertyGetter = property.GetMethod;
			Assert.AreEqual ("System.Collections.Generic.IList`1<System.Maui.Setter> System.Maui.Style::get_Setters()", propertyGetter.FullName);

			var propertyGetterRef = module.ImportReference (propertyGetter);
			Assert.AreEqual ("System.Collections.Generic.IList`1<System.Maui.Setter> System.Maui.Style::get_Setters()", propertyGetterRef.FullName);
			propertyGetterRef = module.ImportReference (propertyGetterRef.ResolveGenericParameters (declaringTypeReference, module));
			Assert.AreEqual ("System.Collections.Generic.IList`1<System.Maui.Setter> System.Maui.Style::get_Setters()", propertyGetterRef.FullName);
			var returnType = propertyGetterRef.ReturnType.ResolveGenericParameters (declaringTypeReference);
			Assert.AreEqual ("System.Collections.Generic.IList`1<System.Maui.Setter>", returnType.FullName);
		}

		[Test]
		public void ResolveChildren ()
		{
			var propertyType = module.ImportReference (typeof (IList<View>));
			var adderT = propertyType.GetMethods (md => md.Name == "Add" && md.Parameters.Count == 1, module).Single ();
			var adder = adderT.Item1;
			var ptype = adderT.Item2;
			Assert.AreEqual ("System.Void System.Collections.Generic.ICollection`1::Add(T)", adder.FullName);
			Assert.AreEqual ("System.Collections.Generic.ICollection`1<System.Maui.View>", ptype.FullName);
			var adderRef = module.ImportReference (adder);
			Assert.AreEqual ("System.Void System.Collections.Generic.ICollection`1::Add(T)", adderRef.FullName);
			adderRef = adderRef.ResolveGenericParameters (ptype, module);
			Assert.AreEqual ("System.Void System.Collections.Generic.ICollection`1<System.Maui.View>::Add(T)", adderRef.FullName);
		}

		[Test]
		public void GenericParameterReturnType ()
		{
			var type = module.ImportReference (typeof (TestClass<int>));
			var method = type.Resolve ().Methods.Where (md => md.Name == "UnresolvedGenericReturnType").Single ();
			var resolved = method.ResolveGenericParameters (type, module);

			Assert.AreEqual ("T", resolved.ReturnType.Name);
		}

		[Test]
		public void CustomAttributes ()
		{
			var type = module.ImportReference (typeof (TestClass<int>));
			var method = type.Resolve ().Methods.Where (md => md.Name == "CustmAttributeParameterMethod").Single ();
			var resolved = method.ResolveGenericParameters (type, module);

			Assert.AreEqual ("System.Maui.ParameterAttribute", resolved.Parameters[0].CustomAttributes[0].AttributeType.FullName);
		}

		[Test]
		public void ImportUnresolvedGenericInstanceType ()
		{
			var type = module.ImportReference (typeof (TestClass<int>));
			var method = type.Resolve ().Methods.Where (md => md.Name == "UnresolvedGenericInstanceTypeMethod").Single ();
			var resolved = method.ResolveGenericParameters (type, module);

			Assert.AreEqual ("foo", resolved.Parameters[0].ParameterType.Module.Name);
		}
	}
}