using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
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
		public class Tests
		{
			[Theory]
			[InlineData(true)]
			public void TestDefaultCtor(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("default ctor", layout.v0.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestStringCtor(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("alternate ctor foobar", layout.v1.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestIntCtor(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("int ctor 42", layout.v2.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestArgumentlessFactoryMethod(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("parameterless factory", layout.v3.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestFactoryMethod(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("factory foo42", layout.v4.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestFactoryMethodParametersOrder(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("factory 42foo", layout.v5.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestCtorWithxStatic(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("alternate ctor Property", layout.v6.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestCtorWithxStaticAttribute(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("alternate ctor Property", layout.v7.Content.Content);
			}

			[Theory]
			[InlineData(true)]
			public void TestCtorWithArrayParameter(bool useCompiledXaml)
			{
				var layout = new FactoryMethods(useCompiledXaml);
				Assert.Equal("Foo Bar", layout.v8.Content.Content);
			}
		}
	}
}