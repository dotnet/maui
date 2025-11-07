using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FactoryMethodMissingCtor : MockView
{
	public FactoryMethodMissingCtor() => InitializeComponent();


	public class Tests
	{
		[Theory]
		[Values]
		public void Throw(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				BuildExceptionHelper.AssertThrows(() => MockCompiler.Compile(typeof(FactoryMethodMissingCtor)), 7, 4);
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<MissingMethodException>(() => new FactoryMethodMissingCtor(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
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

[XamlProcessing(XamlInflator.Default, true)]
public partial class FactoryMethods : ContentPage
{
	public FactoryMethods() => InitializeComponent();
}
"""
					)
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class FactoryMethodMissingCtor : MockView
{
	public FactoryMethodMissingCtor() => InitializeComponent();
}

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
""")
					.RunMauiSourceGenerator(typeof(FactoryMethodMissingCtor));
				Assert.True(result.Diagnostics.Any(d => d.Id == "MAUIX2003"));
			}
		}

		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;
	}
}