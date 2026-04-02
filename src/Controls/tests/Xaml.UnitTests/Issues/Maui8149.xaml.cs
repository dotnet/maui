using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui8149 : ContentView
{
	public Maui8149() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void NamescopeWithXamlC(XamlInflator inflator)
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
			Assert.Equal("Microsoft.Maui.Controls.Xaml.UnitTests.Maui8149", (page.Content as Maui8149View).Text);
		}
	}
}