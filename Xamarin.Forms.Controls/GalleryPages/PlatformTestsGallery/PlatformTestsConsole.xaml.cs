using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using Xamarin.Forms.Controls.Tests;
using Xamarin.Forms.Xaml;
using NUnit.Framework.Internal;
using Xamarin.Forms.Internals;
using System;
using System.Threading.Tasks;

namespace Xamarin.Forms.Controls.GalleryPages.PlatformTestsGallery
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PlatformTestsConsole : ContentPage
	{
		const string FailedText = "FAILED";
		const string InconclusiveText = "Inconclusive";
		const string SuccessText = "SUCCESS";
		bool _runFailed;
		bool _runInconclusive;
		readonly Color _successColor = Color.Green;
		readonly Color _failColor = Color.Red;
		readonly Color _inconclusiveColor = Color.Goldenrod;

		int _finishedAssemblyCount = 0;

		public PlatformTestsConsole()
		{
			InitializeComponent();
			MessagingCenter.Subscribe<ITestResult>(this, "AssemblyFinished", AssemblyFinished);

			MessagingCenter.Subscribe<ITest>(this, "TestStarted", TestStarted);
			MessagingCenter.Subscribe<ITestResult>(this, "TestFinished", TestFinished);

			MessagingCenter.Subscribe<Exception>(this, "TestRunnerError", OutputTestRunnerError);
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			// Only want to run a subset of tests? Create a filter and pass it into tests.Run()
			//var filter = new TestNameContainsFilter("Bugzilla");
			
			var tests = new PlatformTestRunner();
			await Task.Run(() => tests.Run()).ConfigureAwait(false);
		}

		void DisplayOverallResult()
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				if (_runFailed)
				{
					DisplayFailResult();
				}
				else if (_runInconclusive)
				{
					Status.Text = InconclusiveText;
					Status.TextColor = _inconclusiveColor;
				}
				else
				{
					Status.Text = SuccessText;
					Status.TextColor = _successColor;
				}
			});
		}

		void DisplayFailResult(string failText = null) 
		{
			failText = failText ?? FailedText;

			Status.Text = failText;
			Status.TextColor = _failColor;
		}

		void AssemblyFinished(ITestResult assembly)
		{
			_finishedAssemblyCount += 1;
			if (_finishedAssemblyCount == 2)
			{
				DisplayOverallResult();
			}
		}

		void TestStarted(ITest test)
		{
			switch (test)
			{
				case TestFixture fixture:
					OutputFixtureStarted(fixture);
					break;
				default:
					break;
			}
		}

		void OutputFixtureStarted(TestFixture testFixture) 
		{
			var name = testFixture.Name;

			var label = new Label { Text = $"{name} Started", LineBreakMode = LineBreakMode.HeadTruncation };

			Device.BeginInvokeOnMainThread(() =>
			{
				Results.Children.Add(label);
			});
		}

		void TestFinished(ITestResult result)
		{
			switch (result)
			{
				case TestCaseResult testCaseResult:
					OutputTestCaseResult(testCaseResult);
					break;
				case TestSuiteResult testSuiteResult:
					OutputSuiteResult(testSuiteResult);
					break;
				default:
					break;
			}
		}

		void OutputTestCaseResult(TestCaseResult result)
		{
			var name = result.Test.Name; // ShortenTestName(result.FullName);

			var outcome = "Fail";

			if (result.PassCount > 0)
			{
				outcome = "Pass";
			}
			else if (result.InconclusiveCount > 0)
			{
				outcome = "Inconclusive";
			}

			var label = new Label { Text = $"{name}: {outcome}.", LineBreakMode = LineBreakMode.HeadTruncation };

			if (result.FailCount > 0)
			{
				label.TextColor = _failColor;
				_runFailed = true;
			}
			else if (result.InconclusiveCount > 0)
			{
				label.TextColor = _inconclusiveColor;
				_runInconclusive = true;
			}
			else
			{
				label.TextColor = _successColor;
			}

			var margin = new Thickness(15, 0, 0, 0);
			label.Margin = margin;

			var toAdd = new List<View> { label };

			foreach (var assertionResult in result.AssertionResults)
			{
				if (assertionResult.Status != AssertionStatus.Passed)
				{
					toAdd.Add(new Label { Text = assertionResult.Message });
					toAdd.Add(new Editor { Text = assertionResult.StackTrace, IsReadOnly = true });
				}
			}

			if (!string.IsNullOrEmpty(result.Output))
			{
				toAdd.Add(new Label { Text = result.Output, Margin = margin });
			}

			if (result.Test.RunState == RunState.NotRunnable)
			{
				var reasonBag = result.Test.Properties[PropertyNames.SkipReason];

				var reasonText = "";
				foreach (var reason in reasonBag)
				{
					reasonText += reason;
				}

				if (string.IsNullOrEmpty(reasonText))
				{
					reasonText = @"¯\_(ツ)_/¯";
				}

				toAdd.Add(new Label { Text = $"Test was not runnable. Reason: {reasonText}", FontAttributes = FontAttributes.Bold, Margin = margin });
			}

			Device.BeginInvokeOnMainThread(() =>
			{
				foreach (var outputView in toAdd)
				{
					Results.Children.Add(outputView);
				}
				
			});
		}

		void OutputSuiteResult(TestSuiteResult result)
		{
			if (!(result.Test is TestFixture))
			{
				return;
			}

			var label = new Label { Text = $"{result.Name} Finished.", LineBreakMode = LineBreakMode.HeadTruncation };
			var counts = new Label { Text = $"Passed: {result.PassCount}; Failed: {result.FailCount}; Inconclusive: {result.InconclusiveCount}" };

			if (result.FailCount > 0)
			{
				label.TextColor = _failColor;
				_runFailed = true;
			}
			else if (result.InconclusiveCount > 0)
			{
				label.TextColor = _inconclusiveColor;
				_runInconclusive = true;
			}
			else
			{
				label.TextColor = _successColor;
			}

			counts.TextColor = label.TextColor;

			Device.BeginInvokeOnMainThread(() =>
			{
				Results.Children.Add(label);
				Results.Children.Add(counts);
			});
		}

		void OutputTestRunnerError(Exception ex)
		{
			Device.BeginInvokeOnMainThread(() =>
			{
				DisplayFailResult(ex.Message);
			});
		}
	}
}