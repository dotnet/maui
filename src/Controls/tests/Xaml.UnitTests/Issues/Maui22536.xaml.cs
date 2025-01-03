using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22536
{
	public Maui22536()
	{
		InitializeComponent();
	}

	public Maui22536(bool useCompiledXaml)
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
		public void TestNonCompiledResourceDictionary([Values(false, true)] bool useCompiledXaml)
		{
			var page = new Maui22536(useCompiledXaml);
			Assert.AreEqual(page.Button.BackgroundColor, Color.FromArgb("#010203"));
		}
	}
}
