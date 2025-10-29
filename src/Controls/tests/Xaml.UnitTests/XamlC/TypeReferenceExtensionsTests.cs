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
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;
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
		[Xunit.Fact]
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
		[Xunit.Fact]
		public void OneTimeTearDown()
		{
			resolver?.Dispose();
			module?.Dispose();
		}

		[Theory]
		[InlineData(typeof(bool), typeof(BindableObject), false)]
		[InlineData(typeof(Dictionary<string, string>), typeof(BindableObject), false)]
		[InlineData(typeof(List<string>), typeof(BindableObject), false)]
		[InlineData(typeof(List<string>), typeof(IEnumerable<string>), true)]
		[InlineData(typeof(List<Button>), typeof(BindableObject), false)]
		[InlineData(typeof(Queue<KeyValuePair<string, string>>), typeof(BindableObject), false)]
		[InlineData(typeof(double), typeof(double), true)]
		[InlineData(typeof(object), typeof(IList<TriggerBase>), false)]
		[InlineData(typeof(object), typeof(double), false)]
		[InlineData(typeof(object), typeof(int?), false)]
		[InlineData(typeof(object), typeof(object), true)]
		[InlineData(typeof(sbyte), typeof(BindableObject), false)]
		[InlineData(typeof(string[]), typeof(System.Collections.IEnumerable), true)]
		[InlineData(typeof(string[]), typeof(object), true)]
		[InlineData(typeof(string[]), typeof(string), false)]
		[InlineData(typeof(string[]), typeof(BindingBase), false)]
		[InlineData(typeof(string[]), typeof(IEnumerable<string>), true)]
		[InlineData(typeof(Type), typeof(object), true)]
		[InlineData(typeof(Type), typeof(Type), true)]
		[InlineData(typeof(Type), typeof(BindableObject), false)]
		[InlineData(typeof(System.Windows.Input.ICommand), typeof(System.Windows.Input.ICommand), true)]
		[InlineData(typeof(System.Windows.Input.ICommand), typeof(BindingBase), false)]
		[InlineData(typeof(BindingBase), typeof(BindingBase), true)]
		[InlineData(typeof(BindingCondition), typeof(BindableObject), false)]
		[InlineData(typeof(Button), typeof(BindableObject), true)]
		[InlineData(typeof(Button), typeof(BindingBase), false)]
		[InlineData(typeof(Button), typeof(View), true)]
		[InlineData(typeof(Color), typeof(BindableObject), false)]
		[InlineData(typeof(Color), typeof(BindingBase), false)]
		[InlineData(typeof(Color), typeof(Color), true)]
		[InlineData(typeof(ColumnDefinition), typeof(BindableObject), true)]
		[InlineData(typeof(ColumnDefinition), typeof(BindingBase), false)]
		[InlineData(typeof(ColumnDefinition), typeof(ColumnDefinitionCollection), false)]
		[InlineData(typeof(Constraint), typeof(BindingBase), false)]
		[InlineData(typeof(Constraint), typeof(Constraint), true)]
		[InlineData(typeof(ConstraintExpression), typeof(BindableObject), false)]
		[InlineData(typeof(ContentPage), typeof(BindableObject), true)]
		[InlineData(typeof(ContentPage), typeof(Page), true)]
		[InlineData(typeof(ContentView), typeof(BindableObject), true)]
		[InlineData(typeof(ContentView[]), typeof(IList<ContentView>), true)]
		[InlineData(typeof(MultiTrigger), typeof(IList<TriggerBase>), false)]
		[InlineData(typeof(OnIdiom<double>), typeof(BindableObject), false)]
		[InlineData(typeof(OnPlatform<string>), typeof(string), false)]
		[InlineData(typeof(OnPlatform<string>), typeof(BindableObject), false)]
		[InlineData(typeof(OnPlatform<string>), typeof(BindingBase), false)]
		[InlineData(typeof(OnPlatform<FontAttributes>), typeof(BindableObject), false)]
		[InlineData(typeof(StackLayout), typeof(Controls.Compatibility.Layout<View>), true)]
		[InlineData(typeof(StackLayout), typeof(View), true)]
		[InlineData(typeof(Foo<string>), typeof(Foo), true)]
		[InlineData(typeof(Bar<string>), typeof(Foo), true)]
		[InlineData(typeof(Bar<string>), typeof(Foo<bool>), false)]
		[InlineData(typeof(Bar<string>), typeof(Foo<string>), true)]
		[InlineData(typeof(Qux<string>), typeof(double), false)] //https://github.com/xamarin/Microsoft.Maui.Controls/issues/1497
		[InlineData(typeof(IGrault<object>), typeof(IGrault<string>), false)]
		[InlineData(typeof(IGrault<string>), typeof(IGrault<object>), false)]
		[InlineData(typeof(ICovariant<object>), typeof(ICovariant<string>), false)]
		[InlineData(typeof(ICovariant<string>), typeof(ICovariant<object>), true)]
		[InlineData(typeof(IContravariant<object>), typeof(IContravariant<string>), true)]
		[InlineData(typeof(IContravariant<string>), typeof(IContravariant<object>), false)]
		[InlineData(typeof(Covariant<object>), typeof(ICovariant<string>), false)]
		[InlineData(typeof(Covariant<string>), typeof(ICovariant<object>), true)]
		public void TestInheritsFromOrImplements(Type typeRef, Type baseClass, bool expected)
		{
			var result = TypeReferenceExtensions.InheritsFromOrImplements(module.ImportReference(typeRef), new XamlCache(), module.ImportReference(baseClass));
			Assert.Equal(expected, result);
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
		[Theory]
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

		[Xunit.Fact]
		public void TestResolveGenericParametersOfGenericMethod()
		{
			var method = new GenericInstanceMethod(module.ImportReference(typeof(Grault)).Resolve().Methods[0]);
			method.GenericArguments.Add(module.TypeSystem.Byte);
			var resolved = method.ReturnType.ResolveGenericParameters(method);

			Assert.True(TypeRefComparer.Default.Equals(module.TypeSystem.Byte, resolved));
		}

		[InlineData(typeof(Garply<byte>), typeof(byte))]
		[Theory]
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
