using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class MockView : View
{
	public MockFactory Content { get; set; }
}

public class MockFactory
{
	public string Content { get; set; }
	public MockFactory() => Content = "default ctor";

	public MockFactory(string arg0, string arg1) => Content = "alternate ctor " + arg0 + arg1;

	public MockFactory(string arg0) => Content = "alternate ctor " + arg0;

	public MockFactory(int arg) => Content = "int ctor " + arg.ToString();

	public MockFactory(object[] args) => Content = string.Join(" ", args);

	public static MockFactory ParameterlessFactory() => new MockFactory
	{
		Content = "parameterless factory",
	};

	public static MockFactory Factory(string arg0, int arg1) => new MockFactory
	{
		Content = "factory " + arg0 + arg1,
	};

	public static MockFactory Factory(int arg0, string arg1) => new MockFactory
	{
		Content = "factory " + arg0 + arg1,
	};
}

public partial class FactoryMethods : ContentPage
{
	public FactoryMethods() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void TestDefaultCtor(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("default ctor", layout.v0.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestStringCtor(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("alternate ctor foobar", layout.v1.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestIntCtor(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("int ctor 42", layout.v2.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestArgumentlessFactoryMethod(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
				MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(FactoryMethods));

			var layout = new FactoryMethods(inflator);
			Assert.Equal("parameterless factory", layout.v3.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestFactoryMethod(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("factory foo42", layout.v4.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestFactoryMethodParametersOrder(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("factory 42foo", layout.v5.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestCtorWithxStatic(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("alternate ctor Property", layout.v6.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestCtorWithxStaticAttribute(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("alternate ctor Property", layout.v7.Content.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void TestCtorWithArrayParameter(XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.Equal("Foo Bar", layout.v8.Content.Content);
		}
	}
}