using System;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh11335 : ContentPage
{
	public Gh11335() => InitializeComponent();

	void Remove(object sender, EventArgs e) => stack.Children.Remove(label);

	void Add(object sender, EventArgs e)
	{
		int index = stack.Children.IndexOf(label);
		Label newLabel = new Label { Text = "New Inserted Label" };
		stack.Children.Insert(index + 1, newLabel);
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		bool enableDiagnosticsInitialState;
		bool eventTriggered;

		public Tests()
		{
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		public void Dispose()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
			VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
		}

		void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
		{
			Assert.Equal(VisualTreeChangeType.Add, e.ChangeType);
			Assert.Equal(1, e.ChildIndex);
			eventTriggered = true;
		}


		[Theory]
		[XamlInflatorData]
		internal void ChildIndexOnAdd(XamlInflator inflator)
		{
			eventTriggered = false;
			var layout = new Gh11335(inflator);
			VisualDiagnostics.VisualTreeChanged += OnVTChanged;
			layout.Add(null, EventArgs.Empty);
			Assert.True(eventTriggered, "VisualTreeChanged event was not triggered");
		}
	}
}
