using System;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

#if DEBUG
public partial class Gh10803 : ContentPage
{
	public Gh10803() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		bool enableDiagnosticsInitialState;
		int failures = 0;

		public Tests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			VisualDiagnostics.VisualTreeChanged += VTChanged;
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		public void Dispose()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
			DispatcherProvider.SetCurrent(null);
			VisualDiagnostics.VisualTreeChanged -= VTChanged;
			failures = 0;
		}

		[Theory]
		[InlineData(XamlInflator.Runtime)]
		[InlineData(XamlInflator.SourceGen)]
		internal void SourceInfoForElementsInDT(XamlInflator inflator)
		{
			var layout = new Gh10803(inflator);
			var listview = layout.listview;
			var cell = listview.TemplatedItems.GetOrCreateContent(0, null);
			Assert.Equal(0, failures);
		}

		void VTChanged(object sender, VisualTreeChangeEventArgs e)
		{
			var parentSourInfo = e.Parent == null ? null : VisualDiagnostics.GetSourceInfo(e.Parent);
			var childSourceInfo = VisualDiagnostics.GetSourceInfo(e.Child);
			if (childSourceInfo == null)
				failures++;
			if (e.Parent != null && parentSourInfo == null)
				failures++;
			if (e.Parent != null && e.ChildIndex == -1)
				failures++;
		}
	}
}
#endif