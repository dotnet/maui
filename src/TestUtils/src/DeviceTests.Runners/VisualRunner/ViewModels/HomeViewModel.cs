#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class HomeViewModel : ViewModelBase
	{
		readonly ITestNavigation _navigation;
		readonly ITestRunner _runner;

		string _diagnosticMessages = string.Empty;
		bool _loaded;
		bool _isBusy;

		internal HomeViewModel(ITestNavigation navigation, ITestRunner runner)
		{
			_navigation = navigation;
			_runner = runner;

			_runner.OnDiagnosticMessage += RunnerOnOnDiagnosticMessage;

			TestAssemblies = new ObservableCollection<TestAssemblyViewModel>();

			CreditsCommand = new Command(CreditsExecute);
			RunEverythingCommand = new Command(RunEverythingExecute, () => !_isBusy);
			NavigateToTestAssemblyCommand = new Command<TestAssemblyViewModel?>(NavigateToTestAssemblyExecute);
		}

		public ObservableCollection<TestAssemblyViewModel> TestAssemblies { get; private set; }

		public Command CreditsCommand { get; }

		public Command RunEverythingCommand { get; }

		public Command NavigateToTestAssemblyCommand { get; }

		public bool IsBusy
		{
			get => _isBusy;
			private set => Set(ref _isBusy, value, RunEverythingCommand.ChangeCanExecute);
		}

		public string DiagnosticMessages
		{
			get => _diagnosticMessages;
			private set => Set(ref _diagnosticMessages, value);
		}

		public override async void OnAppearing()
		{
			base.OnAppearing();

			await StartAssemblyScanAsync();
		}

		public async Task StartAssemblyScanAsync()
		{
			if (_loaded)
				return;

			IsBusy = true;

			try
			{
				var allTests = await _runner.DiscoverAsync();

				TestAssemblies = new ObservableCollection<TestAssemblyViewModel>(allTests);
				RaisePropertyChanged(nameof(TestAssemblies));
			}
			finally
			{
				IsBusy = false;
				_loaded = true;
			}
		}

		async void CreditsExecute()
		{
			await _navigation.NavigateTo(PageType.Credits);
		}

		async void RunEverythingExecute()
		{
			try
			{
				IsBusy = true;

				if (!string.IsNullOrWhiteSpace(DiagnosticMessages))
					DiagnosticMessages += $"----------{Environment.NewLine}";

				await _runner.RunAsync(TestAssemblies.Select(t => t.RunInfo).ToList(), "Run Everything");
			}
			finally
			{
				IsBusy = false;
			}
		}

		async void NavigateToTestAssemblyExecute(TestAssemblyViewModel? vm)
		{
			if (vm == null)
				return;

			await _navigation.NavigateTo(PageType.AssemblyTestList, vm);
		}

		void RunnerOnOnDiagnosticMessage(string s)
		{
			DiagnosticMessages += $"{s}{Environment.NewLine}{Environment.NewLine}";
		}
	}
}