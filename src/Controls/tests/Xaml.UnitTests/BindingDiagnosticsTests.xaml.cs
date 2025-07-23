// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class BindingDiagnosticsTests : ContentPage
	{
		public BindingDiagnosticsTests() => InitializeComponent();

		public BindingDiagnosticsTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
#if !DEBUG
		[Ignore("This test runs only in debug")]
#endif
		public class Tests
		{
			bool enableDiagnosticsInitialState;
			
			[SetUp]
			public void Setup()
			{
				enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
				RuntimeFeature.EnableDiagnostics = true;
			}

			[TearDown]
			public void TearDown()
			{
				RuntimeFeature.EnableDiagnostics = enableDiagnosticsInitialState;			
			}

			[TestCase(false)]
			//[TestCase(true)]
			public void Test(bool useCompiledXaml)
			{
				List<BindingBaseErrorEventArgs> failures = new List<BindingBaseErrorEventArgs>();
				BindingDiagnostics.BindingFailed += (o, e) => failures.Add(e);
				var layout = new BindingDiagnosticsTests(useCompiledXaml) { BindingContext = new { foo = "bar" } };
				Assert.That(failures.Count, Is.GreaterThan(0));
				var failure = failures[0] as BindingErrorEventArgs;
				Assert.That(((Binding)failure.Binding).Path, Is.EqualTo("foobar"));
				Assert.That(failure.XamlSourceInfo.LineNumber, Is.EqualTo(7));
				Assert.That(failure.Target, Is.TypeOf<Label>());
				Assert.That(failure.TargetProperty, Is.EqualTo(Label.TextProperty));
			}
		}
	}
}
