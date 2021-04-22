using System;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Samples.ViewModel
{
	public class FlashlightViewModel : BaseViewModel
	{
		bool isOn;
		bool isSupported = true;

		public FlashlightViewModel()
		{
			ToggleCommand = new Command(OnToggle);
		}

		public ICommand ToggleCommand { get; }

		public bool IsOn
		{
			get => isOn;
			set => SetProperty(ref isOn, value);
		}

		public bool IsSupported
		{
			get => isSupported;
			set => SetProperty(ref isSupported, value);
		}

		public override void OnDisappearing()
		{
			if (!IsOn)
				return;

			try
			{
				Flashlight.TurnOffAsync();
				IsOn = false;
			}
			catch (FeatureNotSupportedException)
			{
				IsSupported = false;
			}

			base.OnDisappearing();
		}

		async void OnToggle()
		{
			try
			{
				if (IsOn)
				{
					await Flashlight.TurnOffAsync();
					IsOn = false;
				}
				else
				{
					await Flashlight.TurnOnAsync();
					IsOn = true;
				}
			}
			catch (FeatureNotSupportedException fnsEx)
			{
				IsSupported = false;
				await DisplayAlertAsync($"Unable toggle flashlight: {fnsEx.Message}");
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable toggle flashlight: {ex.Message}");
			}
		}
	}
}
