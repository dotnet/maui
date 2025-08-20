using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[Test]
		public void TestDefaultCtor([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("default ctor", layout.v0.Content.Content);
		}

		[Test]
		public void TestStringCtor([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("alternate ctor foobar", layout.v1.Content.Content);
		}

		[Test]
		public void TestIntCtor([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("int ctor 42", layout.v2.Content.Content);
		}

		[Test]
		public void TestArgumentlessFactoryMethod([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
				MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(FactoryMethods));

			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("parameterless factory", layout.v3.Content.Content);
		}

		[Test]
		public void TestFactoryMethod([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("factory foo42", layout.v4.Content.Content);
		}

		[Test]
		public void TestFactoryMethodParametersOrder([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("factory 42foo", layout.v5.Content.Content);
		}

		[Test]
		public void TestCtorWithxStatic([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("alternate ctor Property", layout.v6.Content.Content);
		}

		[Test]
		public void TestCtorWithxStaticAttribute([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("alternate ctor Property", layout.v7.Content.Content);
		}

		[Test]
		public void TestCtorWithArrayParameter([Values] XamlInflator inflator)
		{
			var layout = new FactoryMethods(inflator);
			Assert.AreEqual("Foo Bar", layout.v8.Content.Content);
		}
	}
}