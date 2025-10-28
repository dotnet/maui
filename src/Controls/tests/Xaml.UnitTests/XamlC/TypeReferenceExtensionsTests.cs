using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Graphics;
using Mono.Cecil;
using Xunit;

namespace Microsoft.Maui.Controls
{
	public class Effect
	{
	}
}
namespace Microsoft.Maui.Controls.XamlcUnitTests
{
	using Constraint = Microsoft.Maui.Controls.Compatibility.Constraint;
	using ConstraintExpression = Microsoft.Maui.Controls.Compatibility.ConstraintExpression;
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;	public class TypeReferenceExtensionsTests
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


		class Zoo<T> : IGrault<T[]>
		{ }

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

		// NOTE: xUnit doesn't have OneTimeSetUp. This may need to use ICollectionFixture or ModuleInitializer.
		// [OneTimeSetUp]
		public void OneTimeSetUp()
		{
			resolver = new XamlCAssemblyResolver();
			resolver.AddAssembly(typeof(TypeReferenceExtensionsTests).Assembly.Location);
			resolver.AddAssembly(typeof(BindableObject).Assembly.Location);
			resolver.AddAssembly(typeof(object).Assembly.Location);
			resolver.AddAssembly(typeof(IList<>).Assembly.Location);
			resolver.AddAssembly(typeof(Queue<>).Assembly.Location);
			resolver.AddAssembly(typeof(IViewHandler).Assembly.Location);
			resolver.AddAssembly(typeof(Color).Assembly.Location);

			module = ModuleDefinition.CreateModule("foo", new ModuleParameters
			{
				AssemblyResolver = resolver,
				Kind = ModuleKind.NetModule
			});
		}

		// NOTE: xUnit doesn't have OneTimeTearDown.
		// [OneTimeTearDown]
		public void OneTimeTearDown()
		{
			resolver?.Dispose();
			module?.Dispose();
		}

		[InlineData(typeof(bool), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Dictionary<string, string>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(List<string>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(List<string>), typeof(IEnumerable<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(List<Button>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Queue<KeyValuePair<string, string>>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(double), typeof(double), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(object), typeof(IList<TriggerBase>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(object), typeof(double), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(object), typeof(int?), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(object), typeof(object), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(sbyte), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(string[]), typeof(System.Collections.IEnumerable), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(string[]), typeof(object), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(string[]), typeof(string), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(string[]), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(string[]), typeof(IEnumerable<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Type), typeof(object), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Type), typeof(Type), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Type), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(System.Windows.Input.ICommand), typeof(System.Windows.Input.ICommand), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(System.Windows.Input.ICommand), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(BindingBase), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(BindingCondition), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Button), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Button), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Button), typeof(View), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Color), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Color), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Color), typeof(Color), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(ColumnDefinition), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(ColumnDefinition), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(ColumnDefinition), typeof(ColumnDefinitionCollection), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Constraint), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Constraint), typeof(Constraint), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(ConstraintExpression), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(ContentPage), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(ContentPage), typeof(Page), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(ContentView), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(ContentView[]), typeof(IList<ContentView>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(MultiTrigger), typeof(IList<TriggerBase>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(OnIdiom<double>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(OnPlatform<string>), typeof(string), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(OnPlatform<string>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(OnPlatform<string>), typeof(BindingBase), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(OnPlatform<FontAttributes>), typeof(BindableObject), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(StackLayout), typeof(Controls.Compatibility.Layout<View>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(StackLayout), typeof(View), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Foo<string>), typeof(Foo), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Bar<string>), typeof(Foo), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Bar<string>), typeof(Foo<bool>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Bar<string>), typeof(Foo<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(Qux<string>), typeof(double), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)] //https://github.com/xamarin/Microsoft.Maui.Controls/issues/1497
		[InlineData(typeof(IGrault<object>), typeof(IGrault<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(IGrault<string>), typeof(IGrault<object>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(ICovariant<object>), typeof(ICovariant<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(ICovariant<string>), typeof(ICovariant<object>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(IContravariant<object>), typeof(IContravariant<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		[InlineData(typeof(IContravariant<string>), typeof(IContravariant<object>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Covariant<object>), typeof(ICovariant<string>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = false)]
		[InlineData(typeof(Covariant<string>), typeof(ICovariant<object>), // NOTE: xUnit doesnt support ExpectedResult, convert to assertion. // NOTE: xUnit does not support ExpectedResult, convert to assertion. ExpectedResult = true)]
		public bool TestInheritsFromOrImplements(Type typeRef, Type baseClass)
		{
			return TypeReferenceExtensions.InheritsFromOrImplements(module.ImportReference(typeRef), new XamlCache(), module.ImportReference(baseClass));
		}

		[Fact]
		public void TestSameTypeNamesFromDifferentAssemblies()
		{
			var core = typeof(BindableObject).Assembly;
			var test = typeof(TypeReferenceExtensionsTests).Assembly;

			Assert.False(TestInheritsFromOrImplements(test.GetType("Microsoft.Maui.Controls.Effect"), core.GetType("Microsoft.Maui.Controls.Effect")));
		}

		[Fact]
		public void TestResolveSelectedGenericParameter()
		{
			var imported = module.ImportReference(typeof(Bar<byte>));
			var baseType = (GenericInstanceType)imported.Resolve().BaseType;
			var resolvedType = baseType.GenericArguments[0].ResolveGenericParameters(imported);

			Assert.Equal("System", resolvedType.Namespace);
			Assert.Equal("Byte", resolvedType.Name);
		}

		[InlineData(typeof(Bar<byte>), 1)]
		[InlineData(typeof(Quux<byte>), 2)]
		[InlineData(typeof(Corge<byte>), 3)]
		public void TestResolveGenericParameters(Type typeRef, int depth)
		{
			var imported = module.ImportReference(typeRef);
			var resolvedType = imported.Resolve().BaseType.ResolveGenericParameters(imported);

			for (var count = 0; count < depth; count++)
			{
				resolvedType = ((GenericInstanceType)resolvedType).GenericArguments[0];
			}

			Assert.Equal("System", resolvedType.Namespace);
			Assert.Equal("Byte", resolvedType.Name);
		}

		public void TestResolveGenericParametersOfGenericMethod()
		{
			var method = new GenericInstanceMethod(module.ImportReference(typeof(Grault)).Resolve().Methods[0]);
			method.GenericArguments.Add(module.TypeSystem.Byte);
			var resolved = method.ReturnType.ResolveGenericParameters(method);

			Assert.True(TypeRefComparer.Default.Equals(module.TypeSystem.Byte, resolved));
		}

		[InlineData(typeof(Garply<byte>), typeof(byte))]
		[InlineData(typeof(Waldo<byte>), typeof(Foo<byte>))]
		public void TestResolveGenericParametersOfMethodOfGeneric(Type typeRef, Type returnType)
		{
			var type = module.ImportReference(typeRef);
			var method = type.Resolve().Methods[0].ResolveGenericParameters(type, module);
			var resolved = method.Parameters[0].ParameterType.ResolveGenericParameters(method);

			Assert.True(TypeRefComparer.Default.Equals(module.ImportReference(returnType), resolved));
		}

		[Fact]
		public void TestImplementsGenericInterface()
		{
			GenericInstanceType igrault;
			IList<TypeReference> arguments;
			var garply = module.ImportReference(typeof(Garply<System.Byte>));

			Assert.True(garply.ImplementsGenericInterface(new XamlCache(), "Microsoft.Maui.Controls.XamlcUnitTests.TypeReferenceExtensionsTests/IGrault`1", out igrault, out arguments));

			Assert.Equal("System", igrault.GenericArguments[0].Namespace);
			Assert.Equal("Byte", igrault.GenericArguments[0].Name);
			Assert.Equal("System", arguments[0].Namespace);
			Assert.Equal("Byte", arguments[0].Name);
		}

		[Fact]
		//https://github.com/dotnet/maui/issues/10583
		public void TestImplementsGenericInterfaceWithArray()
		{
			GenericInstanceType igrault;
			IList<TypeReference> arguments;
			var garply = module.ImportReference(typeof(Zoo<System.Byte>));

			Assert.True(garply.ImplementsGenericInterface(new XamlCache(), "Microsoft.Maui.Controls.XamlcUnitTests.TypeReferenceExtensionsTests/IGrault`1", out igrault, out arguments));

			Assert.Equal("System", igrault.GenericArguments[0].Namespace);
			Assert.Equal("Byte[]", igrault.GenericArguments[0].Name);
			Assert.Equal("System", arguments[0].Namespace);
			Assert.Equal("Byte[]", arguments[0].Name);
		}
	}
}
