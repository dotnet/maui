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

		// TODO: Convert to IClassFixture or static constructor
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

		// TODO: Convert to IClassFixture
		[Xunit.Fact]
		public void OneTimeTearDown()
		{
			resolver?.Dispose();
			module?.Dispose();
		}


		[Theory(Skip = "TODO: Expected return values need to be asserted")]
		[InlineData(typeof(bool), typeof(BindableObject))]
		[InlineData(typeof(Dictionary<string, string>), typeof(BindableObject))]
		[InlineData(typeof(List<string>), typeof(BindableObject))]
		[InlineData(typeof(List<string>), typeof(IEnumerable<string>))]
		[InlineData(typeof(List<Button>), typeof(BindableObject))]
		[InlineData(typeof(Queue<KeyValuePair<string, string>>), typeof(BindableObject))]
		[InlineData(typeof(double), typeof(double))]
		[InlineData(typeof(object), typeof(IList<TriggerBase>))]
		[InlineData(typeof(object), typeof(double))]
		[InlineData(typeof(object), typeof(int?))]
		[InlineData(typeof(object), typeof(object))]
		[InlineData(typeof(sbyte), typeof(BindableObject))]
		[InlineData(typeof(string[]), typeof(System.Collections.IEnumerable))]
		[InlineData(typeof(string[]), typeof(object))]
		[InlineData(typeof(string[]), typeof(string))]
		[InlineData(typeof(string[]), typeof(BindingBase))]
		[InlineData(typeof(string[]), typeof(IEnumerable<string>))]
		[InlineData(typeof(Type), typeof(object))]
		[InlineData(typeof(Type), typeof(Type))]
		[InlineData(typeof(Type), typeof(BindableObject))]
		[InlineData(typeof(System.Windows.Input.ICommand), typeof(System.Windows.Input.ICommand))]
		[InlineData(typeof(System.Windows.Input.ICommand), typeof(BindingBase))]
		[InlineData(typeof(BindingBase), typeof(BindingBase))]
		[InlineData(typeof(BindingCondition), typeof(BindableObject))]
		[InlineData(typeof(Button), typeof(BindableObject))]
		[InlineData(typeof(Button), typeof(BindingBase))]
		[InlineData(typeof(Button), typeof(View))]
		[InlineData(typeof(Color), typeof(BindableObject))]
		[InlineData(typeof(Color), typeof(BindingBase))]
		[InlineData(typeof(Color), typeof(Color))]
		[InlineData(typeof(ColumnDefinition), typeof(BindableObject))]
		[InlineData(typeof(ColumnDefinition), typeof(BindingBase))]
		[InlineData(typeof(ColumnDefinition), typeof(ColumnDefinitionCollection))]
		[InlineData(typeof(Constraint), typeof(BindingBase))]
		[InlineData(typeof(Constraint), typeof(Constraint))]
		[InlineData(typeof(ConstraintExpression), typeof(BindableObject))]
		[InlineData(typeof(ContentPage), typeof(BindableObject))]
		[InlineData(typeof(ContentPage), typeof(Page))]
		[InlineData(typeof(ContentView), typeof(BindableObject))]
		[InlineData(typeof(ContentView[]), typeof(IList<ContentView>))]
		[InlineData(typeof(MultiTrigger), typeof(IList<TriggerBase>))]
		[InlineData(typeof(OnIdiom<double>), typeof(BindableObject))]
		[InlineData(typeof(OnPlatform<string>), typeof(string))]
		[InlineData(typeof(OnPlatform<string>), typeof(BindableObject))]
		[InlineData(typeof(OnPlatform<string>), typeof(BindingBase))]
		[InlineData(typeof(OnPlatform<FontAttributes>), typeof(BindableObject))]
		[InlineData(typeof(StackLayout), typeof(Controls.Compatibility.Layout<View>))]
		[InlineData(typeof(StackLayout), typeof(View))]
		[InlineData(typeof(Foo<string>), typeof(Foo))]
		[InlineData(typeof(Bar<string>), typeof(Foo))]
		[InlineData(typeof(Bar<string>), typeof(Foo<bool>))]
		[InlineData(typeof(Bar<string>), typeof(Foo<string>))]
		[InlineData(typeof(Qux<string>), typeof(double))]
		[InlineData(typeof(IGrault<object>), typeof(IGrault<string>))]
		[InlineData(typeof(IGrault<string>), typeof(IGrault<object>))]
		[InlineData(typeof(ICovariant<object>), typeof(ICovariant<string>))]
		[InlineData(typeof(ICovariant<string>), typeof(ICovariant<object>))]
		[InlineData(typeof(IContravariant<object>), typeof(IContravariant<string>))]
		[InlineData(typeof(IContravariant<string>), typeof(IContravariant<object>))]
		[InlineData(typeof(Covariant<object>), typeof(ICovariant<string>))]
		[InlineData(typeof(Covariant<string>), typeof(ICovariant<object>))]
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

		[Xunit.Theory]
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

		[Xunit.Fact]
		public void TestResolveGenericParametersOfGenericMethod()
		{
			var method = new GenericInstanceMethod(module.ImportReference(typeof(Grault)).Resolve().Methods[0]);
			method.GenericArguments.Add(module.TypeSystem.Byte);
			var resolved = method.ReturnType.ResolveGenericParameters(method);

			Assert.True(TypeRefComparer.Default.Equals(module.TypeSystem.Byte, resolved));
		}

		[Xunit.Theory]
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
