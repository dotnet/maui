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

		private async void HomePage_Loaded(object? sender, System.EventArgs e)
		{
			string? testResultsFile = null;

#if WINDOWS
			var cliArgs = Environment.GetCommandLineArgs();
			if (cliArgs.Length > 1)
				testResultsFile = HeadlessTestRunner.TestResultsFile = cliArgs.Skip(1).FirstOrDefault();
#endif

			if (!string.IsNullOrEmpty(testResultsFile) && !hasRunHeadless)
			{
				hasRunHeadless = true;

				var headlessRunner = this.Handler!.MauiContext!.Services.GetRequiredService<HeadlessTestRunner>();
				await headlessRunner.RunTestsAsync();

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