using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui2418 : ContentPage
{
	public Maui2418() => InitializeComponent();

#if DEBUG
	[TestFixture]
	class Tests
	{
		bool enableDiagnosticsInitialState;

		[SetUp]
		public void Setup()
		{
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		[TearDown]
		public void TearDown()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
		}

		[Test]
		public void SourceInfoIsRelative([Values(XamlInflator.Runtime, XamlInflator.SourceGen)] XamlInflator inflator)
		{
			var page = new Maui2418(inflator);
			Assert.That(page, Is.Not.Null);
			var label0 = page.label0;
			var sourceInfo = VisualDiagnostics.GetSourceInfo(label0);
			Assert.That(sourceInfo.SourceUri.OriginalString, Is.EqualTo($"Issues{System.IO.Path.DirectorySeparatorChar}Maui2418.rtsg.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests"));
		}
	}
#endif
}
