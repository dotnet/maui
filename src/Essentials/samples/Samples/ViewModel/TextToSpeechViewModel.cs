using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace Samples.ViewModel
{
	public class TextToSpeechViewModel : BaseViewModel
	{
		CancellationTokenSource cts;

		string text;
		bool advancedOptions;
		float volume;
		float pitch;
		float rate;
		string locale = "Default";
		Locale selectedLocale;

		public TextToSpeechViewModel()
		{
			SpeakCommand = new Command<bool>(OnSpeak);
			CancelCommand = new Command(OnCancel);
			PickLocaleCommand = new Command(async () => await OnPickLocale());

			Text = "Xamarin Essentials makes text to speech easy!";

			AdvancedOptions = false;
			Volume = 1.0f;
			Pitch = 1.0f;
			Rate = 1.0f;
		}

		public override void OnDisappearing()
		{
			OnCancel();

			base.OnDisappearing();
		}

		void OnSpeak(bool multiple)
		{
			if (IsBusy)
				return;

			IsBusy = true;

			cts = new CancellationTokenSource();

			SpeechOptions options = null;
			if (AdvancedOptions)
			{
				options = new SpeechOptions
				{
					Volume = Volume,
					Pitch = Pitch,
					Locale = selectedLocale,
					Rate = Rate
				};
			}

			Task speaks = null;
			if (multiple)
			{
				speaks = Task.WhenAll(
					TextToSpeech.SpeakAsync(Text + " 1 ", options, cancelToken: cts.Token),
					TextToSpeech.SpeakAsync(Text + " 2 ", options, cancelToken: cts.Token),
					TextToSpeech.SpeakAsync(Text + " 3 ", options, cancelToken: cts.Token));
			}
			else
			{
				speaks = TextToSpeech.SpeakAsync(Text, options, cts.Token);
			}

			// use ContinueWith so we don't have to catch the cancelled exceptions
			speaks.ContinueWith(t => IsBusy = false);
		}

		void OnCancel()
		{
			if (!IsBusy && (cts?.IsCancellationRequested ?? true))
				return;

			cts.Cancel();

			IsBusy = false;
		}

		async Task OnPickLocale()
		{
			var allLocales = await TextToSpeech.GetLocalesAsync();
			var locales = allLocales
				.OrderBy(i => i.Language.ToLowerInvariant())
				.ToArray();

			var languages = locales
				.Select(i => string.IsNullOrEmpty(i.Country) ? i.Language : $"{i.Language} ({i.Country})")
				.ToArray();

			var result = await Application.Current.Windows[0].Page.DisplayActionSheetAsync("Pick", "OK", null, languages);

			if (!string.IsNullOrEmpty(result) && Array.IndexOf(languages, result) is int idx && idx != -1)
			{
				selectedLocale = locales[idx];
				Locale = result;
			}
			else
			{
				selectedLocale = null;
				Locale = "Default";
			}
		}

		public ICommand CancelCommand { get; }

		public ICommand SpeakCommand { get; }

		public ICommand PickLocaleCommand { get; }

		public string Text
		{
			get => text;
			set => SetProperty(ref text, value);
		}

		public bool AdvancedOptions
		{
			get => advancedOptions;
			set => SetProperty(ref advancedOptions, value);
		}

		public float Volume
		{
			get => volume;
			set => SetProperty(ref volume, value);
		}

		public float Pitch
		{
			get => pitch;
			set => SetProperty(ref pitch, value);
		}

		public float Rate
		{
			get => rate;
			set => SetProperty(ref rate, value);
		}

		public string Locale
		{
			get => locale;
			set => SetProperty(ref locale, value);
		}
	}
}
