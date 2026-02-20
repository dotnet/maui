using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25728, "App crashes when entry bound to float value with fractional format", PlatformAffected.Android)]
	public partial class Issue25728 : ContentPage
	{
		private float _floatValue = 1.5f;

		public float FloatValue
		{
			get => _floatValue;
			set
			{
				_floatValue = value;
				OnPropertyChanged();
			}
		}

		public Issue25728()
		{
			InitializeComponent();
			BindingContext = this;
		}

		private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
		{
			if (sender is Entry entry)
			{
				// Simulate user input that would trigger the crash scenario
				if (float.TryParse(e.NewTextValue, out float newValue))
				{
					FloatValue = newValue;
				}
			}
		}

		private void OnTestButtonClicked(object sender, System.EventArgs e)
		{
			// Test changing the value programmatically to trigger string formatting
			FloatValue = 3.14159f;
		}
	}
}