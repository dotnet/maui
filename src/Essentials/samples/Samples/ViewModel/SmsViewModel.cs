using System;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	public class SmsViewModel : BaseViewModel
	{
		string recipient;
		string messageText;

		public SmsViewModel()
		{
			SendSmsCommand = new Command(OnSendSms);
		}

		public string Recipient
		{
			get => recipient;
			set => SetProperty(ref recipient, value);
		}

		public string MessageText
		{
			get => messageText;
			set => SetProperty(ref messageText, value);
		}

		public ICommand SendSmsCommand { get; }

		async void OnSendSms()
		{
			if (IsBusy)
				return;
			IsBusy = true;

			try
			{
				var message = new SmsMessage(MessageText, Recipient.Split(',', '*'));
				await Sms.ComposeAsync(message);
			}
			catch (FeatureNotSupportedException)
			{
				await DisplayAlertAsync("Sending an SMS is not supported on this device.");
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to send Sms: {ex.Message}");
			}

			IsBusy = false;
		}
	}
}
