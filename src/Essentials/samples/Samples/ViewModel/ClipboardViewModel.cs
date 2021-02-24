using System;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	class ClipboardViewModel : BaseViewModel
	{
		string fieldValue;
		string lastCopied;

		public ClipboardViewModel()
		{
			CopyCommand = new Command(OnCopy);
			PasteCommand = new Command(OnPaste);
			CheckCommand = new Command(OnCheck);
		}

		public ICommand CopyCommand { get; }

		public ICommand PasteCommand { get; }

		public ICommand CheckCommand { get; }

		public string FieldValue
		{
			get => fieldValue;
			set => SetProperty(ref fieldValue, value);
		}

		public string LastCopied
		{
			get => lastCopied;
			set => SetProperty(ref lastCopied, value);
		}

		public override void OnAppearing()
		{
			try
			{
				Clipboard.ClipboardContentChanged += OnClipboardContentChanged;
			}
			catch (FeatureNotSupportedException)
			{
			}
		}

		public override void OnDisappearing()
		{
			try
			{
				Clipboard.ClipboardContentChanged -= OnClipboardContentChanged;
			}
			catch (FeatureNotSupportedException)
			{
			}
		}

		void OnClipboardContentChanged(object sender, EventArgs args)
		{
			LastCopied = $"Last copied text at {DateTime.UtcNow:T}";
		}

		async void OnCopy()
		{
			await Clipboard.SetTextAsync(FieldValue);
		}

		async void OnPaste()
		{
			var text = await Clipboard.GetTextAsync();
			if (!string.IsNullOrWhiteSpace(text))
			{
				FieldValue = text;
			}
		}

		async void OnCheck()
		{
			await DisplayAlertAsync($"Has text: {Clipboard.HasText}");
		}
	}
}
