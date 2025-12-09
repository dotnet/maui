// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class BindingDiagnosticsInvalidTwoWayBindingViewModel(string text)
{
	public string Text { get; } = text; // Read-only property
}

public partial class BindingDiagnosticsInvalidTwoWayBinding : ContentPage
{
	public BindingDiagnosticsInvalidTwoWayBinding() => InitializeComponent();

	[Collection("Xaml Inflation")]
#if !DEBUG
	public class Tests
	{
		// Tests only run in DEBUG mode
		[Fact(Skip = "This test runs only in debug")]
		public void TestSourceGen() { }
		[Fact(Skip = "This test runs only in debug")]
		public void Test() { }
	}
#else
	public class Tests : BaseTestFixture
	{
		bool enableDiagnosticsInitialState;

		protected internal override void Setup()
		{
			base.Setup();
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		protected internal override void TearDown()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
			base.TearDown();
		}

		[Fact]
		public void TestSourceGen()
		{
			List<BindingBaseErrorEventArgs> failures = new();
			BindingDiagnostics.BindingFailed += (o, e) => failures.Add(e);

			var layout = new BindingDiagnosticsInvalidTwoWayBinding(XamlInflator.SourceGen)
			{
				BindingContext = new BindingDiagnosticsInvalidTwoWayBindingViewModel("Hello, World!")
			};

			// after the binding context is set, the Entry will try to push its value to the target through
			// the two way binding
			Assert.Single(failures);
			Assert.Equal("Hello, World!", layout.Entry.Text);
			var failure = failures[0] as BindingErrorEventArgs;
			Assert.Equal(8, failure.XamlSourceInfo.LineNumber);
			Assert.IsType<Entry>(failure.Target);
			Assert.Equal(Entry.TextProperty, failure.TargetProperty);


			layout.Entry.Text = "New Text";

			// after the Entry's Text is changed, it will again try to push its value to the target through
			// the two way binding
			Assert.Equal(2, failures.Count);
			failure = failures[1] as BindingErrorEventArgs;
			Assert.Equal(8, failure.XamlSourceInfo.LineNumber);
			Assert.IsType<Entry>(failure.Target);
			Assert.Equal(Entry.TextProperty, failure.TargetProperty);
		}

		[Fact]
		public void Test()
		{
			List<BindingBaseErrorEventArgs> failures = new();
			BindingDiagnostics.BindingFailed += (o, e) => failures.Add(e);

			var layout = new BindingDiagnosticsInvalidTwoWayBinding(XamlInflator.XamlC)
			{
				BindingContext = new BindingDiagnosticsInvalidTwoWayBindingViewModel("Hello, World!")
			};

			// XamlC doesn't will not throw any exception or report any binding failures
			// the binding will simply pull the value from the VM
			Assert.Empty(failures);
			Assert.Equal("Hello, World!", layout.Entry.Text);

			layout.Entry.Text = "New Text";

			// after the Entry's Text is changed, it will again try to push its value to the target through
			// the two way binding - but nothing will happen
			Assert.Empty(failures);
			Assert.Equal("New Text", layout.Entry.Text);
		}
	}
#endif
}
