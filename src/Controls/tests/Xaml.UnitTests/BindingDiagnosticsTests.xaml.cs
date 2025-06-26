// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Xaml.Diagnostics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class BindingDiagnosticsTests : ContentPage
	{
		public BindingDiagnosticsTests() => InitializeComponent();

		public BindingDiagnosticsTests(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}#if !DEBUG
		[Ignore("This test runs only in debug")]
#endif
		public class Tests
		{
			[Theory]
			[InlineData(false)]
			//[Theory]
			[InlineData(true)]
			public void Test(bool useCompiledXaml)
			{
				List<BindingBaseErrorEventArgs> failures = new List<BindingBaseErrorEventArgs>();
				BindingDiagnostics.BindingFailed += (o, e) => failures.Add(e);
				var layout = new BindingDiagnosticsTests(useCompiledXaml) { BindingContext = new { foo = "bar" } };
				Assert.That(failures.Count, Is.GreaterThan(0));
				var failure = failures[0] as BindingErrorEventArgs;
				Assert.Equal("foobar", ((Binding)failure.Binding).Path);
				Assert.Equal(7, failure.XamlSourceInfo.LineNumber);
				Assert.That(failure.Target, Is.TypeOf<Label>());
				Assert.Equal(Label.TextProperty, failure.TargetProperty);
			}
		}
	}
}
