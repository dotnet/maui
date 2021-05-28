using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using NUnit.Framework;

namespace Microsoft.Maui.Controls
{
	public class Effect
	{
	}
}
namespace Microsoft.Maui.Controls.XamlcUnitTests
{
	[TestFixture]
	public class TypeReferenceExtensionsTests
	{
		class Foo
		{
		}

		class Foo<T> : Foo
		{
		}

		class Bar<T> : Foo<T>
		{
		}

		class Baz<T1, T2> : Foo<T1>
		{
		}

		class Qux<T> : Baz<int, T>
		{
		}

		class Quux<T> : Foo<Foo<T>>
		{
		}

		class Corge<T> : Foo<Foo<Foo<T>>>
		{
		}

		abstract class Grault
		{
			public abstract void Method<T>(T t);
		}

		abstract class Garply<T> : Grault<T>
		{
			public abstract void Method(T t);
		}

		abstract class Waldo<T>
		{
			public abstract void Method(Foo<T> t);
		}

		interface IGrault<T>
		{
		}

		class Grault<T> : IGrault<T>
		{
		}

		interface ICovariant<out T>
		{
		}

		interface IContravariant<in T>
		{
		}

		class Covariant<T> : ICovariant<T>
		{
		}

		XamlCAssemblyResolver resolver;
		ModuleDefinition module;

		[OneTimeSetUp]
		public void OneTimeSetUp()
		{
			resolver = new XamlCAssemblyResolver();
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(TypeReferenceExtensionsTests).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(BindableObject).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(object).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(IList<>).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(Queue<>).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(IViewHandler).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(Color).Assembly.CodeBase)).Path));

			module = ModuleDefinition.CreateModule("foo", new ModuleParameters
			{
				AssemblyResolver = resolver,
				Kind = ModuleKind.NetModule
			});
		}

		[OneTimeTearDown]
		public void OneTimeTearDown()
		{
			resolver?.Dispose();
			module?.Dispose();
		}

		[TestCase(typeof(bool), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(Dictionary<string, string>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(List<string>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(List<string>), typeof(IEnumerable<string>), ExpectedResult = true)]
		[TestCase(typeof(List<Button>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(Queue<KeyValuePair<string, string>>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(double), typeof(double), ExpectedResult = true)]
		[TestCase(typeof(object), typeof(IList<TriggerBase>), ExpectedResult = false)]
		[TestCase(typeof(object), typeof(double), ExpectedResult = false)]
		[TestCase(typeof(object), typeof(int?), ExpectedResult = false)]
		[TestCase(typeof(object), typeof(object), ExpectedResult = true)]
		[TestCase(typeof(sbyte), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(string[]), typeof(System.Collections.IEnumerable), ExpectedResult = true)]
		[TestCase(typeof(string[]), typeof(object), ExpectedResult = true)]
		[TestCase(typeof(string[]), typeof(string), ExpectedResult = false)]
		[TestCase(typeof(string[]), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(string[]), typeof(IEnumerable<string>), ExpectedResult = true)]
		[TestCase(typeof(Type), typeof(object), ExpectedResult = true)]
		[TestCase(typeof(Type), typeof(Type), ExpectedResult = true)]
		[TestCase(typeof(Type), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(System.Windows.Input.ICommand), typeof(System.Windows.Input.ICommand), ExpectedResult = true)]
		[TestCase(typeof(System.Windows.Input.ICommand), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(BindingBase), typeof(BindingBase), ExpectedResult = true)]
		[TestCase(typeof(BindingCondition), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(Button), typeof(BindableObject), ExpectedResult = true)]
		[TestCase(typeof(Button), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(Button), typeof(View), ExpectedResult = true)]
		[TestCase(typeof(Color), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(Color), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(Color), typeof(Color), ExpectedResult = true)]
		[TestCase(typeof(ColumnDefinition), typeof(BindableObject), ExpectedResult = true)]
		[TestCase(typeof(ColumnDefinition), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(ColumnDefinition), typeof(ColumnDefinitionCollection), ExpectedResult = false)]
		[TestCase(typeof(Constraint), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(Constraint), typeof(Constraint), ExpectedResult = true)]
		[TestCase(typeof(ConstraintExpression), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(ContentPage), typeof(BindableObject), ExpectedResult = true)]
		[TestCase(typeof(ContentPage), typeof(Page), ExpectedResult = true)]
		[TestCase(typeof(ContentView), typeof(BindableObject), ExpectedResult = true)]
		[TestCase(typeof(ContentView[]), typeof(IList<ContentView>), ExpectedResult = true)]
		[TestCase(typeof(MultiTrigger), typeof(IList<TriggerBase>), ExpectedResult = false)]
		[TestCase(typeof(OnIdiom<double>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(OnPlatform<string>), typeof(string), ExpectedResult = false)]
		[TestCase(typeof(OnPlatform<string>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(OnPlatform<string>), typeof(BindingBase), ExpectedResult = false)]
		[TestCase(typeof(OnPlatform<FontAttributes>), typeof(BindableObject), ExpectedResult = false)]
		[TestCase(typeof(StackLayout), typeof(Layout<View>), ExpectedResult = true)]
		[TestCase(typeof(StackLayout), typeof(View), ExpectedResult = true)]
		[TestCase(typeof(Foo<string>), typeof(Foo), ExpectedResult = true)]
		[TestCase(typeof(Bar<string>), typeof(Foo), ExpectedResult = true)]
		[TestCase(typeof(Bar<string>), typeof(Foo<bool>), ExpectedResult = false)]
		[TestCase(typeof(Bar<string>), typeof(Foo<string>), ExpectedResult = true)]
		[TestCase(typeof(Qux<string>), typeof(double), ExpectedResult = false)] //https://github.com/xamarin/Microsoft.Maui.Controls/issues/1497
		[TestCase(typeof(IGrault<object>), typeof(IGrault<string>), ExpectedResult = false)]
		[TestCase(typeof(IGrault<string>), typeof(IGrault<object>), ExpectedResult = false)]
		[TestCase(typeof(ICovariant<object>), typeof(ICovariant<string>), ExpectedResult = false)]
		[TestCase(typeof(ICovariant<string>), typeof(ICovariant<object>), ExpectedResult = true)]
		[TestCase(typeof(IContravariant<object>), typeof(IContravariant<string>), ExpectedResult = true)]
		[TestCase(typeof(IContravariant<string>), typeof(IContravariant<object>), ExpectedResult = false)]
		[TestCase(typeof(Covariant<object>), typeof(ICovariant<string>), ExpectedResult = false)]
		[TestCase(typeof(Covariant<string>), typeof(ICovariant<object>), ExpectedResult = true)]
		public bool TestInheritsFromOrImplements(Type typeRef, Type baseClass)
		{
			return TypeReferenceExtensions.InheritsFromOrImplements(module.ImportReference(typeRef), module.ImportReference(baseClass));
		}

		[Test]
		public void TestSameTypeNamesFromDifferentAssemblies()
		{
			var core = typeof(BindableObject).Assembly;
			var test = typeof(TypeReferenceExtensionsTests).Assembly;

			Assert.False(TestInheritsFromOrImplements(test.GetType("Microsoft.Maui.Controls.Effect"), core.GetType("Microsoft.Maui.Controls.Effect")));
		}

		[Test]
		public void TestResolveSelectedGenericParameter()
		{
			var imported = module.ImportReference(typeof(Bar<byte>));
			var baseType = (GenericInstanceType)imported.Resolve().BaseType;
			var resolvedType = baseType.GenericArguments[0].ResolveGenericParameters(imported);

			Assert.AreEqual("System", resolvedType.Namespace);
			Assert.AreEqual("Byte", resolvedType.Name);
		}

		[TestCase(typeof(Bar<byte>), 1)]
		[TestCase(typeof(Quux<byte>), 2)]
		[TestCase(typeof(Corge<byte>), 3)]
		public void TestResolveGenericParameters(Type typeRef, int depth)
		{
			var imported = module.ImportReference(typeRef);
			var resolvedType = imported.Resolve().BaseType.ResolveGenericParameters(imported);

			for (var count = 0; count < depth; count++)
			{
				resolvedType = ((GenericInstanceType)resolvedType).GenericArguments[0];
			}

			Assert.AreEqual("System", resolvedType.Namespace);
			Assert.AreEqual("Byte", resolvedType.Name);
		}

		public void TestResolveGenericParametersOfGenericMethod()
		{
			var method = new GenericInstanceMethod(module.ImportReference(typeof(Grault)).Resolve().Methods[0]);
			method.GenericArguments.Add(module.TypeSystem.Byte);
			var resolved = method.ReturnType.ResolveGenericParameters(method);

			Assert.That(TypeRefComparer.Default.Equals(module.TypeSystem.Byte, resolved));
		}

		[TestCase(typeof(Garply<byte>), typeof(byte))]
		[TestCase(typeof(Waldo<byte>), typeof(Foo<byte>))]
		public void TestResolveGenericParametersOfMethodOfGeneric(Type typeRef, Type returnType)
		{
			var type = module.ImportReference(typeRef);
			var method = type.Resolve().Methods[0].ResolveGenericParameters(type, module);
			var resolved = method.Parameters[0].ParameterType.ResolveGenericParameters(method);

			Assert.That(TypeRefComparer.Default.Equals(module.ImportReference(returnType), resolved));
		}

		[Test]
		public void TestImplementsGenericInterface()
		{
			GenericInstanceType igrault;
			IList<TypeReference> arguments;
			var garply = module.ImportReference(typeof(Garply<System.Byte>));

			Assert.That(garply.ImplementsGenericInterface("Microsoft.Maui.Controls.XamlcUnitTests.TypeReferenceExtensionsTests/IGrault`1<T>", out igrault, out arguments));

			Assert.AreEqual("System", igrault.GenericArguments[0].Namespace);
			Assert.AreEqual("Byte", igrault.GenericArguments[0].Name);
			Assert.AreEqual("System", arguments[0].Namespace);
			Assert.AreEqual("Byte", arguments[0].Name);
		}
	}
}
