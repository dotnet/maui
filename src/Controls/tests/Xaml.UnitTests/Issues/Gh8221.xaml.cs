// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Gh8221VM
	{
		public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string> { { "EntryOne", "One" }, { "EntryTwo", "Two" } };
	}

	public partial class Gh8221 : ContentPage
	{
		public Gh8221() => InitializeComponent();
		public Gh8221(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				if (useCompiledXaml)
					MockCompiler.Compile(typeof(Gh8221));
				var layout = new Gh8221(useCompiledXaml) { BindingContext = new Gh8221VM() };
				Assert.Equal("One", layout.entryone.Text);
				Assert.Equal("Two", layout.entrytwo.Text);
			}
		}
	}
}
