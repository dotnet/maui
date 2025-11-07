using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17461 : ContentPage
{
	public Maui17461() => InitializeComponent();

	public class Test
	{
		// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => AppInfo.SetCurrent(new MockAppInfo());

		[Theory]
		[InlineData("net10.0-android", XamlInflator.XamlC)]
		[InlineData("net10.0-ios", XamlInflator.XamlC)]
		[InlineData("net10.0-android", XamlInflator.SourceGen)]
		[InlineData("net10.0-ios", XamlInflator.SourceGen)]
		[InlineData("net10.0-android", XamlInflator.Runtime)]
		[InlineData("net10.0-ios", XamlInflator.Runtime)]
		public void MissingKeyException(string targetFramework, XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(Maui17461), out var methodDef, targetFramework: targetFramework);
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17461));
				Assert.Empty(result.Diagnostics);
			}
			else
			{ } // TODO: Handle Runtime skip case properly
		}
	}
}
