using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui21038
{
	public Maui21038() => InitializeComponent();

	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[Theory]
		[Values]
		public void XamlParseErrorsHaveFileInfo(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				MockCompiler.Compile(typeof(Maui21038), out var md, out var hasLoggedErrors);
				Assert.True(hasLoggedErrors);
			}
			else if (inflator == XamlInflator.Runtime)
				Assert.Throws<XamlParseException>(() => new Maui21038(inflator));
			else if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui21038
{
	public Maui21038() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui21038));
				Assert.NotEmpty(result.Diagnostics);

			}
		}

		public void Dispose()
		{
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
		}
	}
}
