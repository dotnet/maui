// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BindingDiagnosticsTests : ContentPage
{
	public BindingDiagnosticsTests() => InitializeComponent();


#if !DEBUG
	[Fact(Skip = "This test runs only in debug")]
#else
	public class Tests : IDisposable
	{
		bool enableDiagnosticsInitialState;

		public Tests()
		{
			enableDiagnosticsInitialState = RuntimeFeature.EnableDiagnostics;
			RuntimeFeature.EnableMauiDiagnostics = true;
		}

		public void Dispose()
		{
			RuntimeFeature.EnableMauiDiagnostics = enableDiagnosticsInitialState;
		}

		[Fact]
		public void Test() // TODO: Fix parameters - was (XamlInflator inflator), needs to be parameterized or use fixed value
		{
			// TODO: This test needs to be converted to use a specific inflator or made into a Theory test
			var inflator = XamlInflator.Runtime; // Using Runtime as default
			List<BindingBaseErrorEventArgs> failures = new List<BindingBaseErrorEventArgs>();
			BindingDiagnostics.BindingFailed += (o, e) => failures.Add(e);
			var layout = new BindingDiagnosticsTests(inflator) { BindingContext = new { foo = "bar" } };
			Assert.True(failures.Count > 0);
			var failure = failures[0] as BindingErrorEventArgs;
			Assert.Equal("foobar", ((Binding)failure.Binding).Path);
			Assert.Equal(7, failure.XamlSourceInfo.LineNumber);
			Assert.IsType<Label>(failure.Target);
			Assert.Equal(Label.TextProperty, failure.TargetProperty);
		}
	}
}
