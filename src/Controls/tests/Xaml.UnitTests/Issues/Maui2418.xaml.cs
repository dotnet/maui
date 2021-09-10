using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui2418 : ContentPage
	{
		public Maui2418() => InitializeComponent();
		public Maui2418(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => Device.PlatformServices = new MockPlatformServices();
			[TearDown] public void TearDown() => Device.PlatformServices = null;

			[Test]
			public void SourceInfoIsRelative([Values(false)] bool useCompiledXaml)
			{
				var page = new Maui2418(useCompiledXaml);
				var label0 = page.label0;
				var sourceInfo = Xaml.Diagnostics.VisualDiagnostics.GetXamlSourceInfo(label0);
				Assert.That(sourceInfo.SourceUri.OriginalString, Is.EqualTo("Issues/Maui2418.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"));
			}
		}
	}
}
