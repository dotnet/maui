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
		[XamlInflatorData]
		internal void TestNonCompiledResourceDictionary(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
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
