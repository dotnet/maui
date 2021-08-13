using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Utilities;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    public class TestAssemblyViewModel : ViewModelBase
    {
        readonly ObservableCollection<TestCaseViewModel> allTests;
        readonly FilteredCollectionView<TestCaseViewModel, Tuple<string, TestState>> filteredTests;
        readonly DelegateCommand runAllTestsCommand;
        readonly DelegateCommand runFilteredTestsCommand;

        readonly ITestRunner runner;
        string detailText;
        string displayName;
        CancellationTokenSource filterCancellationTokenSource;
        bool isBusy;
        TestState result;
        TestState resultFilter;
        RunStatus runStatus;
        string searchQuery;
        private int _notRun;
        private int _passed;
        private int _failed;
        private int _skipped;

        internal TestAssemblyViewModel(AssemblyRunInfo runInfo, ITestRunner runner)
        {
            RunInfo = runInfo;
            ;
            this.runner = runner;

            runAllTestsCommand = new DelegateCommand(RunAllTests, () => !isBusy);
            runFilteredTestsCommand = new DelegateCommand(RunFilteredTests, () => !isBusy);

            DisplayName = Path.GetFileNameWithoutExtension(runInfo.AssemblyFileName);

            allTests = new ObservableCollection<TestCaseViewModel>(runInfo.TestCases);

            filteredTests = new FilteredCollectionView<TestCaseViewModel, Tuple<string, TestState>>(
                allTests,
                IsTestFilterMatch,
                Tuple.Create(SearchQuery, ResultFilter),
                new TestComparer()
                );

            filteredTests.ItemChanged += (sender, args) => UpdateCaption();
            filteredTests.CollectionChanged += (sender, args) => UpdateCaption();

            Result = TestState.NotRun;
            RunStatus = RunStatus.NotRun;

            UpdateCaption();
        }

        public ICommand RunAllTestsCommand => runAllTestsCommand;

        public ICommand RunFilteredTestsCommand => runFilteredTestsCommand;


        public IList<TestCaseViewModel> TestCases => filteredTests;

        public string DetailText
        {
            get { return detailText; }
            private set { Set(ref detailText, value); }
        }

        public string DisplayName
        {
            get { return displayName; }
            private set { Set(ref displayName, value); }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            private set
            {
                if (Set(ref isBusy, value))
                {
                    runAllTestsCommand.RaiseCanExecuteChanged();
                    runFilteredTestsCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public TestState Result
        {
            get { return result; }
            set { Set(ref result, value); }
        }

        public TestState ResultFilter
        {
            get { return resultFilter; }
            set
            {
                if (Set(ref resultFilter, value))
                {
                    FilterAfterDelay();
                }
            }
        }

        public RunStatus RunStatus
        {
            get { return runStatus; }
            private set { Set(ref runStatus, value); }
        }

        public string SearchQuery
        {
            get { return searchQuery; }
            set
            {
                if (Set(ref searchQuery, value))
                {
                    FilterAfterDelay();
                }
            }
        }

        internal AssemblyRunInfo RunInfo { get; }

        void FilterAfterDelay()
        {
            filterCancellationTokenSource?.Cancel();

            filterCancellationTokenSource = new CancellationTokenSource();
            var token = filterCancellationTokenSource.Token;

            Task.Delay(500, token)
                .ContinueWith(
                    x => { filteredTests.FilterArgument = Tuple.Create(SearchQuery, ResultFilter); },
                    token,
                    TaskContinuationOptions.None,
                    TaskScheduler.FromCurrentSynchronizationContext());
        }

        static bool IsTestFilterMatch(TestCaseViewModel test, Tuple<string, TestState> query)
        {
            if (test == null) throw new ArgumentNullException(nameof(test));
            if (query == null) throw new ArgumentNullException(nameof(query));

            TestState? requiredTestState;
            switch (query.Item2)
            {
                case TestState.All:
                    requiredTestState = null;
                    break;
                case TestState.Passed:
                    requiredTestState = TestState.Passed;
                    break;
                case TestState.Failed:
                    requiredTestState = TestState.Failed;
                    break;
                case TestState.Skipped:
                    requiredTestState = TestState.Skipped;
                    break;
                case TestState.NotRun:
                    requiredTestState = TestState.NotRun;
                    break;
                default:
                    throw new ArgumentException();
            }

            if (requiredTestState.HasValue && test.Result != requiredTestState.Value)
            {
                return false;
            }

            var pattern = query.Item1;
            return string.IsNullOrWhiteSpace(pattern) || test.DisplayName.IndexOf(pattern.Trim(), StringComparison.OrdinalIgnoreCase) >= 0;
        }

        async void RunAllTests()
        {
            try
            {
                IsBusy = true;
                await runner.Run(new[] { RunInfo });
            }
            finally
            {
                IsBusy = false;
            }
        }

        async void RunFilteredTests()
        {
            try
            {
                IsBusy = true;
                await runner.Run(filteredTests);
            }
            finally
            {
                IsBusy = false;
            }
        }


        public int NotRun { get => _notRun; set => Set(ref _notRun, value); }
        public int Passed { get => _passed; set => Set(ref _passed, value); }
        public int Failed { get => _failed; set => Set(ref _failed, value); }
        public int Skipped { get => _skipped; set => Set(ref _skipped, value); }

        void UpdateCaption()
        {
            var count = allTests.Count;

            if (count == 0)
            {
                DetailText = "no test was found inside this assembly";

                RunStatus = RunStatus.NoTests;
            }
            else
            {
                var outcomes = allTests.GroupBy(r => r.Result);

                var results = outcomes.ToDictionary(k => k.Key, v => v.Count());

                results.TryGetValue(TestState.Passed, out int positive);

                results.TryGetValue(TestState.Failed, out int failure);

                results.TryGetValue(TestState.Skipped, out int skipped);

                results.TryGetValue(TestState.NotRun, out int notRun);

                Passed = positive;
                Failed = failure;
                Skipped = skipped;
                NotRun = notRun;

                string prefix = notRun == 0 ? "Complete - " : string.Empty;

                // No failures and all run
                if (failure == 0 && notRun == 0)
                {
                    DetailText = $"{prefix}✔ {positive}";
                    RunStatus = RunStatus.Ok;

                    Result = TestState.Passed;
                }
                else if (failure > 0 || (notRun > 0 && notRun < count))
                {
                    // we either have failures or some of the tests are not run
                    DetailText = $"{prefix}✔ {positive}, ⛔ {failure}, ⚠ {skipped}, 🔷 {notRun}";

                    if (failure > 0) // always show a fail
                    {
                        RunStatus = RunStatus.Failed;
                        Result = TestState.Failed;
                    }
                    else
                    {
                        if (positive > 0)
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
                    DetailText = $"🔷 {count}, {Result}";
                    RunStatus = RunStatus.NotRun;
                }
            }
        }

        class TestComparer : IComparer<TestCaseViewModel>
        {
            public int Compare(TestCaseViewModel x, TestCaseViewModel y)
            {
                var compare = string.Compare(x.DisplayName, y.DisplayName, StringComparison.OrdinalIgnoreCase);

                return compare;
            }
        }
    }
}