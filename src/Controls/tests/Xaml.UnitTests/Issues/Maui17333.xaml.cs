using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17333 : ResourceDictionary
{
	public Maui17333() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(XamlInflator.XamlC)]
		[InlineData(XamlInflator.SourceGen)]
		internal void CompilerDoesntThrowOnOnPlatform(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
				MockCompiler.Compile(typeof(Maui17333), targetFramework: "net-ios");
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17333));
				Assert.Empty(result.Diagnostics);
			}
		}
	}
}