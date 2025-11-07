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
			VisualDiagnostics.VisualTreeChanged -= OnVTChanged;
		}

		void OnVTChanged(object sender, VisualTreeChangeEventArgs e)
		{
			Assert.Equal(VisualTreeChangeType.Add, e.ChangeType);
			Assert.Equal(1, e.ChildIndex);
			// TODO: XUnit has no // TODO: XUnit has no Assert.Pass() - test passes if no exception is thrown - test passes if no exception is thrown
		}

		[Theory]
		[Values]
		public void ChildIndexOnAdd(XamlInflator inflator)
		{
			var layout = new Gh11335(inflator);
			VisualDiagnostics.VisualTreeChanged += OnVTChanged;
			layout.Add(null, EventArgs.Empty);
			Assert.Fail();
		}
	}
}
