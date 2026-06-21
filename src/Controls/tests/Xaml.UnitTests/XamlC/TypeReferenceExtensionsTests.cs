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

	public class TypeReferenceExtensionsTestsFixture : IDisposable
	{
		internal XamlCAssemblyResolver Resolver { get; }
		public ModuleDefinition Module { get; }

		public TypeReferenceExtensionsTestsFixture()
		{
			Resolver = new XamlCAssemblyResolver();
			Resolver.AddAssembly(typeof(TypeReferenceExtensionsTests).Assembly.Location);
			Resolver.AddAssembly(typeof(BindableObject).Assembly.Location);
			Resolver.AddAssembly(typeof(object).Assembly.Location);
			Resolver.AddAssembly(typeof(IList<>).Assembly.Location);
			Resolver.AddAssembly(typeof(Queue<>).Assembly.Location);
			Resolver.AddAssembly(typeof(IViewHandler).Assembly.Location);
			Resolver.AddAssembly(typeof(Color).Assembly.Location);

			Module = ModuleDefinition.CreateModule("foo", new ModuleParameters
			{
				AssemblyResolver = Resolver,
				Kind = ModuleKind.NetModule
			});
		}

		public void Dispose()
		{
			Resolver?.Dispose();
			Module?.Dispose();
		}
	}

	public class TypeReferenceExtensionsTests : IClassFixture<TypeReferenceExtensionsTestsFixture>
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

		readonly TypeReferenceExtensionsTestsFixture _fixture;
		ModuleDefinition module => _fixture.Module;

		public TypeReferenceExtensionsTests(TypeReferenceExtensionsTestsFixture fixture)
		{
			_fixture = fixture;
		}

		public static IEnumerable<object[]> InheritsFromOrImplementsTestData =>
		[
			[typeof(bool), typeof(BindableObject), false],
			[typeof(Dictionary<string, string>), typeof(BindableObject), false],
			[typeof(List<string>), typeof(BindableObject), false],
			[typeof(List<string>), typeof(IEnumerable<string>), true],
			[typeof(List<Button>), typeof(BindableObject), false],
			[typeof(Queue<KeyValuePair<string, string>>), typeof(BindableObject), false],
			[typeof(double), typeof(double), true],
			[typeof(object), typeof(IList<TriggerBase>), false],
			[typeof(object), typeof(double), false],
			[typeof(object), typeof(int?), false],
			[typeof(object), typeof(object), true],
			[typeof(sbyte), typeof(BindableObject), false],
			[typeof(string[]), typeof(System.Collections.IEnumerable), true],
			[typeof(string[]), typeof(object), true],
			[typeof(string[]), typeof(string), false],
			[typeof(string[]), typeof(BindingBase), false],
			[typeof(string[]), typeof(IEnumerable<string>), true],
			[typeof(Type), typeof(object), true],
			[typeof(Type), typeof(Type), true],
			[typeof(Type), typeof(BindableObject), false],
			[typeof(System.Windows.Input.ICommand), typeof(System.Windows.Input.ICommand), true],
			[typeof(System.Windows.Input.ICommand), typeof(BindingBase), false],
			[typeof(BindingBase), typeof(BindingBase), true],
			[typeof(BindingCondition), typeof(BindableObject), false],
			[typeof(Button), typeof(BindableObject), true],
			[typeof(Button), typeof(BindingBase), false],
			[typeof(Button), typeof(View), true],
			[typeof(Color), typeof(BindableObject), false],
			[typeof(Color), typeof(BindingBase), false],
			[typeof(Color), typeof(Color), true],
			[typeof(ColumnDefinition), typeof(BindableObject), true],
			[typeof(ColumnDefinition), typeof(BindingBase), false],
			[typeof(ColumnDefinition), typeof(ColumnDefinitionCollection), false],
			[typeof(Constraint), typeof(BindingBase), false],
			[typeof(Constraint), typeof(Constraint), true],
			[typeof(ConstraintExpression), typeof(BindableObject), false],
			[typeof(ContentPage), typeof(BindableObject), true],
			[typeof(ContentPage), typeof(Page), true],
			[typeof(ContentView), typeof(BindableObject), true],
			[typeof(ContentView[]), typeof(IList<ContentView>), true],
			[typeof(MultiTrigger), typeof(IList<TriggerBase>), false],
			[typeof(OnIdiom<double>), typeof(BindableObject), false],
			[typeof(OnPlatform<string>), typeof(string), false],
			[typeof(OnPlatform<string>), typeof(BindableObject), false],
			[typeof(OnPlatform<string>), typeof(BindingBase), false],
			[typeof(OnPlatform<FontAttributes>), typeof(BindableObject), false],
			[typeof(StackLayout), typeof(Controls.Compatibility.Layout<View>), true],
			[typeof(StackLayout), typeof(View), true],
			[typeof(Foo<string>), typeof(Foo), true],
			[typeof(Bar<string>), typeof(Foo), true],
			[typeof(Bar<string>), typeof(Foo<bool>), false],
			[typeof(Bar<string>), typeof(Foo<string>), true],
			[typeof(Qux<string>), typeof(double), false], //https://github.com/xamarin/Microsoft.Maui.Controls/issues/1497
			[typeof(IGrault<object>), typeof(IGrault<string>), false],
			[typeof(IGrault<string>), typeof(IGrault<object>), false],
			[typeof(ICovariant<object>), typeof(ICovariant<string>), false],
			[typeof(ICovariant<string>), typeof(ICovariant<object>), true],
			[typeof(IContravariant<object>), typeof(IContravariant<string>), true],
			[typeof(IContravariant<string>), typeof(IContravariant<object>), false],
			[typeof(Covariant<object>), typeof(ICovariant<string>), false],
			[typeof(Covariant<string>), typeof(ICovariant<object>), true],
		];

		[Theory]
		[MemberData(nameof(InheritsFromOrImplementsTestData))]
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

			var typeRef = test.GetType("Microsoft.Maui.Controls.Effect");
			var baseClass = core.GetType("Microsoft.Maui.Controls.Effect");
			var result = TypeReferenceExtensions.InheritsFromOrImplements(module.ImportReference(typeRef), new XamlCache(), module.ImportReference(baseClass));
			Assert.False(result);
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

		[Theory]
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

		[Fact]
		public void TestResolveGenericParametersOfGenericMethod()
		{
			var method = new GenericInstanceMethod(module.ImportReference(typeof(Grault)).Resolve().Methods[0]);
			method.GenericArguments.Add(module.TypeSystem.Byte);
			// The method is `void Method<T>(T t)`, so we check the parameter type, not return type
			var resolved = method.Parameters[0].ParameterType.ResolveGenericParameters(method);

			Assert.True(TypeRefComparer.Default.Equals(module.TypeSystem.Byte, resolved));
		}

		[Theory]
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
