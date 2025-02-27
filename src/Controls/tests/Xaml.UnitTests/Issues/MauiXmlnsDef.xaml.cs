using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.UnitTests;
using NUnit.Framework;


namespace Microsoft.Maui.Controls.Xaml.UnitTests;
[XamlCompilation(XamlCompilationOptions.Skip)]
public partial class MauiXmlnsDef
{
	public MauiXmlnsDef()
	{
		InitializeComponent();
	}

	public MauiXmlnsDef(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}

	[TestFixture]
	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void XamlParseErrorsHaveFileInfo([Values(false, true)] bool useCompiledXaml)
		{
			if (useCompiledXaml)
				MockCompiler.Compile(typeof(MauiXmlnsDef));
			new MauiXmlnsDef(useCompiledXaml);
		}
	}
}
