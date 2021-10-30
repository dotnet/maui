using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Samples.Helpers;

namespace Samples.ViewModel
{
	public class LauncherViewModel : BaseViewModel
	{
		string fileAttachmentName;
		string fileAttachmentContents;

		public string LaunchUri { get; set; }

		public ICommand LaunchCommand { get; }

		public ICommand CanLaunchCommand { get; }

		public ICommand LaunchMailCommand { get; }

		public ICommand LaunchBrowserCommand { get; }

		public ICommand LaunchFileCommand { get; }

		public LauncherViewModel()
		{
			LaunchCommand = new Command(OnLaunch);
			LaunchMailCommand = new Command(OnLaunchMail);
			LaunchBrowserCommand = new Command(OnLaunchBrowser);
			CanLaunchCommand = new Command(CanLaunch);
			LaunchFileCommand = new Command<Microsoft.Maui.Controls.View>(OnFileRequest);
		}

		public string FileAttachmentContents
		{
			get => fileAttachmentContents;
			set => SetProperty(ref fileAttachmentContents, value);
		}

		public string FileAttachmentName
		{
			get => fileAttachmentName;
			set => SetProperty(ref fileAttachmentName, value);
		}

		async void OnLaunchBrowser()
		{
			await Launcher.OpenAsync("https://github.com/xamarin/Essentials");
		}

		async void OnLaunch()
		{
			try
			{
				await Launcher.OpenAsync(LaunchUri);
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Uri {LaunchUri} could not be launched: {ex}");
			}
		}

		async void OnLaunchMail()
		{
			await Launcher.OpenAsync("mailto:");
		}

		async void CanLaunch()
		{
			try
			{
				var canBeLaunched = await Launcher.CanOpenAsync(LaunchUri);
				await DisplayAlertAsync($"Uri {LaunchUri} can be launched: {canBeLaunched}");
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Uri {LaunchUri} could not be verified as launchable: {ex}");
			}
		}

		async void OnFileRequest(Microsoft.Maui.Controls.View element)
		{
			if (!string.IsNullOrWhiteSpace(FileAttachmentContents))
			{
				// create a temprary file
				var fn = string.IsNullOrWhiteSpace(FileAttachmentName) ? "Attachment.txt" : FileAttachmentName.Trim();
				var file = Path.Combine(FileSystem.CacheDirectory, fn);
				File.WriteAllText(file, FileAttachmentContents);

				var rect = element.GetAbsoluteBounds();
				rect.Y += 40;
				await Launcher.OpenAsync(new OpenFileRequest
				{
					File = new ReadOnlyFile(file),
					PresentationSourceBounds = rect
				});
			}
		}
	}
}
