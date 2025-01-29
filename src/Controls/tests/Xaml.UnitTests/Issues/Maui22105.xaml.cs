using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui22105
	{
		public Maui22105()
		{
			InitializeComponent();
		}

		public Maui22105(bool useCompiledXaml)
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
			public void DefaultValueShouldBeApplied([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui22105(useCompiledXaml);
				Assert.That(page.label.FontSize, Is.EqualTo(100));
			}
		}
	}
}