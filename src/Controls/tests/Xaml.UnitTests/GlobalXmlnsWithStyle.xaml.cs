using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class GlobalXmlnsWithStyle : ContentPage
{
	public GlobalXmlnsWithStyle()
	{
		InitializeComponent();
	}


	public class Tests : IDisposable
	{
		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}
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