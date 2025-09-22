using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Graphics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithStyle : ContentPage
{
	public GlobalXmlnsWithStyle()
	{
		InitializeComponent();
	}

	[TestFixture]
	class Tests
	{
		[Test]
		public void GlobalXmlnsWithStyleTest([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var compilation = MockSourceGenerator.CreateMauiCompilation();
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
	"""
[assembly: global::Microsoft.Maui.Controls.Xaml.Internals.AllowImplicitXmlnsDeclaration]
"""));
				compilation.RunMauiSourceGenerator(typeof(GlobalXmlnsWithStyle));
			}

			var page = new GlobalXmlnsWithStyle(inflator);
			Assert.That(page.label0.TextColor, Is.EqualTo(Colors.Red));
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Blue));
		}
	}
}