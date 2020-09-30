using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class MockView : View
	{
		public MockFactory Content { get; set; }
	}

	public class MockFactory
	{
		public string Content { get; set; }
		public MockFactory()
		{
			Content = "default ctor";
		}

		public MockFactory(string arg0, string arg1)
		{
			Content = "alternate ctor " + arg0 + arg1;
		}

		public MockFactory(string arg0)
		{
			Content = "alternate ctor " + arg0;
		}

		public MockFactory(int arg)
		{
			Content = "int ctor " + arg.ToString();
		}

		public MockFactory(object[] args)
		{
			Content = string.Join(" ", args);
		}

		public static MockFactory ParameterlessFactory()
		{
			return new MockFactory
			{
				Content = "parameterless factory",
			};
		}

		public static MockFactory Factory(string arg0, int arg1)
		{
			return new MockFactory
			{
				Content = "factory " + arg0 + arg1,
			};
		}

		public static MockFactory Factory(int arg0, string arg1)
		{
			return new MockFactory
			{
				Content = "factory " + arg0 + arg1,
			};
		}
	}

	public partial class FactoryMethods : ContentPage
	{
		public FactoryMethods()
		{
			InitializeComponent();
		}

		public FactoryMethods(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void SetUp()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestDefaultCtor(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("default ctor", layout.v0.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestStringCtor(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("alternate ctor foobar", layout.v1.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestIntCtor(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("int ctor 42", layout.v2.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestArgumentlessFactoryMethod(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("parameterless factory", layout.v3.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestFactoryMethod(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("factory foo42", layout.v4.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestFactoryMethodParametersOrder(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("factory 42foo", layout.v5.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestCtorWithxStatic(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("alternate ctor Property", layout.v6.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestCtorWithxStaticAttribute(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("alternate ctor Property", layout.v7.Content.Content);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void TestCtorWithArrayParameter(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.AreEqual("Foo Bar", layout.v8.Content.Content);
			}
		}
	}
}