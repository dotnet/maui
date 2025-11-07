using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithStyle : ContentPage
{
	public GlobalXmlnsWithStyle()
	{
		InitializeComponent();
	}


	public class Tests
	{
		[Theory]
		[Values]
		public void GlobalXmlnsWithStyleTest(XamlInflator inflator)
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
			Assert.Equal(Colors.Red, page.label0.TextColor);
			Assert.Equal(Colors.Blue, page.label0.BackgroundColor);
		}
	}
}