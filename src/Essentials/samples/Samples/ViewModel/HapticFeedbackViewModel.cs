using System;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;

namespace Samples.ViewModel
{
	public class HapticFeedbackViewModel : BaseViewModel
	{
		bool isSupported = true;

		public HapticFeedbackViewModel()
		{
			ClickCommand = new Command(OnClick);
			LongPressCommand = new Command(OnLongPress);
		}

		public ICommand ClickCommand { get; }

		public ICommand LongPressCommand { get; }

		public bool IsSupported
		{
			get => isSupported;
			set => SetProperty(ref isSupported, value);
		}

		void OnClick()
		{
			try
			{
				HapticFeedback.Perform(HapticFeedbackType.Click);
			}
			catch (FeatureNotSupportedException)
			{
				IsSupported = false;
			}
			catch (Exception ex)
			{
				DisplayAlertAsync($"Unable to HapticFeedback: {ex.Message}");
			}
		}

		void OnLongPress()
		{
			try
			{
				HapticFeedback.Perform(HapticFeedbackType.LongPress);
			}
			catch (FeatureNotSupportedException)
			{
				IsSupported = false;
			}
			catch (Exception ex)
			{
				DisplayAlertAsync($"Unable to HapticFeedback: {ex.Message}");
			}
		}
	}
}
