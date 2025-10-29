using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class Maui21038
{
	public Maui21038()
	{
		InitializeComponent();
	}

	public Maui21038(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}
	public class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void Method(bool useCompiledXaml)
		{
			if (useCompiledXaml)
			{
				MockCompiler.Compile(typeof(Maui21038), out var md, out var hasLoggedErrors);
				Assert.True(hasLoggedErrors);
			}
			else
				Assert.Throws<XamlParseException>(() => new Maui21038(useCompiledXaml));
		}
	}
}
