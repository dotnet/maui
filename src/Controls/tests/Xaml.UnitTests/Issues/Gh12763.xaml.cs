// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Gh12763 : ContentPage
	{
		public Gh12763() => InitializeComponent();
		public Gh12763(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Tests
		{
			[Fact]
			public void QuotesInStringFormat([Values(false, true)] bool useCompiledXaml)
			{
				var layout = new Gh12763(useCompiledXaml);
				Assert.Equal("\"Foo\"", layout.label.Text);
			}
		}
	}
}
