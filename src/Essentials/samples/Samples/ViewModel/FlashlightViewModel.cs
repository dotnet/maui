using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Samples.ViewModel
{
	public class FlashlightViewModel : BaseViewModel
	{
		bool isOn;
		bool isSupported = true;

		public FlashlightViewModel()
		{
			ToggleCommand = new Command(OnToggle);
			App.Current.Dispatcher.Dispatch(async () => await InitViewModel());
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

		async Task InitViewModel()
		{
			IsSupported = await Flashlight.IsSupportedAsync();
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
