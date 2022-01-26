using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
#if DEBUG
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
			public void SourceInfoIsRelative()
			{
				var page = new Maui2418();
				Assert.That(page, Is.Not.Null);
				var label0 = page.label0;
				var sourceInfo = VisualDiagnostics.GetSourceInfo(label0);
				Assert.That(sourceInfo.SourceUri.OriginalString, Is.EqualTo($"Issues{System.IO.Path.DirectorySeparatorChar}Maui2418.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"));
			}
		}
	}
#endif
}
