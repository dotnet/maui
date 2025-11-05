// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class BindingDiagnosticsInvalidTwoWayBindingViewModel(string text)
{
	public string Text { get; } = text; // Read-only property
}

public partial class BindingDiagnosticsInvalidTwoWayBinding : ContentPage
{
	public BindingDiagnosticsInvalidTwoWayBinding() => InitializeComponent();

	[TestFixture]
#if !DEBUG
	[Ignore("This test runs only in debug")]
#endif
	class Tests
	{
		bool enableDiagnosticsInitialState;

		[SetUp]
		public void Setup()
		{
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		[TearDown]
		public void TearDown()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
		}

		[Test]
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
			Assert.That(failures.Count, Is.EqualTo(1));
			Assert.That(layout.Entry.Text, Is.EqualTo("Hello, World!"));
			var failure = failures[0] as BindingErrorEventArgs;
			Assert.That(failure.XamlSourceInfo.LineNumber, Is.EqualTo(8));
			Assert.That(failure.Target, Is.TypeOf<Entry>());
			Assert.That(failure.TargetProperty, Is.EqualTo(Entry.TextProperty));


			layout.Entry.Text = "New Text";

			// after the Entry's Text is changed, it will again try to push its value to the target through
			// the two way binding
			Assert.That(failures.Count, Is.EqualTo(2));
			failure = failures[1] as BindingErrorEventArgs;
			Assert.That(failure.XamlSourceInfo.LineNumber, Is.EqualTo(8));
			Assert.That(failure.Target, Is.TypeOf<Entry>());
			Assert.That(failure.TargetProperty, Is.EqualTo(Entry.TextProperty));
		}

		[Test]
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
			Assert.That(failures.Count, Is.EqualTo(0));
			Assert.That(layout.Entry.Text, Is.EqualTo("Hello, World!"));

			layout.Entry.Text = "New Text";

			// after the Entry's Text is changed, it will again try to push its value to the target through
			// the two way binding - but nothing will happen
			Assert.That(failures.Count, Is.EqualTo(0));
			Assert.That(layout.Entry.Text, Is.EqualTo("New Text"));
		}
	}
}
