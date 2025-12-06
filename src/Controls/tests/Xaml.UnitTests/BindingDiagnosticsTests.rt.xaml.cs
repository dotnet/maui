// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class BindingDiagnosticsTests : ContentPage
{
	public BindingDiagnosticsTests() => InitializeComponent();

	[Collection("Xaml Inflation")]
#if !DEBUG
	public class Tests
	{
		// Tests only run in DEBUG mode
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
		public void Test()
		{
			var inflator = XamlInflator.Runtime;
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
#endif
}
