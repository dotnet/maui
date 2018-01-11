using System;
using NUnit.Framework;
using Mono.Cecil;
using Xamarin.Forms.Build.Tasks;
using System.Collections.Generic;

namespace Xamarin.Forms
{
	public class Effect
	{
	}
}
namespace Xamarin.Forms.Xaml.XamlcUnitTests
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

		ModuleDefinition module;

		[SetUp]
		public void SetUp()
		{
			var resolver = new XamlCAssemblyResolver();
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(TypeReferenceExtensionsTests).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(BindableObject).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(object).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(IList<>).Assembly.CodeBase)).Path));
			resolver.AddAssembly(Uri.UnescapeDataString((new UriBuilder(typeof(Queue<>).Assembly.CodeBase)).Path));

			module = ModuleDefinition.CreateModule("foo", new ModuleParameters {
				AssemblyResolver = resolver,
				Kind = ModuleKind.NetModule
			});
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
		[TestCase(typeof(Bar<string>), typeof(Foo<string>), ExpectedResult = true)]
		[TestCase(typeof(Qux<string>), typeof(double), ExpectedResult = false)] //https://github.com/xamarin/Xamarin.Forms/issues/1497
		public bool TestInheritsFromOrImplements(Type typeRef, Type baseClass)
		{
			return TypeReferenceExtensions.InheritsFromOrImplements(module.ImportReference(typeRef), module.ImportReference(baseClass));
		}

		[Test]
		public void TestSameTypeNamesFromDifferentAssemblies()
		{
			var core = typeof(BindableObject).Assembly;
			var test = typeof(TypeReferenceExtensionsTests).Assembly;

			Assert.False(TestInheritsFromOrImplements(test.GetType("Xamarin.Forms.Effect"), core.GetType("Xamarin.Forms.Effect")));
		}
	}
}