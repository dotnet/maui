using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17484 : ContentPage
{
	public Maui17484() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test() => AppInfo.SetCurrent(new MockAppInfo());
		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ObsoleteinDT(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				var ex = Record.Exception(() => MockCompiler.Compile(typeof(Maui17484)));
				Assert.Null(ex);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17484));
				Assert.Empty(result.Diagnostics);
			}
			else if (inflator == XamlInflator.Runtime)
			{
				var ex = Record.Exception(() => new Maui17484(inflator));
				Assert.Null(ex);
			}
		}
	}
}