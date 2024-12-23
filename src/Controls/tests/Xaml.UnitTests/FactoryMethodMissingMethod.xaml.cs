using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlCompilation(XamlCompilationOptions.Skip)]
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class FactoryMethodMissingMethod : MockView
{
	public FactoryMethodMissingMethod() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void Throw([Values]XamlInflator inflator)
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
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(directory, "FactoryMethods.xaml.cs"))));
				
				var result = MockSourceGenerator.RunMauiSourceGenerator(compilation, typeof(FactoryMethodMissingMethod));
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2003"));
			}
		}

		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;
	}
}
