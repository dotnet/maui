using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class FactoryMethodMissingCtor : MockView
{
	public FactoryMethodMissingCtor() => InitializeComponent();

	[TestFixture]
	public class Tests
	{
		[Test]
		public void Throw([Values]XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				Assert.Throws(new BuildExceptionConstraint(7, 4), () => MockCompiler.Compile(typeof(FactoryMethodMissingCtor)));
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<MissingMethodException>(() => new FactoryMethodMissingCtor(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var compilation = MockSourceGenerator.CreateMauiCompilation();
				//Add MockView to compilation
				var resourcePath = XamlResourceIdAttribute.GetPathForType(typeof(FactoryMethods));
				var directory = Path.GetDirectoryName(GetThisFilePath());
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(File.ReadAllText(Path.Combine(directory, "FactoryMethods.xaml.cs"))));
				
				var result = MockSourceGenerator.RunMauiSourceGenerator(compilation, typeof(FactoryMethodMissingCtor));
				Assert.That(result.Diagnostics.Any(d => d.Id == "MAUIX2003"));
			}			
		}

		static string GetThisFilePath([System.Runtime.CompilerServices.CallerFilePath] string path = null) => path ?? string.Empty;
	}
}