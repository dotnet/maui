using System;
using System.Windows.Input;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.VisualStateManagerGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class StateTriggerEventsGallery : ContentPage
	{
		public StateTriggerEventsGallery()
		{
			InitializeComponent();
		}

		void OnRedStateTriggerIsActiveChanged(object sender, EventArgs e)
		{
			var stateTrigger = (StateTriggerBase)sender;

			if (InfoLabel != null)
				InfoLabel.Text += $"OnRedStateTriggerIsActiveChanged: {stateTrigger.IsActive} {Environment.NewLine}";
		}

		void OnGreenStateTriggerIsActiveChanged(object sender, EventArgs e)
		{
			var stateTrigger = (StateTriggerBase)sender;

			if (InfoLabel != null)
				InfoLabel.Text += $"OnGreenStateTriggerIsActiveChanged: {stateTrigger.IsActive} {Environment.NewLine}";
		}
	}

	public class StateTriggerEventsGalleryViewModel : BindableObject
	{
		bool _toggleState;

		public bool ToggleState
		{
			get => _toggleState;
			set
			{
				_toggleState = value;
				OnPropertyChanged(nameof(ToggleState));
			}
		}

		public ICommand ToggleCommand => new Command(OnToggle);

		void OnToggle()
		{
			ToggleState = !ToggleState;
		}
	}
}