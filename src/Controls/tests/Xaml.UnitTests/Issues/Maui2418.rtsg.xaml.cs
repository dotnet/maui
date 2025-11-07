using Microsoft.Maui.Controls.Core.UnitTests;
using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui2418 : ContentPage
{
	public Maui2418() => InitializeComponent();

#if DEBUG
	public class Tests : IDisposable
	{
		bool enableDiagnosticsInitialState;

		public Tests()
		{
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		public void Dispose()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
		}

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		public void SourceInfoIsRelative(XamlInflator inflator)
		{
			var page = new Maui2418(inflator);
			Assert.NotNull(page);
			var label0 = page.label0;
			var sourceInfo = VisualDiagnostics.GetSourceInfo(label0);
			Assert.Equal($"Issues{System.IO.Path.DirectorySeparatorChar}Maui2418.rtsg.xaml;assembly=Microsoft.Maui.Controls.Xaml.UnitTests", sourceInfo.SourceUri.OriginalString);
		}
	}
#endif
}
