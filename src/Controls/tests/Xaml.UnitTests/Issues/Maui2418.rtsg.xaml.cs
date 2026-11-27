using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui2418 : ContentPage
{
	public Maui2418() => InitializeComponent();

#if DEBUG
	[Collection("Issue")]
	public class Tests : IDisposable
	{
		bool enableMauiDiagnosticsInitialState;

		public Tests()
		{
			enableMauiDiagnosticsInitialState = RuntimeFeature.EnableMauiDiagnostics;
			AppContext.SetSwitch("Microsoft.Maui.RuntimeFeature.EnableMauiDiagnostics", true);
		}

		public void Dispose()
		{
			AppContext.SetSwitch("Microsoft.Maui.RuntimeFeature.EnableMauiDiagnostics", enableMauiDiagnosticsInitialState);
		}

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void SourceInfoIsRelative(XamlInflator inflator)
		{
			var page = new Maui2418(inflator);
			Assert.NotNull(page);
			var label0 = page.label0;
			var sourceInfo = VisualDiagnostics.GetSourceInfo(label0);
			// Normalize path separators for cross-platform comparison (compiled on Windows, may run on macOS/Linux via Helix)
			Assert.Equal("Issues/Maui2418.rtsg.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", sourceInfo.SourceUri.OriginalString.Replace('\\', '/'));
		}
	}
#endif
}
