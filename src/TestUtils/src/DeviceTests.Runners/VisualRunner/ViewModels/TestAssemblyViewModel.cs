#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	public class TestAssemblyViewModel : ViewModelBase
	{
		readonly ObservableCollection<TestCaseViewModel> _allTests;
		readonly FilteredCollectionView<TestCaseViewModel, FilterArgs> _filteredTests;
		readonly ITestNavigation _navigation;
		readonly ITestRunner _runner;
		readonly List<TestCaseViewModel> _results;

		CancellationTokenSource? _filterCancellationTokenSource;
		TestState _result;
		TestState _resultFilter;
		RunStatus _runStatus;
		string? _searchQuery;

		string? _detailText;
		string? _displayName;

		bool _isBusy;
		int _notRun;
		int _passed;
		int _failed;
		int _skipped;

		internal TestAssemblyViewModel(AssemblyRunInfo runInfo, ITestNavigation navigation, ITestRunner runner)
		{
			_navigation = navigation;
			_runner = runner;

			RunInfo = runInfo;

			RunAllTestsCommand = new Command(RunAllTestsExecute, () => !_isBusy);
			RunFilteredTestsCommand = new Command(RunFilteredTestsExecute, () => !_isBusy);
			NavigateToResultCommand = new Command<TestCaseViewModel?>(NavigateToResultExecute, tc => !_isBusy);

			DisplayName = Path.GetFileNameWithoutExtension(runInfo.AssemblyFileName);

			_allTests = new ObservableCollection<TestCaseViewModel>(runInfo.TestCases);
			_results = new List<TestCaseViewModel>(runInfo.TestCases);

			_allTests.CollectionChanged += (_, args) =>
			{
				lock (_results)
				{
					switch (args.Action)
					{
						case NotifyCollectionChangedAction.Add:
							foreach (TestCaseViewModel item in args.NewItems!)
								_results.Add(item);
							break;
						case NotifyCollectionChangedAction.Remove:
							foreach (TestCaseViewModel item in args.OldItems!)
								_results.Remove(item);
							break;
						default:
							throw new InvalidOperationException($"I can't work with {args.Action}");
					}
				}
			};

			_filteredTests = new FilteredCollectionView<TestCaseViewModel, FilterArgs>(
				_allTests,
				IsTestFilterMatch,
				new FilterArgs(SearchQuery, ResultFilter),
				new TestComparer());

			_filteredTests.ItemChanged += (sender, args) => UpdateCaption();
			_filteredTests.CollectionChanged += (sender, args) => UpdateCaption();

			Result = TestState.NotRun;
			RunStatus = RunStatus.NotRun;

			UpdateCaption();
		}

		public AssemblyRunInfo RunInfo { get; }

		public Command RunAllTestsCommand { get; }

		public Command RunFilteredTestsCommand { get; }

		public Command NavigateToResultCommand { get; }

		public IList<TestCaseViewModel> TestCases => _filteredTests;

		public string DetailText
		{
			get => _detailText ?? string.Empty;
			private set => Set(ref _detailText, value);
		}

		public string DisplayName
		{
			get => _displayName ?? string.Empty;
			private set => Set(ref _displayName, value);
		}

		public bool IsBusy
		{
			get => _isBusy;
			private set
			{
				if (Set(ref _isBusy, value))
				{
					RunAllTestsCommand.ChangeCanExecute();
					RunFilteredTestsCommand.ChangeCanExecute();
				}
			}
		}

		public TestState Result
		{
			get => _result;
			set => Set(ref _result, value);
		}

		public TestState ResultFilter
		{
			get => _resultFilter;
			set => Set(ref _resultFilter, value, FilterAfterDelay);
		}

		public RunStatus RunStatus
		{
			get => _runStatus;
			private set => Set(ref _runStatus, value);
		}

		public string SearchQuery
		{
			get => _searchQuery ?? string.Empty;
			set => Set(ref _searchQuery, value, FilterAfterDelay);
		}

		public int NotRun
		{
			get => _notRun;
			set => Set(ref _notRun, value);
		}

		public int Passed
		{
			get => _passed;
			set => Set(ref _passed, value);
		}

		public int Failed
		{
			get => _failed;
			set => Set(ref _failed, value);
		}

		public int Skipped
		{
			get => _skipped;
			set => Set(ref _skipped, value);
		}

		void FilterAfterDelay()
		{
			_filterCancellationTokenSource?.Cancel();
			_filterCancellationTokenSource = new CancellationTokenSource();

			var token = _filterCancellationTokenSource.Token;

			Task.Delay(500, token)
				.ContinueWith(
					x => { _filteredTests.FilterArgument = new FilterArgs(SearchQuery, ResultFilter); },
					token,
					TaskContinuationOptions.None,
					TaskScheduler.FromCurrentSynchronizationContext());
		}

		static bool IsTestFilterMatch(TestCaseViewModel test, FilterArgs query)
		{
			if (test == null)
				throw new ArgumentNullException(nameof(test));

			var state = query.State;
			var pattern = query.Query;

			TestState? requiredTestState = state switch
			{
				TestState.All => null,
				TestState.Passed => TestState.Passed,
				TestState.Failed => TestState.Failed,
				TestState.Skipped => TestState.Skipped,
				TestState.NotRun => TestState.NotRun,
				_ => throw new ArgumentException(),
			};

			if (requiredTestState.HasValue && test.Result != requiredTestState.Value)
				return false;

			return
				string.IsNullOrWhiteSpace(pattern) ||
				test.DisplayName.IndexOf(pattern.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
		}

		async void RunAllTestsExecute()
		{
			try
			{
				IsBusy = true;
				await _runner.RunAsync(new[] { RunInfo });
			}
			finally
			{
				IsBusy = false;
			}
		}

		async void RunFilteredTestsExecute()
		{
			try
			{
				IsBusy = true;
				await _runner.RunAsync(_filteredTests);
			}
			finally
			{
				IsBusy = false;
			}
		}

		async void NavigateToResultExecute(TestCaseViewModel? testCase)
		{
			if (testCase == null)
				return;

			await _runner.RunAsync(testCase);

			await _navigation.NavigateTo(PageType.TestResult, testCase.TestResult);
		}

		void UpdateCaption()
		{
			var count = _allTests.Count;

			if (count == 0)
			{
				DetailText = "no test was found inside this assembly";
				RunStatus = RunStatus.NoTests;

				return;
			}

			// This would occasionally crash when running the group operation
			// most likely because of thread safety issues.
			Dictionary<TestState, int> results;
			lock (_results)
			{
				results =
					_results
						.GroupBy(r => r.Result)
						.ToDictionary(k => k.Key, v => v.Count());
			}

			results.TryGetValue(TestState.Passed, out int passed);
			results.TryGetValue(TestState.Failed, out int failure);
			results.TryGetValue(TestState.Skipped, out int skipped);
			results.TryGetValue(TestState.NotRun, out int notRun);

			Passed = passed;
			Failed = failure;
			Skipped = skipped;
			NotRun = notRun;

			var prefix = notRun == 0 ? "Complete - " : string.Empty;

			if (failure == 0 && notRun == 0)
			{
				// No failures and all run

				DetailText = $"{prefix}✔ {passed}";
				RunStatus = RunStatus.Ok;

				Result = TestState.Passed;
			}
			else if (failure > 0 || (notRun > 0 && notRun < count))
			{
				// Either some failed or some are not run

				DetailText = $"{prefix}✔ {passed}, ⛔ {failure}, ⚠ {skipped}, 🔷 {notRun}";

				if (failure > 0) // always show a fail
				{
					RunStatus = RunStatus.Failed;
					Result = TestState.Failed;
				}
				else
				{
					if (passed > 0)
					{
						RunStatus = RunStatus.Ok;
						Result = TestState.Passed;
					}
					else if (skipped > 0)
					{
						RunStatus = RunStatus.Skipped;
						Result = TestState.Skipped;
					}
					else
					{
						// just not run
						RunStatus = RunStatus.NotRun;
						Result = TestState.NotRun;
					}
				}
			}
			else if (Result == TestState.NotRun)
			{
				// Not run

				DetailText = $"🔷 {count}, {Result}";
				RunStatus = RunStatus.NotRun;
			}
		}

		class TestComparer : IComparer<TestCaseViewModel>
		{
			public int Compare(TestCaseViewModel? x, TestCaseViewModel? y) =>
				string.Compare(x?.DisplayName, y?.DisplayName, StringComparison.OrdinalIgnoreCase);
		}
	}

	struct FilterArgs
	{
		public string Query { get; set; }
		public TestState State { get; set; }

		public FilterArgs(string query, TestState state)
		{
			Query = query;
			State = state;
		}

		public override bool Equals([NotNullWhen(true)] object? obj)
		{
			if (obj is FilterArgs args)
			{
				return args.State == State && args.Query == Query;
			}

			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Query.GetHashCode(StringComparison.InvariantCulture) ^ State.GetHashCode();
		}
	}
}
