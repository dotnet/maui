#nullable enable
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
	partial class HomePage : ContentPage
	{
		public HomePage()
		{
			InitializeComponent();

			this.Loaded += HomePage_Loaded;
		}

		bool hasRunHeadless = false;

		private async void HomePage_Loaded(object? sender, EventArgs e)
		{
			string? testResultsFile = null;

#if WINDOWS
			var cliArgs = Environment.GetCommandLineArgs();
			if (cliArgs.Length > 1)
			{
				testResultsFile = HeadlessTestRunner.TestResultsFile = ControlsHeadlessTestRunner.TestResultsFile = cliArgs.Skip(1).FirstOrDefault();
				ControlsHeadlessTestRunner.LoopCount = int.Parse(cliArgs.Skip(2).FirstOrDefault() ?? "-1");
			}
#endif

			if (!string.IsNullOrEmpty(testResultsFile) && !hasRunHeadless)
			{
				hasRunHeadless = true;

#if !WINDOWS
				var headlessRunner = Handler!.MauiContext!.Services.GetRequiredService<HeadlessTestRunner>();

				await headlessRunner.RunTestsAsync();
#else
				if (cliArgs.Length >= 3)
				{
					var headlessRunner = Handler!.MauiContext!.Services.GetRequiredService<ControlsHeadlessTestRunner>();
					await headlessRunner.RunTestsAsync();
				}
				else
				{
					var headlessRunner = Handler!.MauiContext!.Services.GetRequiredService<HeadlessTestRunner>();
					await headlessRunner.RunTestsAsync();
				}
#endif

				Process.GetCurrentProcess().Kill();
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			assemblyList.SelectedItem = null;

			if (BindingContext is ViewModelBase vm)
				vm.OnAppearing();
		}
	}
}