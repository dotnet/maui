// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.IO;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7531 : ContentPage
{
	public Gh7531() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Theory]
		[XamlInflatorData]
		internal void XamlOnlyResourceResolvesLocalAssembly(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{

				var compilation = MockSourceGenerator.CreateMauiCompilation();

				compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(
"""
[assembly: global::Microsoft.Maui.Controls.Xaml.XamlResourceId("Microsoft.Maui.Controls.Xaml.UnitTests.AppResources.CompiledColors.xaml", "AppResources/CompiledColors.xaml", typeof(global::__XamlGeneratedCode__.__Type38324563C8074463))]
namespace __XamlGeneratedCode__
{
	[global::Microsoft.Maui.Controls.Xaml.XamlFilePath("AppResources/CompiledColors.xaml")]
	[global::Microsoft.Maui.Controls.Xaml.XamlCompilation(global::Microsoft.Maui.Controls.Xaml.XamlCompilationOptions.Compile)]
	[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
	public partial class __Type38324563C8074463 : global::Microsoft.Maui.Controls.ResourceDictionary
	{
		[global::System.CodeDom.Compiler.GeneratedCode("Microsoft.Maui.Controls.SourceGen", "1.0.0.0")]
		public __Type38324563C8074463()
		{
			InitializeComponent();
		}

		[global::System.CodeDom.Compiler.GeneratedCode("Microsoft.Maui.Controls.SourceGen", "1.0.0.0")]
		private void InitializeComponent()
		{
#pragma warning disable IL2026, IL3050 // The body of InitializeComponent will be replaced by XamlC so LoadFromXaml will never be called in production builds
			global::Microsoft.Maui.Controls.Xaml.Extensions.LoadFromXaml(this, typeof(__Type38324563C8074463));
#pragma warning restore IL2026, IL3050
		}
	}
}
"""));
				var result = MockSourceGenerator.RunMauiSourceGenerator(compilation, typeof(Gh7531));
			}

			Gh7531 layout = null;
			var ex = Record.Exception(() => layout = new Gh7531(inflator));
			Assert.Null(ex);
			var style = ((ResourceDictionary)layout.Resources["Colors"])["style"] as Style;
			Assert.Equal(typeof(Gh7531), style.TargetType);
		}
	}
}