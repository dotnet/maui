using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui17484 : ContentPage
{
	public Maui17484() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			AppInfo.SetCurrent(new MockAppInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void ObsoleteinDT(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				Assert.Null(Record.Exception(() => MockCompiler.Compile(typeof(Maui17484))));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui17484));
				Assert.Empty(result.Diagnostics);
			}
			else if (inflator == XamlInflator.Runtime)
			{
				Assert.Null(Record.Exception(() => new Maui17484(inflator)));
			}
			else
			// TODO: Convert to [Theory(Skip="reason")] or use conditional Skip attribute
			{
				// TODO: This branch was using NUnit Assert.Skip, needs proper handling
			}

		}
	}
}
