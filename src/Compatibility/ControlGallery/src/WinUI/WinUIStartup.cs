using System;
using System.Globalization;
using System.Reflection.PortableExecutable;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics.Display;
using Windows.System;
using Windows.UI.ViewManagement;

namespace Microsoft.Maui.Controls.ControlGallery.WinUI
{
	public class WinUIMauiProgram
	{
		public static MauiApp CreateMauiApp()
		{
			var builder = MauiProgram.CreateMauiAppBuilder();

			builder.ConfigureLifecycleEvents(lifecycle => lifecycle
				.AddWindows(windows => windows
					.OnLaunching((_, e) =>
					{
						if (!string.IsNullOrWhiteSpace(e.Arguments) && e.Arguments.Contains("RunningAsUITests", StringComparison.Ordinal))
						{
							App.RunningAsUITests = true;
							ControlGallery.App.PreloadTestCasesIssuesList = false;
						}
					})
					.OnActivated(WinUIPageStartup.OnActivated)));

			return builder.Build();
		}
	}

	static partial class WinUIPageStartup
	{
		public static void OnActivated(UI.Xaml.Window window, UI.Xaml.WindowActivatedEventArgs e)
		{
			Application.Current.PropertyChanged += OnAppPropertyChanged;

			WireUpKeyDown(window);

			void OnAppPropertyChanged(object sender, global::System.ComponentModel.PropertyChangedEventArgs e)
			{
				if (e.PropertyName == nameof(Application.MainPage))
					WireUpKeyDown(window);
			}
		}

		// TODO WINUI3 not sure the best way to detect the content swap out
		static void WireUpKeyDown(UI.Xaml.Window window)
		{
			window.DispatcherQueue.TryEnqueue(() =>
			{
				if (window.Content != null)
				{
					window.Content.KeyDown -= OnKeyDown;
					window.Content.KeyDown += OnKeyDown;
				}
				else
				{
					WireUpKeyDown(window);
				}
			});
		}

		static void OnKeyDown(object sender, KeyRoutedEventArgs args)
		{
			if (args.Key == VirtualKey.Escape)
			{
				(Application.Current as ControlGallery.App)
					.Reset();

				args.Handled = true;
			}
			else if (args.Key == VirtualKey.F1)
			{
				(Application.Current as ControlGallery.App)
					.PlatformTest();
			}
		}

	}
}