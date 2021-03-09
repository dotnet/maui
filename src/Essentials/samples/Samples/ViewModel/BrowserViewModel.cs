using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	public class BrowserViewModel : BaseViewModel
	{
		string browserStatus;
		string uri = "http://xamarin.com";
		int browserType = (int)BrowserLaunchMode.SystemPreferred;
		int browserTitleType = (int)BrowserTitleMode.Default;
		int controlColor = 0;
		int toolbarColor = 0;
		bool presentAsFormSheet = false;
		bool presentAsPageSheet = false;
		bool launchAdjacent = false;

		Dictionary<string, Color> colorDictionary;

		public List<string> AllColors { get; }

		public BrowserViewModel()
		{
			OpenUriCommand = new Command(OpenUri);

			colorDictionary = typeof(Color)
				.GetFields()
				.Where(f => f.FieldType == typeof(Color) && f.IsStatic && f.IsPublic)
				.ToDictionary(f => f.Name, f => (Color)f.GetValue(null));

			var colors = colorDictionary.Keys.ToList();
			colors.Insert(0, "None");

			AllColors = colors;
		}

		public ICommand OpenUriCommand { get; }

		public string BrowserStatus
		{
			get => browserStatus;
			set => SetProperty(ref browserStatus, value);
		}

		public string Uri
		{
			get => uri;
			set => SetProperty(ref uri, value);
		}

		public List<string> BrowserLaunchModes { get; } =
			new List<string>
			{
				$"Use System-Preferred Browser",
				$"Use Default Browser App"
			};

		public int BrowserType
		{
			get => browserType;
			set => SetProperty(ref browserType, value);
		}

		public List<string> BrowserTitleModes { get; } =
			new List<string>
			{
				$"Use Default Mode",
				$"Show Title",
				$"Hide Title"
			};

		public int BrowserTitleType
		{
			get => browserTitleType;
			set => SetProperty(ref browserTitleType, value);
		}

		public int ToolbarColor
		{
			get => toolbarColor;
			set => SetProperty(ref toolbarColor, value);
		}

		public int ControlColor
		{
			get => controlColor;
			set => SetProperty(ref controlColor, value);
		}

		public bool PresentAsFormSheet
		{
			get => presentAsFormSheet;
			set => SetProperty(ref presentAsFormSheet, value);
		}

		public bool PresentAsPageSheet
		{
			get => presentAsPageSheet;
			set => SetProperty(ref presentAsPageSheet, value);
		}

		public bool LaunchAdjacent
		{
			get => launchAdjacent;
			set => SetProperty(ref launchAdjacent, value);
		}

		async void OpenUri()
		{
			if (IsBusy)
				return;

			IsBusy = true;
			try
			{
				var flags = BrowserLaunchFlags.None;
				if (PresentAsPageSheet)
					flags |= BrowserLaunchFlags.PresentAsPageSheet;
				if (PresentAsFormSheet)
					flags |= BrowserLaunchFlags.PresentAsFormSheet;
				if (LaunchAdjacent)
					flags |= BrowserLaunchFlags.LaunchAdjacent;

				await Browser.OpenAsync(uri, new BrowserLaunchOptions
				{
					LaunchMode = (BrowserLaunchMode)BrowserType,
					TitleMode = (BrowserTitleMode)BrowserTitleType,
					PreferredToolbarColor = GetColor(ToolbarColor),
					PreferredControlColor = GetColor(ControlColor),
					Flags = flags
				});
			}
			catch (Exception e)
			{
				BrowserStatus = $"Unable to open Uri {e.Message}";
				Debug.WriteLine(browserStatus);
			}
			finally
			{
				IsBusy = false;
			}

			Color? GetColor(int index)
			{
				return index <= 0
					? (Color?)null
					: (System.Drawing.Color)colorDictionary[AllColors[index]];
			}
		}
	}
}
