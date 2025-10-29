using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class FactoryMethodMissingMethod : MockView
{
	public FactoryMethodMissingMethod() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void Throw([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(8, 4), () => MockCompiler.Compile(typeof(FactoryMethodMissingMethod)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<MissingMemberException>(() => new FactoryMethodMissingMethod(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var compilation = MockSourceGenerator.CreateMauiCompilation();
				//Add MockView to compilation
				var resourcePath = XamlResourceIdAttribute.GetPathForType(typeof(FactoryMethods));
				var directory = Path.GetDirectoryName(GetThisFilePath());

				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class FactoryMethodMissingMethod : MockView
{
	public FactoryMethodMissingMethod() => InitializeComponent();
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
"""
					).RunMauiSourceGenerator(typeof(FactoryMethodMissingMethod));
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2003"));
			}
		}

		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;
	}
}
