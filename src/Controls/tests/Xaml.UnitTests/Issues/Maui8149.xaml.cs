using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui8149 : ContentView
{
	public Maui8149() => InitializeComponent();

	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void NamescopeWithXamlC([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var compilation = MockSourceGenerator.CreateMauiCompilation();
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui8149Item : ContentView
{
}
"""));
				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui8149View : ContentView
{
	public string Text { get; set; }
	public Microsoft.Maui.Controls.DataTemplate ItemTemplate {get;set;}
}
"""
));

				compilation.RunMauiSourceGenerator(typeof(Maui8149));
			}
			var page = new Maui8149(inflator);
			Assert.That((page.Content as Maui8149View).Text, Is.EqualTo("Microsoft.Maui.Controls.Xaml.UnitTests.Maui8149"));
		}
	}
}