using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Xaml.UnitTests;
using Mono.Cecil;
using Xunit;

namespace Microsoft.Maui.Controls.XamlcUnitTests
{
	[Collection("Xaml Inflation")]
	public class MethodReferenceExtensionsTests : IDisposable
	{
		ModuleDefinition module;

		abstract class TestClass<T>
		{
			public abstract T UnresolvedGenericReturnType();
			public abstract void CustmAttributeParameterMethod([Parameter("Parameter")] int parameter);
			public abstract void UnresolvedGenericInstanceTypeMethod(TestClass<T> unresolved);
		}

		public MethodReferenceExtensionsTests()
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
		public void ResolveRowDefAdder()
		{
			var propertyType = module.ImportReference(typeof(RowDefinitionCollection));
			var adderT = propertyType.GetMethods(new XamlCache(), (md, tr) => md.Name == "Add", module).Single();
			var adder = adderT.Item1;
			var ptype = adderT.Item2;
			Assert.Equal("System.Void Microsoft.Maui.Controls.DefinitionCollection`1::Add(T)", adder.FullName);
			Assert.Equal("Microsoft.Maui.Controls.DefinitionCollection`1<Microsoft.Maui.Controls.RowDefinition>", ptype.FullName);
			var adderRef = module.ImportReference(adder);
			Assert.Equal("System.Void Microsoft.Maui.Controls.DefinitionCollection`1::Add(T)", adderRef.FullName);
			adderRef = adderRef.ResolveGenericParameters(ptype, module);
			Assert.Equal("System.Void Microsoft.Maui.Controls.DefinitionCollection`1<Microsoft.Maui.Controls.RowDefinition>::Add(T)", adderRef.FullName);
		}

		[Fact]
		public void GenericGetter()
		{
			TypeReference declaringTypeReference;
			var type = module.ImportReference(typeof(Compatibility.StackLayout));
			var property = type.GetProperty(new XamlCache(), pd => pd.Name == "Children", out declaringTypeReference);
			Assert.Equal("System.Collections.Generic.IList`1<T> Microsoft.Maui.Controls.Compatibility.Layout`1::Children()", property.FullName);
			Assert.Equal("Microsoft.Maui.Controls.Compatibility.Layout`1<Microsoft.Maui.Controls.View>", declaringTypeReference.FullName);
			var propertyGetter = property.GetMethod;
			Assert.Equal("System.Collections.Generic.IList`1<T> Microsoft.Maui.Controls.Compatibility.Layout`1::get_Children()", propertyGetter.FullName);
			var propertyGetterRef = module.ImportReference(propertyGetter);
			Assert.Equal("System.Collections.Generic.IList`1<T> Microsoft.Maui.Controls.Compatibility.Layout`1::get_Children()", propertyGetterRef.FullName);

			propertyGetterRef = module.ImportReference(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
			Assert.Equal("System.Collections.Generic.IList`1<T> Microsoft.Maui.Controls.Compatibility.Layout`1<Microsoft.Maui.Controls.View>::get_Children()", propertyGetterRef.FullName);
			var returnType = propertyGetterRef.ReturnType.ResolveGenericParameters(declaringTypeReference);
			Assert.Equal("System.Collections.Generic.IList`1<Microsoft.Maui.Controls.View>", returnType.FullName);
		}

		[Fact]
		public void GetterWithGenericReturnType()
		{
			TypeReference declaringTypeReference;
			var type = module.ImportReference(typeof(Style));
			var property = type.GetProperty(new XamlCache(), pd => pd.Name == "Setters", out declaringTypeReference);
			Assert.Equal("System.Collections.Generic.IList`1<Microsoft.Maui.Controls.Setter> Microsoft.Maui.Controls.Style::Setters()", property.FullName);
			Assert.Equal("Microsoft.Maui.Controls.Style", declaringTypeReference.FullName);
			var propertyGetter = property.GetMethod;
			Assert.Equal("System.Collections.Generic.IList`1<Microsoft.Maui.Controls.Setter> Microsoft.Maui.Controls.Style::get_Setters()", propertyGetter.FullName);

			var propertyGetterRef = module.ImportReference(propertyGetter);
			Assert.Equal("System.Collections.Generic.IList`1<Microsoft.Maui.Controls.Setter> Microsoft.Maui.Controls.Style::get_Setters()", propertyGetterRef.FullName);
			propertyGetterRef = module.ImportReference(propertyGetterRef.ResolveGenericParameters(declaringTypeReference, module));
			Assert.Equal("System.Collections.Generic.IList`1<Microsoft.Maui.Controls.Setter> Microsoft.Maui.Controls.Style::get_Setters()", propertyGetterRef.FullName);
			var returnType = propertyGetterRef.ReturnType.ResolveGenericParameters(declaringTypeReference);
			Assert.Equal("System.Collections.Generic.IList`1<Microsoft.Maui.Controls.Setter>", returnType.FullName);
		}

		[Fact]
		public void ResolveChildren()
		{
			var propertyType = module.ImportReference(typeof(IList<View>));
			var adderT = propertyType.GetMethods(new XamlCache(), md => md.Name == "Add" && md.Parameters.Count == 1, module).Single();
			var adder = adderT.Item1;
			var ptype = adderT.Item2;
			Assert.Equal("System.Void System.Collections.Generic.ICollection`1::Add(T)", adder.FullName);
			Assert.Equal("System.Collections.Generic.ICollection`1<Microsoft.Maui.Controls.View>", ptype.FullName);
			var adderRef = module.ImportReference(adder);
			Assert.Equal("System.Void System.Collections.Generic.ICollection`1::Add(T)", adderRef.FullName);
			adderRef = adderRef.ResolveGenericParameters(ptype, module);
			Assert.Equal("System.Void System.Collections.Generic.ICollection`1<Microsoft.Maui.Controls.View>::Add(T)", adderRef.FullName);
		}

		[Fact]
		public void GenericParameterReturnType()
		{
			var type = module.ImportReference(typeof(TestClass<int>));
			var method = type.Resolve().Methods.Where(md => md.Name == "UnresolvedGenericReturnType").Single();
			var resolved = method.ResolveGenericParameters(type, module);

			Assert.Equal("T", resolved.ReturnType.Name);
		}

		[Fact]
		public void CustomAttributes()
		{
			var type = module.ImportReference(typeof(TestClass<int>));
			var method = type.Resolve().Methods.Where(md => md.Name == "CustmAttributeParameterMethod").Single();
			var resolved = method.ResolveGenericParameters(type, module);

			Assert.Equal("Microsoft.Maui.Controls.ParameterAttribute", resolved.Parameters[0].CustomAttributes[0].AttributeType.FullName);
		}

		[Fact]
		public void ImportUnresolvedGenericInstanceType()
		{
			var type = module.ImportReference(typeof(TestClass<int>));
			var method = type.Resolve().Methods.Where(md => md.Name == "UnresolvedGenericInstanceTypeMethod").Single();
			var resolved = method.ResolveGenericParameters(type, module);

			Assert.Equal("foo", resolved.Parameters[0].ParameterType.Module.Name);
		}
	}
}