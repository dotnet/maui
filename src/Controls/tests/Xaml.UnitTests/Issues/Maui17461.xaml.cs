using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17461 : ContentPage
{

	public Maui17461() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData("net7.0-ios", XamlInflator.XamlC)]
		[InlineData("net7.0-android", XamlInflator.XamlC)]
		[InlineData("net7.0-macos", XamlInflator.XamlC)]
		[InlineData("net7.0-ios", XamlInflator.SourceGen)]
		[InlineData("net7.0-android", XamlInflator.SourceGen)]
		[InlineData("net7.0-macos", XamlInflator.SourceGen)]
		internal void MissingKeyException(string targetFramework, XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(Maui17461), out var methodDef, targetFramework: targetFramework);
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17461));
				Assert.Empty(result.Diagnostics);
			}
		}
	}
}