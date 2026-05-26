using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22714
{
	public Maui22714() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(XamlInflator.XamlC, true)]
		[InlineData(XamlInflator.XamlC, false)]
		[InlineData(XamlInflator.SourceGen, false)]
		[InlineData(XamlInflator.Runtime, false)]
		internal void TestNonCompiledResourceDictionary(XamlInflator inflator, bool treatWarningsAsErrors)
		{
			if (inflator == XamlInflator.XamlC)
			{
				if (treatWarningsAsErrors)
				{
					var ex = Assert.Throws<Build.Tasks.BuildException>(() => MockCompiler.Compile(typeof(Maui22714), treatWarningsAsErrors: true));
					Assert.Contains(" XC0024 ", ex.Message, StringComparison.Ordinal);
				}
				else
				{
					MockCompiler.Compile(typeof(Maui22714), treatWarningsAsErrors: false);
				}
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = MockSourceGenerator.RunMauiSourceGenerator(MockSourceGenerator.CreateMauiCompilation(), typeof(Maui22714));
			}
			else if (inflator == XamlInflator.Runtime)
			{
				// This will never affect non-compiled builds
				_ = new Maui22714(inflator);
			}
		}
	}
}

public class PageViewModel22714
{
	public string Title { get; set; }
	public ItemViewModel22714[] Items { get; set; }
}

public class ItemViewModel22714
{
	public string Title { get; set; }
}
